Imports System.Data.SqlClient
Imports LNF.Billing.Apportionment
Imports LNF.Data
Imports sselUser.AppCode
Imports sselUser.AppCode.DAL

Public Class ApportionmentDefault
    Inherits UserPage

    Private dtActiveAccounts As DataTable
    Private dtDefault As DataTable

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            LoadUsers()
            LoadRooms()
        End If
    End Sub

    Private Sub LoadUsers()
        ddlUser.DataSource = GetActiveUsers()
        ddlUser.DataBind()
        ddlUser.SelectedValue = CurrentUser.ClientID.ToString()
        ddlUser.Enabled = CurrentUser.IsStaff()
    End Sub

    Private Function GetActiveUsers() As IEnumerable(Of IPrivileged)
        Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
            Dim repo As New Repository(conn)
            Return repo.GetActiveApportionmentClients().OrderBy(Function(x) x.DisplayName).ToList()
        End Using
    End Function

    Private Function GetRooms() As IEnumerable(Of RoomApportionmentItem)
        Dim dt As DataTable = RoomDA.GetAllActiveRooms()

        Dim result As New List(Of RoomApportionmentItem)

        Dim sortOrder As Integer = 1

        Dim parents As DataRow() = dt.Select("ParentID IS NULL", "Room")

        For Each dr As DataRow In parents
            Dim item As New RoomApportionmentItem() With {
                .RoomID = dr.Field(Of Integer)("RoomID"),
                .RoomName = UserUtility.GetRoomName(dr),
                .IsParent = True,
                .SortOrder = sortOrder
            }
            result.Add(item)
            sortOrder += 1
        Next

        Dim children As DataRow() = dt.Select("ParentID IS NOT NULL", "Room")

        For Each dr As DataRow In children
            Dim item As New RoomApportionmentItem() With {
                .RoomID = dr.Field(Of Integer)("RoomID"),
                .RoomName = UserUtility.GetRoomName(dr),
                .IsParent = False,
                .SortOrder = sortOrder
            }
            result.Add(item)
            sortOrder += 1
        Next

        Return result
    End Function

    Private Function GetSelectedClientID() As Integer
        Dim selectedValue As String = ddlUser.SelectedValue
        Dim result As Integer = Integer.Parse(selectedValue)
        Return result
    End Function

    Private Sub LoadRooms()
        Dim clientId As Integer = GetSelectedClientID()

        phAccounts.Visible = False
        litDebug.Text = String.Empty

        Dim rooms As IEnumerable(Of RoomApportionmentItem) = GetRooms()

        If rooms.Count() > 0 Then
            dtActiveAccounts = ClientAccountDA.GetAllActiveAccountsByClientID(clientId)
            dtDefault = ApportionmentDefaultDA.GetDataByClientID(clientId, 0)
            If dtActiveAccounts.Rows.Count > 0 Then
                phAccounts.Visible = True
                rptRooms.DataSource = rooms
                rptRooms.DataBind()
            Else
                litDebug.Text = "<div><em>No accounts found.</em></div>"
            End If
        Else
            litDebug.Text = "<div><em>No rooms found.</em></div>"
        End If
    End Sub

    Protected Sub BtnGetData_Click(sender As Object, e As EventArgs)
        ShowSaveMessage(Nothing)
        LoadRooms()
    End Sub

    Protected Sub RptRooms_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim rptAccounts As Repeater = CType(e.Item.FindControl("rptAccounts"), Repeater)

            If rptAccounts Is Nothing Then
                Throw New Exception("Cannot find rptAccounts in rptRooms.")
            End If

            Dim roomItem As RoomApportionmentItem = CType(e.Item.DataItem, RoomApportionmentItem)

            Dim clientId As Integer = GetSelectedClientID()
            Dim roomId As Integer = roomItem.RoomID

            Dim items As IEnumerable(Of AccountApportionmentItem) = GetAccountApportionmentItems(clientId, roomId)

            rptAccounts.DataSource = items
            rptAccounts.DataBind()
        End If
    End Sub

    Protected Sub BtnSave_Click(sender As Object, e As EventArgs)
        Try
            ShowSaveMessage(Nothing)

            Dim items As New List(Of Models.DefaultApportionment)

            For Each rItem As RepeaterItem In rptRooms.Items
                Dim totalPct As Double = 0

                Dim hidRoomID As HiddenField = CType(rItem.FindControl("hidRoomID"), HiddenField)
                Dim lblRoomName As Label = CType(rItem.FindControl("lblRoomName"), Label)
                Dim rptAccounts As Repeater = CType(rItem.FindControl("rptAccounts"), Repeater)

                Dim roomId As Integer = Convert.ToInt32(hidRoomID.Value)
                Dim roomName As String = lblRoomName.Text

                For Each aItem As RepeaterItem In rptAccounts.Items
                    Dim hidAccountID As HiddenField = CType(aItem.FindControl("hidAccountID"), HiddenField)
                    Dim txtPercentage As TextBox = CType(aItem.FindControl("txtPercentage"), TextBox)

                    Dim accountId As Integer = Convert.ToInt32(hidAccountID.Value)
                    Dim pct As Double
                    If Not Double.TryParse(txtPercentage.Text, pct) Then
                        txtPercentage.Text = "0"
                    End If

                    items.Add(New Models.DefaultApportionment With {.RoomID = roomId, .AccountID = accountId, .Percentage = pct})

                    totalPct += pct
                Next

                If totalPct <> 100D Then
                    Throw New Exception($"The total for room <strong>{roomName}</strong> is not 100.")
                End If
            Next

            Using conn = New SqlConnection(ConfigurationManager.ConnectionStrings("cnSselData").ConnectionString)
                Dim repo As New Repository(conn)
                repo.SaveDefaultApportionment(GetSelectedClientID(), items)
            End Using

            ShowSaveMessage("Saved OK!", "success")
        Catch ex As Exception
            ShowSaveMessage(ex.Message)
        End Try
    End Sub

    Private Sub ShowSaveMessage(msg As String, Optional alertType As String = "danger")
        If String.IsNullOrEmpty(msg) Then
            phSaveMessage.Visible = False
            litSaveMessage.Text = String.Empty
            divSaveSaveMessage.Attributes("class") = "alert"
        Else
            phSaveMessage.Visible = True
            litSaveMessage.Text = msg
            divSaveSaveMessage.Attributes("class") = $"alert alert-{alertType}"
        End If
    End Sub

    Private Function GetAccountApportionmentItems(clientId As Integer, roomId As Integer) As IEnumerable(Of AccountApportionmentItem)
        Dim result As New List(Of AccountApportionmentItem)

        For Each dr As DataRow In dtActiveAccounts.Rows
            Dim accountName As String = dr("Name").ToString()
            Dim accountId As Integer = dr.Field(Of Integer)("AccountID")
            Dim rows As DataRow() = dtDefault.Select($"ClientID = {clientId} AND RoomID = {roomId} AND AccountID = {accountId}")
            Dim pct As Double = 0

            If rows.Length > 0 Then
                pct = rows(0).Field(Of Double)("Percentage")
            End If

            result.Add(New AccountApportionmentItem With {.AccountID = accountId, .AccountName = accountName, .Percentage = pct})
        Next

        Return result
    End Function
End Class

Public Class AccountApportionmentItem
    Public Property AccountID As Integer
    Public Property AccountName As String
    Public Property Percentage As Double
End Class