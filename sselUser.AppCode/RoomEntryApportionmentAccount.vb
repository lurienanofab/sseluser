Imports LNF
Imports LNF.Billing
Imports LNF.Repository
Imports LNF.Repository.Data

Public Class RoomEntryApportionmentAccount
    Private Shared ReadOnly Property ApportionmentManager As IApportionmentManager = ServiceProvider.Current.Use(Of IApportionmentManager)()

    Public Property Period As DateTime
    Public Property ClientID As Integer
    Public Property RoomID As Integer
    Public Property AccountID As Integer
    Public Property AccountName As String
    Public Property ShortCode As String
    Public Property OrgID As Integer
    Public Property OrgName As String
    Public Property Entries As Double
    Public Property DefaultPercentage As Double

    Public Shared Function Create(item As RoomEntryApportionmentItem, accountId As Integer) As RoomEntryApportionmentAccount

        Dim acct As Account = DA.Current.Single(Of Account)(accountId)

        If acct Is Nothing Then
            Throw New Exception(String.Format("Cannot find Account record with AccountID = {0}", accountId))
        End If

        Dim result As New RoomEntryApportionmentAccount()
        result.Period = item.Period
        result.ClientID = item.ClientID
        result.RoomID = item.RoomID
        result.AccountID = acct.AccountID
        result.AccountName = acct.Name
        result.ShortCode = acct.ShortCode
        result.OrgID = acct.Org.OrgID
        result.OrgName = acct.Org.OrgName
        result.Entries = ApportionmentManager.GetAccountEntries(item.Period, item.ClientID, item.RoomID, acct.AccountID)
        result.DefaultPercentage = ApportionmentManager.GetDefaultApportionmentPercentage(item.ClientID, item.RoomID, acct.AccountID)
        Return result
    End Function
End Class