<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageElections.aspx.vb" Inherits="UniversityElectionSystem.ManageElections" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Elections</title>
    <link href="Css/ManageElections.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Manage Elections</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="form-box">
                <h3>Add New Election</h3>

                <label>Election Title</label>
                <asp:TextBox ID="txtElectionTitle" runat="server" CssClass="input-box"></asp:TextBox>

                <label>Start Date and Time</label>
                <asp:TextBox ID="txtStartDateTime" runat="server" CssClass="input-box" placeholder="2026-07-02 08:00:00"></asp:TextBox>

                <label>End Date and Time</label>
                <asp:TextBox ID="txtEndDateTime" runat="server" CssClass="input-box" placeholder="2026-07-02 16:00:00"></asp:TextBox>

                <label>Status</label>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input-box">
                    <asp:ListItem Text="Draft" Value="Draft"></asp:ListItem>
                    <asp:ListItem Text="Open" Value="Open"></asp:ListItem>
                    <asp:ListItem Text="Closed" Value="Closed"></asp:ListItem>
                    <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                </asp:DropDownList>

                <asp:Button ID="btnAddElection" runat="server" Text="Add Election" CssClass="main-button" />
            </div>

            <h3>Existing Elections</h3>

            <asp:GridView 
                ID="gvElections" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="ElectionID">

                <Columns>
                    <asp:BoundField DataField="ElectionID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election Title" />
                    <asp:BoundField DataField="StartDateTime" HeaderText="Start Date/Time" />
                    <asp:BoundField DataField="EndDateTime" HeaderText="End Date/Time" />

                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlGridStatus" runat="server" CssClass="grid-dropdown">
                                <asp:ListItem Text="Draft" Value="Draft"></asp:ListItem>
                                <asp:ListItem Text="Open" Value="Open"></asp:ListItem>
                                <asp:ListItem Text="Closed" Value="Closed"></asp:ListItem>
                                <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                            </asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnUpdateStatus" runat="server" Text="Update Status" CssClass="small-button" CommandName="UpdateStatus" CommandArgument='<%# Container.DataItemIndex %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

            </asp:GridView>

            <div class="button-area">
                <asp:Button ID="btnBack" runat="server" Text="Back to Admin Dashboard" CssClass="back-button" />
                <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="logout-button" />
            </div>

        </div>

    </form>
</body>
</html>