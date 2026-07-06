Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageFaculties
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
            LoadFaculties()
        End If

    End Sub

    Private Sub LoadFaculties()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        FacultyID,
                        FacultyName,
                        IsActive
                    FROM Faculties
                    ORDER BY FacultyName
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvFaculties.DataSource = dt
                    gvFaculties.DataBind()

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading faculties: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub btnAddFaculty_Click(sender As Object, e As EventArgs) Handles btnAddFaculty.Click

        Dim facultyName As String = txtFacultyName.Text.Trim()

        If facultyName = "" Then
            ShowMessage("Please enter the faculty name.", "warning")
            Return
        End If

        Try
            Using con As New SqlConnection(connectionString)

                con.Open()

                Dim checkQuery As String = "
                    SELECT COUNT(*)
                    FROM Faculties
                    WHERE FacultyName = @FacultyName
                "

                Using checkCmd As New SqlCommand(checkQuery, con)

                    checkCmd.Parameters.AddWithValue("@FacultyName", facultyName)

                    Dim exists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If exists > 0 Then
                        ShowMessage("This faculty already exists.", "warning")
                        Return
                    End If

                End Using

                Dim insertQuery As String = "
                    INSERT INTO Faculties (FacultyName, IsActive)
                    VALUES (@FacultyName, 1)
                "

                Using cmd As New SqlCommand(insertQuery, con)

                    cmd.Parameters.AddWithValue("@FacultyName", facultyName)
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Faculty",
                "Added faculty: " & facultyName
            )

            txtFacultyName.Text = ""
            LoadFaculties()

            ShowMessage("Faculty added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding faculty: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvFaculties_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvFaculties.RowCommand

        If e.CommandName = "UpdateActive" Then

            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim facultyID As Integer = Convert.ToInt32(gvFaculties.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvFaculties.Rows(rowIndex)
            Dim chk As CheckBox = TryCast(row.FindControl("chkIsActive"), CheckBox)

            If chk Is Nothing Then
                ShowMessage("Active checkbox not found.", "error")
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Faculties
                        SET IsActive = @IsActive
                        WHERE FacultyID = @FacultyID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@IsActive", chk.Checked)
                        cmd.Parameters.AddWithValue("@FacultyID", facultyID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Faculty",
                    "Updated FacultyID: " & facultyID.ToString() &
                    ", Active = " & chk.Checked.ToString()
                )

                LoadFaculties()

                ShowMessage("Faculty updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating faculty: " & ex.Message, "error")
            End Try

        End If

    End Sub

    Private Sub AddAuditLog(actionType As String, actionDetails As String)

        Try
            If Session("ADUsername") Is Nothing Then
                Return
            End If

            Dim adminUsername As String = Session("ADUsername").ToString()

            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO AuditLog (ADUsername, ActionType, ActionDetails)
                    VALUES (@ADUsername, @ActionType, @ActionDetails)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ADUsername", adminUsername)
                    cmd.Parameters.AddWithValue("@ActionType", actionType)
                    cmd.Parameters.AddWithValue("@ActionDetails", actionDetails)

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

        Catch
            ' If audit log fails, do not stop the main action.
        End Try

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