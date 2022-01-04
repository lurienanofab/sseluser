Imports System.Configuration
Imports System.Data.SqlClient
Imports LNF
Imports LNF.Billing.Apportionment
Imports LNF.CommonTools

Namespace DAL
    Public Class RoomApportionmentInDaysMonthlyDA
        Public Shared Function SelectRoomBilling(period As Date, clientId As Integer, roomId As Integer) As DataRow()
            Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
                Dim repo As New Repository(conn)
                Dim dt As DataTable = repo.GetRoomBillingByPeriod(period)
                Dim rows As DataRow() = dt.Select($"ClientID = {clientId} AND RoomID = {roomId}")
                Return rows
            End Using
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

            Dim rows As DataRow() = SelectRoomBilling(period, clientId, roomId)

            For Each dr As DataRow In rows
                Dim ndr As DataRow = dtRoomBilling.NewRow()

                Dim orgId As Integer = dr.Field(Of Integer)("OrgID")
                Dim orgName As String = dr.Field(Of String)("OrgName")
                Dim accountId As Integer = dr.Field(Of Integer)("AccountID")
                Dim accountName As String = dr.Field(Of String)("AccountName")
                Dim accountNumber As String = dr.Field(Of String)("AccountNumber")

                ndr.SetField("AccountID", accountId)
                ndr.SetField("Name", accountName)
                ndr.SetField("Number", accountNumber)
                ndr.SetField("OrgID", orgId)
                ndr.SetField("OrgName", orgName)
                dtRoomBilling.Rows.Add(ndr)
            Next

            Dim orgs = rows.Select(Function(x) New With {.OrgID = x.Field(Of Integer)("OrgID"), .OrgName = x.Field(Of String)("OrgName")}).ToList()
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
            Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
                Dim repo As New Repository(conn)
                Dim result As ArrayList = repo.GetAccountDaysAndPhysicalDays(Period, ClientID, RoomID, AccountID)
                Return result
            End Using
        End Function
    End Class
End Namespace
