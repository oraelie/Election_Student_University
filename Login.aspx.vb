Imports System.Data.SqlClient
Imports System.Configuration
Imports System.DirectoryServices.AccountManagement

Public Class Login
    Inherits System.Web.UI.Page

    Private ReadOnly domainName As String = "uls.edu.lb"

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click

        Dim adUsername As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()

        If adUsername = "" Then
            lblMessage.Text = "Please enter your AD username."
            Return
        End If

        If password = "" Then
            lblMessage.Text = "Please enter your password."
            Return
        End If

        Try
            ' 1. Validate username and password against Active Directory
            Using context As New PrincipalContext(ContextType.Domain, domainName)

                Dim isValid As Boolean = context.ValidateCredentials(adUsername, password)

                If Not isValid Then
                    lblMessage.Text = "Invalid username or password."
                    Return
                End If

                Dim user As UserPrincipal = UserPrincipal.FindByIdentity(context, adUsername)

                If user Is Nothing Then
                    lblMessage.Text = "User not found in Active Directory."
                    Return
                End If

                ' Optional: save full name from AD
                Session("FullName") = user.DisplayName

            End Using

            ' 2. After AD login succeeds, check role in SQL Server
            Dim connectionString As String = ConfigurationManager.ConnectionStrings("ElectionConnection").ConnectionString

            Using con As New SqlConnection(connectionString)
                con.Open()

                ' Check admin table
                Dim adminQuery As String = "
                    SELECT COUNT(*)
                    FROM Admins
                    WHERE ADUsername = @ADUsername
                    AND IsActive = 1
                "

                Using cmd As New SqlCommand(adminQuery, con)
                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)

                    Dim adminCount As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                    If adminCount > 0 Then
                        Session("ADUsername") = adUsername
                        Session("UserRole") = "Admin"

                        Response.Redirect("AdminDashboard.aspx")
                        Return
                    End If
                End Using

                ' Check eligible voters table
                Dim voterQuery As String = "
                    SELECT COUNT(*)
                    FROM EligibleVoters
                    WHERE ADUsername = @ADUsername
                    AND IsActive = 1
                "

                Using cmd As New SqlCommand(voterQuery, con)
                    cmd.Parameters.AddWithValue("@ADUsername", adUsername)

                    Dim voterCount As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                    If voterCount > 0 Then
                        Session("ADUsername") = adUsername
                        Session("UserRole") = "Student"

                        Response.Redirect("StudentDashboard.aspx")
                        Return
                    End If
                End Using

                lblMessage.Text = "Login successful, but you are not registered as an admin or eligible voter."

            End Using

        Catch ex As Exception
            lblMessage.Text = "Login error: " & ex.Message
        End Try

    End Sub

End Class