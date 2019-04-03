Imports System.IO
Imports LNF.Models.Data
Imports LNF.Web

Public Class UserMaster
    Inherits MasterPage

    Private _contextBase As HttpContextBase

    Protected ReadOnly Property ContextBase As HttpContextBase
        Get
            Return _contextBase
        End Get
    End Property

    Protected ReadOnly Property CurrentUser As IClient
        Get
            Return ContextBase.CurrentUser()
        End Get
    End Property

    Protected Overrides Sub OnInit(e As EventArgs)
        _contextBase = New HttpContextWrapper(Context)

        ContextBase.CheckSession()

        hypApporOld.Visible = CurrentUser.HasPriv(ClientPrivilege.Administrator)

        Select Case Path.GetFileName(Request.Path)
            Case "ApportionmentStep1.aspx", "ApportionmentStep2.aspx"
                hypAppor.CssClass = "nav-selected"
            Case "ApportionmentDefault.aspx"
                hypApporDef.CssClass = "nav-selected"
            Case "ApportionmentOld.aspx"
                hypApporOld.CssClass = "nav-selected"
            Case "ChangePassword.aspx"
                hypChangePW.CssClass = "nav-selected"
        End Select

        MyBase.OnInit(e)
    End Sub

End Class