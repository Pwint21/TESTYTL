Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json

Public Class GetGeofenceData
    Inherits SecurePageBase
    Implements IRequiresCSRF

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            ' Validate authentication
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.StatusCode = 401
                Response.End()
                Return
            End If
            
            Dim operation As String = SecurityHelper.SanitizeForHtml(Request.QueryString("op"))
            
            ' Validate CSRF token for state-changing operations
            If operation = "2" Then
                If Not ValidateCSRF(Request.Form("__CSRFToken")) Then
                    Response.StatusCode = 403
                    Response.Write("{""error"":""Invalid request""}")
                    Response.End()
                    Return
                End If
            End If
            
            Select Case operation
                Case "0"
                    Response.Write(loadOssShipToCodeData())
                Case "1"
                    Response.Write(loadAvlsGeofenceData())
                Case "2"
                    Dim geofenceID As String = SecurityHelper.SanitizeForHtml(Request.QueryString("geofenceID"))
                    Dim newSTC As String = SecurityHelper.SanitizeForHtml(Request.QueryString("newSTC"))
                    Dim name As String = SecurityHelper.SanitizeForHtml(Request.QueryString("name"))
                    Response.Write(UpdateOssNewShipToCode(geofenceID, newSTC, name))
                Case Else
                    Response.StatusCode = 400
                    Response.Write("{""error"":""Invalid operation""}")
            End Select
            
            Response.ContentType = "application/json"
            
        Catch ex As Exception
            SecurityHelper.LogError("GetGeofenceData error", ex, Server)
            Response.StatusCode = 500
            Response.Write("{""error"":""Internal server error""}")
        End Try
    End Sub

    Public Function loadOssShipToCodeData() As String
        Dim aa As New ArrayList()
        Dim a As ArrayList
        Dim json As String = ""
        
        Try
            ' Validate user access
            Dim userslist As String = SecurityHelper.ValidateAndGetUsersList(Request)
            
            Dim geofencetable As New DataTable
            geofencetable.Columns.Add(New DataColumn("S No"))
            geofencetable.Columns.Add(New DataColumn("Name"))
            geofencetable.Columns.Add(New DataColumn("Ship To Code"))
            geofencetable.Columns.Add(New DataColumn("Address"))
            geofencetable.Columns.Add(New DataColumn(""))

            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@ossSTC", 0)
            
            Dim query As String = "SELECT * FROM oss_ship_to_code WHERE OssSTC = @ossSTC ORDER BY name"
            Dim shipToData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            
            For Each dr As DataRow In shipToData.Rows
                Dim r As DataRow = geofencetable.NewRow
                r(0) = dr("shiptocode")
                r(1) = SanitizeOutput(dr("name").ToString())
                r(2) = dr("shiptocode")
                
                If IsDBNull(dr("address5")) OrElse dr("address5").ToString() = "" Then
                    r(3) = ""
                Else
                    r(3) = SanitizeOutput(dr("address5").ToString().Replace(vbCr, " ").Replace(vbLf, " "))
                End If
                
                r(4) = "<a href=""javascript:void(0)"" onclick=""MoveShipToCode('" & SanitizeOutput(dr("shiptocode").ToString()) & "','" & SanitizeOutput(dr("name").ToString()) & "')"" title=""Copy ShipToCode"" id='" & SanitizeOutput(dr("shiptocode").ToString()) & "a'><img src='images/rightarrow.png' alt='right' style='width:12px;height:12px;' id='" & SanitizeOutput(dr("shiptocode").ToString()) & "i'/></a>"
                geofencetable.Rows.Add(r)
            Next
            
            If geofencetable.Rows.Count = 0 Then
                Dim r As DataRow = geofencetable.NewRow
                For i As Integer = 0 To 4
                    r(i) = "-"
                Next
                geofencetable.Rows.Add(r)
            End If
            
            For i As Integer = 0 To geofencetable.Rows.Count - 1
                Try
                    a = New ArrayList
                    For j As Integer = 0 To 4
                        a.Add(geofencetable.DefaultView.Item(i)(j))
                    Next
                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Row processing error", ex, Server)
                End Try
            Next
            
            json = JsonConvert.SerializeObject(aa, Formatting.None)
            AuditLogger.LogDataAccess("oss_ship_to_code", "READ")
            
        Catch ex As Exception
            SecurityHelper.LogError("loadOssShipToCodeData error", ex, Server)
            json = "{""error"":""Data loading failed""}"
        End Try
        
        Return json
    End Function

    Public Function loadAvlsGeofenceData() As String
        Dim aa As New ArrayList()
        Dim a As ArrayList
        Dim json As String = ""
        
        Try
            Dim geofencetable As New DataTable
            geofencetable.Columns.Add(New DataColumn("S No"))
            geofencetable.Columns.Add(New DataColumn("Geofence Name"))
            geofencetable.Columns.Add(New DataColumn("Address"))
            geofencetable.Columns.Add(New DataColumn("ShipToCode"))

            Dim query As String = "SELECT * FROM geofence ORDER BY geofencename"
            Dim geofenceData As DataTable = DatabaseHelper.ExecuteQuery(query, Nothing)
            
            For Each dr As DataRow In geofenceData.Rows
                Dim r As DataRow = geofencetable.NewRow
                Dim latlng As String = ""
                
                Try
                    If CBool(dr("geofencetype")) Then
                        latlng = dr("data").ToString().Split(";"c)(0)
                        latlng = latlng.Split(","c)(1) & "," & latlng.Split(","c)(0)
                    Else
                        latlng = dr("data").ToString().Split(","c)(1) & "," & dr("data").ToString().Split(","c)(0)
                    End If
                Catch ex As Exception
                    latlng = "0,0"
                End Try
                
                r(0) = dr("geofenceid")
                r(1) = "<span id='" & SanitizeOutput(dr("geofenceid").ToString()) & "' style='cursor:pointer;' onclick='selectSTC(this)'>" & SanitizeOutput(dr("geofencename").ToString()) & "</span>"
                r(2) = "<a href='https://maps.google.com/maps?f=q&hl=en&q=" & SecurityHelper.UrlEncode(latlng) & "&om=1&t=k' target='_blank' style='text-decoration:none;color:blue;'>" & SanitizeOutput(latlng) & "</a>"
                r(3) = "<span id='" & SanitizeOutput(dr("geofenceid").ToString()) & "s'>" & SanitizeOutput(dr("shiptocode").ToString()) & "</span>"
                geofencetable.Rows.Add(r)
            Next
            
            If geofencetable.Rows.Count = 0 Then
                Dim r As DataRow = geofencetable.NewRow
                For i As Integer = 0 To 3
                    r(i) = "-"
                Next
                geofencetable.Rows.Add(r)
            End If
            
            For i As Integer = 0 To geofencetable.Rows.Count - 1
                Try
                    a = New ArrayList
                    For j As Integer = 0 To 3
                        a.Add(geofencetable.DefaultView.Item(i)(j))
                    Next
                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Row processing error", ex, Server)
                End Try
            Next
            
            json = JsonConvert.SerializeObject(aa, Formatting.None)
            AuditLogger.LogDataAccess("geofence", "READ")
            
        Catch ex As Exception
            SecurityHelper.LogError("loadAvlsGeofenceData error", ex, Server)
            json = "{""error"":""Data loading failed""}"
        End Try
        
        Return json
    End Function

    Public Function UpdateOssNewShipToCode(ByVal gid As String, ByVal newSTC As String, ByVal name As String) As String
        Dim aa As New ArrayList()
        Dim json As String = ""
        
        Try
            ' Validate inputs
            Dim geofenceId As Integer
            If Not Integer.TryParse(gid, geofenceId) OrElse geofenceId <= 0 Then
                aa.Add("0")
                json = JsonConvert.SerializeObject(aa, Formatting.None)
                Return json
            End If
            
            If String.IsNullOrWhiteSpace(newSTC) OrElse String.IsNullOrWhiteSpace(name) Then
                aa.Add("0")
                json = JsonConvert.SerializeObject(aa, Formatting.None)
                Return json
            End If
            
            ' Validate name and STC lengths
            If name.Length > 100 OrElse newSTC.Length > 50 Then
                aa.Add("0")
                json = JsonConvert.SerializeObject(aa, Formatting.None)
                Return json
            End If
            
            Using conn As SqlConnection = DatabaseHelper.CreateSecureConnection()
                conn.Open()
                Using transaction As SqlTransaction = conn.BeginTransaction()
                    Try
                        ' Update geofence
                        Dim geofenceParams As New Dictionary(Of String, Object)
                        geofenceParams.Add("@shiptocode", newSTC)
                        geofenceParams.Add("@geofencename", name)
                        geofenceParams.Add("@geofenceid", gid)
                        
                        Dim geofenceQuery As String = "UPDATE geofence SET shiptocode = @shiptocode, geofencename = @geofencename WHERE geofenceid = @geofenceid"
                        
                        Using cmd1 As SqlCommand = New SqlCommand(geofenceQuery, conn, transaction)
                            For Each param In geofenceParams
                                cmd1.Parameters.AddWithValue(param.Key, param.Value)
                            Next
                            cmd1.ExecuteNonQuery()
                        End Using
                        
                        ' Update OSS ship to code
                        Dim ossParams As New Dictionary(Of String, Object)
                        ossParams.Add("@ossSTC", 1)
                        ossParams.Add("@shiptocode", newSTC)
                        
                        Dim ossQuery As String = "UPDATE oss_ship_to_code SET OssSTC = @ossSTC WHERE shiptocode = @shiptocode"
                        
                        Using cmd2 As SqlCommand = New SqlCommand(ossQuery, conn, transaction)
                            For Each param In ossParams
                                cmd2.Parameters.AddWithValue(param.Key, param.Value)
                            Next
                            cmd2.ExecuteNonQuery()
                        End Using
                        
                        transaction.Commit()
                        aa.Add("1")
                        
                        AuditLogger.LogUserAction("GEOFENCE_UPDATE", "Updated geofence ID: " & gid & " with STC: " & newSTC)
                        
                    Catch ex As Exception
                        transaction.Rollback()
                        SecurityHelper.LogError("UpdateOssNewShipToCode transaction error", ex, Server)
                        aa.Add("0")
                    End Try
                End Using
            End Using
            
        Catch ex As Exception
            SecurityHelper.LogError("UpdateOssNewShipToCode error", ex, Server)
            aa.Add("0")
        End Try
        
        json = JsonConvert.SerializeObject(aa, Formatting.None)
        Return json
    End Function
End Class