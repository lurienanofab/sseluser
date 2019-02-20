Imports System.IO
Imports LNF.Cache
Imports LNF.Models.Data

Public Class UserMaster
    Inherits MasterPage

    Protected Overrides Sub OnInit(e As EventArgs)
        CacheManager.Current.CheckSession()
        hypApporOld.Visible = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator)

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