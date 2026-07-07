<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManagePositions.aspx.vb" Inherits="UniversityElectionSystem.ManagePositions" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Positions</title>
    <link href="Css/ManagePositions.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Manage Positions</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-box">
                <h3>Add Position</h3>

                <label>Election</label>
                <asp:DropDownList 
                    ID="ddlElection" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <label>Position Title</label>
                <asp:TextBox 
                    ID="txtPositionTitle" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: President">
                </asp:TextBox>

                <label>Faculty</label>
                <asp:DropDownList 
                    ID="ddlFaculty" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <asp:Button 
                    ID="btnAddPosition" 
                    runat="server" 
                    Text="Add Position" 
                    CssClass="main-button" />
            </div>

            <div class="filter-box">

                <h3>Filter Positions</h3>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Election</label>
                        <asp:DropDownList 
                            ID="ddlElectionFilter" 
                            runat="server" 
                            CssClass="input-box">
                        </asp:DropDownList>
                    </div>

                    <div class="filter-item">
                        <label>Position Title</label>
                        <asp:TextBox 
                            ID="txtPositionTitleFilter" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="Search by position title">
                        </asp:TextBox>
                    </div>

                </div>

                <div class="filter-row">

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
                </div>

            </div>

            <h3>Existing Positions</h3>

            <asp:GridView 
                ID="gvPositions" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="PositionID,FacultyID"
                EmptyDataText="No positions found.">

                <Columns>

                    <asp:BoundField DataField="PositionID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election" ReadOnly="True" />

                    <asp:TemplateField HeaderText="Position Title">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridPositionTitle" 
                                runat="server" 
                                CssClass="grid-title-input"
                                Text='<%# Eval("PositionTitle") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

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
                                ID="btnUpdatePosition" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdatePosition" 
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