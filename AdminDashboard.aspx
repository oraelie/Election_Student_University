<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AdminDashboard.aspx.vb" Inherits="UniversityElectionSystem.AdminDashboard" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Admin Dashboard</title>
    <link href="Css/AdminDashboard.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="dashboard-box">

            <h2>Admin Dashboard</h2>

            <asp:Label ID="lblFullName" runat="server" CssClass="welcome-label"></asp:Label>
            <br />
            <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>

            <br /><br />

            <asp:Button 
                ID="btnResults" 
                runat="server" 
                Text="View Results" 
                CssClass="main-button" />

            <asp:Button 
                ID="btnLogout" 
                runat="server" 
                Text="Logout" 
                CssClass="logout-button" />

        </div>

    </form>
</body>
</html>