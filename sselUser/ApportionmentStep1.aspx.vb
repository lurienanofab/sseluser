Imports LNF.Billing
Imports LNF.CommonTools
Imports LNF.Repository
Imports sselUser.AppCode
Imports sselUser.AppCode.DAL

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
        hypViewEntryApportionment.NavigateUrl = String.Format("~/ApportionmentStep2.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}", UserID, RoomID, Month, Year)
    End Sub

    Protected Overrides Sub OnSave(e As CommandEventArgs)
        If SaveMultiOrg() Then
            Response.Redirect(String.Format("~/ApportionmentStep2.aspx?UserID={0}&RoomID={1}&Month={2}&Year={3}&Recalculate={4}", UserID, RoomID, Month, Year, chkBilling.Checked), False)
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
                lblOrgMsg.Text = "The sum cannot be less than " + minday.ToString() + " day" + If(minday.Equals(1), String.Empty, "s") + "."
                lblOrgMsg.Visible = True
                fail = True
            ElseIf usrday > physicalDays Then
                lblOrgMsg.Text = "The sum cannot exceed your lab physical days (" + physicalDays.ToString() + ")."
                lblOrgMsg.Visible = True
                fail = True
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

        'Step 2: After checking is finished, we update the data.
        'Get the necessary data that needs to be updated into this table
        Dim dsFromDB As DataSet = RoomApportionmentInDaysMonthlyDA.GetData(Period, UserID, RoomID)
        Dim dtApportFromDB As DataTable = dsFromDB.Tables(0)
        Dim dtUserApportFromDB As DataTable = dsFromDB.Tables(2) 'From RoomBillingUserApportionData

        'modify current row and add new rows if necessary
        Dim totalHours As Double = UserUtility.ConvertToDouble(dtApportFromDB.Compute("SUM(Hours)", ""), 0)
        Dim totalEntries As Double = UserUtility.ConvertToDouble(dtApportFromDB.Compute("SUM(Entries)", ""), 0)
        For Each dr As DataRow In dtApportFromDB.Rows
            Dim amount As Double = GetChargeDaysFromMultiGrid(dr("OrgID").ToString(), dr("AccountID").ToString())
            dr("ChargeDays") = amount
            dr("IsDefault") = False
            dr("Entries") = totalEntries * (UserUtility.ConvertToDouble(dr("ChargeDays"), 0) / totalUserDays)
            dr("Hours") = totalHours * (UserUtility.ConvertToDouble(dr("ChargeDays"), 0) / totalUserDays)

            'We have to save the data back to RoomBillingUserApportionData
            Dim drUserData As DataRow() = dtUserApportFromDB.Select("AccountID = " + dr("AccountID").ToString())
            If drUserData.Length = 0 Then
                Dim newrow As DataRow = dtUserApportFromDB.NewRow()

                newrow("Period") = dr("Period")
                newrow("ClientID") = dr("ClientID")
                newrow("RoomID") = dr("RoomID")
                newrow("AccountID") = dr("AccountID")
                newrow("ChargeDays") = dr("ChargeDays")
                newrow("Entries") = 0 'should be 0 in step 1, a value is assigned in step 2

                dtUserApportFromDB.Rows.Add(newrow)
            Else
                drUserData(0)("ChargeDays") = dr("ChargeDays")
            End If
        Next

        'we have to recalculate the MonthlyRoomCharge - only apply to people who still pay monthly usage fee
        'Remove this code after 2010/07/01
        If RoomID = 6 Then
            Dim array() As Integer = {5, 15, 25}
            Dim TotalMonthlyRoomCharge As Double = 0
            Dim totalChargeDays As Double = 0
            Dim finalPercentage As Double = 0
            'we loop three times based on chargetype, and for each charge type, we find out the monthly fee independently
            For Each _chargeTypeID As Integer In array
                Dim temprows() As DataRow = dtApportFromDB.Select(String.Format("ChargeTypeID = {0}", _chargeTypeID))

                If temprows.Length > 0 Then
                    TotalMonthlyRoomCharge = Convert.ToDouble(dtApportFromDB.Compute("SUM(MonthlyRoomCharge)", String.Format("ChargeTypeID = {0}", _chargeTypeID))) 'we have to get the total sum of clean room monthly fee, and then divide the sum proportionally in next line of code
                    'TotalMonthlyRoomCharge = temprows(0)("MonthlyRoomCharge") 'IMPORTANT - the code above will assign the correct fee amount for each monthly account, so it's all the same among the same org type
                    totalChargeDays = Convert.ToDouble(dtApportFromDB.Compute("SUM(ChargeDays)", String.Format("ChargeTypeID = {0}", _chargeTypeID)))

                    For Each dr As DataRow In temprows
                        If totalChargeDays > 0 Then
                            finalPercentage = dr.Field(Of Double)("ChargeDays") / totalChargeDays
                            dr("MonthlyRoomCharge") = TotalMonthlyRoomCharge * finalPercentage
                        Else
                            dr("MonthlyRoomCharge") = 0
                        End If
                    Next
                End If
            Next
        End If

        'Save to DB
        Dim act As Action(Of UpdateConfiguration)

        act = Sub(cfg)
                  'Update the data using dateset's batch update feature
                  cfg.Update.SetCommandText("dbo.RoomApportionmentInDaysMonthly_Update")
                  cfg.Update.AddParameter("@AppID", SqlDbType.Int)
                  cfg.Update.AddParameter("@ChargeDays", SqlDbType.Float)
                  cfg.Update.AddParameter("@Entries", SqlDbType.Float)
                  cfg.Update.AddParameter("@Hours", SqlDbType.Float)
                  cfg.Update.AddParameter("@MonthlyRoomCharge", SqlDbType.Float)
                  cfg.Update.AddParameter("@IsDefault", SqlDbType.Bit)
              End Sub

        If DA.Command().Update(dtApportFromDB, act) >= 0 Then
            SetMessage("The apportionment data is saved.")
        Else
            SetMessage("Saving data failed, please contact the administrator (Help menu)")
            Return False
        End If

        'Save to RoomBillingUserApportionData
        act = Sub(cfg)
                  cfg.Insert.SetCommandText("dbo.RoomBillingUserApportionData_Insert")
                  cfg.Insert.AddParameter("Period", SqlDbType.DateTime2)
                  cfg.Insert.AddParameter("ClientID", SqlDbType.Int)
                  cfg.Insert.AddParameter("RoomID", SqlDbType.Int)
                  cfg.Insert.AddParameter("AccountID", SqlDbType.Int)
                  cfg.Insert.AddParameter("ChargeDays", SqlDbType.Float)
                  cfg.Insert.AddParameter("Entries", SqlDbType.Float)

                  cfg.Update.SetCommandText("dbo.RoomBillingUserApportionData_Update")
                  cfg.Update.AddParameter("AppID", SqlDbType.Int)
                  cfg.Update.AddParameter("ChargeDays", SqlDbType.Float)
              End Sub

        DA.Command().Update(dtUserApportFromDB, act)

        'update child room entry apportionment based on room days
        ApportionmentManager.UpdateChildRoomEntryApportionment(Period, UserID, RoomID)

        ' [jg 2016-09-15] This is now handled after the redirect to ApportionmentStep2.aspx
        'If chkBilling.Checked Then
        '    'Update the necessary billing tables
        '    Using billingClient = Await ApiProvider.Current.NewBillingClient()
        '        Dim step1Result As BillingProcessResult = Await billingClient.BillingProcessStep1(OnlineServices.Api.Billing.BillingCategory.Room, Period, Period.AddMonths(1), UserID, 0, False, True)
        '        Dim step4Result As BillingProcessResult = Await billingClient.BillingProcessStep4("subsidy", Period, UserID)
        '    End Using
        'End If

        'Clear out saved Session data so next time we load the data is refreshed
        Session("CurrentAcct") = Nothing
        Session("MultipleOrg") = Nothing
        Session("LabUsageDay") = Nothing

        Return True
    End Function

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