<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageVoters.aspx.vb" Inherits="UniversityElectionSystem.ManageVoters" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Voters</title>
    <link href="Css/ManageVoters.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Manage Eligible Voters</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-box">
                <h3>Add Eligible Voter</h3>

                <label>AD Username / Student ID</label>
                <asp:TextBox 
                    ID="txtADUsername" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: 202400699">
                </asp:TextBox>

                <label>Faculty</label>
                <asp:DropDownList 
                    ID="ddlFaculty" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <asp:Button 
                    ID="btnAddVoter" 
                    runat="server" 
                    Text="Add Voter" 
                    CssClass="main-button" />
            </div>

            <div class="filter-box">

                <h3>Filter Voters</h3>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Username / Student ID</label>
                        <asp:TextBox 
                            ID="txtUsernameFilter" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="Search by username">
                        </asp:TextBox>
                    </div>

                    <div class="filter-item">
                        <label>Faculty</label>
                        <asp:DropDownList 
                            ID="ddlFacultyFilter" 
                            runat="server" 
                            CssClass="input-box">
                        </asp:DropDownList>
                    </div>

                    <div class="filter-item">
                        <label>Status</label>
                        <asp:DropDownList 
                            ID="ddlStatusFilter" 
                            runat="server" 
                            CssClass="input-box">

                            <asp:ListItem Text="All" Value=""></asp:ListItem>
                            <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="0"></asp:ListItem>

                        </asp:DropDownList>
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

                    <asp:Button 
                        ID="btnExportExcel" 
                        runat="server" 
                        Text="Export to Excel" 
                        CssClass="export-button" />
                </div>

            </div>

            <h3>Existing Eligible Voters</h3>

            <asp:GridView 
                ID="gvVoters" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="ADUsername,FacultyID"
                EmptyDataText="No voters found.">

                <Columns>

                    <asp:BoundField DataField="ADUsername" HeaderText="Username / Student ID" ReadOnly="True" />

                    <asp:TemplateField HeaderText="Faculty">
                        <ItemTemplate>
                            <asp:DropDownList 
                                ID="ddlGridFaculty" 
                                runat="server" 
                                CssClass="grid-dropdown">
                            </asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Active">
                        <ItemTemplate>
                            <asp:CheckBox 
                                ID="chkIsActive" 
                                runat="server" 
                                Checked='<%# Convert.ToBoolean(Eval("IsActive")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button 
                                ID="btnUpdateVoter" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdateVoter" 
                                CommandArgument='<%# Container.DataItemIndex %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

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