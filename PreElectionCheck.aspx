<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PreElectionCheck.aspx.vb" Inherits="UniversityElectionSystem.PreElectionCheck" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Pre-Election Check</title>
    <link href="Css/PreElectionCheck.css?v=1" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="page-container">

            <h2>Pre-Election Validation Check</h2>

            <div class="user-box">
                <asp:Label ID="lblAdminName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="filter-box">

                <h3>Select Election to Check</h3>

                <label>Election</label>
                <asp:DropDownList 
                    ID="ddlElections" 
                    runat="server" 
                    CssClass="input-box">
                </asp:DropDownList>

                <asp:Button 
                    ID="btnRunCheck" 
                    runat="server" 
                    Text="Run Validation Check" 
                    CssClass="main-button" />

            </div>

            <h3>Validation Results</h3>

            <asp:GridView 
                ID="gvValidationResults" 
                runat="server"
                AutoGenerateColumns="False"
                CssClass="data-table"
                GridLines="None"
                EmptyDataText="No validation check has been run yet.">

                <Columns>

                    <asp:BoundField DataField="CheckName" HeaderText="Check Name" />

                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label 
                                ID="lblStatus" 
                                runat="server"
                                Text='<%# Eval("Status") %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="Message" HeaderText="Message" />

                </Columns>

            </asp:GridView>

            <div class="legend-box">
                <h3>Status Meaning</h3>

                <p>
                    <span class="legend-ok">OK</span>
                    The check passed.
                </p>

                <p>
                    <span class="legend-warning">WARNING</span>
                    Check carefully before opening the election.
                </p>

                <p>
                    <span class="legend-error">ERROR</span>
                    Must be fixed before opening the election.
                </p>
            </div>

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