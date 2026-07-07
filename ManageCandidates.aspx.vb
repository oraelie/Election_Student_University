Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageCandidates
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
            LoadPositions()
            LoadFaculties()
            LoadCandidates()
        End If

    End Sub

    Private Sub LoadPositions()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        P.PositionID,
                        E.ElectionTitle + ' - ' + P.PositionTitle AS PositionDisplay
                    FROM Positions P
                    INNER JOIN Elections E
                        ON P.ElectionID = E.ElectionID
                    WHERE P.IsActive = 1
                    ORDER BY E.ElectionID DESC, P.PositionTitle
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlPosition.DataSource = dt
                    ddlPosition.DataTextField = "PositionDisplay"
                    ddlPosition.DataValueField = "PositionID"
                    ddlPosition.DataBind()

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading positions: " & ex.Message, "error")
        End Try

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

    Private Sub LoadCandidates()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        C.CandidateID,
                        E.ElectionTitle,
                        P.PositionTitle,
                        C.FullName,
                        C.FacultyID,
                        F.FacultyName,
                        C.Major,
                        C.YearLevel,
                        C.Description,
                        C.IsActive
                    FROM Candidates C
                    INNER JOIN Positions P
                        ON C.PositionID = P.PositionID
                    INNER JOIN Elections E
                        ON P.ElectionID = E.ElectionID
                    INNER JOIN Faculties F
                        ON C.FacultyID = F.FacultyID
                    ORDER BY E.ElectionID DESC, P.PositionTitle, C.FullName
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvCandidates.DataSource = dt
                    gvCandidates.DataBind()

                    BindFacultyDropdownsInGrid(dt)

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading candidates: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropdownsInGrid(candidatesTable As DataTable)

        If FacultiesTable Is Nothing Then
            Return
        End If

        For Each row As GridViewRow In gvCandidates.Rows

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            If ddl IsNot Nothing Then

                ddl.DataSource = FacultiesTable
                ddl.DataTextField = "FacultyName"
                ddl.DataValueField = "FacultyID"
                ddl.DataBind()

                Dim currentFacultyID As String =
                    candidatesTable.Rows(row.RowIndex)("FacultyID").ToString()

                If ddl.Items.FindByValue(currentFacultyID) IsNot Nothing Then
                    ddl.SelectedValue = currentFacultyID
                End If

            End If

        Next

    End Sub

    Protected Sub btnAddCandidate_Click(sender As Object, e As EventArgs) Handles btnAddCandidate.Click

        If ddlPosition.SelectedValue = "" Then
            ShowMessage("Please select a position.", "warning")
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            ShowMessage("Please select a faculty.", "warning")
            Return
        End If

        Dim candidateName As String = txtFullName.Text.Trim()
        Dim major As String = txtMajor.Text.Trim()
        Dim description As String = txtDescription.Text.Trim()

        If candidateName = "" Then
            ShowMessage("Please enter the candidate full name.", "warning")
            Return
        End If

        Dim yearLevel As Integer

        If txtYearLevel.Text.Trim() <> "" Then
            If Not Integer.TryParse(txtYearLevel.Text.Trim(), yearLevel) Then
                ShowMessage("Year level must be a number.", "warning")
                Return
            End If
        End If

        Dim positionID As Integer = Convert.ToInt32(ddlPosition.SelectedValue)
        Dim facultyID As Integer = Convert.ToInt32(ddlFaculty.SelectedValue)
        Dim positionText As String = ddlPosition.SelectedItem.Text
        Dim facultyText As String = ddlFaculty.SelectedItem.Text

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO Candidates 
                        (FullName, PositionID, FacultyID, Major, YearLevel, Description, IsActive)
                    VALUES 
                        (@FullName, @PositionID, @FacultyID, @Major, @YearLevel, @Description, 1)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@FullName", candidateName)
                    cmd.Parameters.AddWithValue("@PositionID", positionID)
                    cmd.Parameters.AddWithValue("@FacultyID", facultyID)

                    If major = "" Then
                        cmd.Parameters.AddWithValue("@Major", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@Major", major)
                    End If

                    If txtYearLevel.Text.Trim() = "" Then
                        cmd.Parameters.AddWithValue("@YearLevel", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@YearLevel", yearLevel)
                    End If

                    If description = "" Then
                        cmd.Parameters.AddWithValue("@Description", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@Description", description)
                    End If

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Candidate",
                "Added candidate: " & candidateName &
                ", Position: " & positionText &
                ", PositionID: " & positionID.ToString() &
                ", Faculty: " & facultyText &
                ", FacultyID: " & facultyID.ToString()
            )

            txtFullName.Text = ""
            txtMajor.Text = ""
            txtYearLevel.Text = ""
            txtDescription.Text = ""

            LoadCandidates()

            ShowMessage("Candidate added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding candidate: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvCandidates_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvCandidates.RowCommand

        If e.CommandName = "UpdateCandidate" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim candidateID As Integer =
                Convert.ToInt32(gvCandidates.DataKeys(rowIndex).Values("CandidateID"))

            Dim oldFacultyID As String =
                gvCandidates.DataKeys(rowIndex).Values("FacultyID").ToString()

            Dim row As GridViewRow = gvCandidates.Rows(rowIndex)

            Dim txtName As TextBox =
                TryCast(row.FindControl("txtGridFullName"), TextBox)

            Dim ddlFaculty As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim txtMajor As TextBox =
                TryCast(row.FindControl("txtGridMajor"), TextBox)

            Dim txtYear As TextBox =
                TryCast(row.FindControl("txtGridYearLevel"), TextBox)

            Dim txtDesc As TextBox =
                TryCast(row.FindControl("txtGridDescription"), TextBox)

            Dim chk As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If txtName Is Nothing OrElse ddlFaculty Is Nothing OrElse txtMajor Is Nothing OrElse txtYear Is Nothing OrElse txtDesc Is Nothing OrElse chk Is Nothing Then
                ShowMessage("One or more controls were not found.", "error")
                Return
            End If

            Dim candidateName As String = txtName.Text.Trim()
            Dim major As String = txtMajor.Text.Trim()
            Dim description As String = txtDesc.Text.Trim()
            Dim yearText As String = txtYear.Text.Trim()

            If candidateName = "" Then
                ShowMessage("Candidate name cannot be empty.", "warning")
                Return
            End If

            Dim yearLevel As Integer

            If yearText <> "" Then
                If Not Integer.TryParse(yearText, yearLevel) Then
                    ShowMessage("Year level must be a number.", "warning")
                    Return
                End If
            End If

            Dim newFacultyID As Integer = Convert.ToInt32(ddlFaculty.SelectedValue)
            Dim isActive As Boolean = chk.Checked

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Candidates
                        SET 
                            FullName = @FullName,
                            FacultyID = @FacultyID,
                            Major = @Major,
                            YearLevel = @YearLevel,
                            Description = @Description,
                            IsActive = @IsActive
                        WHERE CandidateID = @CandidateID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@FullName", candidateName)
                        cmd.Parameters.AddWithValue("@FacultyID", newFacultyID)

                        If major = "" Then
                            cmd.Parameters.AddWithValue("@Major", DBNull.Value)
                        Else
                            cmd.Parameters.AddWithValue("@Major", major)
                        End If

                        If yearText = "" Then
                            cmd.Parameters.AddWithValue("@YearLevel", DBNull.Value)
                        Else
                            cmd.Parameters.AddWithValue("@YearLevel", yearLevel)
                        End If

                        If description = "" Then
                            cmd.Parameters.AddWithValue("@Description", DBNull.Value)
                        Else
                            cmd.Parameters.AddWithValue("@Description", description)
                        End If

                        cmd.Parameters.AddWithValue("@IsActive", isActive)
                        cmd.Parameters.AddWithValue("@CandidateID", candidateID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Candidate",
                    "Updated CandidateID: " & candidateID.ToString() &
                    ", Name: " & candidateName &
                    ", Old FacultyID: " & oldFacultyID &
                    ", New FacultyID: " & newFacultyID.ToString() &
                    ", Active = " & isActive.ToString()
                )

                LoadCandidates()

                ShowMessage("Candidate updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating candidate: " & ex.Message, "error")
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