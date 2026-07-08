Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web

Public Class Results
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
            LoadElections()
            LoadFaculties()
            LoadPositionsFilter()
            LoadResults()
        End If

    End Sub

    Private Sub LoadElections()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        ElectionID,
                        ElectionTitle
                    FROM Elections
                    ORDER BY ElectionID DESC
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlElections.Items.Clear()
                    ddlElections.Items.Add(New ListItem("Select Election", ""))

                    For Each row As DataRow In dt.Rows

                        ddlElections.Items.Add(
                            New ListItem(
                                row("ElectionTitle").ToString(),
                                row("ElectionID").ToString()
                            )
                        )

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading elections: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadFaculties()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        FacultyID,
                        FacultyName
                    FROM Faculties
                    ORDER BY FacultyName
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlFacultyFilter.Items.Clear()
                    ddlFacultyFilter.Items.Add(New ListItem("All Faculties", ""))

                    For Each row As DataRow In dt.Rows

                        ddlFacultyFilter.Items.Add(
                            New ListItem(
                                row("FacultyName").ToString(),
                                row("FacultyID").ToString()
                            )
                        )

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading faculties: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadPositionsFilter()

        Try
            ddlPositionFilter.Items.Clear()
            ddlPositionFilter.Items.Add(New ListItem("All Positions", ""))

            If ddlElections.SelectedValue = "" Then
                Return
            End If

            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT DISTINCT
                        P.PositionID,
                        P.PositionTitle
                    FROM Positions P
                    INNER JOIN Candidates C
                        ON C.PositionID = P.PositionID
                    WHERE P.ElectionID = @ElectionID
                "

                If ddlFacultyFilter.SelectedValue <> "" Then
                    query &= "
                        AND C.FacultyID = @FacultyID
                    "
                End If

                query &= "
                    ORDER BY P.PositionTitle
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ElectionID", Convert.ToInt32(ddlElections.SelectedValue))

                    If ddlFacultyFilter.SelectedValue <> "" Then
                        cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFacultyFilter.SelectedValue))
                    End If

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    For Each row As DataRow In dt.Rows

                        ddlPositionFilter.Items.Add(
                            New ListItem(
                                row("PositionTitle").ToString(),
                                row("PositionID").ToString()
                            )
                        )

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading positions filter: " & ex.Message, "error")
        End Try

    End Sub

    Private Function GetResultsData() As DataTable

        Dim dt As New DataTable()

        If ddlElections.SelectedValue = "" Then
            Return dt
        End If

        Dim electionID As Integer = Convert.ToInt32(ddlElections.SelectedValue)

        Using con As New SqlConnection(connectionString)

            Using cmd As New SqlCommand("sp_GetElectionResults", con)

                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@ElectionID", electionID)

                If ddlFacultyFilter.SelectedValue = "" Then
                    cmd.Parameters.AddWithValue("@FacultyID", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFacultyFilter.SelectedValue))
                End If

                If ddlPositionFilter.SelectedValue = "" Then
                    cmd.Parameters.AddWithValue("@PositionID", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(ddlPositionFilter.SelectedValue))
                End If

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using

            End Using

        End Using

        Return dt

    End Function

    Private Sub LoadResults()

        Try
            Dim dt As DataTable = GetResultsData()

            gvResults.DataSource = dt
            gvResults.DataBind()

            If ddlElections.SelectedValue = "" Then
                ShowMessage("Please select an election to view results.", "warning")
                Return
            End If

            If dt.Rows.Count = 0 Then
                ShowMessage("No results found for the selected election, faculty, and position.", "warning")
            Else
                pnlMessage.Visible = False
                lblMessage.Text = ""
            End If

        Catch ex As Exception
            ShowMessage("Error loading results: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub ddlElections_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlElections.SelectedIndexChanged

        LoadPositionsFilter()
        LoadResults()

    End Sub

    Protected Sub ddlFacultyFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacultyFilter.SelectedIndexChanged

        LoadPositionsFilter()
        LoadResults()

    End Sub

    Protected Sub ddlPositionFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPositionFilter.SelectedIndexChanged

        LoadResults()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadPositionsFilter()
        LoadResults()

    End Sub

    Protected Sub btnExportExcel_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click

        Try
            Dim dt As DataTable = GetResultsData()

            If ddlElections.SelectedValue = "" Then
                ShowMessage("Please select an election before exporting.", "warning")
                Return
            End If

            If dt.Rows.Count = 0 Then
                ShowMessage("No results available to export for the selected election, faculty, and position.", "warning")
                Return
            End If

            ExportResultsToExcel(dt)

        Catch ex As Exception
            ShowMessage("Error exporting results: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub ExportResultsToExcel(dt As DataTable)

        Dim electionTitle As String = ddlElections.SelectedItem.Text
        Dim facultyTitle As String = ddlFacultyFilter.SelectedItem.Text
        Dim positionTitle As String = ddlPositionFilter.SelectedItem.Text

        Dim fileName As String =
            "Election_Results_" & DateTime.Now.ToString("dd-MM-yyyy_HHmm") & ".xls"

        Dim sb As New StringBuilder()

        sb.Append("<html>")
        sb.Append("<head>")
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />")
        sb.Append("</head>")
        sb.Append("<body>")

        sb.Append("<h2>Election Results</h2>")
        sb.Append("<p><b>Export Date:</b> " & DateTime.Now.ToString("dd-MM-yyyy HH:mm") & "</p>")
        sb.Append("<p><b>Election:</b> " & HttpUtility.HtmlEncode(electionTitle) & "</p>")
        sb.Append("<p><b>Faculty:</b> " & HttpUtility.HtmlEncode(facultyTitle) & "</p>")
        sb.Append("<p><b>Position:</b> " & HttpUtility.HtmlEncode(positionTitle) & "</p>")

        sb.Append("<table border='1'>")

        sb.Append("<tr>")
        sb.Append("<th>Election</th>")
        sb.Append("<th>Position</th>")
        sb.Append("<th>Faculty</th>")
        sb.Append("<th>Candidate ID</th>")
        sb.Append("<th>Candidate Name</th>")
        sb.Append("<th>Major</th>")
        sb.Append("<th>Year Level</th>")
        sb.Append("<th>Total Votes</th>")
        sb.Append("</tr>")

        For Each row As DataRow In dt.Rows

            sb.Append("<tr>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ElectionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("PositionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FacultyName").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("CandidateID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FullName").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("Major").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("YearLevel").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("TotalVotes").ToString()) & "</td>")
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