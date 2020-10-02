Imports LNF
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Repository
Imports LNF.Web

Public Class ChangePassword
    Inherits Page

    Private _contextBase As HttpContextBase

    <Inject> Public Property Provider As IProvider

    Protected ReadOnly Property ContextBase As HttpContextBase
        Get
            Return _contextBase
        End Get
    End Property

    Protected ReadOnly Property CurrentUser As IClient
        Get
            Return ContextBase.CurrentUser(Provider)
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        _contextBase = New HttpContextWrapper(Context)

        'only these privs can change password
        Dim AuthTypes As ClientPrivilege = ClientPrivilege.LabUser Or ClientPrivilege.Staff Or ClientPrivilege.StoreUser Or ClientPrivilege.Executive Or ClientPrivilege.Administrator Or ClientPrivilege.WebSiteAdmin Or ClientPrivilege.StoreManager Or ClientPrivilege.OnlineAccess

        litAuthMsg.Text = String.Empty
        If Not CurrentUser.HasPriv(AuthTypes) Then
            panDisplay.Visible = False
            litAuthMsg.Text = "<div class=""error"" style=""padding-left: 5px;"">You do not have authorization to change your password. Please contact the system administrator if you have any questions.</div>"
        End If

        If Not Page.IsPostBack Then
            'assumes a password change is forced if any QueryString parameter is present
            If Request.QueryString("ForceChange") IsNot Nothing Then
                Session("ForcePwdChange") = Request.QueryString("ForceChange").Equals("True")
            Else
                Session("ForcePwdChange") = False
            End If
            'hide the vertical navigation menu if a password change is forced
            styHideNav.Visible = Convert.ToBoolean(Session("ForcePwdChange"))
        End If
    End Sub

    Protected Sub BtnSave_Click(ByVal sender As System.Object, ByVal e As EventArgs)
        litChangePasswordMsg.Text = String.Empty

        ' the new passwords are the same, but are they valid?
        If txtNewPassword.Text.Contains(" ") Then
            litChangePasswordMsg.Text = "<div class=""error"">Your password must be at least six characters long with no spaces.</div>"
            Return
        ElseIf txtNewPassword.Text.Trim().Length < 6 Then
            litChangePasswordMsg.Text = "<div class=""error"">Your password must be at least six characters long with no spaces.</div>"
            Return
        ElseIf txtNewPassword.Text.Trim() = Page.User.Identity.Name Then
            litChangePasswordMsg.Text = "<div class=""error"">Your password may not be the same as your username.</div>"
            Return
        ElseIf Not txtNewPassword.Text.Trim().Equals(txtRepeatNew.Text.Trim()) Then
            litChangePasswordMsg.Text = "<div class=""error"">The passwords you entered do not match.</div>"
            Return
        End If

        Dim valid As Boolean = False
        Dim enc As New Encryption()

        'check if record exists in DB
        Using reader As ExecuteReaderResult = DefaultDataCommand.Create().Param("Action", "LoginCheck").Param("Username", Page.User.Identity.Name).Param("Password", enc.EncryptText(txtOldPassword.Text.Trim())).ExecuteReader("dbo.Client_CheckAuth")
            'will return false when a bad password is supplied
            valid = reader.Read()
            reader.Close()
        End Using

        If valid Then
            DefaultDataCommand.Create() _
                .Param("Action", "pwUpdate") _
                .Param("Username", User.Identity.Name) _
                .Param("Password", enc.EncryptText(txtNewPassword.Text.Trim())) _
                .ExecuteNonQuery("dbo.Client_Update")

            If Convert.ToBoolean(Session("ForcePwdChange")) Then
                Session.Remove("ForcePwdChange")
                Response.Redirect(ConfigurationManager.AppSettings("ChangePasswordRedirect"))
            Else
                litChangePasswordMsg.Text = "<div style=""color: #003366; font-weight: bold; font-size: 14pt;"">Your password has been changed successfully.</div>"
            End If
        Else
            litChangePasswordMsg.Text = "<div class=""error"">Your old password is not correct. Please try again or contact the system adminstrator if you have any questions.</div>"
        End If
    End Sub
End Class