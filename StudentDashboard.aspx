<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StudentDashboard.aspx.vb" Inherits="UniversityElectionSystem.StudentDashboard" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Student Dashboard</title>
    <link href="Css/StudentDashboard.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="dashboard-box">

            <h2>Student Dashboard</h2>

            <asp:Label ID="lblFullName" runat="server" CssClass="welcome-label"></asp:Label>
            <br />
            <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>

            <br /><br />

            <asp:Button ID="btnVote" runat="server" Text="Go to Voting Page" CssClass="main-button" />

            <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="logout-button" />

        </div>

    </form>
</body>
</html>