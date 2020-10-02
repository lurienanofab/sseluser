
Imports System.Web
Imports System.Web.UI
Imports LNF
Imports LNF.Data
Imports LNF.Web

Public MustInherit Class UserPage
    Inherits Page

    <Inject> Public Property Provider As IProvider

    Public Property ContextBase As HttpContextBase

    Public Property Helper As ContextHelper

    Protected Overrides Sub OnInit(e As EventArgs)
        ContextBase = New HttpContextWrapper(Context)
        Helper = New ContextHelper(ContextBase, Provider)
        Helper.CheckSession()
    End Sub

    Public ReadOnly Property CurrentUser As IClient
        Get
            Return Helper.CurrentUser()
        End Get
    End Property
End Class
