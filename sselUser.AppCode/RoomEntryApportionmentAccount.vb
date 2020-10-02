Imports LNF
Imports LNF.Data

Public Class RoomEntryApportionmentAccount
    Public Property Provider As IProvider
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

    Public Shared Function Create(provider As IProvider, item As RoomEntryApportionmentItem, accountId As Integer) As RoomEntryApportionmentAccount
        Dim acct As IAccount = provider.Data.Account.GetAccount(accountId)

        If acct Is Nothing Then
            Throw New ItemNotFoundException("Account", "AccountID", accountId)
        End If

        Dim _entries As Double = provider.Billing.Apportionment.GetAccountEntries(item.Period, item.ClientID, item.RoomID, acct.AccountID)
        Dim _defaultPercentage As Double = provider.Billing.Apportionment.GetDefaultApportionmentPercentage(item.ClientID, item.RoomID, acct.AccountID)

        Dim result As New RoomEntryApportionmentAccount With {
            .Period = item.Period,
            .ClientID = item.ClientID,
            .RoomID = item.RoomID,
            .AccountID = acct.AccountID,
            .AccountName = acct.AccountName,
            .ShortCode = acct.ShortCode,
            .OrgID = acct.OrgID,
            .OrgName = acct.OrgName,
            .Entries = _entries,
            .DefaultPercentage = _defaultPercentage
        }
        Return result
    End Function
End Class