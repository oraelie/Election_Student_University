<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Vote.aspx.vb" Inherits="UniversityElectionSystem.Vote" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Vote</title>
    <link href="Css/Vote.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">

        <div class="vote-container">

            <h2>University Election Voting Page</h2>

            <div class="user-box">
                <asp:Label ID="lblFullName" runat="server" CssClass="name-label"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>
            </div>

            <hr />

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label"></asp:Label>

            <div class="form-group">
                <label>Select Position</label>
                <asp:DropDownList 
                    ID="ddlPositions" 
                    runat="server" 
                    CssClass="dropdown"
                    AutoPostBack="True">
                </asp:DropDownList>
            </div>

            <div class="form-group">
                <label>Select Candidate</label>

                <asp:RadioButtonList 
                    ID="rblCandidates" 
                    runat="server" 
                    CssClass="candidate-list">
                </asp:RadioButtonList>
            </div>

            <div class="button-area">
                <asp:Button 
                    ID="btnSubmitVote" 
                    runat="server" 
                    Text="Submit Vote" 
                    CssClass="submit-button" />

                <asp:Button 
                    ID="btnBack" 
                    runat="server" 
                    Text="Back to Dashboard" 
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