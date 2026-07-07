<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageFaculties.aspx.vb" Inherits="UniversityElectionSystem.ManageFaculties" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Faculties</title>
    <link href="Css/ManageFaculties.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Manage Faculties</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-box">
                <h3>Add Faculty</h3>

                <label>Faculty Name</label>
                <asp:TextBox 
                    ID="txtFacultyName" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: Faculty of Engineering">
                </asp:TextBox>

                <asp:Button 
                    ID="btnAddFaculty" 
                    runat="server" 
                    Text="Add Faculty" 
                    CssClass="main-button" />
            </div>

            <div class="filter-box">

                <h3>Filter Faculties</h3>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Faculty Name</label>
                        <asp:TextBox 
                            ID="txtFacultyNameFilter" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="Search by faculty name">
                        </asp:TextBox>
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

            <h3>Existing Faculties</h3>

            <asp:GridView 
                ID="gvFaculties" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="FacultyID"
                EmptyDataText="No faculties found.">

                <Columns>

                    <asp:BoundField DataField="FacultyID" HeaderText="ID" ReadOnly="True" />

                    <asp:TemplateField HeaderText="Faculty Name">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridFacultyName" 
                                runat="server" 
                                CssClass="grid-input"
                                Text='<%# Eval("FacultyName") %>'>
                            </asp:TextBox>
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
                                ID="btnUpdateFaculty" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdateFaculty" 
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