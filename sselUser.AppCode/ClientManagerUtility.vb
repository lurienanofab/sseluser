Imports LNF.Repository

Public Class ClientManagerUtility
    Public Shared Function GetAllClientsByDateAndPrivs(ByVal sDate As DateTime, ByVal eDate As DateTime, ByVal privs As Integer) As DataTable
        Return DA.Command() _
            .Param("Action", "All") _
            .Param("sDate", sDate) _
            .Param("eDate", eDate) _
            .Param("Privs", privs) _
            .FillDataTable("dbo.Client_Select")
    End Function

    Public Shared Function GetClientsByManagerID(ByVal sDate As DateTime, ByVal eDate As DateTime, ByVal clientId As Integer) As DataTable
        Return DA.Command() _
            .Param("Action", "ByMgr") _
            .Param("sDate", sDate) _
            .Param("eDate", eDate) _
            .Param("ClientID", clientId) _
            .FillDataTable("dbo.Client_Select")
    End Function
End Class
