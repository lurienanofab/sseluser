<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMasterBootstrap.Master" CodeBehind="ChangePassword.aspx.vb" Inherits="sselUser.ChangePassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5>Change Password</h5>
    <div class="alert alert-secondary" role="alert">Passwords are now changed by clicking the "Set new or reset password" link on the login page.</div>
    <h4><a href="/login/request-password-reset">Click here to change your password</a></h4>
</asp:Content>
    