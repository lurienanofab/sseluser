Imports LNF.Impl
Imports Owin

Public Class Startup

    Public Sub Configuration(app As IAppBuilder)
        app.Use(GetType(ServiceMiddleware))
    End Sub
End Class
