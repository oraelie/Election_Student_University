Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

Public Class AuditLog
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
            LoadAuditLog()
        End If

    End Sub

    Private Sub LoadAuditLog()

        Try
            Using con As New SqlConnection(connectionString)

                Dim query As String = "
                    SELECT 
                        LogID,
                        ADUsername,
                        ActionType,
                        ActionDetails,
                        CONVERT(VARCHAR(19), ActionDateTime, 120) AS ActionDateTime
                    FROM AuditLog
                    ORDER BY LogID DESC
                "

                Using cmd As New SqlCommand(query, con)

                    Dim dt As New DataTable()

                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using

                    gvAuditLog.DataSource = dt
                    gvAuditLog.DataBind()

                    If dt.Rows.Count = 0 Then
                        lblMessage.Text = "No audit log records found."
                    Else
                        lblMessage.Text = ""
                    End If

                End Using

            End Using

        Catch ex As Exception
            lblMessage.Text = "Error loading audit log: " & ex.Message
        End Try

    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadAuditLog()
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