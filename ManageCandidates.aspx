<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageCandidates.aspx.vb" Inherits="UniversityElectionSystem.ManageCandidates" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Candidates</title>
    <link href="Css/ManageCandidates.css?v=4" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Manage Candidates</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-box">
                <h3>Add Candidate</h3>

                <label>Candidate Username / Student ID</label>
                <asp:TextBox 
                    ID="txtCandidateADUsername" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: 202400699">
                </asp:TextBox>

                <label>Full Name</label>
                <asp:TextBox 
                    ID="txtFullName" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Candidate full name">
                </asp:TextBox>

                <label>Position</label>
                <asp:DropDownList 
                    ID="ddlPosition" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <label>Faculty</label>
                <asp:DropDownList 
                    ID="ddlFaculty" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <label>Major</label>
                <asp:TextBox 
                    ID="txtMajor" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: MIS">
                </asp:TextBox>

                <label>Year Level</label>
                <asp:TextBox 
                    ID="txtYearLevel" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: 3">
                </asp:TextBox>

                <label>Description</label>
                <asp:TextBox 
                    ID="txtDescription" 
                    runat="server" 
                    CssClass="input-box"
                    TextMode="MultiLine"
                    Rows="3"
                    placeholder="Short candidate description">
                </asp:TextBox>

                <asp:Button 
                    ID="btnAddCandidate" 
                    runat="server" 
                    Text="Add Candidate" 
                    CssClass="main-button" />
            </div>

            <div class="filter-box">

                <h3>Filter Candidates</h3>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Candidate Name</label>
                        <asp:TextBox 
                            ID="txtCandidateNameFilter" 
                            runat="server" 
                            CssClass="input-box"
                            placeholder="Search by candidate name">
                        </asp:TextBox>
                    </div>

                    <div class="filter-item">
                        <label>Election</label>
                        <asp:DropDownList 
                            ID="ddlElectionFilter" 
                            runat="server" 
                            CssClass="input-box"
                            AutoPostBack="True">
                        </asp:DropDownList>
                    </div>

                </div>

                <div class="filter-row">

                    <div class="filter-item">
                        <label>Position</label>
                        <asp:DropDownList 
                            ID="ddlPositionFilter" 
                            runat="server" 
                            CssClass="input-box">
                        </asp:DropDownList>
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

            <h3>Existing Candidates</h3>

            <asp:GridView 
                ID="gvCandidates" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="CandidateID,FacultyID,PositionID"
                EmptyDataText="No candidates found.">

                <Columns>

                    <asp:BoundField DataField="CandidateID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election" ReadOnly="True" />
                    <asp:BoundField DataField="PositionTitle" HeaderText="Position" ReadOnly="True" />

                    <asp:TemplateField HeaderText="Username / Student ID">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridCandidateADUsername" 
                                runat="server" 
                                CssClass="grid-username-input"
                                Text='<%# Eval("CandidateADUsername") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Full Name">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridFullName" 
                                runat="server" 
                                CssClass="grid-input"
                                Text='<%# Eval("FullName") %>'>
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

                    <asp:TemplateField HeaderText="Major">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridMajor" 
                                runat="server" 
                                CssClass="grid-input"
                                Text='<%# Eval("Major") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Year">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridYearLevel" 
                                runat="server" 
                                CssClass="grid-year-input"
                                Text='<%# Eval("YearLevel") %>'>
                            </asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Description">
                        <ItemTemplate>
                            <asp:TextBox 
                                ID="txtGridDescription" 
                                runat="server" 
                                CssClass="grid-description"
                                Text='<%# Eval("Description") %>'
                                TextMode="MultiLine"
                                Rows="2">
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
                                ID="btnUpdateCandidate" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdateCandidate" 
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