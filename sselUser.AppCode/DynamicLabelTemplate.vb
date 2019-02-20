Imports System.Web.UI
Imports System.Web.UI.WebControls

Public Class DynamicLabelTemplate
    Implements ITemplate

    Private _LabelID As String
    Private _LabelText As String
    Private _HiddenFieldID As String
    Private _HiddenFieldValue As String

    Public Sub New(ByVal LabelID As String, ByVal LabelText As String, ByVal HiddenFieldID As String, ByVal HiddenFieldValue As String)
        _LabelID = LabelID
        _LabelText = LabelText
        _HiddenFieldID = HiddenFieldID
        _HiddenFieldValue = HiddenFieldValue
    End Sub

    Public Sub InstantiateIn(ByVal container As Control) Implements ITemplate.InstantiateIn
        Dim lbl As New Label()
        lbl.ID = _LabelID
        lbl.Text = _LabelText
        container.Controls.Add(lbl)
        Dim hid As New HiddenField()
        hid.ID = _HiddenFieldID
        hid.Value = _HiddenFieldValue
        container.Controls.Add(hid)
    End Sub
End Class
