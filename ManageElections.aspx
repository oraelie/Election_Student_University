<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageElections.aspx.vb" Inherits="UniversityElectionSystem.ManageElections" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Elections</title>
    <link href="Css/ManageElections.css?v=2" rel="stylesheet" type="text/css" />
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

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-box">
                <h3>Add New Election</h3>

                <label>Election Title</label>
                <asp:TextBox 
                    ID="txtElectionTitle" 
                    runat="server" 
                    CssClass="input-box">
                </asp:TextBox>

                <label>Start Date and Time</label>
                <asp:TextBox 
                    ID="txtStartDateTime" 
                    runat="server" 
                    CssClass="input-box" 
                    placeholder="07-07-2026 11:21">
                </asp:TextBox>

                <label>End Date and Time</label>
                <asp:TextBox 
                    ID="txtEndDateTime" 
                    runat="server" 
                    CssClass="input-box" 
                    placeholder="07-07-2026 12:21">
                </asp:TextBox>

                <label>Status</label>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input-box">
                    <asp:ListItem Text="Draft" Value="Draft"></asp:ListItem>
                    <asp:ListItem Text="Open" Value="Open"></asp:ListItem>
                    <asp:ListItem Text="Closed" Value="Closed"></asp:ListItem>
                    <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                </asp:DropDownList>

                <asp:Button 
                    ID="btnAddElection" 
                    runat="server" 
                    Text="Add Election" 
                    CssClass="main-button" />
            </div>

            <div class="action-box">
                <h3>Export Elections</h3>

                <asp:Button 
                    ID="btnExportExcel" 
                    runat="server" 
                    Text="Export to Excel" 
                    CssClass="export-button" />
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

                    <asp:TemplateField HeaderText="Election Title">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridElectionTitle" 
                                runat="server" 
                                CssClass="grid-title-input"
                                Text='<%# Eval("ElectionTitle") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Start Date/Time">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridStartDateTime" 
                                runat="server" 
                                CssClass="grid-date-input"
                                Text='<%# Eval("StartDateTime") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="End Date/Time">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridEndDateTime" 
                                runat="server" 
                                CssClass="grid-date-input"
                                Text='<%# Eval("EndDateTime") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

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
                            <asp:Button 
                                ID="btnUpdateElection" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdateElection" 
                                CommandArgument='<%# Container.DataItemIndex %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
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