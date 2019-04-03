Imports LNF
Imports LNF.Models.Billing
Imports LNF.Repository
Imports LNF.Repository.Data

Public Class RoomEntryApportionmentAccount
    Private Shared ReadOnly Property ApportionmentManager As IApportionmentManager = ServiceProvider.Current.Billing.ApportionmentManager

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

        Dim result As New RoomEntryApportionmentAccount With {
            .Period = item.Period,
            .ClientID = item.ClientID,
            .RoomID = item.RoomID,
            .AccountID = acct.AccountID,
            .AccountName = acct.Name,
            .ShortCode = acct.ShortCode,
            .OrgID = acct.Org.OrgID,
            .OrgName = acct.Org.OrgName,
            .Entries = ApportionmentManager.GetAccountEntries(item.Period, item.ClientID, item.RoomID, acct.AccountID),
            .DefaultPercentage = ApportionmentManager.GetDefaultApportionmentPercentage(item.ClientID, item.RoomID, acct.AccountID)
        }
        Return result
    End Function
End Class