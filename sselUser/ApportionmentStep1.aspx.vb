Imports System.Data.SqlClient
Imports LNF.Billing.Apportionment
Imports sselUser.AppCode

Public Class ApportionmentStep1
    Inherits ApportionmentPage

    Public Overrides ReadOnly Property RoomDayReadOnly As Boolean
        Get
            Return False
        End Get
    End Property

    Protected Overrides Function GetBillingCheckBox() As CheckBox
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

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Request.QueryString("ThrowError") = "1" Then
            Throw New Exception("This is a test error.")
        End If

        hypViewEntryApportionment.NavigateUrl = String.Format("~/ApportionmentStep2.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}", UserID, RoomID, Month, Year)
    End Sub

    Protected Overrides Sub OnSave(e As CommandEventArgs)
        If SaveMultiOrg() Then
            'Response.Redirect(String.Format("~/ApportionmentStep2.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}&Recalculate={4}", UserID, RoomID, Month, Year, chkBilling.Checked), False)
            Response.Redirect(String.Format("~/ApportionmentStep2.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}", UserID, RoomID, Month, Year), False)
        End If
    End Sub

    Private Function SaveMultiOrg() As Boolean
        panDisplay.Visible = True

        Dim dtCurrentAcct As DataTable = CType(Session("CurrentAcct"), DataTable)
        Dim dtMultipleOrg As DataTable = CType(Session("MultipleOrg"), DataTable)

        Dim physicalDays As Double = UserUtility.ConvertToDouble(litPhysicalDays.Text, 0)
        Dim totalMinDays As Double = 0
        Dim totalUserDays As Double = 0

        Dim fail As Boolean = False

        'Step 1: go through each grid and do the data checking
        For Each item As RepeaterItem In rptMultiOrg.Items

            Dim lblOrgMsg As Label = CType(item.FindControl("lblOrgMsg"), Label)

            Dim usrday As Double = 0
            Dim gvOrg As GridView = CType(item.FindControl("gvOrg"), GridView)
            Dim gvr As GridViewRow = gvOrg.Rows(0)
            Dim lblMinDays As Label = CType(gvr.Cells(0).FindControl("lblMinDays"), Label)
            Dim hidOrgID As HiddenField = CType(gvr.Cells(0).FindControl("hidOrgID"), HiddenField)
            Dim minday As Double = UserUtility.ConvertToDouble(lblMinDays.Text, 0)
            Dim rows As DataRow() = dtCurrentAcct.Select(String.Format("OrgID = {0}", hidOrgID.Value))

            For Each dr As DataRow In rows
                usrday += UserUtility.ConvertToDouble(CType(gvr.FindControl("txt" + dr("AccountID").ToString()), TextBox).Text, 0)
            Next

            'MinimumDays.Add(minday)
            totalMinDays += minday

            'UserDays.Add(usrday)
            totalUserDays += usrday

            If Math.Round(usrday, 2) < Math.Round(minday, 2) Then
                lblOrgMsg.Text = "<span class=""badge bg-danger"">The sum cannot be less than " + minday.ToString() + " day" + If(minday.Equals(1), String.Empty, "s") + ".</span>"
                lblOrgMsg.Visible = True
                fail = True
            ElseIf usrday > physicalDays Then
                ' [2021-09-16 jg] Users can allocate more than physical days if they want to. Requested in ticket #567297 (https://lnf.umich.edu/helpdesk/scp/tickets.php?id=12866)
                Dim enforcePhysicalDaysLimitPerOrg As Boolean = Boolean.Parse(ConfigurationManager.AppSettings("EnforcePhysicalDaysLimitPerOrg"))
                If enforcePhysicalDaysLimitPerOrg Then
                    lblOrgMsg.Text = "<span class=""badge bg-danger"">The sum cannot exceed your lab physical days (" + physicalDays.ToString() + ").</span>"
                    lblOrgMsg.Visible = True
                    fail = True
                End If
            End If
        Next

        If fail Then Return False

        'The true minimum days are the greater of physical days | combination of all minimum days per org
        Dim trueMinDays As Double = If(totalMinDays > physicalDays, totalMinDays, physicalDays)

        If Math.Round(totalUserDays, 2) < Math.Round(trueMinDays, 2) Then
            SetMessage("The sum cannot be less than " + trueMinDays.ToString() + " day" + If(trueMinDays.Equals(1), String.Empty, "s") + ".")
            fail = True
        End If

        If fail Then Return False

        Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
            Dim repo As New Repository(conn)
            'Step 2: After checking is finished, we update the data.
            'Get the necessary data that needs to be updated into this table
            Dim ds As DataSet = repo.GetDataForApportion(Period, UserID, RoomID)

            Dim dtApport As DataTable = ds.Tables(0)
            Dim dtUserApport As DataTable = ds.Tables(2) 'From RoomBillingUserApportionData

            'modify current row and add new rows if necessary
            Dim totalHours As Double = UserUtility.ConvertToDouble(dtApport.Compute("SUM(Hours)", String.Empty), 0)
            Dim totalEntries As Double = UserUtility.ConvertToDouble(dtApport.Compute("SUM(Entries)", String.Empty), 0)

            For Each dr As DataRow In dtApport.Rows
                Dim amount As Double = GetChargeDaysFromMultiGrid(dr("OrgID").ToString(), dr("AccountID").ToString())

                dr("ChargeDays") = amount
                dr("IsDefault") = False
                dr("Entries") = totalEntries * (amount / totalUserDays)
                dr("Hours") = totalHours * (amount / totalUserDays)

                'We have to save the data back to RoomBillingUserApportionData
                Dim drUserData As DataRow() = dtUserApport.Select($"AccountID = {dr("AccountID")}")
                If drUserData.Length = 0 Then
                    Dim ndr As DataRow = dtUserApport.NewRow()
                    ndr("Period") = dr("Period")
                    ndr("ClientID") = dr("ClientID")
                    ndr("RoomID") = dr("RoomID")
                    ndr("AccountID") = dr("AccountID")
                    ndr("ChargeDays") = dr("ChargeDays")
                    ndr("Entries") = 0 'should be 0 in step 1, a value is assigned in step 2

                    dtUserApport.Rows.Add(ndr)
                Else
                    drUserData(0)("ChargeDays") = dr("ChargeDays")
                End If
            Next

            'we have to recalculate the MonthlyRoomCharge - only apply to people who still pay monthly usage fee
            'Remove this code after 2010/07/01 [commented out 11 years later on 2021/03/19]
            'RecalculateMonthlyRoomCharge(dtApportFromDB)

            'Save to DB
            If repo.UpdateRoomApportionmentInDaysMonthly(dtApport) >= 0 Then
                SetMessage("The apportionment data is saved.", "success")
            Else
                SetMessage("Saving data failed, please contact the administrator (Help menu)")
                Return False
            End If

            'Save to RoomBillingUserApportionData
            repo.SaveRoomBillingUserApportionData(dtUserApport)

            'update child room entry apportionment based on room days
            repo.UpdateChildRoomEntryApportionment(Period, UserID, RoomID)
        End Using

        'Clear out saved Session data so next time we load the data is refreshed
        Session("CurrentAcct") = Nothing
        Session("MultipleOrg") = Nothing
        Session("LabUsageDay") = Nothing

        Return True
    End Function

    Public Sub RecalculateMonthlyRoomCharge(dt As DataTable)
        If RoomID = 6 Then
            Dim array() As Integer = {5, 15, 25}

            'we loop three times based on chargetype, and for each charge type, we find out the monthly fee independently
            For Each _chargeTypeID As Integer In array
                Dim temprows() As DataRow = dt.Select(String.Format("ChargeTypeID = {0}", _chargeTypeID))

                If temprows.Length > 0 Then
                    Dim TotalMonthlyRoomCharge As Double = Convert.ToDouble(dt.Compute("SUM(MonthlyRoomCharge)", String.Format("ChargeTypeID = {0}", _chargeTypeID)))
                    'TotalMonthlyRoomCharge = temprows(0)("MonthlyRoomCharge") 'IMPORTANT - the code above will assign the correct fee amount for each monthly account, so it's all the same among the same org type
                    Dim totalChargeDays As Double = Convert.ToDouble(dt.Compute("SUM(ChargeDays)", String.Format("ChargeTypeID = {0}", _chargeTypeID)))

                    For Each dr As DataRow In temprows
                        If totalChargeDays > 0 Then
                            Dim finalPercentage As Double = dr.Field(Of Double)("ChargeDays") / totalChargeDays
                            dr("MonthlyRoomCharge") = TotalMonthlyRoomCharge * finalPercentage
                        Else
                            dr("MonthlyRoomCharge") = 0
                        End If
                    Next
                End If
            Next
        End If
    End Sub

    Private Function GetChargeDaysFromMultiGrid(ByVal OrgID As String, ByVal AccountID As String) As Double
        Dim gv As GridView = Nothing
        Dim result As Double = 0

        For Each item As RepeaterItem In rptMultiOrg.Items
            Dim hid As HtmlInputHidden = CType(item.FindControl("hidOrgID"), HtmlInputHidden)
            If hid.Value = OrgID Then
                gv = CType(item.FindControl("gvOrg"), GridView)
                Exit For
            End If
        Next

        If gv IsNot Nothing Then
            Dim gvr As GridViewRow = gv.Rows(0)
            Dim txt As TextBox = CType(gvr.FindControl("txt" + AccountID), TextBox)
            If txt IsNot Nothing Then
                Dim temp As Double = UserUtility.ConvertToDouble(txt.Text, 0)
                If rdoDay.Checked Then
                    result = temp
                Else
                    Dim lbl As Label = CType(gvr.FindControl("lblMinDays"), Label)
                    Dim TotalDays As Double = UserUtility.ConvertToDouble(lbl.Text, 0)
                    result = temp * TotalDays / 100
                End If
            End If
        End If

        Return result
    End Function
End Class