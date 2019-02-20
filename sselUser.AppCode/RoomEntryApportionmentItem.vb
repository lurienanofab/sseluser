Imports LNF
Imports LNF.Billing
Imports LNF.Repository.Data

Public Class RoomEntryApportionmentItem
    Private Shared ReadOnly Property ApportionmentManager As IApportionmentManager = ServiceProvider.Current.Use(Of IApportionmentManager)()

    Public Property Period As DateTime
    Public Property ClientID As Integer
    Public Property RoomID As Integer
    Public Property RoomName As String
    Public Property DisplayName As String
    Public Property TotalEntries As Double

    Public Shared Function Create(period As DateTime, clientId As Integer, r As Room) As RoomEntryApportionmentItem
        Dim result As New RoomEntryApportionmentItem()
        result.Period = period
        result.ClientID = clientId
        result.RoomID = r.RoomID
        result.RoomName = r.RoomName
        result.DisplayName = If(String.IsNullOrEmpty(r.DisplayName), r.RoomName, r.DisplayName)
        result.TotalEntries = ApportionmentManager.GetTotalEntries(period, clientId, r.RoomID)
        Return result
    End Function
End Class
