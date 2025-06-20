Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports AspMap

Public Class GetGeofenceGeoJson
    Inherits SecurePageBase

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ' Validate authentication
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.StatusCode = 401
                Response.Write("{""error"":""Unauthorized""}")
                Response.End()
                Return
            End If
            
            Dim json As String = ""
            Dim geofenceid As String = SecurityHelper.SanitizeForHtml(Request.QueryString("gid"))
            
            If String.IsNullOrEmpty(geofenceid) Then
                Response.StatusCode = 400
                Response.Write("{""error"":""Missing geofence ID""}")
                Response.End()
                Return
            End If
            
            ' Validate geofence ID
            Dim geoId As Integer
            If Not Integer.TryParse(geofenceid, geoId) OrElse geoId <= 0 Then
                Response.StatusCode = 400
                Response.Write("{""error"":""Invalid geofence ID""}")
                Response.End()
                Return
            End If
            
            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@geofenceid", geofenceid)
            
            Dim query As String = "SELECT * FROM geofence WHERE geofenceid = @geofenceid"
            Dim geofenceData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            
            Dim aa As New ArrayList()
            
            For Each dr As DataRow In geofenceData.Rows
                Try
                    Dim at As Integer = 0
                    Dim status As Byte = 0
                    Dim a As New ArrayList()
                    
                    If dr("accesstype").ToString() = "1" Then
                        at = 1
                    ElseIf dr("accesstype").ToString() = "0" Then
                        at = 0
                    Else
                        at = 2
                    End If
                    
                    If CBool(dr("status")) Then
                        status = 1
                    Else
                        status = 0
                    End If

                    a.Add(status)
                    a.Add(at)
                    a.Add(Convert.ToUInt32(dr("geofenceid")))
                    a.Add(SanitizeOutput(dr("geofencename").ToString()))
                    a.Add(SanitizeOutput(dr("data").ToString()))

                    Try
                        Dim polygonShape As New AspMap.Shape
                        polygonShape.ShapeType = ShapeType.mcPolygonShape

                        Dim shpPoints As New AspMap.Points()
                        Dim points() As String = dr("data").ToString().Split(";"c)
                        Dim values() As String

                        For i As Integer = 0 To points.Length - 1
                            values = points(i).Split(","c)
                            If values.Length = 2 Then
                                Dim lat, lng As Double
                                If Double.TryParse(values(0), lat) AndAlso Double.TryParse(values(1), lng) Then
                                    If SecurityHelper.ValidateCoordinate(lat.ToString(), lng.ToString()) Then
                                        shpPoints.AddPoint(lat, lng)
                                    End If
                                End If
                            End If
                        Next
                        
                        If shpPoints.Count > 0 Then
                            a.Add(shpPoints.Centroid.Y)
                            a.Add(shpPoints.Centroid.X)
                        Else
                            a.Add(0)
                            a.Add(0)
                        End If
                    Catch ex As Exception
                        SecurityHelper.LogError("Geofence coordinate processing error", ex, Server)
                        a.Add(0)
                        a.Add(0)
                    End Try
                    
                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Geofence data processing error", ex, Server)
                End Try
            Next

            json = JsonConvert.SerializeObject(aa, Formatting.None)
            
            Response.Write(json)
            Response.ContentType = "application/json"
            
            AuditLogger.LogDataAccess("geofence", "READ", geofenceid)

        Catch ex As Exception
            SecurityHelper.LogError("GetGeofenceGeoJson error", ex, Server)
            Response.StatusCode = 500
            Response.Write("{""error"":""Internal server error""}")
        End Try
    End Sub
End Class