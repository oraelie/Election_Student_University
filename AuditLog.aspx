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

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="filter-box">

                <h3>Filter Audit Log</h3>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Username</label>
                        <asp:TextBox 
                            ID="txtUsernameFilter" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="Example: 202400699 or admin username">
                        </asp:TextBox>
                    </div>

                    <div class="filter-item">
                        <label>Action Type</label>
                        <asp:DropDownList 
                            ID="ddlActionType" 
                            runat="server" 
                            CssClass="input-box">
                        </asp:DropDownList>
                    </div>

                </div>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>From Date</label>
                        <asp:TextBox 
                            ID="txtFromDate" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="07-07-2026">
                        </asp:TextBox>
                    </div>

                    <div class="filter-item">
                        <label>To Date</label>
                        <asp:TextBox 
                            ID="txtToDate" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="10-07-2026">
                        </asp:TextBox>
                    </div>

                </div>

                <div class="filter-buttons">
                    <asp:Button 
                        ID="btnApplyFilter" 
                        runat="server" 
                        Text="Apply Filter" 
                        CssClass="main-button" />

                    <asp:Button 
                        ID="btnClearFilter" 
                        runat="server" 
                        Text="Clear Filter" 
                        CssClass="clear-button" />
                </div>

            </div>

            <asp:GridView 
                ID="gvAuditLog" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                EmptyDataText="No audit log records found.">

                <Columns>
                    <asp:BoundField DataField="LogID" HeaderText="ID" />
                    <asp:BoundField DataField="ADUsername" HeaderText="Username" />
                    <asp:BoundField DataField="ActionType" HeaderText="Action Type" />
                    <asp:BoundField DataField="ActionDetails" HeaderText="Details" />
                    <asp:BoundField DataField="ActionDateTimeFormatted" HeaderText="Date / Time" />
                </Columns>

            </asp:GridView>

            <div class="button-area">

                <asp:Button 
                    ID="btnRefresh" 
                    runat="server" 
                    Text="Refresh" 
                    CssClass="main-button" />

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