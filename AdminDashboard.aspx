<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AdminDashboard.aspx.vb" Inherits="UniversityElectionSystem.AdminDashboard" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Admin Dashboard</title>
    <link href="Css/AdminDashboard.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="dashboard-container">

            <div class="header-box">
                <h2>Admin Dashboard</h2>

                <asp:Label ID="lblFullName" runat="server" CssClass="welcome-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <div class="cards-grid">

                <div class="dashboard-card">
                    <h3>Manage Elections</h3>
                    <p>Create elections, set dates, and change election status.</p>
                    <asp:Button 
                        ID="btnManageElections" 
                        runat="server" 
                        Text="Open" 
                        CssClass="card-button" />
                </div>

                <div class="dashboard-card">
                    <h3>Manage Positions</h3>
                    <p>Add positions such as President, Vice President, or Faculty Representative.</p>
                    <asp:Button 
                        ID="btnManagePositions" 
                        runat="server" 
                        Text="Open" 
                        CssClass="card-button" />
                </div>

                <div class="dashboard-card">
                    <h3>Manage Candidates</h3>
                    <p>Add candidates and activate or deactivate them.</p>
                    <asp:Button 
                        ID="btnManageCandidates" 
                        runat="server" 
                        Text="Open" 
                        CssClass="card-button" />
                </div>

                <div class="dashboard-card">
                    <h3>Manage Voters</h3>
                    <p>Add eligible AD users and assign them to faculties.</p>
                    <asp:Button 
                        ID="btnManageVoters" 
                        runat="server" 
                        Text="Open" 
                        CssClass="card-button" />
                </div>

                <div class="dashboard-card">
                    <h3>View Results</h3>
                    <p>View election results and total votes per candidate.</p>
                    <asp:Button 
                        ID="btnResults" 
                        runat="server" 
                        Text="Open" 
                        CssClass="card-button" />
                </div>

                <div class="dashboard-card">
                <h3>Audit Log</h3>
                <p>View admin actions such as adding voters, candidates, positions, and election updates.</p>
                <asp:Button 
                    ID="btnAuditLog" 
                    runat="server" 
                    Text="Open" 
                    CssClass="card-button" />
                 </div>

                <div class="dashboard-card logout-card">
                    <h3>Logout</h3>
                    <p>End the current admin session safely.</p>
                    <asp:Button 
                        ID="btnLogout" 
                        runat="server" 
                        Text="Logout" 
                        CssClass="logout-button" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>