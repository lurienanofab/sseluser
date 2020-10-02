Imports sselUser.AppCode

Public Class Index
    Inherits UserPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Redirect("~/ApportionmentStep1.aspx")
    End Sub

End Class