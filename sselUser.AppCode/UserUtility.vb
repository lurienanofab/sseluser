Imports System.Web.UI.WebControls
Imports LNF.Billing.Apportionment.Models
Imports LNF.Repository

Public Class UserUtility
    Public Shared Function IsWithinBusinessDays(period As Date) As Boolean
        Return DataCommand.Create(CommandType.Text).Param("CurDate", period).ExecuteScalar(Of Boolean)("SELECT dbo.udf_IsWithinBusinessDay(@CurDate, NULL)").Value
    End Function

    Public Shared Function MakeTextBox(AccountID As String) As TextBox
        Dim result As New TextBox With {
            .ID = "txt" + AccountID,
            .Width = Unit.Pixel(25),
            .MaxLength = 4,
            .CssClass = "numeric-text"
        }

        Return result
    End Function

    Public Shared Function YearsData(Count As Integer) As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("YearValue", GetType(Integer))
        dt.Columns.Add("YearText", GetType(String))
        For y As Integer = Date.Now.Date.Year - Count + 1 To Date.Now.Date.Year
            dt.Rows.Add(y, y.ToString())
        Next
        Return dt
    End Function

    Public Shared Function ConvertToDouble(val As Object, defval As Double) As Double
        If val Is Nothing Then Return defval
        If val.Equals(DBNull.Value) Then Return defval
        If val.ToString() = String.Empty Then Return defval
        Try
            Return Convert.ToDouble(val)
        Catch
            Return defval
        End Try
    End Function

    ''' <summary>
    ''' Adds either 'Daily Fee' or 'Entry Fee' to the room name depending on if the room configuration.
    ''' </summary>
    Public Shared Function GetRoomName(dr As DataRow) As String
        '[2020-01-10 jg] Per discussion with sandrine: Add appropriate suffix depending on room ApportionDailyFee and ApportionEntryFee fields.
        '   Add "Daily Fee" if room is ApportionDailyFee only
        '   Add "Entry Fee" if room is ApportionEntryFee only
        '   Do not add anything if both true or both false.

        Dim apportionDailyFee As Boolean = dr.Field(Of Boolean)("ApportionDailyFee")
        Dim apportionEntryFee As Boolean = dr.Field(Of Boolean)("ApportionEntryFee")

        Dim roomName As String = dr.Field(Of String)("Room")
        Dim displayName As String = dr.Field(Of String)("DisplayName")

        Dim result As String

        If String.IsNullOrEmpty(displayName) Then
            result = roomName
        Else
            result = displayName
        End If

        If apportionDailyFee AndAlso Not apportionEntryFee Then
            result += " (Daily Fee)"
        End If

        If apportionEntryFee AndAlso Not apportionDailyFee Then
            result += " (Entry Fee)"
        End If

        Return result
    End Function

    Public Shared Function GetRoomName(room As ApportionmentRoom) As String
        Dim result As String = room.RoomName

        If room.ApportionDailyFee AndAlso Not room.ApportionEntryFee Then
            result += " (Daily Fee)"
        End If

        If room.ApportionEntryFee AndAlso Not room.ApportionDailyFee Then
            result += " (Entry Fee)"
        End If

        Return result
    End Function
End Class
