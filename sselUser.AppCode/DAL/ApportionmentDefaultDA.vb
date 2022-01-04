Imports LNF.Repository

Namespace DAL
    Public Class ApportionmentDefaultDA
        Public Shared Function GetDataByClientID(ByVal clientId As Integer, ByVal roomId As Integer) As DataTable
            Return DataCommand.Create() _
                .Param("Action", "ByClientIDAndRoomID") _
                .Param("ClientID", clientId > 0, clientId) _
                .Param("RoomID", roomId > 0, roomId) _
                .FillDataTable("dbo.ApportionmentDefault_Select")
        End Function
    End Class
End Namespace