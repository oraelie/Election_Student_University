Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class ManagePositions
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
            lblMessage.Text = "Error loading elections: " & ex.Message
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
            lblMessage.Text = "Error loading faculties: " & ex.Message
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

                End Using

            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading positions: " & ex.Message
        End Try

    End Sub

    Protected Sub btnAddPosition_Click(sender As Object, e As EventArgs) Handles btnAddPosition.Click

        If ddlElection.SelectedValue = "" Then
            lblMessage.Text = "Please select an election."
            Return
        End If

        Dim positionTitle As String = txtPositionTitle.Text.Trim()

        If positionTitle = "" Then
            lblMessage.Text = "Please enter the position title."
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

            lblMessage.Text = "Position added successfully."
            txtPositionTitle.Text = ""
            ddlFaculty.SelectedIndex = 0

            LoadPositions()

        Catch ex As Exception
            lblMessage.Text = "Error adding position: " & ex.Message
        End Try

    End Sub

    Protected Sub gvPositions_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvPositions.RowCommand

        If e.CommandName = "UpdateActive" Then

            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim positionID As Integer = Convert.ToInt32(gvPositions.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvPositions.Rows(rowIndex)
            Dim chk As CheckBox = TryCast(row.FindControl("chkIsActive"), CheckBox)

            If chk Is Nothing Then
                lblMessage.Text = "Active checkbox not found."
                Return
            End If

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Positions
                        SET IsActive = @IsActive
                        WHERE PositionID = @PositionID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@IsActive", chk.Checked)
                        cmd.Parameters.AddWithValue("@PositionID", positionID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Position",
                    "Updated PositionID: " & positionID.ToString() &
                    ", Active = " & chk.Checked.ToString()
                )

                lblMessage.Text = "Position updated successfully."
                LoadPositions()

            Catch ex As Exception
                lblMessage.Text = "Error updating position: " & ex.Message
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