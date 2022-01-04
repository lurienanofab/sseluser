Imports LNF.Repository

Namespace DAL
    Public Class BillingTypeDA
        Public Shared Function GetBillingTypeID(ByVal clientId As Integer) As DataTable
            Return DataCommand.Create() _
                .Param("Action", "GetCurrentTypeIDByClientID") _
                .Param("ClientID", clientId) _
                .FillDataTable("dbo.ClientOrgBillingTypeTS_Select")
        End Function
    End Class
End Namespace
