<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Vote.aspx.vb" Inherits="UniversityElectionSystem.Vote" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Vote</title>
    <link href="Css/Vote.css?v=3" rel="stylesheet" type="text/css" />
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

            <asp:Panel ID="pnlMessage" runat="server" CssClass="alert-box" Visible="False">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-group">
                <label>Select Open Election</label>

                <asp:DropDownList 
                    ID="ddlElections" 
                    runat="server" 
                    CssClass="dropdown"
                    AutoPostBack="True">
                </asp:DropDownList>
            </div>

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
                    CssClass="submit-button"
                    OnClientClick="showVoteConfirm(); return false;" />

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

        <!-- Custom Confirmation Popup -->
        <div id="voteConfirmOverlay" class="confirm-overlay" style="display:none;">
            <div class="confirm-box">

                <h3>Confirm Vote Submission</h3>

                <p class="confirm-text">
                    Please confirm that you want to submit your vote.
                </p>

                <p class="confirm-warning">
                    Once submitted, your vote for this position cannot be changed.
                </p>

                <div class="confirm-buttons">

                    <asp:Button 
                        ID="btnConfirmVote" 
                        runat="server" 
                        Text="Yes, Submit Vote" 
                        CssClass="yes-button" />

                    <button 
                        type="button" 
                        class="no-button" 
                        onclick="hideVoteConfirm()">
                        Cancel
                    </button>

                </div>

            </div>
        </div>

        <script type="text/javascript">
			function showVoteConfirm() {
				document.getElementById("voteConfirmOverlay").style.display = "flex";
			}

			function hideVoteConfirm() {
				document.getElementById("voteConfirmOverlay").style.display = "none";
			}
		</script>

    </form>
</body>
</html>