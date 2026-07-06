Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageVoters
    Inherits System.Web.UI.Page

    Private ReadOnly connectionString As String =
        ConfigurationManager.ConnectionStrings("ElectionConnection").ConnectionString

    Private Property FacultiesTable As DataTable
        Get
            Return TryCast(ViewState("FacultiesTable"), DataTable)
        End Get
        Set(value As DataTable)
            ViewState("FacultiesTable") = value
        End Set
    End Property

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

                    FacultiesTable = dt

                    ddlFaculty.DataSource = dt
                    ddlFaculty.DataTextField = "FacultyName"
                    ddlFaculty.DataValueField = "FacultyID"
                    ddlFaculty.DataBind()

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading faculties: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadVoters()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        V.ADUsername,
                        V.FacultyID,
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

                    BindFacultyDropdownsInGrid(dt)

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading voters: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropdownsInGrid(votersTable As DataTable)

        If FacultiesTable Is Nothing Then
            Return
        End If

        For Each row As GridViewRow In gvVoters.Rows

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            If ddl IsNot Nothing Then

                ddl.DataSource = FacultiesTable
                ddl.DataTextField = "FacultyName"
                ddl.DataValueField = "FacultyID"
                ddl.DataBind()

                Dim currentFacultyID As String =
                    votersTable.Rows(row.RowIndex)("FacultyID").ToString()

                If ddl.Items.FindByValue(currentFacultyID) IsNot Nothing Then
                    ddl.SelectedValue = currentFacultyID
                End If

            End If

        Next

    End Sub

    Protected Sub btnAddVoter_Click(sender As Object, e As EventArgs) Handles btnAddVoter.Click

        Dim voterUsername As String = txtADUsername.Text.Trim()

        If voterUsername = "" Then
            ShowMessage("Please enter the AD username.", "warning")
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            ShowMessage("Please select a faculty.", "warning")
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

                    checkCmd.Parameters.AddWithValue("@ADUsername", voterUsername)

                    Dim exists As Integer =
                        Convert.ToInt32(checkCmd.ExecuteScalar())

                    If exists > 0 Then
                        ShowMessage("This voter already exists. You can update the voter faculty or active status from the table.", "warning")
                        Return
                    End If

                End Using

                Dim insertQuery As String = "
                    INSERT INTO EligibleVoters (ADUsername, FacultyID, IsActive)
                    VALUES (@ADUsername, @FacultyID, 1)
                "

                Using cmd As New SqlCommand(insertQuery, con)

                    cmd.Parameters.AddWithValue("@ADUsername", voterUsername)
                    cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFaculty.SelectedValue))

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Voter",
                "Added eligible voter: " & voterUsername &
                ", FacultyID: " & ddlFaculty.SelectedValue
            )

            txtADUsername.Text = ""

            LoadVoters()

            ShowMessage("Eligible voter added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding voter: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvVoters_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvVoters.RowCommand

        If e.CommandName = "UpdateVoter" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim voterUsername As String =
                gvVoters.DataKeys(rowIndex).Values("ADUsername").ToString()

            Dim oldFacultyID As String =
                gvVoters.DataKeys(rowIndex).Values("FacultyID").ToString()

            Dim row As GridViewRow = gvVoters.Rows(rowIndex)

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim chk As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If ddl Is Nothing Then
                ShowMessage("Faculty dropdown not found.", "error")
                Return
            End If

            If chk Is Nothing Then
                ShowMessage("Active checkbox not found.", "error")
                Return
            End If

            Dim newFacultyID As Integer =
                Convert.ToInt32(ddl.SelectedValue)

            Dim isActive As Boolean = chk.Checked

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE EligibleVoters
                        SET 
                            FacultyID = @FacultyID,
                            IsActive = @IsActive
                        WHERE ADUsername = @ADUsername
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@FacultyID", newFacultyID)
                        cmd.Parameters.AddWithValue("@IsActive", isActive)
                        cmd.Parameters.AddWithValue("@ADUsername", voterUsername)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Voter",
                    "Updated voter: " & voterUsername &
                    ", Old FacultyID: " & oldFacultyID &
                    ", New FacultyID: " & newFacultyID.ToString() &
                    ", Active = " & isActive.ToString()
                )

                LoadVoters()

                ShowMessage("Voter updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating voter: " & ex.Message, "error")
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