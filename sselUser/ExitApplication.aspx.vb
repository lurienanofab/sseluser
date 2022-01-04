Public Class ExitApplication
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim exitApplicationUrl = ConfigurationManager.AppSettings("ExitApplicationUrl")
        If String.IsNullOrEmpty(exitApplicationUrl) Then
            Throw New Exception("Missing required appSetting: ExitApplicationUrl")
        Else
            Response.Redirect(exitApplicationUrl)
        End If
    End Sub

End Class