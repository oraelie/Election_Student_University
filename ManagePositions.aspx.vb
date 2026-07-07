Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

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
                    SELECT ElectionID, ElectionTitle
                    FROM Elections
                    ORDER BY ElectionID DESC
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    ddlElection.DataSource = dt
                    ddlElection.DataTextField = "ElectionTitle"
                    ddlElection.DataValueField = "ElectionID"
                    ddlElection.DataBind()

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

                    ddlFaculty.Items.Clear()
                    ddlFaculty.Items.Add(New ListItem("General Position", ""))

                    For Each row As DataRow In dt.Rows
                        ddlFaculty.Items.Add(
                            New ListItem(
                                row("FacultyName").ToString(),
                                row("FacultyID").ToString()
                            )
                        )
                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading faculties: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub LoadPositions()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        P.PositionID,
                        E.ElectionTitle,
                        P.PositionTitle,
                        P.FacultyID,
                        ISNULL(F.FacultyName, 'General Position') AS FacultyName,
                        P.IsActive
                    FROM Positions P
                    INNER JOIN Elections E
                        ON P.ElectionID = E.ElectionID
                    LEFT JOIN Faculties F
                        ON P.FacultyID = F.FacultyID
                    ORDER BY E.ElectionID DESC, P.PositionID
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvPositions.DataSource = dt
                    gvPositions.DataBind()

                    BindFacultyDropdownsInGrid(dt)

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading positions: " & ex.Message, "error")
        End Try

    End Sub

    Private Sub BindFacultyDropdownsInGrid(positionsTable As DataTable)

        For Each row As GridViewRow In gvPositions.Rows

            Dim ddl As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            If ddl IsNot Nothing Then

                ddl.Items.Clear()
                ddl.Items.Add(New ListItem("General Position", ""))

                If FacultiesTable IsNot Nothing Then
                    For Each facultyRow As DataRow In FacultiesTable.Rows
                        ddl.Items.Add(
                            New ListItem(
                                facultyRow("FacultyName").ToString(),
                                facultyRow("FacultyID").ToString()
                            )
                        )
                    Next
                End If

                Dim currentFacultyValue As Object =
                    positionsTable.Rows(row.RowIndex)("FacultyID")

                If currentFacultyValue Is DBNull.Value Then
                    ddl.SelectedValue = ""
                Else
                    Dim currentFacultyID As String = currentFacultyValue.ToString()

                    If ddl.Items.FindByValue(currentFacultyID) IsNot Nothing Then
                        ddl.SelectedValue = currentFacultyID
                    End If
                End If

            End If

        Next

    End Sub

    Protected Sub btnAddPosition_Click(sender As Object, e As EventArgs) Handles btnAddPosition.Click

        If ddlElection.SelectedValue = "" Then
            ShowMessage("Please select an election.", "warning")
            Return
        End If

        Dim positionTitle As String = txtPositionTitle.Text.Trim()

        If positionTitle = "" Then
            ShowMessage("Please enter the position title.", "warning")
            Return
        End If

        Dim electionID As Integer = Convert.ToInt32(ddlElection.SelectedValue)
        Dim facultyText As String = "General Position"
        Dim facultyIDText As String = "NULL"

        If ddlFaculty.SelectedValue <> "" Then
            facultyText = ddlFaculty.SelectedItem.Text
            facultyIDText = ddlFaculty.SelectedValue
        End If

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO Positions (ElectionID, PositionTitle, FacultyID, IsActive)
                    VALUES (@ElectionID, @PositionTitle, @FacultyID, 1)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ElectionID", electionID)
                    cmd.Parameters.AddWithValue("@PositionTitle", positionTitle)

                    If ddlFaculty.SelectedValue = "" Then
                        cmd.Parameters.AddWithValue("@FacultyID", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFaculty.SelectedValue))
                    End If

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Position",
                "Added position: " & positionTitle &
                ", ElectionID: " & electionID.ToString() &
                ", Faculty: " & facultyText &
                ", FacultyID: " & facultyIDText
            )

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

            If gvPositions.DataKeys(rowIndex).Values("FacultyID") IsNot Nothing Then
                oldFacultyID = gvPositions.DataKeys(rowIndex).Values("FacultyID").ToString()
            End If

            If oldFacultyID = "" Then
                oldFacultyID = "NULL"
            End If

            Dim row As GridViewRow = gvPositions.Rows(rowIndex)

            Dim txtTitle As TextBox =
                TryCast(row.FindControl("txtGridPositionTitle"), TextBox)

            Dim ddlFaculty As DropDownList =
                TryCast(row.FindControl("ddlGridFaculty"), DropDownList)

            Dim chk As CheckBox =
                TryCast(row.FindControl("chkIsActive"), CheckBox)

            If txtTitle Is Nothing Then
                ShowMessage("Position title textbox not found.", "error")
                Return
            End If

            If ddlFaculty Is Nothing Then
                ShowMessage("Faculty dropdown not found.", "error")
                Return
            End If

            If chk Is Nothing Then
                ShowMessage("Active checkbox not found.", "error")
                Return
            End If

            Dim positionTitle As String = txtTitle.Text.Trim()

            If positionTitle = "" Then
                ShowMessage("Position title cannot be empty.", "warning")
                Return
            End If

            Dim newFacultyIDText As String = "NULL"
            Dim isActive As Boolean = chk.Checked

            If ddlFaculty.SelectedValue <> "" Then
                newFacultyIDText = ddlFaculty.SelectedValue
            End If

            Try
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

                        If ddlFaculty.SelectedValue = "" Then
                            cmd.Parameters.AddWithValue("@FacultyID", DBNull.Value)
                        Else
                            cmd.Parameters.AddWithValue("@FacultyID", Convert.ToInt32(ddlFaculty.SelectedValue))
                        End If

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
                    ", Old FacultyID: " & oldFacultyID &
                    ", New FacultyID: " & newFacultyIDText &
                    ", Active = " & isActive.ToString()
                )

                LoadPositions()

                ShowMessage("Position updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating position: " & ex.Message, "error")
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