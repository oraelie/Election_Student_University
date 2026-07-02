Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageElections
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
                        CONVERT(VARCHAR(19), StartDateTime, 120) AS StartDateTime,
                        CONVERT(VARCHAR(19), EndDateTime, 120) AS EndDateTime,
                        Status
                    FROM Elections
                    ORDER BY ElectionID DESC
                "

                Using cmd As New SqlCommand(query, con)
                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvElections.DataSource = dt
                    gvElections.DataBind()

                    For Each row As GridViewRow In gvElections.Rows
                        Dim ddl As DropDownList = TryCast(row.FindControl("ddlGridStatus"), DropDownList)

                        If ddl IsNot Nothing Then
                            Dim status As String = dt.Rows(row.RowIndex)("Status").ToString()
                            ddl.SelectedValue = status
                        End If
                    Next
                End Using
            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading elections: " & ex.Message
        End Try

    End Sub

    Protected Sub btnAddElection_Click(sender As Object, e As EventArgs) Handles btnAddElection.Click

        If txtElectionTitle.Text.Trim() = "" Then
            lblMessage.Text = "Please enter the election title."
            Return
        End If

        Dim startDateTime As DateTime
        Dim endDateTime As DateTime

        If Not DateTime.TryParse(txtStartDateTime.Text.Trim(), startDateTime) Then
            lblMessage.Text = "Invalid start date/time. Use format: 2026-07-02 08:00:00"
            Return
        End If

        If Not DateTime.TryParse(txtEndDateTime.Text.Trim(), endDateTime) Then
            lblMessage.Text = "Invalid end date/time. Use format: 2026-07-02 16:00:00"
            Return
        End If

        If endDateTime <= startDateTime Then
            lblMessage.Text = "End date/time must be after start date/time."
            Return
        End If

        Try
            Using con As New SqlConnection(connectionString)
                Dim query As String = "
                    INSERT INTO Elections (ElectionTitle, StartDateTime, EndDateTime, Status)
                    VALUES (@ElectionTitle, @StartDateTime, @EndDateTime, @Status)
                "

                Using cmd As New SqlCommand(query, con)
                    cmd.Parameters.AddWithValue("@ElectionTitle", txtElectionTitle.Text.Trim())
                    cmd.Parameters.AddWithValue("@StartDateTime", startDateTime)
                    cmd.Parameters.AddWithValue("@EndDateTime", endDateTime)
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue)

                    con.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            lblMessage.Text = "Election added successfully."

            txtElectionTitle.Text = ""
            txtStartDateTime.Text = ""
            txtEndDateTime.Text = ""
            ddlStatus.SelectedValue = "Draft"

            LoadElections()

        Catch ex As Exception
            lblMessage.Text = "Error adding election: " & ex.Message
        End Try

    End Sub

    Protected Sub gvElections_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvElections.RowCommand

        If e.CommandName = "UpdateStatus" Then

            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim electionID As Integer = Convert.ToInt32(gvElections.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvElections.Rows(rowIndex)
            Dim ddl As DropDownList = TryCast(row.FindControl("ddlGridStatus"), DropDownList)

            If ddl Is Nothing Then
                lblMessage.Text = "Status dropdown not found."
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)
                    Dim query As String = "
                        UPDATE Elections
                        SET Status = @Status
                        WHERE ElectionID = @ElectionID
                    "

                    Using cmd As New SqlCommand(query, con)
                        cmd.Parameters.AddWithValue("@Status", ddl.SelectedValue)
                        cmd.Parameters.AddWithValue("@ElectionID", electionID)

                        con.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                lblMessage.Text = "Election status updated successfully."
                LoadElections()

            Catch ex As Exception
                lblMessage.Text = "Error updating status: " & ex.Message
            End Try

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