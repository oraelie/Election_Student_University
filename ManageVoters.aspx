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

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="form-box">
                <h3>Add New Eligible Voter</h3>

                <label>AD Username</label>
                <asp:TextBox 
                    ID="txtADUsername" 
                    runat="server" 
                    CssClass="input-box"
                    placeholder="Example: elie.tawk or 202400699">
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
                    Text="Add Eligible Voter" 
                    CssClass="main-button" />
            </div>

            <h3>Existing Eligible Voters</h3>

            <asp:GridView 
                ID="gvVoters" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="ADUsername">

                <Columns>
                    <asp:BoundField DataField="ADUsername" HeaderText="AD Username" ReadOnly="True" />
                    <asp:BoundField DataField="FacultyName" HeaderText="Faculty" />

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