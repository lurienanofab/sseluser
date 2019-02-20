Imports LNF.Cache
Imports LNF.Models.Data
Imports LNF.Repository
Imports sselUser.AppCode
Imports sselUser.AppCode.DAL

Public Class ApportionmentDefault
    Inherits Page

    Public ReadOnly Property UserID As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("UserID")) Then
                If Not Integer.TryParse(Request.QueryString("UserID"), result) Then
                    result = 0
                End If
            End If
            Return result
        End Get
    End Property

    Public ReadOnly Property RoomID As Integer
        Get
            Dim result As Integer = 0
            If Not String.IsNullOrEmpty(Request.QueryString("RoomID")) Then
                If Not Integer.TryParse(Request.QueryString("RoomID"), result) Then
                    result = 0
                End If
            End If
            Return result
        End Get
    End Property

    'The activeAccounts table must be store in Session because it's required to re-generate the grid dynamically on every postback.  The dilemma here is we won't be able to get the current user selected in
    'drop down list because it's not bounded yet.  So keep the table in session is the best strategy.
    Public ReadOnly Property CurrentAcct As DataTable
        Get
            If ViewState("CurrentAcct") Is Nothing Then
                ViewState("CurrentAcct") = ClientAccountDA.GetAllActiveAccountsByClientID(UserID)
            End If
            Return CType(ViewState("CurrentAcct"), DataTable)
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Always do this but only set TextBox values if Not IsPostBack
        ConstructGrid()
        LoadGrid()

        If Not Page.IsPostBack Then

            'populate the user dropdown list
            Dim privs As Integer = Convert.ToInt32(ClientPrivilege.LabUser Or ClientPrivilege.Staff)
            Dim sDate As Date = Now

            If CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator Or ClientPrivilege.Executive) Then
                If CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator) Then
                    ddlUser.DataSource = AppCode.ClientManagerUtility.GetAllClientsByDateAndPrivs(sDate, sDate.AddMonths(1), privs)
                ElseIf CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Executive) Then
                    ddlUser.DataSource = AppCode.ClientManagerUtility.GetClientsByManagerID(sDate, sDate.AddMonths(1), CacheManager.Current.CurrentUser.ClientID)
                End If

                ddlUser.DataBind()
                ddlUser.Items.Insert(0, New ListItem("-- Select --", "-1"))
            Else
                ddlUser.Items.Add(New ListItem(CacheManager.Current.CurrentUser.DisplayName, CacheManager.Current.CurrentUser.ClientID.ToString()))
                ddlUser.SelectedIndex = 0 'only has himself
            End If

            ddlRoom.DataSource = GetRooms(RoomDA.GetAllActiveRooms())
            ddlRoom.DataBind()

            '2009-07-14
            'The code below would see if user comes from a dropdownlist selection or complete new entry from other pages.
            'UserID querystring is set in GetData click function, it's redirected from there

            If UserID > 0 Then
                'set the selected user.
                ddlUser.SelectedValue = UserID.ToString()
            End If

            If RoomID > 0 Then
                'set the selected room.
                ddlRoom.SelectedValue = RoomID.ToString()
            End If
        End If
    End Sub

    Protected Sub BtnGetData_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGetData.Click
        '2009-07-04 
        'I must use redirect as if we come to the page for the first time whenever user wants to get the data
        'The reason is due to a bug in asp.net that the dynamic grid view would lose its template control after databind for the second time.
        'So it means user can change an individual's data for one time, and if choose a second person and tries to modify the data, an error would occur because the grid view lost the template controls
        'Thus, the solution is to treat every button click as a complete new entry into this form instead of a postback.
        Dim SelectedUserID As Integer = Convert.ToInt32(ddlUser.SelectedValue)
        Dim SelectedRoomID As Integer = Convert.ToInt32(ddlRoom.SelectedValue)
        Response.Redirect(String.Format("{0}?UserID={1}&RoomID={2}", Request.Url.GetLeftPart(UriPartial.Path), SelectedUserID, SelectedRoomID))
    End Sub

    Private Sub LoadGrid()
        If RoomID > 0 And UserID > 0 Then
            'user selects from the drop down list

            panDisplay.Visible = True

            'create a dummny table and bind the grid
            Dim dtPivot As New DataTable 'this table is a pivot table of dtDefault.  We have to create this table because we must bind something to the gvAppDefault in order to show something.
            dtPivot.Columns.Add("Nothing", GetType(Double))
            dtPivot.Rows.Add(1.1)

            gvAppDefault.DataSource = dtPivot
            gvAppDefault.DataBind()
        End If
    End Sub

    'Construct the grid dynamically.  We need to call this function everytime the page loaded back because asp.net run time cannot construct it back to map the values from client side
    Private Sub ConstructGrid()
        'We should always show the most current active accounts, so we just need to get data from ClientAccount table.  At this stage we don't need
        'the real apportionment values from ApportionmentDefault table. The apportionment values are assigned during data bounded event.

        'always clean all columns before construction
        If gvAppDefault.Columns.Count > 0 Then
            gvAppDefault.Columns.Clear()
        End If

        For Each row As DataRow In CurrentAcct.Rows
            Dim tf As TemplateField = New TemplateField With {
                .HeaderText = row("Name").ToString()
            }
            tf.HeaderStyle.Width = Unit.Pixel(100)
            tf.HeaderStyle.HorizontalAlign = HorizontalAlign.Center
            tf.ItemTemplate = New DynamicTextBoxTemplate("txt" + row("AccountID").ToString(), Unit.Pixel(50), 5, "numeric-text")
            gvAppDefault.Columns.Add(tf)
        Next
    End Sub

    Protected Sub GvAppDefault_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvAppDefault.DataBound
        If gvAppDefault.Rows.Count = 1 Then
            'This is real data assignment, the data bounded object is just a dummy object.  Here we get what users truly want to see

            lblMsg.Text = String.Empty

            'Get data from ApportionmentDefault to get the true percentage values (if there is any)
            Dim dtDefault As DataTable = ApportionmentDefaultDA.GetDataByClientID(UserID, RoomID)

            Dim txt As TextBox = Nothing

            'always loop through the most active accounts table
            For Each dr As DataRow In CurrentAcct.Rows
                'we have to find out if there exist such account apportionment percentage
                Dim rows As DataRow() = dtDefault.Select(String.Format("AccountID = {0}", dr("AccountID")))

                If Not Page.IsPostBack Then
                    txt = CType(gvAppDefault.Rows(0).FindControl("txt" + dr("AccountID").ToString()), TextBox)
                    If txt IsNot Nothing AndAlso rows.Length = 1 Then
                        txt.Text = If(rows(0)("Percentage").Equals(DBNull.Value), "0", rows(0)("Percentage").ToString())
                    ElseIf txt IsNot Nothing AndAlso rows.Length = 0 Then
                        'it means this account is newly enabled, we have to assign 0 by default
                        txt.Text = "0"
                    Else
                        'Serious Error, we should not come here
                        lblMsg.Text = "Error: Please contact the IT adminstrator for this problem - Accounts Overlapping"
                        lblMsg.Visible = True
                        gvAppDefault.Visible = False
                    End If
                End If
            Next


            If CurrentAcct.Rows.Count <= 1 Then
                txt = CType(gvAppDefault.Rows(0).Cells(0).Controls(0), TextBox)
                If txt IsNot Nothing Then
                    txt.Text = "100"
                End If
                btnSave.Enabled = False
                lblMsg.Text = "You have only one account, no apportionment is needed"
            Else
                lblMsg.Text = String.Empty
                btnSave.Enabled = True
            End If
        End If
    End Sub

    Protected Sub BtnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click

        'Step 1. Make sure all data is added up to 100% and no data is negative
        Dim gvRow As GridViewRow = gvAppDefault.Rows(0) 'there is always one row of data

        Dim SelectedUserID As Integer = Convert.ToInt32(ddlUser.SelectedValue)
        Dim SelectedRoomID As Integer = Convert.ToInt32(ddlRoom.SelectedValue)
        Dim total_percentage As Double = 0
        Dim percentage As Double = 0

        For Each dr As DataRow In CurrentAcct.Rows
            percentage = 0
            Dim txt As TextBox = CType(gvRow.FindControl("txt" + dr("AccountID").ToString()), TextBox)
            If txt IsNot Nothing Then
                Double.TryParse(txt.Text, percentage)
            End If
            total_percentage += percentage
        Next

        If total_percentage <> 100 Then
            lblMsg.Text = "All numbers should add up to 100%"
            lblMsg.Visible = True
            Return
        End If

        'Step 2: Contruct an input table so we can store the data back to database

        'Get the necessary data that needs to be updated into this table
        Dim dtApporDef As DataTable = ApportionmentDefaultDA.GetDataByClientID(SelectedUserID, SelectedRoomID)

        'modify current row and add new rows if necessary
        For Each dr As DataRow In CurrentAcct.Rows
            Dim txt As TextBox = CType(gvRow.FindControl("txt" + dr("AccountID").ToString()), TextBox)
            percentage = 0
            If txt IsNot Nothing Then
                Double.TryParse(txt.Text, percentage)
            End If

            Dim fdr() As DataRow = dtApporDef.Select("AccountID = " + dr("AccountID").ToString())

            If fdr.Length = 1 Then
                fdr(0)("Percentage") = percentage
            Else
                'new account added (remote account)
                Dim newrow As DataRow = dtApporDef.NewRow()
                newrow("ClientID") = SelectedUserID
                newrow("RoomID") = SelectedRoomID
                newrow("AccountID") = dr("AccountID")
                newrow("Percentage") = percentage
                dtApporDef.Rows.Add(newrow)
            End If
        Next

        'delete old rows (deleted accounts)
        For Each dr As DataRow In dtApporDef.Rows
            Dim fdr() As DataRow = CurrentAcct.Select("AccountID = " + dr("AccountID").ToString())
            If fdr.Length = 0 Then
                dr.Delete()
            End If
        Next

        Dim updates As Integer = DA.Command().Update(dtApporDef, Sub(cfg)
                                                                     'Insert prepration - it's necessary because we may have to add new account that is a remote account
                                                                     cfg.Insert.SetCommandText("dbo.ApportionmentDefault_Insert")
                                                                     cfg.Insert.AddParameter("ClientID", SqlDbType.Int)
                                                                     cfg.Insert.AddParameter("RoomID", SqlDbType.Int)
                                                                     cfg.Insert.AddParameter("AccountID", SqlDbType.Int)
                                                                     cfg.Insert.AddParameter("Percentage", SqlDbType.Float)

                                                                     'Update the data using dateset's batch update feature
                                                                     cfg.Update.SetCommandText("dbo.ApportionmentDefault_Update")
                                                                     cfg.Update.AddParameter("AppID", SqlDbType.Int)
                                                                     cfg.Update.AddParameter("Percentage", SqlDbType.Float)

                                                                     'Update the data using dateset's batch update feature
                                                                     cfg.Delete.SetCommandText("dbo.ApportionmentDefault_Delete")
                                                                     cfg.Delete.AddParameter("@AppID", SqlDbType.Int)
                                                                 End Sub)

        If updates >= 0 Then
            lblMsg.Text = "The apportionment data is saved."
        Else
            lblMsg.Text = "Saving data failed, please contact the administrator (Help menu)"
        End If
    End Sub

    Private Function GetRooms(dt As DataTable) As IList(Of RoomItem)
        Dim result As New List(Of RoomItem)

        Dim sortOrder As Integer = 1

        Dim parents As DataRow() = dt.Select("ParentID IS NULL", "Room")

        For Each dr As DataRow In parents
            Dim item As New RoomItem() With {
                .RoomID = dr.Field(Of Integer)("RoomID"),
                .RoomName = dr.Field(Of String)("Room"),
                .SortOrder = sortOrder
            }
            result.Add(item)
            sortOrder += 1
        Next

        Dim children As DataRow() = dt.Select("ParentID IS NOT NULL", "Room")

        For Each dr As DataRow In children
            Dim item As New RoomItem() With {
                .RoomID = dr.Field(Of Integer)("RoomID"),
                .RoomName = dr.Field(Of String)("Room"),
                .SortOrder = sortOrder
            }
            result.Add(item)
            sortOrder += 1
        Next

        Return result
    End Function
End Class

Public Class RoomItem
    Public Property RoomID As Integer
    Public Property RoomName As String
    Public Property SortOrder As Integer
End Class