<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Results.aspx.vb" Inherits="UniversityElectionSystem.Results" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Election Results</title>
    <link href="Css/Results.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="results-container">

            <h2>Election Results</h2>

            <div class="user-box">
                <asp:Label ID="lblFullName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="filter-box">
                <label>Select Election</label>

                <asp:DropDownList 
                    ID="ddlElections" 
                    runat="server" 
                    CssClass="dropdown"
                    AutoPostBack="True">
                </asp:DropDownList>
            </div>

            <div class="button-area top-buttons">
                <asp:Button 
                    ID="btnRefresh" 
                    runat="server" 
                    Text="Refresh Results" 
                    CssClass="main-button" />
            </div>

            <asp:GridView 
                ID="gvResults" 
                runat="server" 
                AutoGenerateColumns="False" 
                CssClass="results-table"
                GridLines="None"
                EmptyDataText="No results found.">

                <Columns>
                    <asp:BoundField DataField="ElectionTitle" HeaderText="Election" />
                    <asp:BoundField DataField="PositionTitle" HeaderText="Position" />
                    <asp:BoundField DataField="FacultyName" HeaderText="Faculty" />
                    <asp:BoundField DataField="CandidateID" HeaderText="Candidate ID" />
                    <asp:BoundField DataField="FullName" HeaderText="Candidate Name" />
                    <asp:BoundField DataField="Major" HeaderText="Major" />
                    <asp:BoundField DataField="YearLevel" HeaderText="Year" />
                    <asp:BoundField DataField="TotalVotes" HeaderText="Total Votes" />
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