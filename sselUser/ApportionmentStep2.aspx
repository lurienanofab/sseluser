<%@ Page Title="Lab Time Apportionment: Step 2" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMasterBootstrap.Master" CodeBehind="ApportionmentStep2.aspx.vb" Inherits="sselUser.ApportionmentStep2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- <link rel="stylesheet" href="styles/apportionment.css" /> -->

    <style>
        .year-select {
            display: inline-block;
            width: 90px;
        }

        .month-select {
            display: inline-block;
            width: 170px;
        }

        .grid tbody tr th {
            padding: 5px;
            background-color: #dcdcdc;
        }

        .grid tbody tr td {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5>Lab Time Apportionment</h5>

    <div class="alert alert-info" role="alert">
        Please note: As of January 1, 2015 the Clean Room and ROBIN labs have been combined for daily fee apportionment. Room entry apportionment is now available by specific room.
    </div>

    <div class="filter">
        <div class="row mb-3">
            <label for="ddlYear" class="col-xl-1 col-md-2 col-sm-3 col-form-label" style="min-width: 120px;">Select Time:</label>
            <div class="col-md-5">
                <asp:DropDownList runat="server" ID="ddlYear" DataValueField="YearValue" DataTextField="YearText" AutoPostBack="True" CssClass="year-select form-control" ClientIDMode="Static">
                    <asp:ListItem Value="2009">2009</asp:ListItem>
                    <asp:ListItem Value="2010">2010</asp:ListItem>
                    <asp:ListItem Value="2011">2011</asp:ListItem>
                    <asp:ListItem Value="2012">2012</asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList runat="server" ID="ddlMonth" AutoPostBack="True" CssClass="month-select form-control">
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
            </div>
        </div>

        <div class="row mb-3">
            <label for="ddlUser" class="col-xl-1 col-md-2 col-sm-3 col-form-label" style="min-width: 120px;">Select User:</label>
            <div class="col-xl-4 col-md-5">
                <asp:DropDownList runat="server" ID="ddlUser" DataTextField="DisplayName" DataValueField="ClientID" CssClass="form-control" ClientIDMode="Static">
                </asp:DropDownList>
            </div>
        </div>

        <div class="row mb-3">
            <label for="ddlRoom" class="col-xl-1 col-md-2 col-sm-3 col-form-label" style="min-width: 120px;">Select Room:</label>
            <div class="col-xl-4 col-md-5">
                <asp:DropDownList runat="server" ID="ddlRoom" DataTextField="RoomName" DataValueField="RoomID" CssClass="form-control" ClientIDMode="Static">
                </asp:DropDownList>
            </div>
        </div>

        <div class="mb-2">
            <asp:PlaceHolder runat="server" ID="phGetData">
                <asp:Button runat="server" ID="btnGetData" Text="Get Data" CssClass="btn btn-primary load-button" OnClick="GetData_Click" Enabled="false" />
                <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Working..." class="loader" style="display: none;" />
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="phAfterCutoff" Visible="false">
                <div class="text-muted">
                    <em>You may not apportion room charges after the fourth business day.</em>
                </div>
            </asp:PlaceHolder>
        </div>

        <div class="save-error">
            <asp:Label ID="lblMsg1" runat="server" CssClass="error"></asp:Label>
        </div>

        <div style="margin-top: 0; margin-bottom: 20px;">
            <asp:HyperLink runat="server" ID="hypCancel1">&larr; Return to Step 1</asp:HyperLink>
        </div>
    </div>

    <hr />

    <div style="padding-left: 10px;">
        <asp:Panel runat="server" ID="panDisplay" Visible="false">

            <asp:Repeater runat="server" ID="rptBilling">
                <ItemTemplate>
                    <div class="billing" data-client_id='<%#Eval("ClientID")%>' data-start_date='<%#Eval("StartDate")%>' data-end_date='<%#Eval("EndDate")%>'>
                        <h5>Billing Recalculation</h5>
                        <div style="padding-top: 0; padding-left: 20px; margin-bottom: 20px;">
                            <div class="loading step1" style="display: none;">Recalculating room billing...</div>
                            <div class="loading step4">Recalculating subsidy...</div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div style="padding-left: 10px;">
                <input runat="server" type="hidden" id="hidReadOnly" class="readonly" />
                <div class="mb-2">
                    <h5>Step 1: Room Day Apportionment</h5>
                    <div style="padding-top: 10px; padding-left: 20px;">
                        <div style="padding-bottom: 10px; color: #003366;">
                            <table style="border-collapse: collapse;">
                                <tr>
                                    <td style="vertical-align: middle; padding: 5px 10px 0px 0px; font-weight: bold;">Total days in lab:</td>
                                    <td style="vertical-align: middle; padding: 5px 10px 0px 0px; font-weight: bold;">
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
                                            <strong style="font-size: 15px;"><%#Eval("OrgName") %></strong>
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

                <div class="mb-2">
                    <h5>Step 2: Room Entry Apportionment</h5>
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
                                        <strong style="font-size: 15px;"><%#Eval("DisplayName")%> (Total Entries: <%#GetTotalEntriesValue(Container.DataItem)%>)</strong>
                                    </div>
                                    <table class="room-entry-apportionment-account-table" data-room-id='<%#Eval("RoomID")%>' data-total-entries='<%#Eval("TotalEntries")%>'>
                                        <tbody>
                                            <asp:Repeater runat="server" ID="rptAccountRoomEntries" OnItemDataBound="RptAccountRoomEntries_ItemDataBound">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <input type="hidden" runat="server" id="hidAccountID" value='<%#Eval("AccountID")%>' class="account-id" />
                                                            <asp:TextBox runat="server" ID="txtAccountEntries" Width="80" Text='<%#GetEntriesValue(Container.DataItem)%>' CssClass="account-entries-text form-control"></asp:TextBox>
                                                        </td>
                                                        <th><%#Eval("AccountName")%></th>
                                                        <th runat="server" visible='<%#ShowShortCodeColumn()%>'><%#Eval("ShortCode")%></th>
                                                        <th><%#Eval("OrgName")%></th>
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
                        <asp:PlaceHolder runat="server" ID="phRoomEntryApportionmentNoData" Visible="false">
                            <div class="nodata">
                                There are no child rooms, so entry apportionment is not needed.
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>

                <div class="mb-2" style="display: none;">
                    <asp:CheckBox runat="server" ID="chkBilling" Text="Recalculate all fees now (this will take several seconds). You can still see the accurate data after 3rd business day without recalculation" Checked="true" Visible="false" />
                </div>

                <div class="mt-3 mb-2">
                    <asp:Button ID="btnSave" runat="server" Text="Save Room Entry Apportionment" OnCommand="Save_Command" Enabled="false" CssClass="save-button btn btn-primary" CommandName="save" CommandArgument="room-days"></asp:Button>
                    <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Saving..." class="loader" style="display: none;" />
                </div>

                <div class="mb-2 save-error">
                    <asp:Label ID="lblMsg2" runat="server" CssClass="error"></asp:Label>
                </div>

                <div class="mb-2">
                    <asp:HyperLink runat="server" ID="hypCancel2">&larr; Return to Step 1</asp:HyperLink>
                </div>
            </div>
        </asp:Panel>

        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        var subsidyOnly = true;

        if (subsidyOnly) {
            $(".billing .step1").hide();
        }

        $(".billing").each(function () {
            var $this = $(this);

            $this.show();

            function recalcRoomBiling(callback) {
                if (subsidyOnly) {
                    callback();
                    return;
                }

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
