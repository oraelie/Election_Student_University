Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Globalization

Public Class ManageElections
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
            LoadElections()
        End If

    End Sub

    Private Sub LoadElections()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        ElectionID,
                        ElectionTitle,
                        FORMAT(StartDateTime, 'dd-MM-yyyy HH:mm') AS StartDateTime,
                        FORMAT(EndDateTime, 'dd-MM-yyyy HH:mm') AS EndDateTime,
                        Status
                    FROM Elections
                    ORDER BY ElectionID DESC
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvElections.DataSource = dt
                    gvElections.DataBind()

                    For Each row As GridViewRow In gvElections.Rows

                        Dim ddl As DropDownList =
                            TryCast(row.FindControl("ddlGridStatus"), DropDownList)

                        If ddl IsNot Nothing Then
                            Dim status As String = dt.Rows(row.RowIndex)("Status").ToString()

                            If ddl.Items.FindByValue(status) IsNot Nothing Then
                                ddl.SelectedValue = status
                            End If
                        End If

                    Next

                End Using

            End Using

        Catch ex As Exception
            ShowMessage("Error loading elections: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub btnAddElection_Click(sender As Object, e As EventArgs) Handles btnAddElection.Click

        Dim electionTitle As String = txtElectionTitle.Text.Trim()

        If electionTitle = "" Then
            ShowMessage("Please enter the election title.", "warning")
            Return
        End If

        Dim startDateTime As DateTime
        Dim endDateTime As DateTime

        If Not DateTime.TryParseExact(
            txtStartDateTime.Text.Trim(),
            "dd-MM-yyyy HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            startDateTime
        ) Then
            ShowMessage("Invalid start date/time. Use format: 07-07-2026 11:21", "warning")
            Return
        End If

        If Not DateTime.TryParseExact(
            txtEndDateTime.Text.Trim(),
            "dd-MM-yyyy HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            endDateTime
        ) Then
            ShowMessage("Invalid end date/time. Use format: 07-07-2026 12:21", "warning")
            Return
        End If

        If endDateTime <= startDateTime Then
            ShowMessage("End date/time must be after start date/time.", "warning")
            Return
        End If

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO Elections (ElectionTitle, StartDateTime, EndDateTime, Status)
                    VALUES (@ElectionTitle, @StartDateTime, @EndDateTime, @Status)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ElectionTitle", electionTitle)
                    cmd.Parameters.AddWithValue("@StartDateTime", startDateTime)
                    cmd.Parameters.AddWithValue("@EndDateTime", endDateTime)
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue)

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

            AddAuditLog(
                "Add Election",
                "Added election: " & electionTitle &
                ", Start: " & startDateTime.ToString("dd-MM-yyyy HH:mm") &
                ", End: " & endDateTime.ToString("dd-MM-yyyy HH:mm") &
                ", Status: " & ddlStatus.SelectedValue
            )

            txtElectionTitle.Text = ""
            txtStartDateTime.Text = ""
            txtEndDateTime.Text = ""
            ddlStatus.SelectedValue = "Draft"

            LoadElections()

            ShowMessage("Election added successfully.", "success")

        Catch ex As Exception
            ShowMessage("Error adding election: " & ex.Message, "error")
        End Try

    End Sub

    Protected Sub gvElections_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvElections.RowCommand

        If e.CommandName = "UpdateElection" Then

            Dim rowIndex As Integer =
                Convert.ToInt32(e.CommandArgument)

            Dim electionID As Integer =
                Convert.ToInt32(gvElections.DataKeys(rowIndex).Value)

            Dim row As GridViewRow = gvElections.Rows(rowIndex)

            Dim txtTitle As TextBox =
                TryCast(row.FindControl("txtGridElectionTitle"), TextBox)

            Dim txtStart As TextBox =
                TryCast(row.FindControl("txtGridStartDateTime"), TextBox)

            Dim txtEnd As TextBox =
                TryCast(row.FindControl("txtGridEndDateTime"), TextBox)

            Dim ddlStatusGrid As DropDownList =
                TryCast(row.FindControl("ddlGridStatus"), DropDownList)

            If txtTitle Is Nothing OrElse txtStart Is Nothing OrElse txtEnd Is Nothing OrElse ddlStatusGrid Is Nothing Then
                ShowMessage("One or more election controls were not found.", "error")
                Return
            End If

            Dim electionTitle As String = txtTitle.Text.Trim()

            If electionTitle = "" Then
                ShowMessage("Election title cannot be empty.", "warning")
                Return
            End If

            Dim startDateTime As DateTime
            Dim endDateTime As DateTime

            If Not DateTime.TryParseExact(
                txtStart.Text.Trim(),
                "dd-MM-yyyy HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                startDateTime
            ) Then
                ShowMessage("Invalid start date/time. Use format: 07-07-2026 11:21", "warning")
                Return
            End If

            If Not DateTime.TryParseExact(
                txtEnd.Text.Trim(),
                "dd-MM-yyyy HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                endDateTime
            ) Then
                ShowMessage("Invalid end date/time. Use format: 07-07-2026 12:21", "warning")
                Return
            End If

            If endDateTime <= startDateTime Then
                ShowMessage("End date/time must be after start date/time.", "warning")
                Return
            End If

            Dim newStatus As String = ddlStatusGrid.SelectedValue

            Try
                Using con As New SqlConnection(connectionString)

                    Dim query As String = "
                        UPDATE Elections
                        SET 
                            ElectionTitle = @ElectionTitle,
                            StartDateTime = @StartDateTime,
                            EndDateTime = @EndDateTime,
                            Status = @Status
                        WHERE ElectionID = @ElectionID
                    "

                    Using cmd As New SqlCommand(query, con)

                        cmd.Parameters.AddWithValue("@ElectionTitle", electionTitle)
                        cmd.Parameters.AddWithValue("@StartDateTime", startDateTime)
                        cmd.Parameters.AddWithValue("@EndDateTime", endDateTime)
                        cmd.Parameters.AddWithValue("@Status", newStatus)
                        cmd.Parameters.AddWithValue("@ElectionID", electionID)

                        con.Open()
                        cmd.ExecuteNonQuery()

                    End Using

                End Using

                AddAuditLog(
                    "Update Election",
                    "Updated ElectionID: " & electionID.ToString() &
                    ", Title: " & electionTitle &
                    ", Start: " & startDateTime.ToString("dd-MM-yyyy HH:mm") &
                    ", End: " & endDateTime.ToString("dd-MM-yyyy HH:mm") &
                    ", Status: " & newStatus
                )

                LoadElections()

                ShowMessage("Election updated successfully.", "success")

            Catch ex As Exception
                ShowMessage("Error updating election: " & ex.Message, "error")
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