Imports LNF.Billing
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Impl.Repository.Billing
Imports LNF.Repository

Namespace DAL
    Public Class RoomApportionmentInDaysMonthlyDA
        ''' <summary>
        ''' This will return a dataset with three tables
        ''' #1 table: This user's apportion data based on room
        ''' #2 table: The actual minimum physical days (by UNION the physical days in lab and account days of using tools)
        ''' #3 table: User's Apportion Data
        ''' </summary>
        Public Shared Function GetData(ByVal period As Date, ByVal clientId As Integer, ByVal roomId As Integer) As DataSet
            Return DefaultDataCommand.Create() _
                .Param("Action", "ForApportion") _
                .Param("Period", period) _
                .Param("ClientID", clientId) _
                .Param("RoomID", roomId) _
                .FillDataSet("dbo.RoomApportionmentInDaysMonthly_Select")
        End Function

        Public Shared Function SelectRoomBilling(period As Date, clientId As Integer, roomId As Integer) As IList(Of RoomBilling)
            Dim result As IList(Of RoomBilling) = DA.Current.Query(Of RoomBilling)().Where(Function(x) x.Period = period AndAlso x.ClientID = clientId AndAlso x.RoomID = roomId).ToList()
            Return result
        End Function

        ''' <summary>
        ''' Return two tables
        ''' 1st table: all accounts used by this user
        ''' 2nd table: all orgs the user belong to, we need these because multiple org could result in different operations
        ''' </summary>
        Public Shared Function GetAllAccountsUsed(ByVal period As Date, ByVal clientId As Integer, ByVal roomId As Integer) As DataSet
            ' [2016-02-03 jg] The logic here is identical to RoomApportionmentInDaysMonthly_Select @Action = 'GetAllAccountsUsed'

            Dim dtRoomBilling As New DataTable()

            dtRoomBilling.Columns.Add("AccountID", GetType(Integer))
            dtRoomBilling.Columns.Add("Name", GetType(String))
            dtRoomBilling.Columns.Add("Number", GetType(String))
            dtRoomBilling.Columns.Add("OrgID", GetType(Integer))
            dtRoomBilling.Columns.Add("OrgName", GetType(String))

            Dim dtOrg As New DataTable()

            dtOrg.Columns.Add("OrgID", GetType(Integer))
            dtOrg.Columns.Add("OrgName", GetType(String))

            Dim query As IList(Of RoomBilling) = SelectRoomBilling(period, clientId, roomId)

            For Each rb As RoomBilling In query
                Dim ndr As DataRow = dtRoomBilling.NewRow()

                Dim org As IOrg = rb.GetOrg()
                Dim acct As IAccount = rb.GetAccount()

                ndr.SetField("AccountID", rb.AccountID)
                ndr.SetField("Name", acct.AccountName)
                ndr.SetField("Number", acct.AccountNumber)
                ndr.SetField("OrgID", rb.OrgID)
                ndr.SetField("OrgName", org.OrgName)
                dtRoomBilling.Rows.Add(ndr)
            Next

            Dim orgs = query.Select(Function(x) New With {x.OrgID, x.GetOrg().OrgName}).ToList()
            Dim distinctOrgs = orgs.Distinct(orgs.CreateEqualityComparer(Function(x, y) x.OrgID = y.OrgID, Function(x) x.OrgID.GetHashCode())).ToList()

            For Each o In distinctOrgs
                Dim ndr As DataRow = dtOrg.NewRow()
                ndr.SetField("OrgID", o.OrgID)
                ndr.SetField("OrgName", o.OrgName)
                dtOrg.Rows.Add(ndr)
            Next

            dtRoomBilling.AcceptChanges()
            dtOrg.AcceptChanges()

            Dim ds As New DataSet()
            ds.Tables.Add(dtRoomBilling)
            ds.Tables.Add(dtOrg)

            Return ds
        End Function

        ''' <summary>
        ''' Get PhysicalDays and AccountDays for this account.  This is used when new account is discovered and has not been added to apportionment table
        ''' </summary>
        Public Shared Function GetAccountDaysAndPhysicalDays(ByVal Period As Date, ByVal ClientID As Integer, ByVal RoomID As Integer, ByVal AccountID As Integer) As ArrayList
            Dim dc As IDataCommand = DefaultDataCommand.Create() _
                .Param("Action", "ForApportion") _
                .Param("Period", Period) _
                .Param("ClientID", ClientID) _
                .Param("RoomID", RoomID) _
                .Param("AccountID", AccountID)

            Using reader As ExecuteReaderResult = dc.ExecuteReader("RoomApportionmentInDaysMonthly_Select")
                If reader.Read() Then
                    Dim arr As New ArrayList From {
                        reader("PhysicalDays"),
                        reader("AccountDays")
                    }

                    Return arr
                Else
                    Return Nothing
                End If
            End Using
        End Function
    End Class
End Namespace
