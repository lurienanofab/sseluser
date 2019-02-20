Imports LNF.Repository

Namespace DAL
    Public Class RoomApportionDataDA
        ''' <summary>
        ''' Get this month's non antipassback room apportionment data, the return table is a pivot table of RoomApportionData
        ''' </summary>
        Public Shared Function GetApportionmenetPivot(ByVal clientId As Integer, ByVal roomId As Integer, ByVal period As DateTime) As DataTable
            Return DA.Command() _
                .Param("Action", "SelectPivot") _
                .Param("ClientID", clientId) _
                .Param("RoomID", roomId) _
                .Param("Period", period) _
                .FillDataTable("dbo.RoomApportionData_Select")
        End Function

        Public Shared Function GetApportionmentData(ByVal clientId As Integer, ByVal roomId As Integer, ByVal period As DateTime) As DataTable
            Return DA.Command() _
                .Param("Action", "SelectByPeriodAndClientID") _
                .Param("ClientID", clientId) _
                .Param("RoomID", roomId) _
                .Param("Period", period) _
                .FillDataTable("dbo.RoomApportionData_Select")
        End Function
    End Class
End Namespace