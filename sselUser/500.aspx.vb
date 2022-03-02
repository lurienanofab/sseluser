Imports LNF
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Web.User

Public Class _500
    Inherits Page

    Public Property ContextBase As HttpContextBase

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        ContextBase = New HttpContextWrapper(Context)

        Dim lastEx As Exception = Server.GetLastError()
        If lastEx IsNot Nothing Then
            Dim innerEx = lastEx.InnerException
            If innerEx IsNot Nothing Then
                HandleError(innerEx)
            Else
                HandleError(lastEx)
            End If
        Else
            litMessage.Text = "No error found."
            phStackTrace.Visible = False
            litStackTrace.Text = String.Empty
        End If
    End Sub

    Private Sub HandleError(ex As Exception)
        litMessage.Text = ex.Message
        phStackTrace.Visible = True
        litStackTrace.Text = ex.StackTrace
        Dim client As IPrivileged = Nothing
        If ContextBase.Session IsNot Nothing Then
            client = ContextBase.CurrentUser()
            SendEmail.SendErrorEmail(ex, Nothing, ContextBase.CurrentUser(), "sselUser", ContextBase.CurrentIP(), ContextBase.Request.Url)
        End If
        SendEmail.SendErrorEmail(ex, Nothing, client, "sselUser", ContextBase.CurrentIP(), ContextBase.Request.Url)
    End Sub
End Class