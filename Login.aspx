l<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="UniversityElectionSystem.Login" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>University Election Login</title>
    <link href="Css/Login.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-box">
            <h2>University Election Login</h2>

            <asp:Label ID="lblUsername" runat="server" Text="AD Username"></asp:Label>
            <asp:TextBox ID="txtUsername" runat="server" CssClass="input-box"></asp:TextBox>

            <asp:Label ID="lblPassword" runat="server" Text="Password"></asp:Label>
            <asp:TextBox ID="txtPassword" runat="server" CssClass="input-box" TextMode="Password"></asp:TextBox>

            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="login-button" />

            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
        </div>
    </form>
</body>
</html>