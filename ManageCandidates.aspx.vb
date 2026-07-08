Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web

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
            LoadFaculties()
            LoadElections()
            LoadPositions()
            LoadPositionFilter()
            LoadCandidates()
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

                    ddlElectionFilter.Items.Clear()
                    ddlElectionFilter.Items.Add(New ListItem("All Elections", ""))

                    For Each row As DataRow In dt.Rows
                        ddlElectionFilter.Items.Add(
                            New ListItem(
                                row("ElectionTitle").ToString(),
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

                    ddlPosition.Items.Clear()
                    ddlPosition.Items.Add(New ListItem("Select Position", ""))

                    For Each row As DataRow In dt.Rows
                        ddlPosition.Items.Add(
                            New ListItem(
                                row("PositionDisplay").ToString(),
                                row("PositionID").ToString()
                            )
                        )
                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading positions: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadPositionFilter()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        P.PositionID,
                        E.ElectionID,
                        E.ElectionTitle + ' - ' + P.PositionTitle AS PositionDisplay
                    FROM Positions P
                    INNER JOIN Elections E
                        ON P.ElectionID = E.ElectionID
                    WHERE 1 = 1
                "

                If ddlElectionFilter.SelectedValue <> "" Then
                    query &= "
                        AND E.ElectionID = @ElectionID
                    "
                End If

                query &= "
                    ORDER BY E.ElectionID DESC, P.PositionTitle
                "

                Using cmd As New SqlCommand(query, con)

                    If ddlElectionFilter.SelectedValue <> "" Then
                        cmd.Parameters.AddWithValue("@ElectionID", Convert.ToInt32(ddlElectionFilter.SelectedValue))
                    End If

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlPositionFilter.Items.Clear()
                    ddlPositionFilter.Items.Add(New ListItem("All Positions", ""))

                    For Each row As DataRow In dt.Rows
                        ddlPositionFilter.Items.Add(
                            New ListItem(
                                row("PositionDisplay").ToString(),
                                row("PositionID").ToString()
                            )
                        )
                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading position filter: " & ex.Message, "error")
        End Try

    End Sub

    Private Function GetCandidatesData() As DataTable

        Dim candidateNameFilter As String = txtCandidateNameFilter.Text.Trim()
        Dim electionFilter As String = ddlElectionFilter.SelectedValue
        Dim positionFilter As String = ddlPositionFilter.SelectedValue
        Dim facultyFilter As String = ddlFacultyFilter.SelectedValue
        Dim statusFilter As String = ddlStatusFilter.SelectedValue

        Dim dt As New DataTable()

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT 
                    C.CandidateID,
                    C.CandidateADUsername,
                    C.FullName,
                    C.PositionID,
                    P.PositionTitle,
                    E.ElectionID,
                    E.ElectionTitle,
                    C.FacultyID,
                    F.FacultyName,
                    C.Major,
                    C.YearLevel,
                    C.Description,
                    C.IsActive,
                    CASE 
                        WHEN C.IsActive = 1 THEN 'Active'
                        ELSE 'Inactive'
                    END AS StatusText
                FROM Candidates C
                INNER JOIN Positions P
                    ON C.PositionID = P.PositionID
                INNER JOIN Elections E
                    ON P.ElectionID = E.ElectionID
                INNER JOIN Faculties F
                    ON C.FacultyID = F.FacultyID
                WHERE 1 = 1
            "

            If candidateNameFilter <> "" Then
                query &= "
                    AND (
                        C.FullName LIKE @FullName
                        OR C.CandidateADUsername LIKE @FullName
                    )
                "
            End If

            If electionFilter <> "" Then
                query &= "
                    AND E.ElectionID = @ElectionID
                "
            End If

            If positionFilter <> "" Then
                query &= "
                    AND C.PositionID = @PositionID
                "
            End If

            If facultyFilter <> "" Then
                query &= "
                    AND C.FacultyID = @FacultyID
                "
            End If

            If statusFilter <> "" Then
                query &= "
                    AND C.IsActive = @IsActive
                "
            End If

            query &= "
                ORDER BY E.ElectionID DESC, P.PositionTitle, C.FullName
            "

            Using cmd As New SqlCommand(query, con)

                If candidateNameFilter <> "" Then
                    cmd.Parameters.AddWithValue("@FullName", "%" & candidateNameFilter & "%")
                End If

                If electionFilter <> "" Then
                    cmd.Parameters.AddWithValue("@ElectionID", Convert.ToInt32(electionFilter))
                End If

                If positionFilter <> "" Then
                    cmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(positionFilter))
                End If

                If facultyFilter <> "" Then
                    cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(facultyFilter))
                End If

                If statusFilter <> "" Then
                    cmd.Parameters.AddWithValue("@IsActive", Convert.ToBoolean(Convert.ToInt32(statusFilter)))
                End If

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using

            End Using

        End Using

        Return dt

    End Function

    Private Sub LoadCandidates()

        Try
            Dim dt As DataTable = GetCandidatesData()

            gvCandidates.DataSource = dt
            gvCandidates.DataBind()

            BindFacultyDropDownsInGrid(dt)

            If dt.Rows.Count = 0 Then
                ShowMessage("No candidates found for the selected filter.", "warning")
            Else
                pnlMessage.Visible = False
                lblMessage.Text = ""
            End If

        Catch ex As Exception
            ShowMessage("Error loading candidates: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropDownsInGrid(candidatesTable As DataTable)

        If FacultiesTable Is Nothing Then
            Return
        End If

        For Each row As GridViewRow In gvCandidates.Rows

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
                    candidatesTable.Rows(row.RowIndex)("FacultyID").ToString()

                If ddl.Items.FindByValue(selectedFacultyID) IsNot Nothing Then
                    ddl.SelectedValue = selectedFacultyID
                End If

            End If

        Next

    End Sub

    Private Function EligibleVoterIsActive(candidateADUsername As String) As Boolean

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT COUNT(*)
                FROM EligibleVoters
                WHERE ADUsername = @ADUsername
                AND IsActive = 1
            "

            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@ADUsername", candidateADUsername)

                con.Open()

                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                Return count > 0

            End Using

        End Using

    End Function

    Private Function CandidateUsernameAlreadyExists(
        candidateADUsername As String,
        positionID As Integer,
        excludeCandidateID As Integer
    ) As Boolean

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT COUNT(*)
                FROM Candidates
                WHERE PositionID = @PositionID
                AND LOWER(LTRIM(RTRIM(CandidateADUsername))) = LOWER(LTRIM(RTRIM(@CandidateADUsername)))
            "

            If excludeCandidateID > 0 Then
                query &= "
                    AND CandidateID <> @ExcludeCandidateID
                "
            End If

            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@PositionID", positionID)
                cmd.Parameters.AddWithValue("@CandidateADUsername", candidateADUsername)

                If excludeCandidateID > 0 Then
                    cmd.Parameters.AddWithValue("@ExcludeCandidateID", excludeCandidateID)
                End If

                con.Open()

                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                Return count > 0

            End Using

        End Using

    End Function

    Protected Sub btnAddCandidate_Click(sender As Object, e As EventArgs) Handles btnAddCandidate.Click

        Dim candidateADUsername As String = txtCandidateADUsername.Text.Trim()
        Dim fullName As String = txtFullName.Text.Trim()
        Dim major As String = txtMajor.Text.Trim()
        Dim description As String = txtDescription.Text.Trim()

        If candidateADUsername = "" Then
            ShowMessage("Please enter the candidate username / student ID.", "warning")
            Return
        End If

        If fullName = "" Then
            ShowMessage("Please enter the candidate full name.", "warning")
            Return
        End If

        If ddlPosition.SelectedValue = "" Then
            ShowMessage("Please select a position.", "warning")
            Return
        End If

        If ddlFaculty.SelectedValue = "" Then
            ShowMessage("Please select a faculty.", "warning")
            Return
        End If

        If Not EligibleVoterIsActive(candidateADUsername) Then
            ShowMessage("This username/student ID does not exist as an active eligible voter.", "warning")
            Return
        End If

        Dim yearLevel As Integer

        If txtYearLevel.Text.Trim() <> "" Then
            If Not Integer.TryParse(txtYearLevel.Text.Trim(), yearLevel) Then
                ShowMessage("Year level must be a number.", "warning")
                Return
            End If
        Else
            yearLevel = 0
        End If

        Dim positionID As Integer = Convert.ToInt32(ddlPosition.SelectedValue)
        Dim facultyID As Integer = Convert.ToInt32(ddlFaculty.SelectedValue)

        Try
            If CandidateUsernameAlreadyExists(candidateADUsername, positionID, 0) Then
                ShowMessage("This username/student ID is already a candidate for the selected position.", "warning")
                Return
            End If

            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO Candidates 
                        (CandidateADUsername, FullName, PositionID, FacultyID, Major, YearLevel, Description, IsActive)
                    VALUES 
                        (@CandidateADUsername, @FullName, @PositionID, @FacultyID, @Major, @YearLevel, @Description, 1)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@CandidateADUsername", candidateADUsername)
                    cmd.Parameters.AddWithValue("@FullName", fullName)
                    cmd.Parameters.AddWithValue("@PositionID", positionID)
                    cmd.Parameters.AddWithValue("@FacultyID", facultyID)
                    cmd.Parameters.AddWithValue("@Major", major)
                    cmd.Parameters.AddWithValue("@YearLevel", yearLevel)
                    cmd.Parameters.AddWithValue("@Description", description)

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Candidate",
                "Added candidate username: " & candidateADUsername &
                ", Name: " & fullName &
                ", PositionID: " & positionID.ToString() &
                ", FacultyID: " & facultyID.ToString()
            )

            txtCandidateADUsername.Text = ""
            txtFullName.Text = ""
            txtMajor.Text = ""
            txtYearLevel.Text = ""
            txtDescription.Text = ""

            ddlPosition.SelectedIndex = 0
            ddlFaculty.SelectedIndex = 0

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

            Dim positionID As Integer =
                Convert.ToInt32(gvCandidates.DataKeys(rowIndex).Values("PositionID"))

            Dim oldFacultyID As String =
                gvCandidates.DataKeys(rowIndex).Values("FacultyID").ToString()

            Dim row As GridViewRow = gvCandidates.Rows(rowIndex)

            Dim txtGridCandidateADUsername As TextBox =
                TryCast(row.FindControl("txtGridCandidateADUsername"), TextBox)

            Dim txtGridFullName As TextBox =
                TryCast(row.FindControl("txtGridFullName"), TextBox)

            Dim ddlGridFaculty As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim txtGridMajor As TextBox =
                TryCast(row.FindControl("txtGridMajor"), TextBox)

            Dim txtGridYearLevel As TextBox =
                TryCast(row.FindControl("txtGridYearLevel"), TextBox)

            Dim txtGridDescription As TextBox =
                TryCast(row.FindControl("txtGridDescription"), TextBox)

            Dim chkIsActive As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If txtGridCandidateADUsername Is Nothing OrElse txtGridFullName Is Nothing OrElse ddlGridFaculty Is Nothing OrElse
               txtGridMajor Is Nothing OrElse txtGridYearLevel Is Nothing OrElse txtGridDescription Is Nothing OrElse chkIsActive Is Nothing Then

                ShowMessage("One or more candidate controls were not found.", "error")
                Return

            End If

            Dim candidateADUsername As String = txtGridCandidateADUsername.Text.Trim()
            Dim fullName As String = txtGridFullName.Text.Trim()
            Dim major As String = txtGridMajor.Text.Trim()
            Dim description As String = txtGridDescription.Text.Trim()
            Dim newFacultyID As Integer = Convert.ToInt32(ddlGridFaculty.SelectedValue)
            Dim isActive As Boolean = chkIsActive.Checked

            If candidateADUsername = "" Then
                ShowMessage("Candidate username / student ID cannot be empty.", "warning")
                Return
            End If

            If fullName = "" Then
                ShowMessage("Candidate name cannot be empty.", "warning")
                Return
            End If

            If Not EligibleVoterIsActive(candidateADUsername) Then
                ShowMessage("This username/student ID does not exist as an active eligible voter.", "warning")
                Return
            End If

            Dim yearLevel As Integer

            If txtGridYearLevel.Text.Trim() <> "" Then
                If Not Integer.TryParse(txtGridYearLevel.Text.Trim(), yearLevel) Then
                    ShowMessage("Year level must be a number.", "warning")
                    Return
                End If
            Else
                yearLevel = 0
            End If

            Try
                If CandidateUsernameAlreadyExists(candidateADUsername, positionID, candidateID) Then
                    ShowMessage("Another candidate with the same username/student ID already exists for this position.", "warning")
                    Return
                End If

                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Candidates
                        SET 
                            CandidateADUsername = @CandidateADUsername,
                            FullName = @FullName,
                            FacultyID = @FacultyID,
                            Major = @Major,
                            YearLevel = @YearLevel,
                            Description = @Description,
                            IsActive = @IsActive
                        WHERE CandidateID = @CandidateID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@CandidateADUsername", candidateADUsername)
                        cmd.Parameters.AddWithValue("@FullName", fullName)
                        cmd.Parameters.AddWithValue("@FacultyID", newFacultyID)
                        cmd.Parameters.AddWithValue("@Major", major)
                        cmd.Parameters.AddWithValue("@YearLevel", yearLevel)
                        cmd.Parameters.AddWithValue("@Description", description)
                        cmd.Parameters.AddWithValue("@IsActive", isActive)
                        cmd.Parameters.AddWithValue("@CandidateID", candidateID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Candidate",
                    "Updated CandidateID: " & candidateID.ToString() &
                    ", Username: " & candidateADUsername &
                    ", Name: " & fullName &
                    ", Old FacultyID: " & oldFacultyID &
                    ", New FacultyID: " & newFacultyID.ToString() &
                    ", Active: " & isActive.ToString()
                )

                LoadCandidates()

                ShowMessage("Candidate updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating candidate: " & ex.Message, "error")
            End Try

        End If

    End Sub

    Protected Sub ddlElectionFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlElectionFilter.SelectedIndexChanged

        LoadPositionFilter()
        LoadCandidates()

    End Sub

    Protected Sub btnApplyFilter_Click(sender As Object, e As EventArgs) Handles btnApplyFilter.Click

        LoadCandidates()

    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click

        txtCandidateNameFilter.Text = ""

        If ddlElectionFilter.Items.Count > 0 Then
            ddlElectionFilter.SelectedIndex = 0
        End If

        LoadPositionFilter()

        If ddlPositionFilter.Items.Count > 0 Then
            ddlPositionFilter.SelectedIndex = 0
        End If

        If ddlFacultyFilter.Items.Count > 0 Then
            ddlFacultyFilter.SelectedIndex = 0
        End If

        ddlStatusFilter.SelectedValue = ""

        LoadCandidates()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadFaculties()
        LoadElections()
        LoadPositions()
        LoadPositionFilter()
        LoadCandidates()

    End Sub

    Protected Sub btnExportExcel_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click

        Try
            Dim dt As DataTable = GetCandidatesData()

            If dt.Rows.Count = 0 Then
                ShowMessage("No candidates available to export for the selected filter.", "warning")
                Return
            End If

            ExportCandidatesToExcel(dt)

        Catch ex As Exception
            ShowMessage("Error exporting candidates: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub ExportCandidatesToExcel(dt As DataTable)

        Dim fileName As String =
            "Candidates_" & DateTime.Now.ToString("dd-MM-yyyy_HHmm") & ".xls"

        Dim sb As New StringBuilder()

        sb.Append("<html>")
        sb.Append("<head>")
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />")
        sb.Append("</head>")
        sb.Append("<body>")

        sb.Append("<h2>Candidates List</h2>")
        sb.Append("<p><b>Export Date:</b> " & DateTime.Now.ToString("dd-MM-yyyy HH:mm") & "</p>")

        sb.Append("<p><b>Candidate Name / Username Filter:</b> " & HttpUtility.HtmlEncode(txtCandidateNameFilter.Text.Trim()) & "</p>")
        sb.Append("<p><b>Election Filter:</b> " & HttpUtility.HtmlEncode(ddlElectionFilter.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>Position Filter:</b> " & HttpUtility.HtmlEncode(ddlPositionFilter.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>Faculty Filter:</b> " & HttpUtility.HtmlEncode(ddlFacultyFilter.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>Status Filter:</b> " & HttpUtility.HtmlEncode(ddlStatusFilter.SelectedItem.Text) & "</p>")

        sb.Append("<table border='1'>")

        sb.Append("<tr>")
        sb.Append("<th>Candidate ID</th>")
        sb.Append("<th>Username / Student ID</th>")
        sb.Append("<th>Candidate Name</th>")
        sb.Append("<th>Election</th>")
        sb.Append("<th>Position</th>")
        sb.Append("<th>Faculty ID</th>")
        sb.Append("<th>Faculty Name</th>")
        sb.Append("<th>Major</th>")
        sb.Append("<th>Year Level</th>")
        sb.Append("<th>Description</th>")
        sb.Append("<th>Status</th>")
        sb.Append("</tr>")

        For Each row As DataRow In dt.Rows

            sb.Append("<tr>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("CandidateID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("CandidateADUsername").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FullName").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ElectionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("PositionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FacultyID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FacultyName").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("Major").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("YearLevel").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("Description").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("StatusText").ToString()) & "</td>")
            sb.Append("</tr>")

        Next

        sb.Append("</table>")
        sb.Append("</body>")
        sb.Append("</html>")

        Response.Clear()
        Response.Buffer = True
        Response.ContentType = "application/vnd.ms-excel"
        Response.AddHeader("Content-Disposition", "attachment;filename=" & fileName)
        Response.Charset = "utf-8"
        Response.ContentEncoding = Encoding.UTF8
        Response.Write(sb.ToString())
        Response.Flush()
        Response.End()

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