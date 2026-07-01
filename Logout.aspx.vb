Public Class Logout
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        ' Clear all session values
        Session.Clear()

        ' End the current session
        Session.Abandon()

        ' Redirect user back to login page
        Response.Redirect("Login.aspx")

    End Sub

End Class