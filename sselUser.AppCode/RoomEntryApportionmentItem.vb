Imports LNF
Imports LNF.Data

Public Class RoomEntryApportionmentItem
    Public Property Period As DateTime
    Public Property ClientID As Integer
    Public Property RoomID As Integer
    Public Property RoomName As String
    Public Property DisplayName As String
    Public Property TotalEntries As Double

    Public Shared Function Create(provider As IProvider, period As Date, clientId As Integer, r As IRoom) As RoomEntryApportionmentItem
        Dim _displayName As String = If(String.IsNullOrEmpty(r.RoomDisplayName), r.RoomName, r.RoomDisplayName)
        Dim _totalEntries As Double = provider.Billing.Apportionment.GetTotalEntries(period, clientId, r.RoomID)

        Dim result As New RoomEntryApportionmentItem With {
            .Period = period,
            .ClientID = clientId,
            .RoomID = r.RoomID,
            .RoomName = r.RoomName,
            .DisplayName = _displayName,
            .TotalEntries = _totalEntries
        }

        Return result
    End Function
End Class
