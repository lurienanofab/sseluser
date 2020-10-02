Imports LNF.Repository

Namespace DAL
    Public Class RoomDA
        ''' <summary>
        ''' Check if this room is a passback room
        ''' </summary>
        Public Shared Function IsAntiPassbackRoom(ByVal roomId As Integer) As Boolean
            Using dr As ExecuteReaderResult = DA.Command().Param("Action", "IsPassback").Param("RoomID", roomId).ExecuteReader("dbo.Room_Select")
                If dr.Read() Then
                    Dim result As Boolean
                    Try
                        result = Convert.ToBoolean(dr("PassbackRoom"))
                        If result Then
                            Return True
                        Else
                            Return False
                        End If
                    Catch ex As Exception
                        dr.Close()
                        Return False
                    End Try
                Else
                    dr.Close()
                    Return False
                End If
            End Using
        End Function

        Public Shared Function GetAllActiveRooms() As DataTable
            Return DA.Command().Param("Action", "ForEditing").FillDataTable("dbo.Room_Select")
        End Function
    End Class
End Namespace