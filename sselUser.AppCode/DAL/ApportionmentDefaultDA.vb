Imports LNF.Repository

Namespace DAL
    Public Class ApportionmentDefaultDA
        Public Shared Function GetDataByClientID(ByVal clientId As Integer, ByVal roomId As Integer) As DataTable
            Return DA.Command() _
                .Param("Action", "ByClientIDAndRoomID") _
                .Param("ClientID", clientId) _
                .Param("RoomID", roomId) _
                .FillDataTable("dbo.ApportionmentDefault_Select")
        End Function
    End Class
End Namespace