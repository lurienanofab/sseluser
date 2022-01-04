Imports LNF.Web.User
Imports sselUser.AppCode
Imports System.IO

Public Class UserMasterBootstrap
    Inherits MasterPage

    Public ReadOnly Property UserPage As UserPage
        Get
            Return CType(Page, UserPage)
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        InitSiteMenu()
        ClearNavLinks()
        SetActiveNavLink()
    End Sub

    Private Sub InitSiteMenu()
        litSiteMenu.Text = ApiClient.Current.GetSiteMenu(UserPage.CurrentUser.ClientID)
    End Sub

    Private Sub ClearNavLinks()
        hypLabTimeApportionment.CssClass = "nav-link"
        hypDefaultApportionment.CssClass = "nav-link"
        hypChangePassword.CssClass = "nav-link"
        hypExitApplication.CssClass = "nav-link"
    End Sub

    Private Sub SetActiveNavLink()
        Dim activeLink As HyperLink = Nothing
        Dim pageFileName = Path.GetFileName(Request.Url.AbsolutePath)

        Select Case pageFileName
            Case "ApportionmentStep1.aspx", "ApportionmentStep2.aspx"
                activeLink = hypLabTimeApportionment
            Case "ApportionmentDefault.aspx"
                activeLink = hypDefaultApportionment
            Case "ChangePassword.aspx"
                activeLink = hypChangePassword
            Case "ExitApplication.aspx"
                activeLink = hypExitApplication
        End Select

        If activeLink Is Nothing Then
            Throw New Exception($"Cannot determine navigation link for page: {pageFileName}")
        End If

        activeLink.CssClass = "nav-link active"
    End Sub
End Class