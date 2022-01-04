
Imports System.Web
Imports System.Web.UI
Imports LNF.Billing.Apportionment.Models
Imports LNF.Data
Imports LNF.Web.User

Public MustInherit Class UserPage
    Inherits Page

    Public Property ContextBase As HttpContextBase

    Protected Overrides Sub OnInit(e As EventArgs)
        ContextBase = New HttpContextWrapper(Context)
    End Sub

    Public ReadOnly Property CurrentUser As IPrivileged
        Get
            Return ContextBase.CurrentUser()
        End Get
    End Property
End Class
