<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AuditLog.aspx.vb" Inherits="UniversityElectionSystem.AuditLog" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Audit Log</title>
    <link href="Css/AuditLog.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Audit Log</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="button-area top-buttons">
                <asp:Button 
                    ID="btnRefresh" 
                    runat="server" 
                    Text="Refresh Log" 
                    CssClass="main-button" />
            </div>

            <asp:GridView 
                ID="gvAuditLog" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                EmptyDataText="No audit log records found.">

                <Columns>
                    <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                    <asp:BoundField DataField="ADUsername" HeaderText="Admin Username" />
                    <asp:BoundField DataField="ActionType" HeaderText="Action Type" />
                    <asp:BoundField DataField="ActionDetails" HeaderText="Action Details" />
                    <asp:BoundField DataField="ActionDateTime" HeaderText="Date / Time" />
                </Columns>

            </asp:GridView>

            <div class="button-area">
                <asp:Button 
                    ID="btnBack" 
                    runat="server" 
                    Text="Back to Admin Dashboard" 
                    CssClass="back-button" />

                <asp:Button 
                    ID="btnLogout" 
                    runat="server" 
                    Text="Logout" 
                    CssClass="logout-button" />
            </div>

        </div>

    </form>
</body>
</html>