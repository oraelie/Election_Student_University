Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class PreElectionCheck
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
        End If

    End Sub

    Private Sub LoadElections()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        ElectionID,
                        ElectionTitle,
                        Status
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

                        Dim displayText As String =
                            row("ElectionTitle").ToString() &
                            " (" & row("Status").ToString() & ")"

                        ddlElections.Items.Add(
                            New ListItem(
                                displayText,
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

    Protected Sub btnRunCheck_Click(sender As Object, e As EventArgs) Handles btnRunCheck.Click

        If ddlElections.SelectedValue = "" Then
            ShowMessage("Please select an election first.", "warning")
            Return
        End If

        RunValidationCheck()

    End Sub

    Private Sub RunValidationCheck()

        Try
            Dim electionID As Integer = Convert.ToInt32(ddlElections.SelectedValue)

            Using con As New SqlConnection(connectionString)

                Using cmd As New SqlCommand("sp_PreElectionValidation", con)

                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ElectionID", electionID)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvValidationResults.DataSource = dt
                    gvValidationResults.DataBind()

                    If dt.Rows.Count = 0 Then
                        ShowMessage("No validation results returned.", "warning")
                        Return
                    End If

                    Dim hasError As Boolean = False
                    Dim hasWarning As Boolean = False

                    For Each row As DataRow In dt.Rows

                        Dim status As String = row("Status").ToString().ToUpper()

                        If status = "ERROR" Then
                            hasError = True
                        ElseIf status = "WARNING" Then
                            hasWarning = True
                        End If

                    Next

                    If hasError Then
                        ShowMessage("Validation completed. Errors found. Do not open this election until errors are fixed.", "error")
                    ElseIf hasWarning Then
                        ShowMessage("Validation completed. No errors, but warnings need review before opening the election.", "warning")
                    Else
                        ShowMessage("Validation completed successfully. This election looks ready.", "success")
                    End If

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error running validation check: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvValidationResults_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvValidationResults.RowDataBound

        If e.Row.RowType = DataControlRowType.DataRow Then

            Dim lblStatus As Label =
                TryCast(e.Row.FindControl("lblStatus"), Label)

            If lblStatus IsNot Nothing Then

                Dim status As String = lblStatus.Text.Trim().ToUpper()

                If status = "OK" Then
                    lblStatus.CssClass = "status-ok"
                ElseIf status = "WARNING" Then
                    lblStatus.CssClass = "status-warning"
                ElseIf status = "ERROR" Then
                    lblStatus.CssClass = "status-error"
                End If

            End If

        End If

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