Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageVoters
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
            LoadFaculties()
            LoadVoters()
        End If

    End Sub

    Private Sub LoadFaculties()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT FacultyID, FacultyName
                    FROM Faculties
                    WHERE IsActive = 1
                    ORDER BY FacultyName
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlFaculty.DataSource = dt
                    ddlFaculty.DataTextField = "FacultyName"
                    ddlFaculty.DataValueField = "FacultyID"
                    ddlFaculty.DataBind()

                End Using

            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading faculties: " & ex.Message
        End Try

    End Sub

    Private Sub LoadVoters()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        V.ADUsername,
                        F.FacultyName,
                        V.IsActive
                    FROM EligibleVoters V
                    INNER JOIN Faculties F
                        ON V.FacultyID = F.FacultyID
                    ORDER BY V.ADUsername
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvVoters.DataSource = dt
                    gvVoters.DataBind()

                End Using

            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading voters: " & ex.Message
        End Try

    End Sub

    Protected Sub btnAddVoter_Click(sender As Object, e As EventArgs) Handles btnAddVoter.Click

        Dim adUsername As String = txtADUsername.Text.Trim()

        If adUsername = "" Then
            lblMessage.Text = "Please enter the AD username."
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            lblMessage.Text = "Please select a faculty."
            Return
        End If

        Try
            Using con As New SqlConnection(connectionString)

                con.Open()

                Dim checkQuery As String = "
                    SELECT COUNT(*)
                    FROM EligibleVoters
                    WHERE ADUsername = @ADUsername
                "

                Using checkCmd As New SqlCommand(checkQuery, con)
                    checkCmd.Parameters.AddWithValue("@ADUsername", adUsername)

                    Dim exists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If exists > 0 Then
                        lblMessage.Text = "This voter already exists. You can activate or deactivate the voter from the table."
                        Return
                    End If
                End Using

                Dim insertQuery As String = "
                    INSERT INTO EligibleVoters (ADUsername, FacultyID, IsActive)
                    VALUES (@ADUsername, @FacultyID, 1)
                "

                Using cmd As New SqlCommand(insertQuery, con)
                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)
                    cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFaculty.SelectedValue))

                    cmd.ExecuteNonQuery()
                End Using

            End Using

            lblMessage.Text = "Eligible voter added successfully."
            txtADUsername.Text = ""

            LoadVoters()

        Catch ex As Exception
            lblMessage.Text = "Error adding voter: " & ex.Message
        End Try

    End Sub

    Protected Sub gvVoters_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvVoters.RowCommand

        If e.CommandName = "UpdateActive" Then

            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim adUsername As String = gvVoters.DataKeys(rowIndex).Value.ToString()

            Dim row As GridViewRow = gvVoters.Rows(rowIndex)
            Dim chk As CheckBox = TryCast(row.FindControl("chkIsActive"), CheckBox)

            If chk Is Nothing Then
                lblMessage.Text = "Active checkbox not found."
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE EligibleVoters
                        SET IsActive = @IsActive
                        WHERE ADUsername = @ADUsername
                    "

                    Using cmd As New SqlCommand(query, con)
                        cmd.Parameters.AddWithValue("@IsActive", chk.Checked)
                        cmd.Parameters.AddWithValue("@ADUsername", adUsername)

                        con.Open()
                        cmd.ExecuteNonQuery()
                    End Using

                End Using

                lblMessage.Text = "Voter updated successfully."
                LoadVoters()

            Catch ex As Exception
                lblMessage.Text = "Error updating voter: " & ex.Message
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