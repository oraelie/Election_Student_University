Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

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

        lblFullName.Text = "Admin: " & fullName
        lblUsername.Text = "Username: " & adUsername

        If Not IsPostBack Then
            pnlMessage.Visible = False
            LoadElections()
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

                    If dt.Rows.Count = 0 Then
                        ddlElections.Items.Add(New ListItem("No elections available", ""))
                        ShowMessage("No elections found.", "warning")
                        Return
                    End If

                    ddlElections.DataSource = dt
                    ddlElections.DataTextField = "ElectionTitle"
                    ddlElections.DataValueField = "ElectionID"
                    ddlElections.DataBind()

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading elections: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadResults()

        If ddlElections.SelectedValue = "" Then
            gvResults.DataSource = Nothing
            gvResults.DataBind()
            Return
        End If

        Dim electionID As Integer = Convert.ToInt32(ddlElections.SelectedValue)

        Try
            Using con As New SqlConnection(connectionString)

                Using cmd As New SqlCommand("sp_GetElectionResults", con)

                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ElectionID", electionID)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvResults.DataSource = dt
                    gvResults.DataBind()

                    If dt.Rows.Count = 0 Then
                        ShowMessage("No results found for the selected election.", "warning")
                    Else
                        pnlMessage.Visible = False
                        lblMessage.Text = ""
                    End If

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading results: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub ddlElections_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlElections.SelectedIndexChanged

        LoadResults()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadResults()

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