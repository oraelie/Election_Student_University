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
                    SELECT 
                        FacultyID,
                        FacultyName
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

                    ddlFaculty.Items.Clear()
                    ddlFacultyFilter.Items.Clear()

                    ddlFaculty.Items.Add(New ListItem("Select Faculty", ""))
                    ddlFacultyFilter.Items.Add(New ListItem("All Faculties", ""))

                    For Each row As DataRow In dt.Rows

                        Dim facultyID As String = row("FacultyID").ToString()
                        Dim facultyName As String = row("FacultyName").ToString()

                        ddlFaculty.Items.Add(New ListItem(facultyName, facultyID))
                        ddlFacultyFilter.Items.Add(New ListItem(facultyName, facultyID))

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading faculties: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadVoters()

        Dim usernameFilter As String = txtUsernameFilter.Text.Trim()
        Dim facultyFilter As String = ddlFacultyFilter.SelectedValue
        Dim statusFilter As String = ddlStatusFilter.SelectedValue

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
                    WHERE 1 = 1
                "

                If usernameFilter <> "" Then
                    query &= "
                        AND V.ADUsername LIKE @ADUsername
                    "
                End If

                If facultyFilter <> "" Then
                    query &= "
                        AND V.FacultyID = @FacultyID
                    "
                End If

                If statusFilter <> "" Then
                    query &= "
                        AND V.IsActive = @IsActive
                    "
                End If

                query &= "
                    ORDER BY V.ADUsername
                "

                Using cmd As New SqlCommand(query, con)

                    If usernameFilter <> "" Then
                        cmd.Parameters.AddWithValue("@ADUsername", "%" & usernameFilter & "%")
                    End If

                    If facultyFilter <> "" Then
                        cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(facultyFilter))
                    End If

                    If statusFilter <> "" Then
                        cmd.Parameters.AddWithValue("@IsActive", Convert.ToBoolean(Convert.ToInt32(statusFilter)))
                    End If

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvVoters.DataSource = dt
                    gvVoters.DataBind()

                    BindFacultyDropDownsInGrid(dt)

                    If dt.Rows.Count = 0 Then
                        ShowMessage("No voters found for the selected filter.", "warning")
                    Else
                        pnlMessage.Visible = False
                        lblMessage.Text = ""
                    End If

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading voters: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropDownsInGrid(votersTable As DataTable)

        If FacultiesTable Is Nothing Then
            Return
        End If

        For Each row As GridViewRow In gvVoters.Rows

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            If ddl IsNot Nothing Then

                ddl.Items.Clear()

                For Each facultyRow As DataRow In FacultiesTable.Rows

                    ddl.Items.Add(
                        New ListItem(
                            facultyRow("FacultyName").ToString(),
                            facultyRow("FacultyID").ToString()
                        )
                    )

                Next

                Dim selectedFacultyID As String =
                    votersTable.Rows(row.RowIndex)("FacultyID").ToString()

                If ddl.Items.FindByValue(selectedFacultyID) IsNot Nothing Then
                    ddl.SelectedValue = selectedFacultyID
                End If

            End If

        Next

    End Sub

    Protected Sub btnAddVoter_Click(sender As Object, e As EventArgs) Handles btnAddVoter.Click

        Dim adUsername As String = txtADUsername.Text.Trim()

        If adUsername = "" Then
            ShowMessage("Please enter the AD username or student ID.", "warning")
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            ShowMessage("Please select a faculty.", "warning")
            Return
        End If

        Dim facultyID As Integer = Convert.ToInt32(ddlFaculty.SelectedValue)

        Try
            Using con As New SqlConnection(connectionString)

                Dim checkQuery As String = "
                    SELECT COUNT(*)
                    FROM EligibleVoters
                    WHERE ADUsername = @ADUsername
                "

                Using checkCmd As New SqlCommand(checkQuery, con)

                    checkCmd.Parameters.AddWithValue("@ADUsername", adUsername)

                    con.Open()

                    Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If count > 0 Then
                        ShowMessage("This voter already exists.", "warning")
                        Return
                    End If

                End Using

                Dim insertQuery As String = "
                    INSERT INTO EligibleVoters (ADUsername, FacultyID, IsActive)
                    VALUES (@ADUsername, @FacultyID, 1)
                "

                Using insertCmd As New SqlCommand(insertQuery, con)

                    insertCmd.Parameters.AddWithValue("@ADUsername", adUsername)
                    insertCmd.Parameters.AddWithValue("@FacultyID", facultyID)

                    insertCmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Voter",
                "Added voter: " & adUsername &
                ", FacultyID: " & facultyID.ToString()
            )

            txtADUsername.Text = ""
            ddlFaculty.SelectedIndex = 0

            LoadVoters()

            ShowMessage("Voter added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding voter: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvVoters_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvVoters.RowCommand

        If e.CommandName = "UpdateVoter" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim adUsername As String =
                gvVoters.DataKeys(rowIndex).Values("ADUsername").ToString()

            Dim oldFacultyID As String =
                gvVoters.DataKeys(rowIndex).Values("FacultyID").ToString()

            Dim row As GridViewRow = gvVoters.Rows(rowIndex)

            Dim ddlGridFaculty As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim chkIsActive As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If ddlGridFaculty Is Nothing OrElse chkIsActive Is Nothing Then
                ShowMessage("One or more voter controls were not found.", "error")
                Return
            End If

            Dim newFacultyID As Integer = Convert.ToInt32(ddlGridFaculty.SelectedValue)
            Dim isActive As Boolean = chkIsActive.Checked

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
                        cmd.Parameters.AddWithValue("@ADUsername", adUsername)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Voter",
                    "Updated voter: " & adUsername &
                    ", Old FacultyID: " & oldFacultyID &
                    ", New FacultyID: " & newFacultyID.ToString() &
                    ", Active: " & isActive.ToString()
                )

                LoadVoters()

                ShowMessage("Voter updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating voter: " & ex.Message, "error")
            End Try

        End If

    End Sub

    Protected Sub btnApplyFilter_Click(sender As Object, e As EventArgs) Handles btnApplyFilter.Click

        LoadVoters()

    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click

        txtUsernameFilter.Text = ""

        If ddlFacultyFilter.Items.Count > 0 Then
            ddlFacultyFilter.SelectedIndex = 0
        End If

        ddlStatusFilter.SelectedValue = ""

        LoadVoters()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadFaculties()
        LoadVoters()

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