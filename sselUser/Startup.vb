Imports LNF.Impl.DataAccess
Imports LNF.Web
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        Dim sessionManager As ISessionManager = WebApp.Current.GetInstance(Of ISessionManager)()
        app.Use(GetType(NHibernateMiddleware), sessionManager)
    End Sub
End Class
