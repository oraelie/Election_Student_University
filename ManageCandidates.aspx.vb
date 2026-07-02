Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManageCandidates
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
            lblMessage.Text = "Error loading positions: " & ex.Message
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

    Private Sub LoadCandidates()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        C.CandidateID,
                        E.ElectionTitle,
                        P.PositionTitle,
                        C.FullName,
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

                End Using

            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading candidates: " & ex.Message
        End Try

    End Sub

    Protected Sub btnAddCandidate_Click(sender As Object, e As EventArgs) Handles btnAddCandidate.Click

        If ddlPosition.SelectedValue = "" Then
            lblMessage.Text = "Please select a position."
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            lblMessage.Text = "Please select a faculty."
            Return
        End If

        Dim candidateName As String = txtFullName.Text.Trim()
        Dim major As String = txtMajor.Text.Trim()
        Dim description As String = txtDescription.Text.Trim()

        If candidateName = "" Then
            lblMessage.Text = "Please enter the candidate full name."
            Return
        End If

        Dim yearLevel As Integer

        If txtYearLevel.Text.Trim() <> "" Then
            If Not Integer.TryParse(txtYearLevel.Text.Trim(), yearLevel) Then
                lblMessage.Text = "Year level must be a number."
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

            lblMessage.Text = "Candidate added successfully."

            txtFullName.Text = ""
            txtMajor.Text = ""
            txtYearLevel.Text = ""
            txtDescription.Text = ""

            LoadCandidates()

        Catch ex As Exception
            lblMessage.Text = "Error adding candidate: " & ex.Message
        End Try

    End Sub

    Protected Sub gvCandidates_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvCandidates.RowCommand

        If e.CommandName = "UpdateActive" Then

            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim candidateID As Integer = Convert.ToInt32(gvCandidates.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvCandidates.Rows(rowIndex)
            Dim chk As CheckBox = TryCast(row.FindControl("chkIsActive"), CheckBox)

            If chk Is Nothing Then
                lblMessage.Text = "Active checkbox not found."
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Candidates
                        SET IsActive = @IsActive
                        WHERE CandidateID = @CandidateID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@IsActive", chk.Checked)
                        cmd.Parameters.AddWithValue("@CandidateID", candidateID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Candidate",
                    "Updated CandidateID: " & candidateID.ToString() &
                    ", Active = " & chk.Checked.ToString()
                )

                lblMessage.Text = "Candidate updated successfully."
                LoadCandidates()

            Catch ex As Exception
                lblMessage.Text = "Error updating candidate: " & ex.Message
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

    Protected Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Response.Redirect("AdminDashboard.aspx")
    End Sub

    Protected Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Session.Clear()
        Session.Abandon()
        Response.Redirect("Login.aspx")
    End Sub

End Class