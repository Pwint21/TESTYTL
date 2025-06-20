Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json
Public Class GetGeofenceData
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim operation As String = Request.QueryString("op")
        Select Case operation
            Case "0"
                Response.Write(loadOssShipToCodeData())
            Case "1"
                Response.Write(loadAvlsGeofenceData())
            Case "2"
                Response.Write(UpdateOssNewShipToCode(Request.QueryString("geofenceID"), Request.QueryString("newSTC"), Request.QueryString("name")))
        End Select
        Response.ContentType = "text/plain"
    End Sub

    Public Function loadOssShipToCodeData() As String
        Dim aa As New ArrayList()
        Dim a As ArrayList
        Dim json As String = ""
        Dim firstpoint As String = ""
        Dim geofencetable As New DataTable
        geofencetable.Rows.Clear()
        geofencetable.Columns.Add(New DataColumn("S No"))
        geofencetable.Columns.Add(New DataColumn("Name"))
        geofencetable.Columns.Add(New DataColumn("Ship To Code"))
        geofencetable.Columns.Add(New DataColumn("Address"))
        geofencetable.Columns.Add(New DataColumn(""))
        Try
            Dim userslist As String = Request.Cookies("userinfo")("userslist")
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim cmd As New SqlCommand()
            cmd.Connection = conn
            cmd.CommandText = "select * from oss_ship_to_code where OssSTC=0 order by name"
            Try
                conn.Open()
                Dim r As DataRow
                Dim dr As SqlDataReader = cmd.ExecuteReader
                While dr.Read
                    r = geofencetable.NewRow
                    r(0) = dr("shiptocode")
                    r(1) = dr("name").ToString
                    r(2) = dr("shiptocode")
                    If IsDBNull(dr("address5")) Then
                        r(3) = ""
                    ElseIf dr("address5") = "" Then
                        r(3) = ""
                    Else
                        r(3) = dr("address5").ToString.Replace(vbCr, " ").Replace(vbLf, " ")
                    End If
                    r(4) = "<a href=""javascript:void(0)"" onclick=""MoveShipToCode('" & dr("shiptocode").ToString & "','" & dr("name").ToString & "')"" title=""Copy ShipToCode"" id='" & dr("shiptocode").ToString & "a'><img src='images/rightarrow.png' alt='right' style='width:12px;height:12px;' id='" & dr("shiptocode").ToString & "i'/></a>"
                    geofencetable.Rows.Add(r)
                End While
                If geofencetable.Rows.Count = 0 Then
                    r = geofencetable.NewRow
                    r(0) = 1
                    r(1) = "-"
                    r(2) = "-"
                    r(3) = "-"
                    r(4) = "-"
                    geofencetable.Rows.Add(r)
                End If
                For i As Integer = 0 To geofencetable.Rows.Count - 1
                    Try
                        a = New ArrayList
                        a.Add(geofencetable.DefaultView.Item(i)(0))
                        a.Add(geofencetable.DefaultView.Item(i)(1))
                        a.Add(geofencetable.DefaultView.Item(i)(2))
                        a.Add(geofencetable.DefaultView.Item(i)(3))
                        a.Add(geofencetable.DefaultView.Item(i)(4))
                        aa.Add(a)
                    Catch ex As Exception

                    End Try
                Next
            Catch ex As Exception

            Finally
                conn.Close()
            End Try

        Catch ex As Exception

        End Try
        json = JsonConvert.SerializeObject(aa, Formatting.None)
        Return json
    End Function

    Public Function loadAvlsGeofenceData() As String
        Dim aa As New ArrayList()
        Dim a As ArrayList
        Dim json As String = ""
        Dim firstpoint As String = ""
        Dim geofencetable As New DataTable
        geofencetable.Rows.Clear()
        geofencetable.Columns.Add(New DataColumn("S No"))
        geofencetable.Columns.Add(New DataColumn("Geofence Name"))
        geofencetable.Columns.Add(New DataColumn("Address"))
        geofencetable.Columns.Add(New DataColumn("ShipToCode"))
        Try
            Dim userslist As String = Request.Cookies("userinfo")("userslist")
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim cmd As New SqlCommand()
            cmd.Connection = conn
            cmd.CommandText = "select * from geofence order by geofencename"
            Try
                conn.Open()
                Dim r As DataRow
                Dim dr As SqlDataReader = cmd.ExecuteReader
                While dr.Read
                    r = geofencetable.NewRow
                    Dim latlng As String = ""
                    If (dr("geofencetype")) Then
                        latlng = dr("data").ToString.Split(";")(0)
                        latlng = latlng.Split(",")(1) & "," & latlng.Split(",")(0)
                    Else
                        latlng = dr("data").ToString.Split(",")(1) & "," & dr("data").ToString.Split(",")(0)
                    End If
                    r(0) = dr("geofenceid")
                    r(1) = "<span id='" & dr("geofenceid") & "' style='cursor:pointer;' onclick='selectSTC(this)'>" & dr("geofencename").ToString() & "</span>"
                    r(2) = "<a href='http://maps.google.com/maps?f=q&hl=en&q=" & latlng & "&om=1&t=k' target='_blank' style='text-decoration:none;color:blue;'>" & latlng & "</a>"
                    r(3) = "<span id='" & dr("geofenceid") & "s'>" & dr("shiptocode") & "</span"
                    geofencetable.Rows.Add(r)
                End While
                If geofencetable.Rows.Count = 0 Then
                    r = geofencetable.NewRow
                    r(0) = 1
                    r(1) = "-"
                    r(2) = "-"
                    r(3) = "-"
                    geofencetable.Rows.Add(r)
                End If
                For i As Integer = 0 To geofencetable.Rows.Count - 1
                    Try
                        a = New ArrayList
                        a.Add(geofencetable.DefaultView.Item(i)(0))
                        a.Add(geofencetable.DefaultView.Item(i)(1))
                        a.Add(geofencetable.DefaultView.Item(i)(2))
                        a.Add(geofencetable.DefaultView.Item(i)(3))
                        aa.Add(a)
                    Catch ex As Exception

                    End Try
                Next
            Catch ex As Exception

            Finally
                conn.Close()
            End Try

        Catch ex As Exception

        End Try
        json = JsonConvert.SerializeObject(aa, Formatting.None)
        Return json
    End Function

    'Public Function loadOssGeofenceData() As String
    '    Dim aa As New ArrayList()
    '    Dim a As ArrayList
    '    Dim json As String = ""
    '    Dim firstpoint As String = ""
    '    Dim geofencetable As New DataTable
    '    geofencetable.Rows.Clear()
    '    geofencetable.Columns.Add(New DataColumn("S No"))
    '    geofencetable.Columns.Add(New DataColumn("Geofence Name"))
    '    geofencetable.Columns.Add(New DataColumn("Address"))
    '    geofencetable.Columns.Add(New DataColumn("ShipToCode"))
    '    geofencetable.Columns.Add(New DataColumn("New ShipToCode"))
    '    Try
    '        Dim userslist As String = Request.Cookies("userinfo")("userslist")
    '        Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
    '        Dim cmd As New SqlCommand()
    '        cmd.Connection = conn
    '        cmd.CommandText = "select * from oss_geofence_table where new_shiptocode is null order by geofencename"
    '        Try
    '            conn.Open()
    '            Dim r As DataRow
    '            Dim dr As SqlDataReader = cmd.ExecuteReader
    '            While dr.Read
    '                r = geofencetable.NewRow
    '                Dim latlng As String = ""
    '                If (dr("geofencetype")) Then
    '                    latlng = dr("data").ToString.Split(";")(0)
    '                    latlng = latlng.Split(",")(1) & "," & latlng.Split(",")(0)
    '                Else
    '                    latlng = dr("data").ToString.Split(",")(1) & "," & dr("data").ToString.Split(",")(0)
    '                End If
    '                r(0) = dr("geofenceid")
    '                r(1) = "<span id='" & dr("geofenceid") & "' style='cursor:pointer;' onclick='selectSTC(this)'>" & dr("geofencename").ToString() & "</span>"
    '                r(2) = "<a href='http://maps.google.com/maps?f=q&hl=en&q=" & latlng & "&om=1&t=k' target='_blank' style='text-decoration:none;color:blue;'>" & latlng & "</a>"
    '                r(3) = dr("old_shiptocode")
    '                r(4) = dr("new_shiptocode")
    '                geofencetable.Rows.Add(r)
    '            End While
    '            If geofencetable.Rows.Count = 0 Then
    '                r = geofencetable.NewRow
    '                r(0) = 1
    '                r(1) = "-"
    '                r(2) = "-"
    '                r(3) = "-"
    '                r(4) = "-"
    '                geofencetable.Rows.Add(r)
    '            End If
    '            For i As Integer = 0 To geofencetable.Rows.Count - 1
    '                Try
    '                    a = New ArrayList
    '                    a.Add(geofencetable.DefaultView.Item(i)(0))
    '                    a.Add(geofencetable.DefaultView.Item(i)(1))
    '                    a.Add(geofencetable.DefaultView.Item(i)(2))
    '                    a.Add(geofencetable.DefaultView.Item(i)(3))
    '                    a.Add(geofencetable.DefaultView.Item(i)(4))
    '                    aa.Add(a)
    '                Catch ex As Exception

    '                End Try
    '            Next
    '        Catch ex As Exception

    '        Finally
    '            conn.Close()
    '        End Try

    '    Catch ex As Exception

    '    End Try
    '    json = JsonConvert.SerializeObject(aa, Formatting.None)
    '    Return json
    'End Function

    Public Function UpdateOssNewShipToCode(ByVal gid As String, ByVal newSTC As String, ByVal name As String) As String
        Dim aa As New ArrayList()
        Dim json As String = ""
        Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
        Dim cmd As New SqlCommand()
        Dim cmd1 As New SqlCommand()
        cmd.Connection = conn
        cmd1.Connection = conn
        conn.Open()
        Dim sqltrans As SqlTransaction
        sqltrans = conn.BeginTransaction()
        'cmd.CommandText = "update avls_geofence_table set new_shiptocode='" & newSTC & "',ossname='" & name & "',ossaddress5='" & address & "' where geofenceid='" & gid & "'"
        cmd.CommandText = "update geofence set shiptocode='" & newSTC & "',geofencename='" & name & "'  where geofenceid='" & gid & "'"
        cmd1.CommandText = "update oss_ship_to_code set OssSTC=" & 1 & " where shiptocode='" & newSTC & "'"
        Try
            If Not cmd.CommandText = "" Then
                cmd.Transaction = sqltrans
                cmd.ExecuteNonQuery()
            End If
            If Not cmd1.CommandText = "" Then
                cmd1.Transaction = sqltrans
                cmd1.ExecuteNonQuery()
            End If
            sqltrans.Commit()
            aa.Add("1")
        Catch ex As Exception
            Dim error1 As String = ex.Message
            aa.Add("0")
            sqltrans.Rollback()
        Finally
            conn.Close()
        End Try
        json = JsonConvert.SerializeObject(aa, Formatting.None)
        Return json
    End Function
End Class