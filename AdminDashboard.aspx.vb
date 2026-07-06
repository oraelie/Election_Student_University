Public Class AdminDashboard
    Inherits System.Web.UI.Page

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

        lblFullName.Text = "Welcome Admin, " & fullName
        lblUsername.Text = "Username: " & adUsername

    End Sub

    Protected Sub btnManageElections_Click(sender As Object, e As EventArgs) Handles btnManageElections.Click
        Response.Redirect("ManageElections.aspx")
    End Sub

    Protected Sub btnManagePositions_Click(sender As Object, e As EventArgs) Handles btnManagePositions.Click
        Response.Redirect("ManagePositions.aspx")
    End Sub

    Protected Sub btnManageCandidates_Click(sender As Object, e As EventArgs) Handles btnManageCandidates.Click
        Response.Redirect("ManageCandidates.aspx")
    End Sub

    Protected Sub btnManageVoters_Click(sender As Object, e As EventArgs) Handles btnManageVoters.Click
        Response.Redirect("ManageVoters.aspx")
    End Sub

    Protected Sub btnResults_Click(sender As Object, e As EventArgs) Handles btnResults.Click
        Response.Redirect("Results.aspx")
    End Sub

    Protected Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Session.Clear()
        Session.Abandon()
        Response.Redirect("Login.aspx")
    End Sub
    Protected Sub btnAuditLog_Click(sender As Object, e As EventArgs) Handles btnAuditLog.Click
        Response.Redirect("AuditLog.aspx")
    End Sub
    Protected Sub btnManageFaculties_Click(sender As Object, e As EventArgs) Handles btnManageFaculties.Click
        Response.Redirect("ManageFaculties.aspx")
    End Sub

End Class