Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports LNF
Imports LNF.Billing.Apportionment
Imports LNF.Billing.Apportionment.Models
Imports LNF.Data
Imports sselUser.AppCode.DAL

Public MustInherit Class ApportionmentPage
    Inherits UserPage

    Protected OrgCount As Integer = 0

    Public MustOverride ReadOnly Property RoomDayReadOnly As Boolean

    Public ReadOnly Property UserID As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("UserID")) Then
                Integer.TryParse(Request.QueryString("UserID"), result)
            End If
            Return result
        End Get
    End Property

    Public ReadOnly Property RoomID As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("RoomID")) Then
                Integer.TryParse(Request.QueryString("RoomID"), result)
            End If
            Return result
        End Get
    End Property

    Public ReadOnly Property Month As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("Month")) Then
                Integer.TryParse(Request.QueryString("Month"), result)
            End If
            Return result
        End Get
    End Property

    Public ReadOnly Property Year As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("Year")) Then
                Integer.TryParse(Request.QueryString("Year"), result)
            End If
            Return result
        End Get
    End Property

    Public ReadOnly Property Period As Date
        Get
            If Year = 0 OrElse Month = 0 Then
                Return Date.MinValue
            Else
                Return New Date(Year, Month, 1)
            End If
        End Get
    End Property

    Protected MustOverride Function GetSaveButton() As Button
    Protected MustOverride Function GetBillingCheckBox() As CheckBox
    Protected MustOverride Function GetUserDropDownList() As DropDownList
    Protected MustOverride Function GetRoomDropDownList() As DropDownList
    Protected MustOverride Function GetMonthDropDownList() As DropDownList
    Protected MustOverride Function GetYearDropDownList() As DropDownList
    Protected MustOverride Function GetPhysicalDaysMessage() As Literal
    Protected MustOverride Function GetDisplayPanel() As Panel
    Protected MustOverride Function GetApporUnitPanel() As Panel
    Protected MustOverride Function GetMultiOrgRepeater() As Repeater
    Protected MustOverride Function GetMessageLabel1() As Label
    Protected MustOverride Function GetMessageLabel2() As Label
    Protected MustOverride Function GetDayRadio() As RadioButton
    Protected MustOverride Function GetPercentageRadio() As RadioButton
    Protected MustOverride Function GetReadonlyHidden() As HtmlInputHidden
    Protected MustOverride Sub OnSave(e As CommandEventArgs)

    Protected litLastBillingUpdate As Literal
    Protected phLastBillingUpdate As PlaceHolder
    Protected chkUpdateBilling As CheckBox

    Protected MustOverride Sub SetGetDataVisible(visible As Boolean)

    Protected Sub SetMessage(text As String, Optional alertType As String = "danger")
        SetMessage1(text, alertType)
        SetMessage2(text, alertType)
    End Sub

    Protected Sub SetMessage1(text As String, alertType As String)
        Dim lbl As Label = GetMessageLabel1()
        lbl.Text = $"<div class=""alert alert-{alertType}"" role=""alert"">{text}</div>"
        lbl.Visible = True
    End Sub

    Protected Sub SetMessage2(text As String, alertType As String)
        Dim lbl As Label = GetMessageLabel2()
        lbl.Text = $"<div class=""alert alert-{alertType}"" role=""alert"">{text}</div>"
        lbl.Visible = True
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        Dim btnSave As Button = GetSaveButton()
        Dim chkBilling As CheckBox = GetBillingCheckBox()
        Dim ddlMonth As DropDownList = GetMonthDropDownList()
        Dim ddlYear As DropDownList = GetYearDropDownList()
        Dim panDisplay As Panel = GetDisplayPanel()
        Dim lblMsg1 As Label = GetMessageLabel1()
        Dim lblMsg2 As Label = GetMessageLabel2()

        lblMsg1.Text = String.Empty
        lblMsg1.Visible = False
        lblMsg2.Text = String.Empty
        lblMsg2.Visible = False
        panDisplay.Visible = False

        '--------------------------------------------------------------------------------------------------------------------------------------------
        'No matter what we have to recreate the gridview(s). By now the ViewState has been loaded so we can generate the grids with data saved in
        'the ViewState, however don't overwrite any TextBox values or else we lose user input. Buried in Org_DataBound is a check for IsPostBack
        'to see if each dynamic TextBox needs to have a value assigned. After a postback the value entered by the user will be used automatically
        'but the TextBox still must be created first.
        '--------------------------------------------------------------------------------------------------------------------------------------------
        'This is a very helpful article regarding PostBack, ViewState and dynamic controls: http://msdn.microsoft.com/en-us/library/ms972976.aspx
        '--------------------------------------------------------------------------------------------------------------------------------------------
        LoadMultiOrgRepeater()

        If Not Page.IsPostBack Then
            '---------- First Load ----------

            'Load the filter controls and set selected values
            SetupFilter()

            Dim lastMonth As New Date(Date.Now.AddMonths(-1).Year, Date.Now.AddMonths(-1).Month, 1)
            Dim withinBusinessDayCutoff As Boolean = UserUtility.IsWithinBusinessDays(Date.Now.Date) And Period = lastMonth
            Dim isAdmin As Boolean = CurrentUser.HasPriv(ClientPrivilege.Administrator)

            ' hides the Get Data button unless within the business day cutoff or current user is administrator
            SetGetDataVisible(withinBusinessDayCutoff OrElse isAdmin)

            If RoomID > 0 And UserID > 0 Then
                'To get here the user must have clicked the "Get Data" button. This means that when LoadMultiOrgRepeater
                'was called something interesting to the user was generated so we should show it.

                'Check if selected date is in the past
                If withinBusinessDayCutoff Then
                    GetSaveButton().Enabled = True
                Else
                    If isAdmin Then
                        btnSave.Enabled = True
                        chkBilling.Visible = True
                    End If
                End If

                'Apparently the AllowSave setting trumps the logic above
                If Boolean.Parse(ConfigurationManager.AppSettings("AllowSave")) Then
                    btnSave.Enabled = True
                    chkBilling.Visible = True
                End If
            Else
                'Default to previous month
                ddlMonth.SelectedValue = Date.Now.AddMonths(-1).Month.ToString()
                ddlYear.SelectedValue = Date.Now.AddMonths(-1).Year.ToString()
            End If

            'If UserID = 0 then it's the first load (no QueryString param). It will only be -1 if
            'the DropDownList was previously loaded and no selection was made.
            If UserID = -1 Then
                SetMessage1("Please select a user.", "danger")
            End If

            GetReadonlyHidden().Value = If(RoomDayReadOnly, "1", "0")

            GetLastBillingUpdateText()
        End If

        MyBase.OnLoad(e)
    End Sub

    Protected Sub SetLastBillingUpdateText(model As Web.User.Models.ApportionmentModel)
        Dim errmsg As String = String.Empty

        If model.Errors.Count() > 0 Then
            errmsg = String.Join(Environment.NewLine + Environment.NewLine, model.Errors)
        End If

        Dim text As String = String.Format("Billing updated in {0:0.0} seconds", model.TimeTaken.TotalSeconds)

        If Not String.IsNullOrEmpty(errmsg) Then
            text += String.Format("<hr/><div style=""color: #aa0000; font-family: 'Courier New'; white-space: pre;"">{0}</div><hr/>", errmsg)
        End If

        text += Environment.NewLine + Environment.NewLine + "<!--" + Environment.NewLine
        text += "DataCleanResult:" + Environment.NewLine
        text += model.DataCleanResult.LogText + Environment.NewLine + Environment.NewLine
        text += "DataResult:" + Environment.NewLine
        text += model.DataResult.LogText + Environment.NewLine + Environment.NewLine
        text += "Step1Result:" + Environment.NewLine
        text += model.Step1Result.LogText + Environment.NewLine + Environment.NewLine
        If model.PopulateSubsidyBillingResult IsNot Nothing Then
            text += "PopulateSubsidyBillingResult:" + Environment.NewLine
            text += model.PopulateSubsidyBillingResult.LogText + Environment.NewLine + Environment.NewLine
        End If
        text += "-->" + Environment.NewLine + Environment.NewLine

        Session("LastBillingUpdateText") = text
    End Sub

    Protected Sub GetLastBillingUpdateText()
        If litLastBillingUpdate IsNot Nothing AndAlso litLastBillingUpdate IsNot Nothing Then
            Dim text As String = String.Empty

            If Session("LastBillingUpdateText") IsNot Nothing Then
                text = Session("LastBillingUpdateText").ToString()
                Session.Remove("LastBillingUpdateText")
            End If

            litLastBillingUpdate.Text = text
            phLastBillingUpdate.Visible = Not String.IsNullOrEmpty(text)
        End If
    End Sub

    Protected Function UpdateBilling() As Boolean
        Dim result As Boolean = False
        If chkUpdateBilling IsNot Nothing Then
            result = chkUpdateBilling.Checked
        End If
        Return result
    End Function

    ''' <summary>
    ''' The ActiveAccounts table must be stored in Session because it's required to re-generate the grid
    ''' dynamically on every postback. The dilemma here is we won't be able to get the current user selected
    ''' in drop down list because it's not bounded yet.  So keep the table in session is the best strategy.
    ''' </summary>
    Protected Overridable Sub CreateCurrentActiveAccountsTable()
        If Session("CurrentAcct") Is Nothing OrElse Session("MultipleOrg") Is Nothing OrElse Request.QueryString("Reload") = "1" Then
            If Period = Date.MinValue Then
                Throw New Exception("Invalid period.")
            End If

            If UserID = 0 Then
                Throw New Exception("Invalid UserID.")
            End If

            If RoomID = 0 Then
                Throw New Exception("Invalid RoomID.")
            End If

            Dim ds As DataSet = RoomApportionmentInDaysMonthlyDA.GetAllAccountsUsed(Period, UserID, RoomID)
            Session("CurrentAcct") = ds.Tables(0)
            Session("MultipleOrg") = ds.Tables(1)
        End If
    End Sub

    Protected Overridable Sub ConstructGrid(gv As GridView, orgId As Integer)
        Dim dtCurrentAcct As DataTable = CType(Session("CurrentAcct"), DataTable)
        Dim dtMultipleOrg As DataTable = CType(Session("MultipleOrg"), DataTable)

        Dim tf As TemplateField
        Dim lbl As New Label()
        Dim hid As New HiddenField()
        tf = New TemplateField() With {
            .HeaderText = If(OrgCount.Equals(1), "Total Days", "Minimum Days")
        }
        tf.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
        tf.ItemTemplate = New DynamicLabelTemplate("lblMinDays", String.Empty, "hidOrgID", orgId.ToString())
        gv.Columns.Add(tf)

        Dim rows As DataRow() = dtCurrentAcct.Select(String.Format("OrgID = {0}", orgId))

        Dim textBoxIds As New List(Of String)

        'for each active account, we also create a column and create a control into that column
        Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
            Dim repo As New Repository(conn)
            For Each dr As DataRow In rows
                tf = New TemplateField()

                Dim acctId As Integer = dr.Field(Of Integer)("AccountID")
                Dim acct As ApportionmentAccount = repo.GetApportionmentAccount(acctId)
                If String.IsNullOrEmpty(acct.ShortCode.Trim()) Then
                    tf.HeaderText = dr.Field(Of String)("Name")
                Else
                    tf.HeaderText = String.Format("{0} ({1})", dr("Name"), acct.ShortCode)
                End If

                tf.HeaderStyle.Width = Unit.Pixel(100)
                tf.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
                Dim txtBoxId As String = $"txt{dr("AccountID")}"
                If textBoxIds.Contains(txtBoxId) Then
                    Throw New Exception($"A control with ID {txtBoxId} has already been added.")
                End If
                Dim template As New DynamicTextBoxTemplate(txtBoxId, Unit.Pixel(80), 5, "numeric-text account-day-text form-control")
                textBoxIds.Add(txtBoxId)
                template.AddAttribute("data-account-id", dr("AccountID").ToString())
                template.AddAttribute("data-org-id", dr("OrgID").ToString())
                tf.ItemTemplate = template
                gv.Columns.Add(tf)
            Next
        End Using
    End Sub

    Protected Overridable Sub LoadMultiOrgRepeater()
        Dim panDisplay As Panel = GetDisplayPanel()
        Dim panApporUnit As Panel = GetApporUnitPanel()
        Dim rptMultiOrg As Repeater = GetMultiOrgRepeater()

        If Period = Date.MinValue Then
            'This happens when the page is reloaded using the left-side nav menu link after data has been loaded.
            'In this case Session("MultipleOrg") will not be null but Period will be unset. This causes dtMultipleOrg to
            'have a value which means MultiOrg_ItemDataBound and Org_DataBound will be called and errors will occur
            'because Period is an invalid date.
            Return
        End If

        CreateCurrentActiveAccountsTable()
        Dim dtMultipleOrg As DataTable = CType(Session("MultipleOrg"), DataTable)

        'dtMultipleOrg may be Nothing here if there are no QueryString params yet. This is not a
        'problem - nothing gets loaded and MultiOrg_ItemDataBound and Org_DataBound methods never get called.

        If dtMultipleOrg IsNot Nothing Then

            OrgCount = dtMultipleOrg.Rows.Count

            'If Rows.Count = 0 then this client does not have multiple accounts so no apportionment is needed
            If OrgCount = 0 Then
                SetMessage1("You only have one account or did not use that room, no apportionment is needed.", "warning")
                panDisplay.Visible = False
            Else
                ClearMessage()
                panDisplay.Visible = True

                ' [2021-09-16 jg] Only allow apportionment by day, because we are now allowing greater than 100% apportionment per org (see ticket #567297).
                Dim allowPercentageApportionment As Boolean = Boolean.Parse(ConfigurationManager.AppSettings("AllowPercentageApportionment"))
                If allowPercentageApportionment Then
                    panApporUnit.Visible = OrgCount.Equals(1)
                Else
                    panApporUnit.Visible = False
                End If
            End If
        End If

        rptMultiOrg.DataSource = dtMultipleOrg
        rptMultiOrg.DataBind()
    End Sub

    Protected Overridable Sub SetupFilter()
        Dim ddlUser As DropDownList = GetUserDropDownList()
        Dim ddlRoom As DropDownList = GetRoomDropDownList()
        Dim ddlMonth As DropDownList = GetMonthDropDownList()
        Dim ddlYear As DropDownList = GetYearDropDownList()

        'populate the user dropdown list
        Dim privs As Integer = Convert.ToInt32(ClientPrivilege.LabUser Or ClientPrivilege.Staff)
        Dim sDate As Date = Date.Now

        ddlYear.DataSource = UserUtility.YearsData(3)
        ddlYear.DataBind()

        If CurrentUser.HasPriv(ClientPrivilege.Administrator Or ClientPrivilege.Executive) Then
            If CurrentUser.HasPriv(ClientPrivilege.Administrator) Then
                ddlUser.DataSource = ClientManagerUtility.GetAllClientsByDateAndPrivs(sDate, sDate.AddMonths(1), privs)
            ElseIf CurrentUser.HasPriv(ClientPrivilege.Executive) Then
                ddlUser.DataSource = ClientManagerUtility.GetClientsByManagerID(sDate, sDate.AddMonths(1), CurrentUser.ClientID)
            End If

            ddlUser.DataBind()

            ddlUser.Items.Insert(0, New ListItem("-- Select ---", "-1"))
        Else
            ddlUser.Items.Add(New ListItem(CurrentUser.DisplayName, CurrentUser.ClientID.ToString()))
            ddlUser.SelectedIndex = 0 ' only has himself
        End If

        Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
            Dim repo As New Repository(conn)
            ddlRoom.DataSource = repo.GetActiveApportionmentRooms().Where(Function(x) x.Billable AndAlso x.ApportionDailyFee).OrderBy(Function(x) x.RoomName).Select(Function(x) New With {x.RoomID, .RoomName = UserUtility.GetRoomName(x)})
            ddlRoom.DataBind()
        End Using

        '2009-07-14
        'The code below would see if user comes from a dropdownlist selection or complete new entry from other pages.
        'UserID querystring is set in GetData click function, it's redirected from there
        If UserID > 0 Then
            ddlUser.SelectedValue = UserID.ToString()
        End If

        If RoomID > 0 Then
            ddlRoom.SelectedValue = RoomID.ToString()
        End If

        If Month > 0 Then
            ddlMonth.SelectedValue = Month.ToString()
        End If

        If Year > 0 Then
            ddlYear.SelectedValue = Year.ToString()
        End If
    End Sub

    Protected Function GetEntriesValue(obj As Object) As String
        Dim item As RoomEntryApportionmentAccount = CType(obj, RoomEntryApportionmentAccount)
        Return Math.Round(item.Entries, 3, MidpointRounding.AwayFromZero).ToString("#0.000")
    End Function

    Protected Function GetTotalEntriesValue(obj As Object) As String
        Dim item As RoomEntryApportionment = CType(obj, RoomEntryApportionment)
        Return Math.Round(item.TotalEntries, 3, MidpointRounding.AwayFromZero).ToString()
    End Function

    ''' <summary>
    ''' This method is used for mutliple org grid databound - we want to save a database trip to the server, so data is saved in Session 
    ''' </summary>
    Protected Overridable Function GetLabUsageDaysData(repo As Repository) As DataSet
        If Session("LabUsageDay") Is Nothing Then
            Session("LabUsageDay") = repo.GetDataForApportion(Period, UserID, RoomID)
        End If
        Return CType(Session("LabUsageDay"), DataSet)
    End Function

    Protected Overridable Sub LoadOrgGridView(ByVal gvOrg As GridView)
        Dim litPhysicalDays As Literal = GetPhysicalDaysMessage()
        Dim panDisplay As Panel = GetDisplayPanel()

        'Try
        'This is real data assignment, the data bounded object is just a dummy object. Here we get what users truly want to see
        'Get data from Apportionment table to get the true chargedays values (we always assume the apportionment table has the accurate data)

        Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
            Dim repo As New Repository(conn)
            Dim dsApp As DataSet = GetLabUsageDaysData(repo)
            Dim dtApp As DataTable = dsApp.Tables(0) ' this is every column from RoomApportionmentInDaysMonthly for the current Period, ClientID, and RoomID
            Dim dtPhysicalDates As DataTable = dsApp.Tables(1) 'it contains all the dates in a month where we should charge room fee

            'Loop through each row to populate the TextBox.
            'There is never more than 1 row in the GridView. (because each account is an additional column)

            Dim physicalDays As Integer = repo.GetPhysicalDays(Period, UserID, RoomID)
            Dim minimumDays As Integer = 0

            If gvOrg.Rows.Count > 0 Then

                Dim lblMinDays As Label = CType(gvOrg.Rows(0).FindControl("lblMinDays"), Label)
                Dim hidOrgID As HiddenField = CType(gvOrg.Rows(0).FindControl("hidOrgID"), HiddenField)
                Dim orgId As Integer = Integer.Parse(hidOrgID.Value)

                ' Minimum days is the number of tool usage days per Org
                minimumDays = repo.GetMinimumDays(Period, UserID, RoomID, orgId)

                For Each dr As DataRow In dtApp.Select(String.Format("OrgID = {0}", orgId))
                    Dim txt As TextBox = CType(gvOrg.Rows(0).FindControl("txt" + dr("AccountID").ToString()), TextBox)
                    If txt IsNot Nothing Then
                        If Not Page.IsPostBack OrElse RoomDayReadOnly Then
                            txt.Text = Math.Round(dr.Field(Of Decimal)("ChargeDays"), 3, MidpointRounding.AwayFromZero).ToString("#0.000")
                        End If
                        If RoomDayReadOnly Then
                            txt.ReadOnly = True
                            txt.Enabled = False
                        End If
                    End If
                Next

                Dim dtMultipleOrg As DataTable = CType(Session("MultipleOrg"), DataTable)
                If dtMultipleOrg.Rows.Count = 1 OrElse physicalDays < minimumDays Then 'minimum days = sum of account days (per org)
                    lblMinDays.Text = physicalDays.ToString()
                Else
                    lblMinDays.Text = minimumDays.ToString()
                End If
            End If

            litPhysicalDays.Text = physicalDays.ToString()
        End Using
        'Catch ex As Exception
        '    SetMessage1("<div style=""border-bottom: solid 1px #D2D2D2; padding-bottom: 5px;"">An error has occurred:</div><pre>" + ex.Message + Environment.NewLine + ex.StackTrace + "</pre><div style=""border-top: solid 1px #D2D2D2; padding-top: 5px;"">Please contact the system administrator.</div>")
        '    panDisplay.Visible = False
        'End Try
    End Sub

    Protected Overridable Sub OnGetData(e As EventArgs)
        Dim ddlUser As DropDownList = GetUserDropDownList()
        Dim ddlRoom As DropDownList = GetRoomDropDownList()
        Dim ddlMonth As DropDownList = GetMonthDropDownList()
        Dim ddlYear As DropDownList = GetYearDropDownList()

        Session("CurrentAcct") = Nothing
        Session("MultipleOrg") = Nothing
        Session("LabUsageDay") = Nothing

        '2009-07-04
        'I must use redirect as if we come to the page for the first time whenever user wants to get the data
        'The reason is due to a bug in asp.net that the dynamic grid view would lose its template control after databind for the second time.
        'So it means user can change an individual's data for one time, and if choose a second person and tries to modify the data, an error would occur because the grid view lost the template controls
        'Thus, the solution is to treat every button click as a complete new entry into this form instead of a postback.

        '2011-06-03
        'This isn't true, you just have to recreate the dynamic controls before the PostBack data is loaded.
        'I'll leave it the way it is since it works and allows params to be passed thourgh the QueryString.

        'This is the only time the variables are read from the form input elements. All the processing uses QueryString params
        Dim selectedUserId As Integer = Convert.ToInt32(ddlUser.SelectedValue)
        Dim selectedRoomId As Integer = Convert.ToInt32(ddlRoom.SelectedValue)
        Dim selectedMonth As Integer = Convert.ToInt32(ddlMonth.SelectedValue)
        Dim selectedYear As Integer = Convert.ToInt32(ddlYear.SelectedValue)

        If selectedUserId > 0 AndAlso UpdateBilling() Then
            Dim p As New Date(selectedYear, selectedMonth, 1)
            Session.Remove("UpdateBilling")
            Dim model As New Web.User.Models.ApportionmentModel() With {.Period = p, .ClientID = selectedUserId}
            model.UpdateBillingData()
            SetLastBillingUpdateText(model)
        End If

        Response.Redirect(String.Format("~/ApportionmentStep1.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}", selectedUserId, selectedRoomId, selectedMonth, selectedYear), False)
    End Sub

    Protected Sub GetData_Click(sender As Object, e As EventArgs)
        OnGetData(e)
    End Sub

    Protected Sub MultiOrg_ItemDataBound(ByVal sender As Object, ByVal e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim gvOrg As GridView = CType(e.Item.FindControl("gvOrg"), GridView)
            Dim hid As HtmlInputHidden = CType(e.Item.FindControl("hidOrgID"), HtmlInputHidden)
            Dim orgId As Integer = Convert.ToInt32(hid.Value)

            'add a column for each account to the GridView
            ConstructGrid(gvOrg, orgId)

            Dim dt As New DataTable() 'this table is a pivot table of dtDefault.  We have to create this table because we must bind something to the gvAppDefault in order to show something.
            dt.Columns.Add("Nothing", GetType(Double))
            dt.Rows.Add(1.1)

            gvOrg.DataSource = dt
            gvOrg.DataBind()
        End If
    End Sub

    Protected Sub Org_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        LoadOrgGridView(CType(sender, GridView))
    End Sub

    'Convert values from days to percentages
    Protected Sub RdoDay_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)

        GetDisplayPanel().Visible = True

        Dim physicalDays As Double = UserUtility.ConvertToDouble(GetPhysicalDaysMessage().Text, 0)

        For Each item As RepeaterItem In GetMultiOrgRepeater().Items
            Dim gv As GridView = CType(item.FindControl("gvOrg"), GridView)
            Dim gvr As GridViewRow = gv.Rows(0)
            Dim lbl As Label = CType(gvr.FindControl("lblMinDays"), Label)

            If GetPercentageRadio().Checked Then
                For Each cell As TableCell In gvr.Cells
                    Dim txt As TextBox = Nothing
                    For Each ctrl As Control In cell.Controls
                        If ctrl.GetType().Equals(GetType(TextBox)) Then
                            txt = CType(ctrl, TextBox)
                            Exit For
                        End If
                    Next
                    If txt IsNot Nothing Then
                        Dim days As Double = UserUtility.ConvertToDouble(txt.Text, 0)
                        Dim pct As Double = (days / physicalDays) * 100
                        txt.Text = pct.ToString("#0")
                        Dim l As New Label() With {
                            .Text = "%"
                        }
                        cell.Controls.Add(l)
                    End If
                Next
            Else
                For Each cell As TableCell In gvr.Cells
                    Dim txt As TextBox = Nothing
                    For Each ctrl As Control In cell.Controls
                        If ctrl.GetType().Equals(GetType(TextBox)) Then
                            txt = CType(ctrl, TextBox)
                            Exit For
                        End If
                    Next
                    If txt IsNot Nothing Then
                        Dim pct As Double = UserUtility.ConvertToDouble(txt.Text, 0)
                        Dim days As Double = (pct * physicalDays) / 100
                        txt.Text = days.ToString("#0")
                    End If
                Next
            End If
        Next
    End Sub

    Protected Sub ClearMessage()
        Dim label As Label

        label = GetMessageLabel1()
        label.Text = String.Empty
        label.Visible = False

        label = GetMessageLabel2()
        label.Text = String.Empty
        label.Visible = False
    End Sub

    Protected Sub Save_Command(ByVal sender As Object, ByVal e As CommandEventArgs)
        OnSave(e)
    End Sub
End Class
