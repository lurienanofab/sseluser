<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMaster.Master" CodeBehind="ApportionmentStep2.aspx.vb" Inherits="sselUser.ApportionmentStep2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/apportionment.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div style="padding: 10px;">
        <h2>Lab Time Apportionment</h2>

        <div class="alert alert-info">
            Please note: As of January 1, 2015 the Clean Room and ROBIN labs have been combined for daily fee apportionment. Room entry apportionment is now available by specific room.
        </div>

        <div class="filter">
            <table>
                <tr>
                    <td style="width: 100px;">Select Time:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlYear" Width="72" DataValueField="YearValue" DataTextField="YearText" AutoPostBack="True">
                            <asp:ListItem Value="2009">2009</asp:ListItem>
                            <asp:ListItem Value="2010">2010</asp:ListItem>
                            <asp:ListItem Value="2011">2011</asp:ListItem>
                            <asp:ListItem Value="2012">2012</asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList runat="server" ID="ddlMonth" Width="169" AutoPostBack="True">
                            <asp:ListItem Value="1">January</asp:ListItem>
                            <asp:ListItem Value="2">February</asp:ListItem>
                            <asp:ListItem Value="3">March</asp:ListItem>
                            <asp:ListItem Value="4">April</asp:ListItem>
                            <asp:ListItem Value="5">May</asp:ListItem>
                            <asp:ListItem Value="6">June</asp:ListItem>
                            <asp:ListItem Value="7">July</asp:ListItem>
                            <asp:ListItem Value="8">August</asp:ListItem>
                            <asp:ListItem Value="9">September</asp:ListItem>
                            <asp:ListItem Value="10">October</asp:ListItem>
                            <asp:ListItem Value="11">November</asp:ListItem>
                            <asp:ListItem Value="12">December</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Select User:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlUser" Width="245" DataTextField="DisplayName" DataValueField="ClientID">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Select Room:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlRoom" Width="245" DataTextField="RoomName" DataValueField="RoomID">
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>
            <div style="padding-top: 10px; padding-bottom: 10px;">
                <asp:Button ID="btnGetData" runat="server" Text="Get Data" Width="100" OnClick="GetData_Click" CssClass="load-button" Enabled="false" />
                <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Working..." class="loader" style="display: none;" />
            </div>
            <div class="save-error">
                <asp:Label ID="lblMsg1" runat="server" CssClass="error"></asp:Label>
            </div>
            <div style="margin-top: 0; margin-bottom: 20px;">
                <asp:HyperLink runat="server" ID="hypCancel1">&larr; Return to Step 1</asp:HyperLink>
            </div>
        </div>

        <asp:Panel runat="server" ID="panDisplay" Visible="false">
            <asp:Repeater runat="server" ID="rptBilling">
                <ItemTemplate>
                    <div class="billing" data-client_id='<%#Eval("ClientID")%>' data-start_date='<%#Eval("StartDate")%>' data-end_date='<%#Eval("EndDate")%>'>
                        <h2>Billing Recalculation</h2>
                        <div style="padding-top: 0; padding-left: 20px; margin-bottom: 20px;">
                            <div class="loading step1">Recalculating room billing...</div>
                            <div class="loading step4">Recalculating subsidy...</div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div style="padding-left: 10px; padding-bottom: 10px; border-top: solid 1px #AAAAAA;">
                <input runat="server" type="hidden" id="hidReadOnly" class="readonly" />
                <div style="padding-top: 10px; font-size: 10pt;">
                    <h2>Step 1: Room Day Apportionment</h2>
                    <div style="padding-top: 10px; padding-left: 20px;">
                        <div style="padding-bottom: 10px; color: #003366;">
                            <table style="border-collapse: collapse;">
                                <tr>
                                    <td style="vertical-align: middle; padding: 5px 10px 0px 0px;">Total days in lab:</td>
                                    <td style="vertical-align: middle; padding: 5px 10px 0px 0px;">
                                        <span id="physday">
                                            <asp:Literal ID="litPhysicalDays" runat="server" Text="0"></asp:Literal></span>&nbsp;days
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <asp:Panel runat="server" ID="panApporUnit">
                            <div style="color: #800000; padding-bottom: 5px;">
                                Apportionment Unit:&nbsp;
                            <asp:RadioButton ID="rdoDay" runat="server" Text="Days" Checked="true" GroupName="a" AutoPostBack="true" CssClass="day-radio" OnCheckedChanged="RdoDay_CheckedChanged" />
                                <asp:RadioButton ID="rdoPercentage" runat="server" Text="Percentage" GroupName="a" AutoPostBack="true" CssClass="pct-radio" />
                                <span>&nbsp;(The sum of all input days must be equal to the total days OR the total percentage must be 100%)</span>
                            </div>
                        </asp:Panel>
                        <div>
                            <asp:Repeater runat="server" ID="rptMultiOrg" OnItemDataBound="MultiOrg_ItemDataBound">
                                <ItemTemplate>
                                    <div style="padding: 10px 0px 10px 0px;">
                                        <input type="hidden" runat="server" id="hidOrgID" value='<%#Eval("OrgID") %>' class="org-id" />
                                        <div class="org-name">
                                            <%#Eval("OrgName") %>
                                        </div>
                                        <div class="org-grid">
                                            <asp:GridView ID="gvOrg" runat="server" AutoGenerateColumns="false" OnDataBound="Org_DataBound" CssClass="grid" EnableViewState="true">
                                                <RowStyle CssClass="item" />
                                                <AlternatingRowStyle CssClass="item" />
                                            </asp:GridView>
                                        </div>
                                        <div style="padding-left: 5px;">
                                            <asp:Label ID="lblOrgMsg" runat="server" CssClass="error"></asp:Label>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>

                <div style="padding-top: 10px; font-size: 10pt;">
                    <h2>Step 2: Room Entry Apportionment</h2>
                    <div style="padding-top: 10px; padding-left: 20px;">
                        <div style="margin-bottom: 10px; display: none;">
                            <a href="#" class="apply-percentages">Apply Room Day Percentages</a>
                            <span class="nodata">(Use values entered above for entries apportionment)</span>
                        </div>
                        <asp:Repeater runat="server" ID="rptRoomEntries" OnItemDataBound="RptRoomEntries_ItemDataBound">
                            <HeaderTemplate>
                                <div class="child-room-entries-apportionment">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div style="margin-bottom: 10px;">
                                    <input type="hidden" runat="server" id="hidRoomID" value='<%#Eval("RoomID")%>' class="room-id" />
                                    <div class="org-name">
                                        <%#Eval("DisplayName")%> (Total Entries: <%#GetTotalEntriesValue(Container.DataItem)%>)
                                    </div>
                                    <table class="room-entry-apportionment-account-table" data-room-id='<%#Eval("RoomID")%>' data-total-entries='<%#Eval("TotalEntries")%>'>
                                        <tbody>
                                            <asp:Repeater runat="server" ID="rptAccountRoomEntries" OnItemDataBound="RptAccountRoomEntries_ItemDataBound">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <input type="hidden" runat="server" id="hidAccountID" value='<%#Eval("AccountID")%>' class="account-id" />
                                                            <asp:TextBox runat="server" ID="txtAccountEntries" Width="60" Text='<%#GetEntriesValue(Container.DataItem)%>' CssClass="account-entries-text"></asp:TextBox>
                                                        </td>
                                                        <td><%#Eval("AccountName")%></td>
                                                        <td><%#Eval("ShortCode")%></td>
                                                        <td><%#Eval("OrgName")%></td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <div style="padding-top: 10px; font-size: 10pt;">
                    <asp:CheckBox runat="server" ID="chkBilling" Text="Recalculate all fees now (this will take several seconds). You can still see the accurate data after 3rd business day without recalculation" Checked="true" Visible="false" />
                </div>
                <div style="padding-top: 20px;">
                    <asp:Button ID="btnSave" runat="server" Text="Save Room Entry Apportionment" OnCommand="Save_Command" Enabled="false" CssClass="save-button" CommandName="save" CommandArgument="room-days"></asp:Button>
                    <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Saving..." class="loader" style="display: none;" />
                    <div class="save-error">
                        <asp:Label ID="lblMsg2" runat="server" CssClass="error"></asp:Label>
                    </div>
                </div>
                <div style="margin-top: 10px;">
                    <asp:HyperLink runat="server" ID="hypCancel2">&larr; Return to Step 1</asp:HyperLink>
                </div>
            </div>
        </asp:Panel>

        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        //$(window).scrollposition();

        $('.numeric-text').numerictext({
            'integer': false
        });

        $(".billing").each(function () {
            var $this = $(this);

            $this.show();



            function recalcRoomBiling(callback) {
                var data = {
                    "Period": $this.data("start_date"),
                    "ClientID": $this.data("client_id"),
                    "BillingCategory": 'room',
                    "Delete": true,
                    "IsTemp": false,
                    "Record": 0
                }

                $.ajax({
                    "url": "/webapi/billing/process/step1",
                    "type": "POST",
                    "data": data
                }).done(function (data, textStatus, jqXHR) {
                    $(".loading.step1", $this).append($("<strong/>", { "style": "color: #008000;" }).html("&nbsp;OK"));
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $(".loading.step1", $this).append($("<strong/>", { "style": "color: #ff0000;" }).html("&nbsp;" + errorThrown));
                }).always(function () {
                    $(".loading.step1", $this).css({ "background-image": "none", "padding-left": "0" });
                    callback();
                });
            }

            function recalcSubsidy() {
                var data = {
                    "Period": $this.data("start_date"),
                    "ClientID": $this.data("client_id"),
                    "Command": "subsidy"
                };

                $.ajax({
                    "url": "/webapi/billing/process/step4",
                    "type": "POST",
                    "data": data
                }).done(function (data, textStatus, jqXHR) {
                    $(".loading.step4", $this).append($("<strong/>", { "style": "color: #008000;" }).html("&nbsp;OK"));
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $(".loading.step4", $this).append($("<strong/>", { "style": "color: #ff0000;" }).html("&nbsp;" + errorThrown));
                }).always(function () {
                    $(".loading.step4", $this).css({ "background-image": "none", "padding-left": "0" });
                });
            }

            recalcRoomBiling(function () {
                recalcSubsidy();
            });
        })
    </script>
</asp:Content>
