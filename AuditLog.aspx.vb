Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Globalization
Imports System.Text
Imports System.Web

Public Class AuditLog
    Inherits System.Web.UI.Page

    Private ReadOnly connectionString As String =
        ConfigurationManager.ConnectionStrings("ElectionConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If Session("ADUsername") Is Nothing OrElse Session("UserRole") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Session("UserRole").ToString() <> "Admin" Then
            Response.Redirect("Login.aspx")
            Return
        End If

        Dim adUsername As String = Session("ADUsername").ToString()
        Dim fullName As String = ""

        If Session("FullName") IsNot Nothing Then
            fullName = Session("FullName").ToString()
        End If

        If fullName = "" Then
            fullName = adUsername
        End If

        lblAdminName.Text = "Admin: " & fullName
        lblUsername.Text = "Username: " & adUsername

        If Not IsPostBack Then
            pnlMessage.Visible = False
            LoadActionTypes()
            LoadAuditLog()
        End If

    End Sub

    Private Sub LoadActionTypes()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT DISTINCT ActionType
                    FROM AuditLog
                    ORDER BY ActionType
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlActionType.Items.Clear()
                    ddlActionType.Items.Add(New ListItem("All Actions", ""))

                    For Each row As DataRow In dt.Rows
                        ddlActionType.Items.Add(
                            New ListItem(row("ActionType").ToString(), row("ActionType").ToString())
                        )
                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading action types: " & ex.Message, "error")
        End Try

    End Sub

    Private Function GetAuditLogData() As DataTable

        Dim usernameFilter As String = txtUsernameFilter.Text.Trim()
        Dim actionTypeFilter As String = ddlActionType.SelectedValue

        Dim fromDate As DateTime
        Dim toDate As DateTime

        Dim hasFromDate As Boolean = False
        Dim hasToDate As Boolean = False

        If txtFromDate.Text.Trim() <> "" Then

            If Not DateTime.TryParseExact(
                txtFromDate.Text.Trim(),
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                fromDate
            ) Then
                Throw New ApplicationException("Invalid From Date. Use format: 07-07-2026")
            End If

            hasFromDate = True

        End If

        If txtToDate.Text.Trim() <> "" Then

            If Not DateTime.TryParseExact(
                txtToDate.Text.Trim(),
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                toDate
            ) Then
                Throw New ApplicationException("Invalid To Date. Use format: 10-07-2026")
            End If

            toDate = toDate.Date.AddDays(1).AddSeconds(-1)
            hasToDate = True

        End If

        If hasFromDate AndAlso hasToDate AndAlso toDate < fromDate Then
            Throw New ApplicationException("To Date must be after From Date.")
        End If

        Dim dt As New DataTable()

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT 
                    LogID,
                    ADUsername,
                    ActionType,
                    ActionDetails,
                    FORMAT(ActionDateTime, 'dd-MM-yyyy HH:mm') AS ActionDateTimeFormatted
                FROM AuditLog
                WHERE 1 = 1
            "

            If usernameFilter <> "" Then
                query &= "
                    AND ADUsername LIKE @ADUsername
                "
            End If

            If actionTypeFilter <> "" Then
                query &= "
                    AND ActionType = @ActionType
                "
            End If

            If hasFromDate Then
                query &= "
                    AND ActionDateTime >= @FromDate
                "
            End If

            If hasToDate Then
                query &= "
                    AND ActionDateTime <= @ToDate
                "
            End If

            query &= "
                ORDER BY LogID DESC
            "

            Using cmd As New SqlCommand(query, con)

                If usernameFilter <> "" Then
                    cmd.Parameters.AddWithValue("@ADUsername", "%" & usernameFilter & "%")
                End If

                If actionTypeFilter <> "" Then
                    cmd.Parameters.AddWithValue("@ActionType", actionTypeFilter)
                End If

                If hasFromDate Then
                    cmd.Parameters.AddWithValue("@FromDate", fromDate)
                End If

                If hasToDate Then
                    cmd.Parameters.AddWithValue("@ToDate", toDate)
                End If

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using

            End Using

        End Using

        Return dt

    End Function

    Private Sub LoadAuditLog()

        Try
            Dim dt As DataTable = GetAuditLogData()

            gvAuditLog.DataSource = dt
            gvAuditLog.DataBind()

            If dt.Rows.Count = 0 Then
                ShowMessage("No audit log records found for the selected filter.", "warning")
            Else
                pnlMessage.Visible = False
                lblMessage.Text = ""
            End If

        Catch ex As ApplicationException
            ShowMessage(ex.Message, "warning")

        Catch ex As Exception
            ShowMessage("Error loading audit log: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub btnApplyFilter_Click(sender As Object, e As EventArgs) Handles btnApplyFilter.Click

        LoadAuditLog()

    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click

        txtUsernameFilter.Text = ""
        txtFromDate.Text = ""
        txtToDate.Text = ""

        If ddlActionType.Items.Count > 0 Then
            ddlActionType.SelectedIndex = 0
        End If

        LoadAuditLog()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadActionTypes()
        LoadAuditLog()

    End Sub

    Protected Sub btnExportExcel_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click

        Try
            Dim dt As DataTable = GetAuditLogData()

            If dt.Rows.Count = 0 Then
                ShowMessage("No audit log records available to export for the selected filter.", "warning")
                Return
            End If

            ExportAuditLogToExcel(dt)

        Catch ex As ApplicationException
            ShowMessage(ex.Message, "warning")

        Catch ex As Exception
            ShowMessage("Error exporting audit log: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub ExportAuditLogToExcel(dt As DataTable)

        Dim fileName As String =
            "Audit_Log_" & DateTime.Now.ToString("dd-MM-yyyy_HHmm") & ".xls"

        Dim sb As New StringBuilder()

        sb.Append("<html>")
        sb.Append("<head>")
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />")
        sb.Append("</head>")
        sb.Append("<body>")

        sb.Append("<h2>Audit Log</h2>")
        sb.Append("<p><b>Export Date:</b> " & DateTime.Now.ToString("dd-MM-yyyy HH:mm") & "</p>")

        sb.Append("<p><b>Username Filter:</b> " & HttpUtility.HtmlEncode(txtUsernameFilter.Text.Trim()) & "</p>")
        sb.Append("<p><b>Action Type Filter:</b> " & HttpUtility.HtmlEncode(ddlActionType.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>From Date:</b> " & HttpUtility.HtmlEncode(txtFromDate.Text.Trim()) & "</p>")
        sb.Append("<p><b>To Date:</b> " & HttpUtility.HtmlEncode(txtToDate.Text.Trim()) & "</p>")

        sb.Append("<table border='1'>")

        sb.Append("<tr>")
        sb.Append("<th>ID</th>")
        sb.Append("<th>Username</th>")
        sb.Append("<th>Action Type</th>")
        sb.Append("<th>Details</th>")
        sb.Append("<th>Date / Time</th>")
        sb.Append("</tr>")

        For Each row As DataRow In dt.Rows

            sb.Append("<tr>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("LogID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ADUsername").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ActionType").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ActionDetails").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ActionDateTimeFormatted").ToString()) & "</td>")
            sb.Append("</tr>")

        Next

        sb.Append("</table>")
        sb.Append("</body>")
        sb.Append("</html>")

        Response.Clear()
        Response.Buffer = True
        Response.ContentType = "application/vnd.ms-excel"
        Response.AddHeader("Content-Disposition", "attachment;filename=" & fileName)
        Response.Charset = "utf-8"
        Response.ContentEncoding = Encoding.UTF8
        Response.Write(sb.ToString())
        Response.Flush()
        Response.End()

    End Sub

    Private Sub ShowMessage(message As String, messageType As String)

        lblMessage.Text = message
        pnlMessage.Visible = True

        If messageType = "success" Then
            pnlMessage.CssClass = "alert-box alert-success"
        ElseIf messageType = "warning" Then
            pnlMessage.CssClass = "alert-box alert-warning"
        Else
            pnlMessage.CssClass = "alert-box alert-error"
        End If

    End Sub

    Protected Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click

        Response.Redirect("AdminDashboard.aspx")

    End Sub

    Protected Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click

        Session.Clear()
        Session.Abandon()
        Response.Redirect("Login.aspx")

    End Sub

End Class