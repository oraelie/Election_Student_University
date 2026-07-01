Imports System.Data.SqlClient
Imports System.Configuration

Public Class TestConnection
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim connectionString As String = ConfigurationManager.ConnectionStrings("ElectionConnection").ConnectionString

        Try
            Using con As New SqlConnection(connectionString)
                con.Open()
                lblMessage.Text = "Connection successful to UniversityElectionDB."
            End Using

        Catch ex As Exception
            lblMessage.Text = "Connection failed: " & ex.Message
        End Try

    End Sub

End Class