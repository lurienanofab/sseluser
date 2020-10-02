Imports System.Reflection
Imports System.Web.Compilation
Imports LNF
Imports LNF.Web

Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(sender As Object, e As EventArgs)
        Dim assemblies As Assembly() = BuildManager.GetReferencedAssemblies().Cast(Of Assembly)().ToArray()
        WebApp.Current.Bootstrap(assemblies)

        If ServiceProvider.Current.IsProduction() Then
            Application("AppServer") = "http://" + Environment.MachineName + ".eecs.umich.edu/"
        Else
            Application("AppServer") = "/"
        End If
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        If Request.IsAuthenticated Then
            Dim ident As FormsIdentity = CType(User.Identity, FormsIdentity)
            Dim roles As String() = ident.Ticket.UserData.Split(Char.Parse("|"))
            Context.User = New Security.Principal.GenericPrincipal(ident, roles)
        End If
    End Sub

    'Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
    '    'Catch the session variable timeout issue first.  For every other types of error, we have to log them
    '    If InStr(Server.GetLastError.InnerException.GetType().ToString, "NullReferenceException") > 0 Then
    '        Session.Abandon()
    '        Server.Transfer("sselUser.aspx")
    '    Else
    '        Dim errorMsg As String = "Error message: " + Server.GetLastError().Message.ToString + "<br /><br />" + _
    '"Inner exception: " + Server.GetLastError().InnerException.ToString + "<br /><br />" + _
    '"Stack trace: " + Server.GetLastError().StackTrace.ToString + "<br /><br />"

    '        Dim cnSselData As New SqlConnection(WebConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)

    '        Dim cmdPassError_Select As New SqlCommand("PassError_Select", cnSselData)
    '        cmdPassError_Select.CommandType = CommandType.StoredProcedure
    '        cmdPassError_Select.Parameters.AddWithValue("@Action", "Set")
    '        cmdPassError_Select.Parameters.AddWithValue("@ErrorMSG", errorMsg)
    '        cmdPassError_Select.Parameters.AddWithValue("@ClientName", Session("DisplayName"))
    '        cmdPassError_Select.Parameters.AddWithValue("@FilePath", Context.Request.FilePath)
    '        cnSselData.Open()
    '        Dim ErrorGuid As Guid = CType(cmdPassError_Select.ExecuteScalar, Guid)
    '        cnSselData.Close()

    '        Context.ClearError()
    '        Response.Redirect(CStr(Application("AppServer")) + "sselOnLine/ErrorPage.aspx?ErrorID=" + ErrorGuid.ToString)
    '    End If
    'End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        'LNF.Repository.Data.Client.CheckSession()
    End Sub
End Class