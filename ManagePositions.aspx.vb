Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web

Public Class ManagePositions
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
            LoadElections()
            LoadFaculties()
            LoadPositions()
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

                    ddlElection.Items.Clear()
                    ddlElectionFilter.Items.Clear()

                    ddlElection.Items.Add(New ListItem("Select Election", ""))
                    ddlElectionFilter.Items.Add(New ListItem("All Elections", ""))

                    For Each row As DataRow In dt.Rows

                        Dim electionID As String = row("ElectionID").ToString()
                        Dim electionTitle As String = row("ElectionTitle").ToString()

                        ddlElection.Items.Add(New ListItem(electionTitle, electionID))
                        ddlElectionFilter.Items.Add(New ListItem(electionTitle, electionID))

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading elections: " & ex.Message, "error")
        End Try

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

                    ddlFaculty.Items.Add(New ListItem("General Position - All Faculties", ""))
                    ddlFacultyFilter.Items.Add(New ListItem("All Faculties / General", ""))

                    ddlFacultyFilter.Items.Add(New ListItem("General Positions Only", "GENERAL"))

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

    Private Function GetPositionsData() As DataTable

        Dim electionFilter As String = ddlElectionFilter.SelectedValue
        Dim positionTitleFilter As String = txtPositionTitleFilter.Text.Trim()
        Dim facultyFilter As String = ddlFacultyFilter.SelectedValue
        Dim statusFilter As String = ddlStatusFilter.SelectedValue

        Dim dt As New DataTable()

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT 
                    P.PositionID,
                    P.PositionTitle,
                    P.ElectionID,
                    E.ElectionTitle,
                    P.FacultyID,
                    ISNULL(F.FacultyName, 'General Position') AS FacultyName,
                    P.IsActive,
                    CASE 
                        WHEN P.IsActive = 1 THEN 'Active'
                        ELSE 'Inactive'
                    END AS StatusText
                FROM Positions P
                INNER JOIN Elections E
                    ON P.ElectionID = E.ElectionID
                LEFT JOIN Faculties F
                    ON P.FacultyID = F.FacultyID
                WHERE 1 = 1
            "

            If electionFilter <> "" Then
                query &= "
                    AND P.ElectionID = @ElectionID
                "
            End If

            If positionTitleFilter <> "" Then
                query &= "
                    AND P.PositionTitle LIKE @PositionTitle
                "
            End If

            If facultyFilter <> "" Then

                If facultyFilter = "GENERAL" Then
                    query &= "
                        AND P.FacultyID IS NULL
                    "
                Else
                    query &= "
                        AND P.FacultyID = @FacultyID
                    "
                End If

            End If

            If statusFilter <> "" Then
                query &= "
                    AND P.IsActive = @IsActive
                "
            End If

            query &= "
                ORDER BY E.ElectionID DESC, P.PositionTitle
            "

            Using cmd As New SqlCommand(query, con)

                If electionFilter <> "" Then
                    cmd.Parameters.AddWithValue("@ElectionID", Convert.ToInt32(electionFilter))
                End If

                If positionTitleFilter <> "" Then
                    cmd.Parameters.AddWithValue("@PositionTitle", "%" & positionTitleFilter & "%")
                End If

                If facultyFilter <> "" AndAlso facultyFilter <> "GENERAL" Then
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

    Private Sub LoadPositions()

        Try
            Dim dt As DataTable = GetPositionsData()

            gvPositions.DataSource = dt
            gvPositions.DataBind()

            BindFacultyDropDownsInGrid(dt)

            If dt.Rows.Count = 0 Then
                ShowMessage("No positions found for the selected filter.", "warning")
            Else
                pnlMessage.Visible = False
                lblMessage.Text = ""
            End If

        Catch ex As Exception
            ShowMessage("Error loading positions: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropDownsInGrid(positionsTable As DataTable)

        If FacultiesTable Is Nothing Then
            Return
        End If

        For Each row As GridViewRow In gvPositions.Rows

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            If ddl IsNot Nothing Then

                ddl.Items.Clear()
                ddl.Items.Add(New ListItem("General Position - All Faculties", ""))

                For Each facultyRow As DataRow In FacultiesTable.Rows

                    ddl.Items.Add(
                        New ListItem(
                            facultyRow("FacultyName").ToString(),
                            facultyRow("FacultyID").ToString()
                        )
                    )

                Next

                Dim selectedFacultyID As String = ""

                If Not IsDBNull(positionsTable.Rows(row.RowIndex)("FacultyID")) Then
                    selectedFacultyID = positionsTable.Rows(row.RowIndex)("FacultyID").ToString()
                End If

                If ddl.Items.FindByValue(selectedFacultyID) IsNot Nothing Then
                    ddl.SelectedValue = selectedFacultyID
                End If

            End If

        Next

    End Sub

    Private Function PositionAlreadyExists(
        electionID As Integer,
        positionTitle As String,
        facultyIDValue As Object,
        excludePositionID As Integer
    ) As Boolean

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT COUNT(*)
                FROM Positions
                WHERE ElectionID = @ElectionID
                AND LOWER(LTRIM(RTRIM(PositionTitle))) = LOWER(LTRIM(RTRIM(@PositionTitle)))
            "

            If facultyIDValue Is DBNull.Value Then
                query &= "
                    AND FacultyID IS NULL
                "
            Else
                query &= "
                    AND FacultyID = @FacultyID
                "
            End If

            If excludePositionID > 0 Then
                query &= "
                    AND PositionID <> @ExcludePositionID
                "
            End If

            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@ElectionID", electionID)
                cmd.Parameters.AddWithValue("@PositionTitle", positionTitle)

                If Not facultyIDValue Is DBNull.Value Then
                    cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(facultyIDValue))
                End If

                If excludePositionID > 0 Then
                    cmd.Parameters.AddWithValue("@ExcludePositionID", excludePositionID)
                End If

                con.Open()

                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                Return count > 0

            End Using

        End Using

    End Function

    Private Function GetElectionIDForPosition(positionID As Integer) As Integer

        Using con As New SqlConnection(connectionString)

            Dim query As String = "
                SELECT ElectionID
                FROM Positions
                WHERE PositionID = @PositionID
            "

            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@PositionID", positionID)

                con.Open()

                Dim result As Object = cmd.ExecuteScalar()

                If result Is Nothing Then
                    Return 0
                End If

                Return Convert.ToInt32(result)

            End Using

        End Using

    End Function

    Protected Sub btnAddPosition_Click(sender As Object, e As EventArgs) Handles btnAddPosition.Click

        Dim positionTitle As String = txtPositionTitle.Text.Trim()

        If ddlElection.SelectedValue = "" Then
            ShowMessage("Please select an election.", "warning")
            Return
        End If

        If positionTitle = "" Then
            ShowMessage("Please enter the position title.", "warning")
            Return
        End If

        Dim electionID As Integer = Convert.ToInt32(ddlElection.SelectedValue)

        Dim facultyIDValue As Object

        If ddlFaculty.SelectedValue = "" Then
            facultyIDValue = DBNull.Value
        Else
            facultyIDValue = Convert.ToInt32(ddlFaculty.SelectedValue)
        End If

        Try
            If PositionAlreadyExists(electionID, positionTitle, facultyIDValue, 0) Then
                ShowMessage("This position already exists for the selected election and faculty.", "warning")
                Return
            End If

            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO Positions (ElectionID, PositionTitle, FacultyID, IsActive)
                    VALUES (@ElectionID, @PositionTitle, @FacultyID, 1)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ElectionID", electionID)
                    cmd.Parameters.AddWithValue("@PositionTitle", positionTitle)
                    cmd.Parameters.AddWithValue("@FacultyID", facultyIDValue)

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Position",
                "Added position: " & positionTitle &
                ", ElectionID: " & electionID.ToString() &
                ", FacultyID: " & If(facultyIDValue Is DBNull.Value, "General", facultyIDValue.ToString())
            )

            ddlElection.SelectedIndex = 0
            txtPositionTitle.Text = ""
            ddlFaculty.SelectedIndex = 0

            LoadPositions()

            ShowMessage("Position added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding position: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvPositions_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvPositions.RowCommand

        If e.CommandName = "UpdatePosition" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim positionID As Integer =
                Convert.ToInt32(gvPositions.DataKeys(rowIndex).Values("PositionID"))

            Dim oldFacultyID As String = ""

            If gvPositions.DataKeys(rowIndex).Values("FacultyID") IsNot Nothing AndAlso
               Not IsDBNull(gvPositions.DataKeys(rowIndex).Values("FacultyID")) Then

                oldFacultyID = gvPositions.DataKeys(rowIndex).Values("FacultyID").ToString()

            End If

            Dim row As GridViewRow = gvPositions.Rows(rowIndex)

            Dim txtGridPositionTitle As TextBox =
                TryCast(row.FindControl("txtGridPositionTitle"), TextBox)

            Dim ddlGridFaculty As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim chkIsActive As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If txtGridPositionTitle Is Nothing OrElse ddlGridFaculty Is Nothing OrElse chkIsActive Is Nothing Then
                ShowMessage("One or more position controls were not found.", "error")
                Return
            End If

            Dim positionTitle As String = txtGridPositionTitle.Text.Trim()
            Dim isActive As Boolean = chkIsActive.Checked

            If positionTitle = "" Then
                ShowMessage("Position title cannot be empty.", "warning")
                Return
            End If

            Dim electionID As Integer = GetElectionIDForPosition(positionID)

            If electionID = 0 Then
                ShowMessage("Could not find the election linked to this position.", "error")
                Return
            End If

            Dim newFacultyIDValue As Object
            Dim newFacultyAuditText As String

            If ddlGridFaculty.SelectedValue = "" Then
                newFacultyIDValue = DBNull.Value
                newFacultyAuditText = "General"
            Else
                newFacultyIDValue = Convert.ToInt32(ddlGridFaculty.SelectedValue)
                newFacultyAuditText = ddlGridFaculty.SelectedValue
            End If

            Try
                If PositionAlreadyExists(electionID, positionTitle, newFacultyIDValue, positionID) Then
                    ShowMessage("Another position with the same title already exists for this election and faculty.", "warning")
                    Return
                End If

                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Positions
                        SET 
                            PositionTitle = @PositionTitle,
                            FacultyID = @FacultyID,
                            IsActive = @IsActive
                        WHERE PositionID = @PositionID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@PositionTitle", positionTitle)
                        cmd.Parameters.AddWithValue("@FacultyID", newFacultyIDValue)
                        cmd.Parameters.AddWithValue("@IsActive", isActive)
                        cmd.Parameters.AddWithValue("@PositionID", positionID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Position",
                    "Updated PositionID: " & positionID.ToString() &
                    ", Title: " & positionTitle &
                    ", Old FacultyID: " & If(oldFacultyID = "", "General", oldFacultyID) &
                    ", New FacultyID: " & newFacultyAuditText &
                    ", Active: " & isActive.ToString()
                )

                LoadPositions()

                ShowMessage("Position updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating position: " & ex.Message, "error")
            End Try

        End If

    End Sub

    Protected Sub btnApplyFilter_Click(sender As Object, e As EventArgs) Handles btnApplyFilter.Click

        LoadPositions()

    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click

        If ddlElectionFilter.Items.Count > 0 Then
            ddlElectionFilter.SelectedIndex = 0
        End If

        txtPositionTitleFilter.Text = ""

        If ddlFacultyFilter.Items.Count > 0 Then
            ddlFacultyFilter.SelectedIndex = 0
        End If

        ddlStatusFilter.SelectedValue = ""

        LoadPositions()

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        LoadElections()
        LoadFaculties()
        LoadPositions()

    End Sub

    Protected Sub btnExportExcel_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click

        Try
            Dim dt As DataTable = GetPositionsData()

            If dt.Rows.Count = 0 Then
                ShowMessage("No positions available to export for the selected filter.", "warning")
                Return
            End If

            ExportPositionsToExcel(dt)

        Catch ex As Exception
            ShowMessage("Error exporting positions: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub ExportPositionsToExcel(dt As DataTable)

        Dim fileName As String =
            "Positions_" & DateTime.Now.ToString("dd-MM-yyyy_HHmm") & ".xls"

        Dim sb As New StringBuilder()

        sb.Append("<html>")
        sb.Append("<head>")
        sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />")
        sb.Append("</head>")
        sb.Append("<body>")

        sb.Append("<h2>Positions List</h2>")
        sb.Append("<p><b>Export Date:</b> " & DateTime.Now.ToString("dd-MM-yyyy HH:mm") & "</p>")

        sb.Append("<p><b>Election Filter:</b> " & HttpUtility.HtmlEncode(ddlElectionFilter.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>Position Title Filter:</b> " & HttpUtility.HtmlEncode(txtPositionTitleFilter.Text.Trim()) & "</p>")
        sb.Append("<p><b>Faculty Filter:</b> " & HttpUtility.HtmlEncode(ddlFacultyFilter.SelectedItem.Text) & "</p>")
        sb.Append("<p><b>Status Filter:</b> " & HttpUtility.HtmlEncode(ddlStatusFilter.SelectedItem.Text) & "</p>")

        sb.Append("<table border='1'>")

        sb.Append("<tr>")
        sb.Append("<th>Position ID</th>")
        sb.Append("<th>Election ID</th>")
        sb.Append("<th>Election</th>")
        sb.Append("<th>Position Title</th>")
        sb.Append("<th>Faculty ID</th>")
        sb.Append("<th>Faculty Name</th>")
        sb.Append("<th>Status</th>")
        sb.Append("</tr>")

        For Each row As DataRow In dt.Rows

            Dim facultyIDText As String = ""

            If Not IsDBNull(row("FacultyID")) Then
                facultyIDText = row("FacultyID").ToString()
            Else
                facultyIDText = "General"
            End If

            sb.Append("<tr>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("PositionID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ElectionID").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("ElectionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("PositionTitle").ToString()) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(facultyIDText) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(row("FacultyName").ToString()) & "</td>")
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