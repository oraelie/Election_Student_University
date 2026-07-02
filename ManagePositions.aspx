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

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="form-box">
                <h3>Add New Position</h3>

                <label>Election</label>
                <asp:DropDownList ID="ddlElection" runat="server" CssClass="input-box"></asp:DropDownList>

                <label>Position Title</label>
                <asp:TextBox ID="txtPositionTitle" runat="server" CssClass="input-box"></asp:TextBox>

                <label>Faculty</label>
                <asp:DropDownList ID="ddlFaculty" runat="server" CssClass="input-box"></asp:DropDownList>

                <p class="hint">
                    Choose "General Position" if this position is for all students.
                </p>

                <asp:Button ID="btnAddPosition" runat="server" Text="Add Position" CssClass="main-button" />
            </div>

            <h3>Existing Positions</h3>

            <asp:GridView 
                ID="gvPositions" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                DataKeyNames="PositionID">

                <Columns>
                    <asp:BoundField DataField="PositionID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election" />
                    <asp:BoundField DataField="PositionTitle" HeaderText="Position" />
                    <asp:BoundField DataField="FacultyName" HeaderText="Faculty" />

                    <asp:TemplateField HeaderText="Active">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkIsActive" runat="server" Checked='<%# Convert.ToBoolean(Eval("IsActive")) %>' />
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