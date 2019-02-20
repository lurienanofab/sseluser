<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/UserMaster.Master" CodeBehind="ApportionmentOld.aspx.vb" Inherits="sselUser.ApportionmentOld" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $('.numeric-text').numerictext({
                'integer': false
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 10px;">
        <h2>Time-in-Lab Apportionment</h2>
        <div style="padding: 10px;">
            <asp:Label ID="Label3" runat="server" Width="144px" CssClass="NormalText">Select User:</asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:DropDownList ID="ddlUser" runat="server" Height="24px" Width="249px" DataTextField="DisplayName" DataValueField="ClientID" AutoPostBack="True">
            </asp:DropDownList>
            <br />
            <br />
            <asp:Label ID="Label5" runat="server" Height="24px" Width="144px" CssClass="NormalText">Select Time:</asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:DropDownList ID="ddlYear" runat="server" Height="24px" Width="72px" AutoPostBack="True">
                <asp:ListItem Value="2003">2003</asp:ListItem>
                <asp:ListItem Value="2004">2004</asp:ListItem>
                <asp:ListItem Value="2005">2005</asp:ListItem>
                <asp:ListItem Value="2006">2006</asp:ListItem>
                <asp:ListItem Value="2007">2007</asp:ListItem>
                <asp:ListItem Value="2008">2008</asp:ListItem>
                <asp:ListItem Value="2009">2009</asp:ListItem>
                <asp:ListItem Value="2010">2010</asp:ListItem>
                <asp:ListItem Value="2011">2011</asp:ListItem>
                <asp:ListItem Value="2012">2012</asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlMonth" runat="server" Height="24px" Width="168px" AutoPostBack="True">
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
            <br />
            <br />
            <asp:Label ID="Label2" runat="server" Width="144px" CssClass="NormalText">Select Room:</asp:Label>&nbsp;&nbsp;
            <asp:DropDownList ID="ddlRoom" DataTextField="Room" DataValueField="RoomID" runat="server" AutoPostBack="True">
            </asp:DropDownList>
        </div>
        <br />
        <div style="padding-left: 10px;">
            <asp:Button ID="butSave" runat="server" CausesValidation="False" Text="Save"></asp:Button>
            <asp:Button ID="butDiscard" runat="server" CausesValidation="False" Text="Return to Main Page" Visible="false"></asp:Button>
        </div>
        <br />
        <asp:Label ID="lblMsg" runat="server" CssClass="WarningText" Visible="False">Label</asp:Label>
        <br />
        <div>
            <asp:GridView ID="gvNAP" runat="server" AutoGenerateColumns="false" Visible="false">
                <EmptyDataTemplate>
                    &nbsp;&nbsp;-- There is no record of entry for this room in this month --&nbsp;&nbsp;
                </EmptyDataTemplate>
            </asp:GridView>
            <asp:DataGrid ID="dgApport" runat="server" AutoGenerateColumns="false">
                <AlternatingItemStyle BackColor="LightCyan"></AlternatingItemStyle>
                <HeaderStyle Font-Bold="True" BackColor="#CCCC99"></HeaderStyle>
                <Columns>
                    <asp:TemplateColumn>
                        <HeaderTemplate>
                            <asp:Label ID="label34" runat="server">Date</asp:Label>
                        </HeaderTemplate>
                        <HeaderStyle Width="150px"></HeaderStyle>
                        <ItemTemplate>
                            <asp:Label ID="lblEvtDate" runat="server"></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="150px"></ItemStyle>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn>
                        <HeaderTemplate>
                            <asp:Label runat="server" ID="Label4">Duration</asp:Label>
                        </HeaderTemplate>
                        <HeaderStyle Width="70px" HorizontalAlign="Center"></HeaderStyle>
                        <ItemTemplate>
                            <asp:Label ID="lblDuration" runat="server"></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="70px" HorizontalAlign="Center"></ItemStyle>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn Visible="False">
                        <HeaderTemplate>
                            <asp:Label runat="server" ID="Label1">Entries</asp:Label>
                        </HeaderTemplate>
                        <HeaderStyle Width="70px" HorizontalAlign="Center"></HeaderStyle>
                        <ItemTemplate>
                            <asp:Label ID="Label6" runat="server"></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="70px" HorizontalAlign="Center"></ItemStyle>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn>
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                            <asp:RadioButtonList ID="rblApportType" runat="server" TextAlign="Left" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="ChangeApportType">
                                <asp:ListItem>Percentage</asp:ListItem>
                                <asp:ListItem>Hours</asp:ListItem>
                            </asp:RadioButtonList>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
        <asp:GridView ID="gv1" Visible="false" runat="server"></asp:GridView>
        <asp:GridView ID="gv2" Visible="false" runat="server"></asp:GridView>
        <asp:GridView ID="gv3" Visible="false" runat="server"></asp:GridView>
        <asp:GridView ID="gv4" Visible="false" runat="server"></asp:GridView>
    </div>
</asp:Content>
