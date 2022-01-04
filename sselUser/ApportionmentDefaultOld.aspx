<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMaster.Master" CodeBehind="ApportionmentDefaultOld.aspx.vb" Inherits="sselUser.ApportionmentDefaultOld" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 10px;">
        <h2>Default Apportionment</h2>
        <div class="note" style="width: 650px;">
            Default values are used by the system to apportion your lab usage cost among your accounts if you do not enter any data. Please update them regularly because of account changes.
        </div>
        <div class="filter">
            <table>
                <tr>
                    <td style="width: 100px;">Select User:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlUser" DataTextField="DisplayName" DataValueField="ClientID" Width="245">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Select Room:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlRoom" DataTextField="RoomName" DataValueField="RoomID" Width="245">
                        </asp:DropDownList>
                        <img src="//ssel-apps.eecs.umich.edu/static/images/arrow-red-left.png" alt="arrow" />&nbsp;<span style="color: Red;">remember to do all rooms</span>
                    </td>
                </tr>
            </table>
            <div style="padding-top: 10px; padding-bottom: 10px;">
                <asp:Button ID="btnGetData" runat="server" Text="Get Data" Width="100" />
            </div>
        </div>
        <asp:Panel runat="server" ID="panDisplay" Visible="false">
            <div style="padding-left: 10px; border-top: solid 1px #AAAAAA;">
                <div style="padding-top: 10px;">
                    <div style="color: #800000; padding-bottom: 5px;">
                        Note: The sum of all numbers in each box must be 100 (e.g. Account 1 = 70, Account 2 = 30).
                    </div>
                    <asp:GridView runat="server" ID="gvAppDefault" AutoGenerateColumns="false" CssClass="grid">
                    </asp:GridView>
                </div>
                <div style="padding-top: 20px;">
                    <asp:Button runat="server" ID="btnSave" Text="Save" Width="100" />
                    <asp:Label ID="lblMsg" runat="server" CssClass="error"></asp:Label>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
