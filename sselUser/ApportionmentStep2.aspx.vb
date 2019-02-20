﻿Imports LNF.Billing
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Repository.Billing
Imports sselUser.AppCode

Public Class ApportionmentStep2
    Inherits ApportionmentPage

    Public Overrides ReadOnly Property RoomDayReadOnly As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides Function GetBillingCheckBox() As Web.UI.WebControls.CheckBox
        Return chkBilling
    End Function

    Protected Overrides Function GetPhysicalDaysMessage() As Literal
        Return litPhysicalDays
    End Function

    Protected Overrides Function GetUserDropDownList() As DropDownList
        Return ddlUser
    End Function

    Protected Overrides Function GetRoomDropDownList() As DropDownList
        Return ddlRoom
    End Function

    Protected Overrides Function GetMonthDropDownList() As DropDownList
        Return ddlMonth
    End Function

    Protected Overrides Function GetYearDropDownList() As DropDownList
        Return ddlYear
    End Function

    Protected Overrides Function GetSaveButton() As Button
        Return btnSave
    End Function

    Protected Overrides Function GetDisplayPanel() As Panel
        Return panDisplay
    End Function

    Protected Overrides Function GetApporUnitPanel() As Panel
        Return panApporUnit
    End Function

    Protected Overrides Function GetMultiOrgRepeater() As Repeater
        Return rptMultiOrg
    End Function

    Protected Overrides Function GetMessageLabel1() As Label
        Return lblMsg1
    End Function

    Protected Overrides Function GetMessageLabel2() As Label
        Return lblMsg2
    End Function

    Protected Overrides Function GetDayRadio() As RadioButton
        Return rdoDay
    End Function

    Protected Overrides Function GetPercentageRadio() As RadioButton
        Return rdoPercentage
    End Function

    Protected Overrides Function GetReadonlyHidden() As HtmlInputHidden
        Return hidReadOnly
    End Function

    Private Function Recalculate() As Boolean
        If Not String.IsNullOrEmpty(Request.QueryString("Recalculate")) Then
            If Request.QueryString("Recalculate") = "True" Then
                Return True
            End If
        End If

        Return False
    End Function

    Protected Overrides Sub OnLoad(e As EventArgs)
        Dim navUrl As String = String.Format("~/ApportionmentStep1.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}", UserID, RoomID, Month, Year)
        hypCancel1.NavigateUrl = navUrl
        hypCancel2.NavigateUrl = navUrl

        If Not Page.IsPostBack Then
            If Recalculate() Then
                LoadBilling()
            Else
                ClearBilling()
            End If
            LoadEntriesApportionment()
        End If

        MyBase.OnLoad(e)
    End Sub

    Protected Overrides Sub OnSave(e As CommandEventArgs)
        UpdateEntriesApportionment()

        If chkBilling.Checked Then
            LoadBilling()
        Else
            ClearBilling()
        End If
    End Sub

    Private Sub LoadBilling()
        Dim dataSource = {New With { _
            .ClientID = UserID, _
            .StartDate = Period.ToString("yyyy-MM-dd"), _
            .EndDate = Period.AddMonths(1).ToString("yyyy-MM-dd")}}

        rptBilling.Visible = True
        rptBilling.DataSource = dataSource
        rptBilling.DataBind()
    End Sub

    Private Sub ClearBilling()
        rptBilling.DataSource = Nothing
        rptBilling.DataBind()
    End Sub

    Private Sub LoadEntriesApportionment()
        If RoomID = 0 Then Return

        Dim rooms As List(Of Room) = New List(Of Room)()

        'check to see if this room should have entries apportioned
        Dim r As Room = DA.Current.Query(Of Room)().First(Function(x) x.RoomID = RoomID)
        If r.ApportionEntryFee Then
            rooms.Add(r)
        End If

        'check to see if this room has children rooms that should have entries appportioned
        Dim children As IEnumerable(Of Room) = DA.Current.Query(Of Room).Where(Function(x) x.ParentID.HasValue AndAlso x.ParentID.Value = RoomID AndAlso x.ApportionEntryFee)

        rooms.AddRange(children)

        rptRoomEntries.DataSource = rooms.Select(Function(x) RoomEntryApportionmentItem.Create(Period, UserID, x))
        rptRoomEntries.DataBind()
    End Sub

    Private Sub UpdateEntriesApportionment()
        'the outer repeater is for each room
        'the inner repeater is for each account

        ClearMessage()

        For Each roomItem As RepeaterItem In rptRoomEntries.Items
            If roomItem.ItemType = ListItemType.Item OrElse roomItem.ItemType = ListItemType.AlternatingItem Then
                Dim rptAccountRoomEntries As Repeater = CType(roomItem.FindControl("rptAccountRoomEntries"), Repeater)
                Dim hidRoomID As HtmlInputHidden = CType(roomItem.FindControl("hidRoomID"), HtmlInputHidden)
                Dim roomId As Integer = Integer.Parse(hidRoomID.Value)
                For Each acctItem As RepeaterItem In rptAccountRoomEntries.Items
                    If acctItem.ItemType = ListItemType.Item OrElse acctItem.ItemType = ListItemType.AlternatingItem Then
                        Dim hidAccountID As HtmlInputHidden = CType(acctItem.FindControl("hidAccountID"), HtmlInputHidden)
                        Dim txtAccountEntries As TextBox = CType(acctItem.FindControl("txtAccountEntries"), TextBox)
                        Dim accountId As Integer = Integer.Parse(hidAccountID.Value)
                        Dim entries As Decimal = Decimal.Parse(txtAccountEntries.Text)
                        ApportionmentManager.UpdateRoomBillingEntries(Period, UserID, roomId, accountId, entries)
                    End If
                Next
            End If
        Next

        SetMessage("Save successful, your room apportionment for this period is complete.")
    End Sub

    Protected Sub RptRoomEntries_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim item As RoomEntryApportionmentItem = CType(e.Item.DataItem, RoomEntryApportionmentItem)
            Dim rptAccountRoomEntries As Repeater = CType(e.Item.FindControl("rptAccountRoomEntries"), Repeater)
            rptAccountRoomEntries.DataSource = GetRoomEntryApportionmentAccountItems(item)
            rptAccountRoomEntries.DataBind()
        End If
    End Sub

    Public Function GetRoomEntryApportionmentAccountItems(r As RoomEntryApportionmentItem) As IEnumerable(Of RoomEntryApportionmentAccount)
        Dim roomBilling As IEnumerable(Of RoomBilling) = DA.Current.Query(Of RoomBilling).Where(Function(x) x.Period = r.Period AndAlso x.ClientID = r.ClientID AndAlso x.RoomID = r.RoomID)
        Return roomBilling.Select(Function(x) RoomEntryApportionmentAccount.Create(r, x.AccountID)).OrderBy(Function(x) x.OrgID).ThenBy(Function(x) x.AccountID)
    End Function

    Protected Sub RptAccountRoomEntries_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim item As RoomEntryApportionmentAccount = CType(e.Item.DataItem, RoomEntryApportionmentAccount)
            Dim txtAccountEntries As TextBox = CType(e.Item.FindControl("txtAccountEntries"), TextBox)
            txtAccountEntries.Attributes.Add("data-org-id", item.OrgID.ToString())
            txtAccountEntries.Attributes.Add("data-room-id", item.RoomID.ToString())
            txtAccountEntries.Attributes.Add("data-account-id", item.AccountID.ToString())
            txtAccountEntries.Attributes.Add("data-default-pct", item.DefaultPercentage.ToString())
        End If
    End Sub


End Class