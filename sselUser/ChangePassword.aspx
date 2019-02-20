<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMaster.Master" CodeBehind="ChangePassword.aspx.vb" Inherits="sselUser.ChangePassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $('.pw-text').val('');
        });
    </script>
    <style runat="server" id="styHideNav" visible="false" type="text/css">
        .nav {
            display: none !important;
        }

        .left-panel {
            background: none !important;
            width: 1px !important;
            border-right: none !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 10px;">
        <h2>Change Password</h2>
        <asp:Panel runat="server" ID="panDisplay">
            <div class="note" style="width: 650px;">
                Your password may not be the same as your username and must be at least six characters long with no spaces. We also strongly suggest that it not be the same as your kerberos password because we cannot guarantee the high degree of security needed to protect this important password.
            </div>
            <div class="filter">
                <table>
                    <tr>
                        <td style="width: 165px;">Old password:
                        </td>
                        <td>
                            <asp:TextBox runat="server" ID="txtOldPassword" TextMode="Password" Width="200" CssClass="pw-text"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>New password:
                        </td>
                        <td>
                            <asp:TextBox runat="server" ID="txtNewPassword" TextMode="Password" Width="200" CssClass="pw-text"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Repeat new password:
                        </td>
                        <td>
                            <asp:TextBox runat="server" ID="txtRepeatNew" TextMode="Password" Width="200" CssClass="pw-text"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <div style="padding-top: 10px; padding-bottom: 10px;">
                    <asp:Button ID="btnSave" runat="server" Text="Save" Width="100" OnClick="BtnSave_Click" />
                </div>
                <asp:Literal runat="server" ID="litChangePasswordMsg"></asp:Literal>
            </div>
        </asp:Panel>
        <asp:Literal runat="server" ID="litAuthMsg"></asp:Literal>
    </div>
</asp:Content>
