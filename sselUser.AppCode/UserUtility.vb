Imports System.Web.UI.WebControls
Imports LNF.Data
Imports LNF.Repository

Public Class UserUtility
    Public Shared Function IsWithinBusinessDays(ByVal period As DateTime) As Boolean
        Return DefaultDataCommand.Create(CommandType.Text).Param("CurDate", period).ExecuteScalar(Of Boolean)("SELECT dbo.udf_IsWithinBusinessDay(@CurDate, NULL)").Value
    End Function

    Public Shared Function MakeTextBox(ByVal AccountID As String) As TextBox
        Dim result As New TextBox With {
            .ID = "txt" + AccountID,
            .Width = Unit.Pixel(25),
            .MaxLength = 4,
            .CssClass = "numeric-text"
        }

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

        Dim result As String = dr.Field(Of String)("Room")

        If apportionDailyFee AndAlso Not apportionEntryFee Then
            result += " (Daily Fee)"
        End If

        If apportionEntryFee AndAlso Not apportionDailyFee Then
            result += " (Entry Fee)"
        End If

        Return result
    End Function

    Public Shared Function GetRoomName(room As IRoom) As String
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
