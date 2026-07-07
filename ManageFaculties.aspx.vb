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

        Dim facultyNameFilter As String = txtFacultyNameFilter.Text.Trim()
        Dim statusFilter As String = ddlStatusFilter.SelectedValue

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        FacultyID,
                        FacultyName,
                        IsActive
                    FROM Faculties
                    WHERE 1 = 1
                "

                If facultyNameFilter <> "" Then
                    query &= "
                        AND FacultyName LIKE @FacultyName
                    "
                End If

                If statusFilter <> "" Then
                    query &= "
                        AND IsActive = @IsActive
                    "
                End If

                query &= "
                    ORDER BY FacultyName
                "

                Using cmd As New SqlCommand(query, con)

                    If facultyNameFilter <> "" Then
                        cmd.Parameters.AddWithValue("@FacultyName", "%" & facultyNameFilter & "%")
                    End If

                    If statusFilter <> "" Then
                        cmd.Parameters.AddWithValue("@IsActive", Convert.ToBoolean(Convert.ToInt32(statusFilter)))
                    End If

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvFaculties.DataSource = dt
                    gvFaculties.DataBind()

                    If dt.Rows.Count = 0 Then
                        ShowMessage("No faculties found for the selected filter.", "warning")
                    Else
                        pnlMessage.Visible = False
                        lblMessage.Text = ""
                    End If

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

                    Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If count > 0 Then
                        ShowMessage("This faculty already exists.", "warning")
                        Return
                    End If

                End Using

                Dim insertQuery As String = "
                    INSERT INTO Faculties (FacultyName, IsActive)
                    VALUES (@FacultyName, 1)
                "

                Using insertCmd As New SqlCommand(insertQuery, con)

                    insertCmd.Parameters.AddWithValue("@FacultyName", facultyName)
                    insertCmd.ExecuteNonQuery()

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

        If e.CommandName = "UpdateFaculty" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim facultyID As Integer =
                Convert.ToInt32(gvFaculties.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvFaculties.Rows(rowIndex)

            Dim txtGridFacultyName As TextBox =
                TryCast(row.FindControl("txtGridFacultyName"), TextBox)

            Dim chkIsActive As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If txtGridFacultyName Is Nothing OrElse chkIsActive Is Nothing Then
                ShowMessage("One or more faculty controls were not found.", "error")
                Return
            End If

            Dim facultyName As String = txtGridFacultyName.Text.Trim()
            Dim isActive As Boolean = chkIsActive.Checked

            If facultyName = "" Then
                ShowMessage("Faculty name cannot be empty.", "warning")
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)

                    con.Open()

                    Dim checkQuery As String = "
                        SELECT COUNT(*)
                        FROM Faculties
                        WHERE FacultyName = @FacultyName
                        AND FacultyID <> @FacultyID
                    "

                    Using checkCmd As New SqlCommand(checkQuery, con)

                        checkCmd.Parameters.AddWithValue("@FacultyName", facultyName)
                        checkCmd.Parameters.AddWithValue("@FacultyID", facultyID)

                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                        If count > 0 Then
                            ShowMessage("Another faculty with this name already exists.", "warning")
                            Return
                        End If

                    End Using

                    Dim updateQuery As String = "
                        UPDATE Faculties
                        SET 
                            FacultyName = @FacultyName,
                            IsActive = @IsActive
                        WHERE FacultyID = @FacultyID
                    "

                    Using updateCmd As New SqlCommand(updateQuery, con)

                        updateCmd.Parameters.AddWithValue("@FacultyName", facultyName)
                        updateCmd.Parameters.AddWithValue("@IsActive", isActive)
                        updateCmd.Parameters.AddWithValue("@FacultyID", facultyID)

                        updateCmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Faculty",
                    "Updated FacultyID: " & facultyID.ToString() &
                    ", Name: " & facultyName &
                    ", Active: " & isActive.ToString()
                )

                LoadFaculties()

                ShowMessage("Faculty updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating faculty: " & ex.Message, "error")
            End Try

        End If

    End Sub

    Protected Sub btnApplyFilter_Click(sender As Object, e As EventArgs) Handles btnApplyFilter.Click

        LoadFaculties()

    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click

        txtFacultyNameFilter.Text = ""
        ddlStatusFilter.SelectedValue = ""

        LoadFaculties()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadFaculties()

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