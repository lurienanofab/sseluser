Imports System.Web.UI.WebControls
Imports LNF.Repository

Public Class UserUtility
    Public Shared Function IsWithinBusinessDays(ByVal period As DateTime) As Boolean
        Return DA.Command(CommandType.Text).Param("CurDate", period).ExecuteScalar(Of Boolean)("SELECT dbo.udf_IsWithinBusinessDay(@CurDate, NULL)")
    End Function

    Public Shared Function MakeTextBox(ByVal AccountID As String) As TextBox
        Dim result As New TextBox()
        result.ID = "txt" + AccountID
        result.Width = Unit.Pixel(25)
        result.MaxLength = 4
        result.CssClass = "numeric-text"
        Return result
    End Function

    Public Shared Function YearsData(ByVal Count As Integer) As DataTable
        Dim dt As DataTable = New DataTable()
        dt.Columns.Add("YearValue", GetType(Integer))
        dt.Columns.Add("YearText", GetType(String))
        For y As Integer = Date.Now.Date.Year - Count + 1 To DateTime.Now.Date.Year
            dt.Rows.Add(y, y.ToString())
        Next
        Return dt
    End Function

    Public Shared Function ConvertToDouble(ByVal val As Object, ByVal defval As Double) As Double
        If val Is Nothing Then Return defval
        If val.Equals(DBNull.Value) Then Return defval
        If val.ToString() = String.Empty Then Return defval
        Try
            Return Convert.ToDouble(val)
        Catch
            Return defval
        End Try
    End Function
End Class
