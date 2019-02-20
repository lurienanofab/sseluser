<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMaster.Master" CodeBehind="ApportionmentStep1.aspx.vb" Inherits="sselUser.ApportionmentStep1" Async="true" AsyncTimeout="600000" %>

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
                <asp:Button ID="btnGetData" runat="server" Text="Get Data" Width="100" OnClick="GetData_Click" CssClass="load-button" />
                <asp:CheckBox runat="server" ID="chkUpdateBilling" Text="Update Billing (check this if any reservations have been modified, i.e. account changed or forgiven)" CssClass="update-billing" />
                <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Working..." class="loader" style="display: none;" />
                <div style="margin-top: 10px; font-style: italic; color: #808080;">
                    <asp:Literal runat="server" ID="litLastBillingUpdate"></asp:Literal>
                </div>
            </div>
            <div class="save-error">
                <asp:Label ID="lblMsg1" runat="server" CssClass="error"></asp:Label>
            </div>
        </div>

        <asp:Panel runat="server" ID="panDisplay" Visible="false">
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
                    <asp:CheckBox runat="server" ID="chkBilling" Text="Recalculate all fees now (this will take several seconds). You can still see the accurate data after 3rd business day without recalculation" Checked="true" Visible="false" />
                </div>

                <div style="padding-top: 20px;">
                    <asp:Button ID="btnSave" runat="server" Text="Save and Apportion Room Entries" OnCommand="Save_Command" Enabled="false" CssClass="save-button" CommandName="save" CommandArgument="room-days"></asp:Button>
                    <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif" alt="Saving..." class="loader" style="display: none;" />
                    <div style="margin-top: 10px;">
                        <asp:HyperLink runat="server" ID="hypViewEntryApportionment" CssClass="continue-link">View entry apportionment (continue without saving)</asp:HyperLink>
                    </div>
                    <div class="save-error">
                        <asp:Label ID="lblMsg2" runat="server" CssClass="error"></asp:Label>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
    </div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="scripts">
    <script>
        var roundNumber = function (n, decimals) {
            var v = parseFloat(n);
            if (isNaN(v))
                return n;
            else
                return parseFloat(v.toFixed(decimals));
        }

        $('.load-button').on('click', function (e) {
            var btn = $(this);
            btn.hide();
            $(".update-billing").hide();
            btn.closest('div').find('.loader').show();
        });

        $('.save-button').on('click', function (e) {
            var btn = $(this);
            btn.hide();
            $('.continue-link').hide();
            btn.closest('div').find('.loader').show();

            //need to check that each room entry apportionment total is at least a much as the total room entries
            $(".save-error").html("");
            $(".room-entry-apportionment-account-table").each(function () {
                var table = $(this);
                var totalEntries = roundNumber(table.data("total-entries"), 1);
                var apportionedEntries = 0;

                $(".account-entries-text", table).each(function () {
                    apportionedEntries += parseFloat($(this).val());
                });

                if (apportionedEntries < totalEntries) {
                    btn.show();
                    $('.loader').hide();
                    $(".save-error").html($("<div/>", { "class": "alert alert-danger", "role": "alert" }).html("At least one room does not have enough entries apportioned. Total entries: " + totalEntries + ", apportioned: " + apportionedEntries));
                    e.preventDefault();
                    console.log({ "apportionedEntries": apportionedEntries, "totalEntries": totalEntries });
                    return false;
                }
            });
        });

        $(".apply-percentages").on("click", function (e) {
            e.preventDefault();
            var accounts = { "totalDays": 0, "items": [] };

            var getOrg = function (id) {
                if (!orgs[id]) orgs[id] = { "accounts": [], "totalDays": 0 };
                return orgs[id];
            }

            $(".account-day-text").each(function () {
                var txt = $(this);
                var accountId = txt.data("account-id");
                var val = parseFloat(txt.val());
                accounts.totalDays += val;
                accounts.items.push({ "accountId": accountId, "days": val });
            })

            $.each(accounts.items, function (id, acct) {
                //set the percentage of each acct

                var pct = accounts.totalDays == 0 ? 0 : acct.days / accounts.totalDays;

                //there's a table for each room, iterate these
                $(".room-entry-apportionment-account-table").each(function () {
                    var table = $(this);
                    var entries = $(".account-entries-text[data-account-id='" + acct.accountId + "']", table);
                    var roomId = table.data("room-id");
                    var totalEntries = parseFloat(table.data("total-entries"));
                    var adjustedEntries = roundNumber(totalEntries * pct, 1);

                    //there will be either one or, for remote accounts, none
                    if (entries.length > 0) {
                        entries.val(adjustedEntries);
                    } else {
                        //in this case we are dealing with a remote account which is not listed under room entry apportionment
                        //so we distribute adjustedEntries based on apportionment defaults
                        $(".account-entries-text", table).each(function () {
                            var defaultPct = parseFloat($(this).data("default-pct")) / 100;
                            var distributed = adjustedEntries * defaultPct;
                            var current = parseFloat($(this).val());
                            $(this).val(roundNumber(current + distributed, 1));
                        });
                    }
                });
            });
        });

        $('.numeric-text').numerictext({
            'integer': false
        });
    </script>
</asp:Content>
