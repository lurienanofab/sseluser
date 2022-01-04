Imports System.Data.SqlClient
Imports LNF
Imports LNF.Billing.Apportionment.Models
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Repository
Imports LNF.Web
Imports LNF.Web.User
Imports sselUser.AppCode
Imports sselUser.AppCode.DAL

Public Class ApportionmentOld
    Inherits Page

    Private Const shiftCols As Integer = 3 ' columns in dg to left of first column of boxes

    Private _contextBase As HttpContextBase

    Private dsReport As DataSet
    Private sDate, eDate As Date

    Private ReadOnly cnSselData As New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)

    Public AuthTypes As ClientPrivilege = ClientPrivilege.LabUser Or ClientPrivilege.Staff Or ClientPrivilege.Executive Or ClientPrivilege.Administrator

    Enum BillingType
        IntGa = 1
        IntSi = 2
        IntHour = 3
        IntTools = 4
        ExtAcGa = 5
        ExtAcSi = 6
        ExtAcTools = 7
        ExtAcHour = 8
        NonAc = 9
        NonAcTools = 10
        NonAcHour = 11
        Other = 99
    End Enum

    Protected ReadOnly Property ContextBase As HttpContextBase
        Get
            Return _contextBase
        End Get
    End Property

    Protected ReadOnly Property CurrentUser As IPrivileged
        Get
            Return ContextBase.CurrentUser()
        End Get
    End Property

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        _contextBase = New HttpContextWrapper(Context)

        '2009-06-23 Get the billing type and see if this user shoud use the new system or old system
        Dim dt As DataTable = BillingTypeDA.GetBillingTypeID(CType(Session("ClientID"), Integer))

        For Each row As DataRow In dt.Rows
            Dim BillingTypeID As BillingType = CType(row("BillingTypeID"), BillingType)

            'If Not BillingTypeID = BillingType.IntGa And Not BillingTypeID = BillingType.IntSi And Not BillingTypeID = BillingType.ExtAcGa And Not BillingTypeID = BillingType.ExtAcSi Then
            '	Response.Redirect("Apportionment_old.aspx")
            'End If
        Next

        'dsReport = CType(Cache.Get(Session("Cache")), DataSet)
        dsReport = CType(Session("dsReport"), DataSet)
        If (Session("ActiveTable") IsNot Nothing) Then
            If (dsReport Is Nothing) Then
                Response.Redirect("~/")
            End If
            ShowApportGrid(False)
            ConstructNAPGridView()
        End If
    End Sub

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not CurrentUser.HasPriv(AuthTypes) Then
            Session.Abandon()
            Response.Redirect(Session("Logout").ToString() + "?Action=Exit")
        End If

        If Not (IsPostBack) Then
            Session("AllowEdit") = True
            Session("ActiveTable") = Nothing

            ' cannot apport time if you are in a lab
            ' this does not apply to apporting other - but admins and execs are rarely in the lab anyway
            'Dim cmdIsInLab As New SqlCommand("Nexwatch_Select", cnSselData)
            'cmdIsInLab.CommandType = CommandType.StoredProcedure
            'cmdIsInLab.Parameters.AddWithValue("@Action", "IsInLab")
            'cmdIsInLab.Parameters.AddWithValue("@ClientID", Session("ClientID"))
            'cnSselData.Open()
            'Session("InLab") = cmdIsInLab.ExecuteScalar()
            'cnSselData.Close()

            dsReport = New DataSet
            Session("dsReport") = dsReport
            'Cache.Insert(Session("Cache"), dsReport, Nothing, DateTime.MaxValue, TimeSpan.FromMinutes(20))

            ' default to current month
            ddlMonth.SelectedValue = DateTime.Now.Month.ToString()
            ddlYear.SelectedValue = DateTime.Now.Year.ToString()

            sDate = New DateTime(Integer.Parse(ddlYear.SelectedValue), Integer.Parse(ddlMonth.SelectedValue), 1)
            eDate = sDate.AddMonths(1)
            UpdateTimeDepData()

            ' get rooms
            Dim cmdRoom As New SqlCommand("Room_Select", cnSselData) With {
                .CommandType = CommandType.StoredProcedure
            }
            cmdRoom.Parameters.AddWithValue("@Action", "ChargedRooms")
            cmdRoom.Parameters.AddWithValue("@ChargedRoomsOnly", True)

            cnSselData.Open()
            ddlRoom.DataSource = cmdRoom.ExecuteReader(CommandBehavior.CloseConnection)
            ddlRoom.DataBind()

            ddlRoom.Items.Insert(0, New ListItem("", "-1"))
            ddlRoom.ClearSelection()
        Else
            lblMsg.Visible = False

            'dsReport = CType(Cache.Get(Session("Cache")), DataSet)
            dsReport = CType(Session("dsReport"), DataSet)
            If (dsReport Is Nothing) Then
                Response.Redirect("~/?Error=SessionError")
            End If

            sDate = New DateTime(Integer.Parse(ddlYear.SelectedValue), Integer.Parse(ddlMonth.SelectedValue), 1)
            eDate = sDate.AddMonths(1)

            If RoomDA.IsAntiPassbackRoom(CType(ddlRoom.SelectedValue, Integer)) Then
                If (Session("ActiveTable") IsNot Nothing) Then
                    ReadApportGrid()
                End If
            End If
        End If
    End Sub

    Private Sub DdlUser_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlUser.SelectedIndexChanged
        ClientAccounts()
        PrepareData()
    End Sub

    Private Sub DdlRoom_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlRoom.SelectedIndexChanged
        PrepareData()
    End Sub

    Private Sub DdlMonth_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlMonth.SelectedIndexChanged
        UpdateTimeDepData()
        PrepareData()
    End Sub

    Private Sub DdlYear_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlYear.SelectedIndexChanged
        UpdateTimeDepData()
        PrepareData()
    End Sub

    Private Sub UpdateTimeDepData()
        '1. Update the User dropdownlist items

        'selectedUser holds the ClientID that is currently selected by the user
        Dim selectedUser As Integer = -1
        If (ddlUser.SelectedValue <> "") Then
            selectedUser = Integer.Parse(ddlUser.SelectedValue)
        End If

        Dim currentPeriod As New DateTime(Now.Year, Now.Month, 1)

        'now we want to see if we should allow the user to modify the data
        If CurrentUser.HasPriv(ClientPrivilege.Developer) Then
            '2008-01-14 allow only developer to modify past data
            Session("AllowEdit") = True
        Else
            If (sDate = currentPeriod) Then
                'user is modifying current month data
                Session("AllowEdit") = True
            ElseIf (sDate = currentPeriod.AddMonths(-1)) Then
                'user is modifying last month data and it has not been 3 business days yet for this month
                Dim cmdBusinessDay As New SqlCommand("SELECT dbo.udf_IsWithinBusinessDay(@CurDate, null)", cnSselData)
                cmdBusinessDay.Parameters.AddWithValue("@CurDate", Today)
                cnSselData.Open()
                Session("AllowEdit") = cmdBusinessDay.ExecuteScalar
                cnSselData.Close()
            Else
                'read-only for all other scenarios
                Session("AllowEdit") = False
            End If
        End If

        'client info - gets put into ddl, not needed in dataset
        Dim privs As Integer = Convert.ToInt32(ClientPrivilege.LabUser Or ClientPrivilege.Staff)

        If CurrentUser.HasPriv(ClientPrivilege.Administrator Or ClientPrivilege.Executive) Then
            If CurrentUser.HasPriv(ClientPrivilege.Administrator) Then
                ddlUser.DataSource = AppCode.ClientManagerUtility.GetAllClientsByDateAndPrivs(sDate, eDate, privs)
            ElseIf CurrentUser.HasPriv(ClientPrivilege.Executive) Then
                ddlUser.DataSource = AppCode.ClientManagerUtility.GetClientsByManagerID(sDate, eDate, CurrentUser.ClientID)
            End If

            ddlUser.DataBind()

            ddlUser.Items.Insert(0, New ListItem("", "-1"))

            'remember to set the selected user back to the original one
            If (ddlUser.Items.FindByValue(CStr(selectedUser)) Is Nothing) Then
                ddlUser.ClearSelection()
            Else
                ddlUser.SelectedValue = selectedUser.ToString()
            End If
        Else
            ddlUser.Items.Add(New ListItem(CurrentUser.DisplayName, CurrentUser.ClientID.ToString()))
            ddlUser.SelectedIndex = 0 ' only has himself
        End If

        ClientAccounts()
    End Sub

    Private Sub ClientAccounts()
        '2009-04-20 This clientAccount table in dsReport already includes the remote processing accounts, it's done in the Store Procedure
        If Integer.Parse(ddlUser.SelectedValue) <= 0 Then
            Exit Sub
        End If

        If (dsReport.Tables.Contains("ClientAccount")) Then
            dsReport.Tables.Remove(dsReport.Tables("ClientAccount"))
        End If

        ' get client's accounts
        Dim daClientAccount As New SqlDataAdapter("ClientAccount_Select", cnSselData)
        daClientAccount.SelectCommand.CommandType = CommandType.StoredProcedure
        daClientAccount.SelectCommand.Parameters.AddWithValue("@Action", "ByClientWithStartDate")
        daClientAccount.SelectCommand.Parameters.AddWithValue("@sDate", sDate)
        daClientAccount.SelectCommand.Parameters.AddWithValue("@eDate", eDate)
        daClientAccount.SelectCommand.Parameters.AddWithValue("@ClientID", ddlUser.SelectedValue)
        daClientAccount.Fill(dsReport, "ClientAccount")

        If (dsReport.Tables("ClientAccount").Rows.Count = 0) Then
            lblMsg.Visible = True
            lblMsg.Text = "SERIOUS ERROR: No accounts assigned to this user. See the administrator!"
            dsReport.Tables.Remove(dsReport.Tables("ClientAccount"))
            Exit Sub
        End If

        ' get previous Apportionment data
        Dim daRoomData As New SqlDataAdapter("RoomData_Select", cnSselData)
        daRoomData.SelectCommand.CommandType = CommandType.StoredProcedure
        daRoomData.SelectCommand.Parameters.AddWithValue("@Action", "ByClientDate")
        daRoomData.SelectCommand.Parameters.AddWithValue("@ClientID", ddlUser.SelectedValue)
        daRoomData.SelectCommand.Parameters.AddWithValue("@Period", sDate)
        If (Not dsReport.Tables.Contains("RoomData")) Then
            daRoomData.FillSchema(dsReport, SchemaType.Mapped, "RoomData")
        End If
        daRoomData.Fill(dsReport, "RoomData")
    End Sub

    Private Sub PrepareData()
        Session("ActiveTable") = Nothing
        dgApport.DataSource = Nothing
        dgApport.DataBind()

        If Integer.Parse(ddlUser.SelectedValue) <= 0 Then
            Exit Sub
        End If

        If (ddlRoom.SelectedIndex = 0) Then
            Exit Sub
        End If

        If (Not dsReport.Tables.Contains("ClientAccount")) Then
            Exit Sub
        End If

        If (dsReport.Tables("ClientAccount").Rows.Count = 1) Then
            lblMsg.Visible = True
            lblMsg.Text = "Only one account assigned to this user. Apportionment not needed."
            butSave.Enabled = False
        End If

        Dim tabType As Char = If(Convert.ToBoolean(Session("AllowEdit")), Char.Parse("t"), Char.Parse("f"))
        Dim tName As String = tabType + "Apport" + Format(sDate, "MMyyyy") + Format(Integer.Parse(ddlUser.SelectedValue), "0000") + Format(Integer.Parse(ddlRoom.SelectedValue), "000")
        Session("ActiveTable") = tName

        If (Not dsReport.Tables.Contains(tName)) Then
            ' get all access data for period
            Dim ds As DataSet = ReadData.Room.ReadRoomDataClean(sDate, eDate, Integer.Parse(ddlUser.SelectedValue), Integer.Parse(ddlRoom.SelectedValue))
            Dim dtLabTime As DataTable = ds.Tables("RoomDataClean")
            dtLabTime.TableName = "LabTime"

            If (dsReport.Tables.Contains("LabTime")) Then
                Dim ndr As DataRow
                For Each dr As DataRow In dtLabTime.Rows
                    ndr = dsReport.Tables("LabTime").NewRow
                    ndr.ItemArray = dr.ItemArray
                    dsReport.Tables("LabTime").Rows.Add(ndr)
                Next
            Else
                dsReport.Tables.Add(dtLabTime)
            End If

            ' now build apport table
            Dim dtApport As New DataTable With {
                .TableName = tName
            }
            dtApport.Columns.Add("EvtDate", GetType(DateTime))
            dtApport.Columns.Add("Entries", GetType(Double))
            dtApport.Columns.Add("Duration", GetType(Double))
            For Each drAcct As DataRow In dsReport.Tables("ClientAccount").Rows
                dtApport.Columns.Add("Acct" + Format(drAcct("AccountID")), GetType(Double))
            Next
            dtApport.Columns.Add("ApportType", GetType(String))

            ' TODO: show warning if activity on date with no available account
            Dim drNew As DataRow
            For Each dr As DataRow In dtLabTime.Rows
                drNew = dtApport.NewRow
                drNew("EvtDate") = dr("EntryDT")
                drNew("Duration") = dr("Duration")
                drNew("ApportType") = "Hours"
                drNew("Entries") = dr("Entries")
                dtApport.Rows.Add(drNew)
            Next

            dsReport.Tables.Add(dtApport)

            ' load previously stored data
            Dim fdr() As DataRow
            Dim strSelect As String = "ClientID = {0} AND RoomID = {1} AND EvtDate = '{2}'"
            For Each dr As DataRow In dsReport.Tables(tName).Rows
                fdr = dsReport.Tables("RoomData").Select(String.Format(strSelect, ddlUser.SelectedValue, ddlRoom.SelectedValue, dr("EvtDate")))
                For i As Integer = 0 To fdr.GetUpperBound(0)
                    dr("Acct" + CStr(fdr(i)("AccountID"))) = fdr(i)("Hours")
                Next
            Next

            Session("dsReport") = dsReport
            'Cache.Insert(Session("Cache"), dsReport, Nothing, DateTime.MaxValue, TimeSpan.FromMinutes(20))
        End If

        '2007-10 We have to check of this selected room is a antipassback room or not.  For non anti-passback, we just have to show one row of accounts
        If RoomDA.IsAntiPassbackRoom(CType(ddlRoom.SelectedValue, Integer)) Then
            ShowApportGrid(True)
        Else
            'Handles the non antipassback room time apportionment table
            PopulateNAPGridView()
        End If
    End Sub

    Private Sub ShowApportGrid(ByVal doBind As Boolean)
        If dgApport IsNot Nothing Then
            dgApport.Visible = True
        Else
            Return
        End If

        If gvNAP IsNot Nothing Then
            gvNAP.Visible = False
        Else
            Return
        End If

        Dim lastCol As Integer = 1 + dgApport.Columns.Count - shiftCols
        For i As Integer = shiftCols To lastCol
            dgApport.Columns.RemoveAt(shiftCols)
        Next

        ' need to create a datagrid with the appropriate columns (accounts) and rows (dates in room)
        ' need to add a row for today (%age only) if current month
        Dim tcol As TemplateColumn

        For i As Integer = 0 To dsReport.Tables("ClientAccount").Rows.Count - 1
            tcol = New TemplateColumn With {
                .HeaderText = dsReport.Tables("ClientAccount").Rows(i)("Name").ToString()
            }
            tcol.HeaderStyle.Width = Unit.Pixel(100)
            tcol.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
            tcol.ItemStyle.Width = Unit.Pixel(100)
            tcol.ItemStyle.HorizontalAlign = HorizontalAlign.Center
            tcol.ItemTemplate = New DynamicTextBoxTemplate("txtAcct" + dsReport.Tables("ClientAccount").Rows(i)("AccountID").ToString(), Unit.Pixel(50), 5, "numeric-text")
            dgApport.Columns.AddAt(i + shiftCols, tcol)
        Next

        If doBind Then BindGrid()
    End Sub

    Private Sub BindGrid()
        dgApport.DataSource = dsReport.Tables(Session("ActiveTable").ToString())
        dgApport.DataBind()

        gv1.DataSource = dsReport.Tables(0)
        gv2.DataSource = dsReport.Tables(1)
        gv3.DataSource = dsReport.Tables(2)
        gv4.DataSource = dsReport.Tables(3)

        gv1.DataBind()
        gv2.DataBind()
        gv3.DataBind()
        gv4.DataBind()

    End Sub

    Private Sub ReadApportGrid()
        ' only need to do this for this for tables with enabled rows
        If (Left(Session("ActiveTable").ToString(), 7) = "tApport") Then
            Dim dr, fdr() As DataRow

            If (dsReport.Tables("ClientAccount").Rows.Count = 1) Then
                ' with one account, each row gets the full duration
                For Each dr In dsReport.Tables(Session("ActiveTable").ToString()).Rows
                    dr(shiftCols) = dr("Duration")
                Next
            Else
                Dim aVals() As Double
                ReDim aVals(dgApport.Columns.Count) ' bigger than needed - simplifies array index

                ' read in the dynamic grid and update
                Dim EvtDate As DateTime
                Dim Duration As Double
                Dim bNotZero As Boolean
                Dim AccountID As Integer
                Dim strNbx As String
                Dim apportType As String
                Dim lastCol As Integer = 1 + dgApport.Columns.Count - shiftCols

                ' TODO: do not assign values to accounts that are not available
                '  this is needed for the lock-down to keep accts from propagating into the future
                ' this is a three-pass operation
                ' 1 - see if there are any entries on the rows
                ' 2 - if there are entries, normalize them to duration or 100%
                ' 3 - store the hours or null
                For Each dgi As DataGridItem In dgApport.Items
                    If ((dgi.ItemType = ListItemType.Item) Or (dgi.ItemType = ListItemType.AlternatingItem)) Then
                        bNotZero = False ' set values in table only if there is at least one entry
                        For i As Integer = shiftCols To lastCol
                            AccountID = dsReport.Tables("ClientAccount").Rows(i - shiftCols).Field(Of Integer)("AccountID")
                            strNbx = CType(dgi.FindControl("txtAcct" + AccountID.ToString), TextBox).Text
                            If ((strNbx.Length > 0) AndAlso (CSng(strNbx) > 0.0F)) Then
                                aVals(i) = CSng(strNbx)
                                bNotZero = True
                            Else
                                aVals(i) = 0.0F
                            End If
                        Next

                        EvtDate = CDate(CType(dgi.FindControl("lblEvtDate"), Label).Text)
                        apportType = CType(dgi.FindControl("rblApportType"), RadioButtonList).SelectedValue

                        If (bNotZero) Then
                            Dim sumVals As Double = 0.0F
                            For i As Integer = shiftCols To lastCol
                                sumVals += aVals(i)
                            Next

                            ' percentages are a display item only - see DataGridBound
                            Dim ClientID As Integer = CInt(Mid(Session("ActiveTable").ToString(), 14, 4))
                            Duration = dsReport.Tables("LabTime").Select(String.Format("ClientID = {0} AND EntryDT = '{1}", ClientID, EvtDate))(0).Field(Of Double)("Duration")
                            For i As Integer = shiftCols To lastCol
                                aVals(i) *= Duration / sumVals
                            Next
                        End If

                        fdr = dsReport.Tables(Session("ActiveTable").ToString()).Select("EvtDate='" + CStr(EvtDate) + "'")
                        For i As Integer = shiftCols To lastCol
                            AccountID = dsReport.Tables("ClientAccount").Rows(i - shiftCols).Field(Of Integer)("AccountID")
                            If (bNotZero) Then
                                fdr(0)("Acct" + AccountID.ToString) = aVals(i)
                            Else
                                fdr(0)("Acct" + AccountID.ToString) = DBNull.Value
                            End If
                        Next
                        fdr(0)("ApportType") = apportType
                    End If
                Next
            End If

            'Cache.Insert(Session("Cache"), dsReport, Nothing, DateTime.MaxValue, TimeSpan.FromMinutes(20))
        End If
    End Sub

    Public Sub ChangeApportType(ByVal sender As Object, ByVal e As System.EventArgs)
        BindGrid()
    End Sub

    Private Sub DgApport_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgApport.ItemDataBound
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            Dim drv As DataRowView = CType(e.Item.DataItem, DataRowView)
            Dim evtDate As DateTime = CType(drv("EvtDate"), DateTime)
            CType(e.Item.FindControl("lblEvtDate"), Label).Text = Format(evtDate, "dd MMMM yyyy")
            If (drv("ApportType").ToString() = "Hours") Then
                CType(e.Item.FindControl("lblDuration"), Label).Text = Format(drv("Duration"), "0.0 hrs")
            Else
                CType(e.Item.FindControl("lblDuration"), Label).Text = "100%"
            End If

            ' hide boxes for accounts that were not active on date
            Dim colName As String
            Dim tBox As TextBox
            Dim tHours As Double = 0.0
            For Each dr As DataRow In dsReport.Tables("ClientAccount").Rows
                colName = "Acct" + CStr(dr("AccountID"))
                tBox = CType(e.Item.FindControl("txt" + colName), TextBox)
                tBox.Enabled = Convert.ToBoolean(Session("AllowEdit"))
                If (evtDate < dr.Field(Of DateTime)("EnableDate") Or (Not IsDBNull(dr("DisableDate")) AndAlso (evtDate > dr.Field(Of DateTime)("DisableDate")))) Then
                    tBox.Visible = False
                Else
                    If (Not IsDBNull(drv(colName))) Then
                        If (drv("ApportType").ToString() = "Hours") Then
                            tHours += Convert.ToDouble(drv(colName))
                            tBox.Text = Format(drv(colName), "0.0")
                        Else
                            Dim acctPct As Double = 100.0 * Convert.ToDouble(drv(colName)) / Convert.ToDouble(drv("Duration"))
                            tBox.Text = Format(acctPct, "0")
                        End If
                    End If
                End If
            Next

            If (drv("ApportType").ToString() = "Hours") Then
                If ((tHours <> 0.0) And (Math.Abs(tHours - Convert.ToDouble(drv("Duration"))) > 0.1)) Then
                    e.Item.BackColor = System.Drawing.Color.LightPink
                End If
            End If

            Dim rbl As RadioButtonList = CType(e.Item.FindControl("rblApportType"), RadioButtonList)
            rbl.SelectedValue = drv("ApportType").ToString()
            rbl.Enabled = Convert.ToBoolean(Session("AllowEdit"))
        End If
    End Sub

    Private Sub ButSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles butSave.Click
        lblMsg.Visible = False
        'gv1.DataSource = dsReport.Tables(0)
        'gv2.DataSource = dsReport.Tables(1)
        'gv3.DataSource = dsReport.Tables(2)
        'gv4.DataSource = dsReport.Tables(3)

        'gv1.DataBind()
        'gv2.DataBind()
        'gv3.DataBind()
        'gv4.DataBind()

        'Exit Sub


        ' this function is divided into two part - either save the AP room or NAP room, they use different algorithm
        If RoomDA.IsAntiPassbackRoom(Integer.Parse(ddlRoom.SelectedValue)) Then
            Dim dtApportionment As DataTable = dsReport.Tables("RoomData")

            ' first put data into Apportionment table
            Dim AccountIDs() As Integer
            Dim ndr, fdr() As DataRow
            For Each dt As DataTable In dsReport.Tables
                'tApport is the 4th table that contains the data shown to the user on this apportionment page
                If (Left(dt.TableName, 7) = "tApport") Then
                    Dim ClientID As Integer = CInt(Mid(dt.TableName, 14, 4)) 'e.g. tApport0220080014006
                    Dim RoomID As Integer = CInt(Mid(dt.TableName, 18, 3))

                    'Get all account IDs from the tApport0220xxxx table, since that talbe has columns with names similar to "AcctXXX"
                    ReDim AccountIDs(dt.Columns.Count)
                    For i As Integer = shiftCols To 1 + dt.Columns.Count - shiftCols
                        AccountIDs(i) = CInt(Mid(dt.Columns(i).ColumnName, 5))
                    Next

                    ' check for a record for each entry
                    ' check all cases - dr(i) is the hours per account
                    'dt has columns 1. EvtDate, 2. Entries, 3. Duration, AcctXXX, ...., ApportType
                    For Each dr As DataRow In dt.Rows
                        ' there should only be one row that satisfies the Select
                        'The Duration varialbe would be the sum of total hours spend in the room on this specific entrydt
                        Dim Duration As Double = dsReport.Tables("LabTime").Select(String.Format("EntryDT = '{0}'", dr("EvtDate"))).First().Field(Of Double)("Duration")

                        Dim strSelect As String = "ClientID=" + CStr(ClientID) + " AND RoomID=" + CStr(RoomID) + " AND EvtDate='" + CStr(dr("EvtDate")) + "' AND AccountID="
                        'this for loop would repeat x times where x is the number of accounts the user has
                        For i As Integer = shiftCols To 1 + dt.Columns.Count - shiftCols
                            fdr = dtApportionment.Select(strSelect + CStr(AccountIDs(i)))
                            If (fdr.Length = 0) Then ' record doesn't exist in DB
                                If (Not IsDBNull(dr(i))) Then ' is if null, then do nothing
                                    ndr = dtApportionment.NewRow
                                    ndr("Period") = sDate
                                    ndr("ClientID") = ClientID
                                    ndr("RoomID") = RoomID
                                    ndr("EvtDate") = dr("EvtDate")
                                    ndr("AccountID") = AccountIDs(i)
                                    ndr("Entries") = dr.Field(Of Double)("Entries") * dr.Field(Of Double)(i) / Duration
                                    ndr("Hours") = dr(i)
                                    'ndr("Days") = 0
                                    'ndr("Months") = 0
                                    ndr("DataSource") = 1
                                    dtApportionment.Rows.Add(ndr)
                                End If
                            Else
                                If (IsDBNull(dr(i))) Then  ' entry exists, delete it
                                    fdr(0).Delete() ' can only be one matching entry
                                Else
                                    If (fdr(0).Field(Of Double)("Hours") <> dr.Field(Of Double)(i)) Then
                                        fdr(0)("Entries") = dr.Field(Of Double)("Entries") * dr.Field(Of Double)(i) / Duration
                                        fdr(0)("Hours") = dr(i)
                                        'fdr(0)("Days") = 0	  'got to recalculate
                                        'fdr(0)("Months") = 0  'got to recalculate
                                        'Bug fix 2006/11/9 - the problem is that datasource is not set to 1 after user modify his apportionment data
                                        'probably the fix is best placed here.  Is it possible that fdr(0)("Hours") == dr(i) after user changes data?  possible....i think
                                        'if user's new input data match exactly the same as old data on certain accounts on the same date.....the chance is slim..but possible								
                                    End If
                                    'Bug fix 2007-01-31 - the problem is that if the system generate correct data, then user won't change.  So user woulc
                                    'think the data is correct.  But at the end of month, that data might be changed again because the data source is not 1
                                    'This happend to Rob, so we have to move the datasource = 1 code whenever user click save button
                                    fdr(0)("DataSource") = 1
                                End If
                            End If
                        Next
                    Next
                End If
            Next


            ''



            Dim daRoomData As New SqlDataAdapter With {
                .InsertCommand = New SqlCommand("RoomData_Insert", cnSselData) With {
                    .CommandType = CommandType.StoredProcedure
                }
            }
            daRoomData.InsertCommand.Parameters.Add("@Period", SqlDbType.DateTime, 8, "Period")
            daRoomData.InsertCommand.Parameters.Add("@ClientID", SqlDbType.Int, 4, "ClientID")
            daRoomData.InsertCommand.Parameters.Add("@EvtDate", SqlDbType.DateTime, 8, "EvtDate")
            daRoomData.InsertCommand.Parameters.Add("@RoomID", SqlDbType.Int, 4, "RoomID")
            daRoomData.InsertCommand.Parameters.Add("@AccountID", SqlDbType.Int, 4, "AccountID")
            daRoomData.InsertCommand.Parameters.Add("@Entries", SqlDbType.Float, 8, "Entries")
            daRoomData.InsertCommand.Parameters.Add("@Hours", SqlDbType.Float, 8, "Hours")
            daRoomData.InsertCommand.Parameters.Add("@DataSource", SqlDbType.Int, 1, "DataSource")

            daRoomData.UpdateCommand = New SqlCommand("RoomData_Update", cnSselData) With {
                .CommandType = CommandType.StoredProcedure
            }
            daRoomData.UpdateCommand.Parameters.Add("@RoomDataID", SqlDbType.Int, 4, "RoomDataID")
            daRoomData.UpdateCommand.Parameters.Add("@Entries", SqlDbType.Float, 8, "Entries")
            daRoomData.UpdateCommand.Parameters.Add("@Hours", SqlDbType.Float, 8, "Hours")
            daRoomData.UpdateCommand.Parameters.Add("@DataSource", SqlDbType.TinyInt, 1, "DataSource") 'bug fix 2006/11/9

            daRoomData.DeleteCommand = New SqlCommand("RoomData_Delete", cnSselData) With {
                .CommandType = CommandType.StoredProcedure
            }
            daRoomData.DeleteCommand.Parameters.AddWithValue("@Action", "BadEntry")
            daRoomData.DeleteCommand.Parameters.Add("@RoomDataID", SqlDbType.Int, 4, "RoomDataID")

            daRoomData.Update(dsReport, "RoomData")

            lblMsg.Visible = True
            lblMsg.Text = "Data saved successfully!"
            'butDiscard_Click(Nothing, Nothing)
        Else
            ReadNAPGridView()
        End If
    End Sub

    Private Sub ButDiscard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butDiscard.Click
        Session("InLab") = Nothing
        Session("AllowEdit") = Nothing
        Session("ActiveTable") = Nothing
        Session("dsReport") = Nothing
        Session("NonAntiPassbackTable") = Nothing
        Response.Redirect("~/")
    End Sub

    ''' <summary>
    ''' Create and populate the non anti-passback table
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PopulateNAPGridView()
        'Disable the Anti Passback data grid and show the Non Anti Passback grid
        dgApport.Visible = False
        gvNAP.Visible = True

        'Always construct the grid view dynamically first
        ConstructNAPGridView()

        'Get the tabel from database, we have to bind the table too because we need the DataBound event to be fired
        Dim dt As DataTable = RoomApportionDataDA.GetApportionmenetPivot(Integer.Parse(ddlUser.SelectedValue), Integer.Parse(ddlRoom.SelectedValue), New DateTime(Integer.Parse(ddlYear.SelectedValue), Integer.Parse(ddlMonth.SelectedValue), 1))
        Session("NonAntiPassbackTable") = dt
        gvNAP.DataSource = dt
        gvNAP.DataBind()
    End Sub

    ''' <summary>
    ''' This method dynamically creates columns for gvNAP.  This method must be called in Pre_Init method on every postback
    ''' in order to retrieve the data set by users
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ConstructNAPGridView()
        'we must clear everything first because the number of accounts might be different
        If gvNAP IsNot Nothing Then
            If gvNAP.Columns.Count > 0 Then
                gvNAP.Columns.Clear()
            End If
        Else
            Return
        End If

        'Create the first column, which shows users that apportionment is using percentage
        Dim bf As New BoundField With {
            .HeaderText = "Apportionment Type"
        }
        bf.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
        gvNAP.Columns.Add(bf)

        'ClientAccount table has 5 columns -> AccountID, Name, Number, EnableDate, DisableDate
        'Create columns based on user's account numbers
        For Each row As DataRow In dsReport.Tables("ClientAccount").Rows
            Dim dtf As New TemplateField With {
                .HeaderText = row("Name").ToString()
            }
            dtf.HeaderStyle.Width = Unit.Pixel(100)
            dtf.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
            'dtf.ItemTemplate = New LNF.CommonTools.DynamicNumberboxTemplate("txt" + row("AccountID").ToString())
            gvNAP.Columns.Add(dtf)
        Next
    End Sub

    Protected Sub GvNAP_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvNAP.DataBound
        If (gvNAP.Rows.Count = 1) Then
            'Get the correct apportionment percentage data from database and populate it to the gridview
            Dim dtApportDataFromDB As DataTable = RoomApportionDataDA.GetApportionmentData(Integer.Parse(ddlUser.SelectedValue), Integer.Parse(ddlRoom.SelectedValue), New DateTime(Integer.Parse(ddlYear.SelectedValue), Integer.Parse(ddlMonth.SelectedValue), 1))

            For Each dr As DataRow In dsReport.Tables("ClientAccount").Rows
                Dim fdr() As DataRow = dtApportDataFromDB.Select("AccountID = " & dr("AccountID").ToString())
                If (fdr.Length = 1) Then
                    CType(gvNAP.Rows(0).FindControl("txt" + dr("AccountID").ToString()), TextBox).Text = fdr(0)("Percentage").ToString()
                End If
            Next
            'We have to populate the first column manually.  It tells the apportionment method type.
            gvNAP.Rows(0).Cells(0).Text = "By Percentage (0 ~ 100%)"
        End If
    End Sub

    ''' <summary>
    ''' Read the data from user and push the changes back to database.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ReadNAPGridView()
        'Always the first row, in NAP room, there should only be one row per month
        Dim gvRow As GridViewRow = gvNAP.Rows(0)

        'Error checking
        Dim total_percentage As Double = 0
        Dim percentage As Double = 0
        For Each dr As DataRow In dsReport.Tables("ClientAccount").Rows
            percentage = CType(CType(gvRow.FindControl("txt" + dr("AccountID").ToString()), TextBox).Text, Double)

            If (percentage < 0) Then
                lblMsg.Text = "Error in input data: value cannot be negative"
                lblMsg.Visible = True
                Return
            End If

            total_percentage += percentage
        Next

        If total_percentage >= 101 Or total_percentage <= 99 Then
            lblMsg.Text = "Error in input data: all numbers should added up to 100%"
            lblMsg.Visible = True
            Return
        End If


        'Get the necessary data that needs to be updated into this table
        Dim dtApportFromDB As DataTable = RoomApportionDataDA.GetApportionmentData(Integer.Parse(ddlUser.SelectedValue), Integer.Parse(ddlRoom.SelectedValue), sDate)

        For Each dr As DataRow In dsReport.Tables("ClientAccount").Rows
            percentage = CType(CType(gvRow.FindControl("txt" + dr("AccountID").ToString()), TextBox).Text, Double)
            Dim fdr() As DataRow = dtApportFromDB.Select("AccountID = " & dr("AccountID").ToString())

            If fdr.Length = 1 Then
                fdr(0)("Percentage") = CType(percentage, Double)
                fdr(0)("DataSource") = 1 'what should be the value here?
            Else
                'new account added (remote account)
                Dim newrow As DataRow = dtApportFromDB.NewRow()
                newrow("Period") = dtApportFromDB.Rows(0)("Period")
                newrow("ClientID") = dtApportFromDB.Rows(0)("ClientID")
                newrow("RoomID") = dtApportFromDB.Rows(0)("RoomID")
                newrow("AccountID") = dr("AccountID")
                newrow("Percentage") = percentage
                newrow("DataSource") = 1
                dtApportFromDB.Rows.Add(newrow)
            End If

        Next

        Dim updates As Integer = DataCommand.Create().Update(dtApportFromDB, Sub(cfg)
                                                                                 'Insert prepration - it's necessary because we may have to add new account that is a remote account
                                                                                 cfg.Insert.SetCommandText("dbo.RoomApportionData_Insert")
                                                                                 cfg.Insert.AddParameter("ClientID", SqlDbType.Int)
                                                                                 cfg.Insert.AddParameter("RoomID", SqlDbType.Int)
                                                                                 cfg.Insert.AddParameter("Period", SqlDbType.DateTime)
                                                                                 cfg.Insert.AddParameter("AccountID", SqlDbType.Int)
                                                                                 cfg.Insert.AddParameter("Percentage", SqlDbType.Float)
                                                                                 cfg.Insert.AddParameter("DataSource", SqlDbType.Int)

                                                                                 'Update the data using dateset's batch update feature
                                                                                 cfg.Update.SetCommandText("dbo.RoomApportionData_Update")
                                                                                 cfg.Update.AddParameter("ClientID", SqlDbType.Int)
                                                                                 cfg.Update.AddParameter("RoomID", SqlDbType.Int)
                                                                                 cfg.Update.AddParameter("Period", SqlDbType.DateTime)
                                                                                 cfg.Update.AddParameter("AccountID", SqlDbType.Int)
                                                                                 cfg.Update.AddParameter("Percentage", SqlDbType.Float)
                                                                                 cfg.Update.AddParameter("DataSource", SqlDbType.Int)
                                                                             End Sub)

        If updates >= 0 Then
            lblMsg.Text = "Data saved successfully"
        Else
            lblMsg.Text = "Saving data failed, please contact the administrator (Help menu)"
        End If

        '2009-03-03 Update the RoomData table with proper Days, Entries charge for those Non Antipassback room.

        lblMsg.Visible = True
    End Sub
End Class