<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StudentDashboard.aspx.vb" Inherits="UniversityElectionSystem.StudentDashboard" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Student Dashboard</title>
    <link href="Css/StudentDashboard.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
		function closeConfirmBox() {
			document.getElementById("confirmOverlay").style.display = "none";
		}

		function logoutUser() {
			window.location = "Logout.aspx";
		}
	</script>
</head>

<body>
    <form id="form1" runat="server">

        <!-- Confirmation Dialog -->
        <div id="confirmOverlay" class="confirm-overlay">
            <div class="confirm-box">
                <h3>Confirm Your Identity</h3>

                <p class="confirm-text">
                    Please check your information before continuing.
                </p>

                <div class="info-card">
                    <div>
                        <span class="info-title">Name</span>
                        <asp:Label ID="lblConfirmFullName" runat="server" CssClass="info-value"></asp:Label>
                    </div>

                    <div>
                        <span class="info-title">Username</span>
                        <asp:Label ID="lblConfirmUsername" runat="server" CssClass="info-value"></asp:Label>
                    </div>
                </div>

                <p class="confirm-question">
                    Is this information correct?
                </p>

                <div class="confirm-buttons">
                    <button type="button" class="yes-button" onclick="closeConfirmBox()">Yes, Continue</button>
                    <button type="button" class="no-button" onclick="logoutUser()">No, Go to Login</button>
                </div>
            </div>
        </div>

        <!-- Dashboard -->
        <div class="dashboard-box">

            <h2>Student Dashboard</h2>

            <asp:Label ID="lblFullName" runat="server" CssClass="welcome-label"></asp:Label>
            <br />
            <asp:Label ID="lblUsername" runat="server" CssClass="username-label"></asp:Label>

            <br /><br />

            <asp:Button 
                ID="btnVote" 
                runat="server" 
                Text="Go to Voting Page" 
                CssClass="main-button" />

            <asp:Button 
                ID="btnLogout" 
                runat="server" 
                Text="Logout" 
                CssClass="logout-button" />

        </div>

    </form>
</body>
</html>