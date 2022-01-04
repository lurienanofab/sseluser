Imports System.IO
Imports LNF.Data
Imports sselUser.AppCode

Public Class UserMaster
    Inherits MasterPage

    Public ReadOnly Property UserPage As UserPage
        Get
            Return CType(Page, UserPage)
        End Get
    End Property

    Protected ReadOnly Property CurrentUser As IPrivileged
        Get
            Return UserPage.CurrentUser
        End Get
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
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
    End Sub

    Protected Function IsProduction() As Boolean
        Return LNF.Configuration.Current.Production
    End Function
End Class