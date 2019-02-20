Imports LNF.Repository

Namespace DAL
    Public Class ClientAccountDA
        Public Shared Function GetAllActiveAccountsByClientID(ByVal clientId As Integer) As DataTable
            Return DA.Command() _
                .Param("Action", "ByClient") _
                .Param("ClientID", clientId) _
                .FillDataTable("dbo.ClientAccount_Select")
        End Function
    End Class
End Namespace