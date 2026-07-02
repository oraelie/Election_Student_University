<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ManageCandidates.aspx.vb" Inherits="UniversityElectionSystem.ManageCandidates" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Manage Candidates</title>
    <link href="Css/ManageCandidates.css" rel="stylesheet" type="text/css" />
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

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="form-box">
                <h3>Add New Candidate</h3>

                <label>Position</label>
                <asp:DropDownList ID="ddlPosition" runat="server" CssClass="input-box"></asp:DropDownList>

                <label>Candidate Faculty</label>
                <asp:DropDownList ID="ddlFaculty" runat="server" CssClass="input-box"></asp:DropDownList>

                <label>Full Name</label>
                <asp:TextBox ID="txtFullName" runat="server" CssClass="input-box"></asp:TextBox>

                <label>Major</label>
                <asp:TextBox ID="txtMajor" runat="server" CssClass="input-box"></asp:TextBox>

                <label>Year Level</label>
                <asp:TextBox ID="txtYearLevel" runat="server" CssClass="input-box" placeholder="Example: 1, 2, 3, 4"></asp:TextBox>

                <label>Description</label>
                <asp:TextBox 
                    ID="txtDescription" 
                    runat="server" 
                    CssClass="input-box textarea-box"
                    TextMode="MultiLine">
                </asp:TextBox>

                <asp:Button ID="btnAddCandidate" runat="server" Text="Add Candidate" CssClass="main-button" />
            </div>

            <h3>Existing Candidates</h3>

            <asp:GridView 
                ID="gvCandidates" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="CandidateID">

                <Columns>
                    <asp:BoundField DataField="CandidateID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election" />
                    <asp:BoundField DataField="PositionTitle" HeaderText="Position" />
                    <asp:BoundField DataField="FullName" HeaderText="Candidate Name" />
                    <asp:BoundField DataField="FacultyName" HeaderText="Faculty" />
                    <asp:BoundField DataField="Major" HeaderText="Major" />
                    <asp:BoundField DataField="YearLevel" HeaderText="Year" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />

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
                                ID="btnUpdateActive" 
                                runat="server" 
                                Text="Update" 
                                CssClass="small-button" 
                                CommandName="UpdateActive" 
                                CommandArgument='<%# Container.DataItemIndex %>' />
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