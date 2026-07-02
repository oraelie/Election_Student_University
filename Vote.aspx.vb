Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class Vote
    Inherits System.Web.UI.Page

    Private ReadOnly connectionString As String =
        ConfigurationManager.ConnectionStrings("ElectionConnection").ConnectionString

    Private Const ElectionID As Integer = 1

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If Session("ADUsername") Is Nothing OrElse Session("UserRole") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Session("UserRole").ToString() <> "Student" Then
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

        lblFullName.Text = "Name: " & fullName
        lblUsername.Text = "Username: " & adUsername

        If Not IsPostBack Then
            LoadAllowedCandidates()
            BindPositions()
            BindCandidates()
        End If

    End Sub

    Private Sub LoadAllowedCandidates()

        Dim adUsername As String = Session("ADUsername").ToString()
        Dim dt As New DataTable()

        Try
            Using con As New SqlConnection(connectionString)
                Using cmd As New SqlCommand("sp_GetCandidatesForVoter", con)

                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)
                    cmd.Parameters.AddWithValue("@ElectionID", ElectionID)

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                End Using
            End Using

            Session("AllowedCandidates") = dt

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No available candidates. The election may not be open now, or you may have already voted for all positions."
                btnSubmitVote.Enabled = False
            Else
                btnSubmitVote.Enabled = True
            End If

        Catch ex As Exception
            lblMessage.Text = "Error loading candidates: " & ex.Message
            btnSubmitVote.Enabled = False
        End Try

    End Sub

    Private Sub BindPositions()

        ddlPositions.Items.Clear()

        Dim dt As DataTable = TryCast(Session("AllowedCandidates"), DataTable)

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            ddlPositions.Items.Add(New ListItem("No positions available", ""))
            rblCandidates.Items.Clear()
            Return
        End If

        Dim positionView As DataView = dt.DefaultView
        Dim positionTable As DataTable = positionView.ToTable(True, "PositionID", "PositionTitle")

        ddlPositions.DataSource = positionTable
        ddlPositions.DataTextField = "PositionTitle"
        ddlPositions.DataValueField = "PositionID"
        ddlPositions.DataBind()

    End Sub

    Private Sub BindCandidates()

        rblCandidates.Items.Clear()

        If ddlPositions.SelectedValue = "" Then
            Return
        End If

        Dim dt As DataTable = TryCast(Session("AllowedCandidates"), DataTable)

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If

        Dim selectedPositionID As Integer = Convert.ToInt32(ddlPositions.SelectedValue)

        Dim rows() As DataRow = dt.Select("PositionID = " & selectedPositionID)

        For Each row As DataRow In rows

            Dim candidateText As String =
                row("FullName").ToString() &
                " - " &
                row("Major").ToString() &
                " - Year " &
                row("YearLevel").ToString()

            Dim candidateID As String = row("CandidateID").ToString()

            rblCandidates.Items.Add(New ListItem(candidateText, candidateID))

        Next

    End Sub

    Protected Sub ddlPositions_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPositions.SelectedIndexChanged
        BindCandidates()
    End Sub

    Protected Sub btnSubmitVote_Click(sender As Object, e As EventArgs) Handles btnSubmitVote.Click

        If ddlPositions.SelectedValue = "" Then
            lblMessage.Text = "Please select a position."
            Return
        End If

        If rblCandidates.SelectedValue = "" Then
            lblMessage.Text = "Please select a candidate."
            Return
        End If

        Dim adUsername As String = Session("ADUsername").ToString()
        Dim positionID As Integer = Convert.ToInt32(ddlPositions.SelectedValue)
        Dim candidateID As Integer = Convert.ToInt32(rblCandidates.SelectedValue)

        Try
            Dim message As String = ""

            Using con As New SqlConnection(connectionString)
                Using cmd As New SqlCommand("sp_SubmitVote", con)

                    cmd.CommandType = CommandType.StoredProcedure

                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)
                    cmd.Parameters.AddWithValue("@ElectionID", ElectionID)
                    cmd.Parameters.AddWithValue("@PositionID", positionID)
                    cmd.Parameters.AddWithValue("@CandidateID", candidateID)

                    con.Open()

                    Dim result As Object = cmd.ExecuteScalar()

                    If result IsNot Nothing Then
                        message = result.ToString()
                    Else
                        message = "Vote submitted."
                    End If

                End Using
            End Using

            If message = "Your vote has been submitted successfully." Then

                AddAuditLog(
                    "Submit Vote",
                    "Student submitted vote for ElectionID: " & ElectionID.ToString() &
                    ", PositionID: " & positionID.ToString()
                )

                LoadAllowedCandidates()
                BindPositions()
                BindCandidates()

            End If

            lblMessage.Text = message

        Catch ex As Exception
            lblMessage.Text = "Error submitting vote: " & ex.Message
        End Try

    End Sub

    Private Sub AddAuditLog(actionType As String, actionDetails As String)

        Try
            If Session("ADUsername") Is Nothing Then
                Return
            End If

            Dim adUsername As String = Session("ADUsername").ToString()

            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    INSERT INTO AuditLog (ADUsername, ActionType, ActionDetails)
                    VALUES (@ADUsername, @ActionType, @ActionDetails)
                "

                Using cmd As New SqlCommand(query, con)

                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)
                    cmd.Parameters.AddWithValue("@ActionType", actionType)
                    cmd.Parameters.AddWithValue("@ActionDetails", actionDetails)

                    con.Open()
                    cmd.ExecuteNonQuery()

                End Using

            End Using

        Catch
            ' If audit log fails, do not stop the voting process.
        End Try

    End Sub

    Protected Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Response.Redirect("StudentDashboard.aspx")
    End Sub

    Protected Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Session.Clear()
        Session.Abandon()
        Response.Redirect("Login.aspx")
    End Sub

End Class