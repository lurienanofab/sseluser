Imports System.Web.UI
Imports System.Web.UI.WebControls

Public Class DynamicTextBoxTemplate
    Implements ITemplate

    Private attributes As New Dictionary(Of String, String)()
    Private _TextBoxID As String
    Private _Width As Unit
    Private _MaxLength As Integer
    Private _CssClass As String

    Public Sub New(ByVal TextBoxID As String, ByVal Width As Unit, ByVal MaxLength As Integer, ByVal CssClass As String)
        _TextBoxID = TextBoxID
        _Width = Width
        _MaxLength = MaxLength
        _CssClass = CssClass
    End Sub

    Public Sub InstantiateIn(ByVal container As Control) Implements ITemplate.InstantiateIn
        Dim txt As TextBox = New TextBox()
        txt.ID = _TextBoxID
        txt.Width = _Width
        txt.MaxLength = _MaxLength
        txt.CssClass = _CssClass
        For Each kvp As KeyValuePair(Of String, String) In attributes
            txt.Attributes.Add(kvp.Key, kvp.Value)
        Next
        container.Controls.Add(txt)
    End Sub

    Public Sub AddAttribute(name As String, value As String)
        attributes.Add(name, value)
    End Sub
End Class
