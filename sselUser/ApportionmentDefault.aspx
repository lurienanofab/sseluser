<%@ Page Title="Default Apportionment" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMasterBootstrap.Master" CodeBehind="ApportionmentDefault.aspx.vb" Inherits="sselUser.ApportionmentDefault" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .default-apportionment-table {
            margin-bottom: 20px;
            border-collapse: separate;
            border-spacing: 2px;
        }

        .account-name {
            font-weight: bold;
            white-space: nowrap;
            font-size: 14px;
            background-color: #dcdcdc;
            padding: 3px;
        }

        .account-percentage {
            font-size: 14px;
            width: 100px;
            padding: 3px;
        }

        .percentage-textbox {
            width: 80px;
            margin: 0 auto;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5>Default Apportionment</h5>

    <div class="alert alert-secondary" role="alert">
        Default values are used by the system to apportion your lab usage cost among your accounts if you do not enter any data. Please update them regularly because of account changes.
    </div>

    <div class="form-group row">
        <label for="ddlUser" class="col-sm-1 col-form-label">Select User</label>
        <div class="col-sm-3">
            <asp:DropDownList runat="server" ID="ddlUser" DataValueField="ClientID" DataTextField="DisplayName" CssClass="form-control" ClientIDMode="Static"></asp:DropDownList>
        </div>
    </div>

    <asp:Button runat="server" ID="btnGetData" Text="Get Data" CssClass="btn btn-primary" OnClick="BtnGetData_Click" />

    <hr />

    <div class="mb-2">
        <asp:PlaceHolder runat="server" ID="phAccounts" Visible="false">
            <div class="alert alert-info" role="alert">
                Note: The sum of all numbers in each room must be 100 (e.g. Account 1 = 70, Account 2 = 30).
            </div>

            <asp:PlaceHolder runat="server" ID="phSaveMessage" Visible="false">
                <div class="mt-2">
                    <div runat="server" id="divSaveSaveMessage" class="alert" role="alert">
                        <asp:Literal runat="server" ID="litSaveMessage"></asp:Literal>
                    </div>
                </div>
            </asp:PlaceHolder>

            <div class="ml-3">
                <asp:Repeater runat="server" ID="rptRooms" OnItemDataBound="RptRooms_ItemDataBound">
                    <ItemTemplate>
                        <div>
                            <asp:HiddenField runat="server" ID="hidRoomID" Value='<%#Eval("RoomID")%>' />
                            <h6>
                                <asp:Label runat="server" ID="lblRoomName" Text='<%#Eval("RoomName")%>'></asp:Label>
                            </h6>
                            <asp:Repeater runat="server" ID="rptAccounts">
                                <HeaderTemplate>
                                    <table class="default-apportionment-table">
                                        <thead></thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td class="account-name">
                                            <asp:HiddenField runat="server" ID="hidAccountID" Value='<%#Eval("AccountID")%>' />
                                            <asp:Label runat="server" ID="lblAccountName" Text='<%#Eval("AccountName")%>'></asp:Label>
                                        </td>
                                        <td class="account-percentage">
                                            <asp:TextBox runat="server" ID="txtPercentage" CssClass="form-control percentage-textbox" Text='<%#Eval("Percentage")%>'></asp:TextBox>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                                </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Button runat="server" ID="btnSave" Text="Save" CssClass="btn btn-primary" OnClick="BtnSave_Click" />
            </div>
        </asp:PlaceHolder>

        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
    </div>
</asp:Content>
