Imports LNF
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Web

Public Class _500
    Inherits Page

    Public Property ContextBase As HttpContextBase

    Public ReadOnly Property Provider As IProvider
        Get
            Return WebApp.Current.GetInstance(Of IProvider)()
        End Get
    End Property


    Public Function CurrentUser() As IClient
        Return ContextBase.CurrentUser(Provider)
    End Function

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
        litMessage.Text = $"{ex.Message}"
        phStackTrace.Visible = True
        litStackTrace.Text = ex.StackTrace
        SendEmail.SendErrorEmail(ex, Nothing, CurrentUser(), "sselUser", ContextBase.CurrentIP(), ContextBase.Request.Url)
    End Sub
End Class