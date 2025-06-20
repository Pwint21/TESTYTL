Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports AspMap
Public Class GetGeofenceGeoJson
    Inherits System.Web.UI.Page

    Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
        Try
            If Request.Cookies("userinfo") Is Nothing Then
                Response.Redirect("Login.aspx")
            End If

        Catch ex As Exception

        End Try
        MyBase.OnInit(e)
    End Sub
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            Dim json As String = ""
            Dim geofenceid As String = Request.QueryString("gid")
            If Not geofenceid Is Nothing Then
                Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
                Dim cmd As SqlCommand = New SqlCommand("select * from geofence where geofenceid =@gid", conn)
                cmd.Parameters.AddWithValue("gid", geofenceid)
                Dim dr As SqlDataReader

                Try
                    conn.Open()

                    dr = cmd.ExecuteReader()

                    Dim aa As New ArrayList()

                    While (dr.Read)
                        Try
                            Dim at As Integer = 0
                            Dim status As Byte = 0
                            Dim a As New ArrayList()
                            If dr("accesstype") = "1" Then
                                at = 1
                            ElseIf dr("accesstype") = "0" Then
                                at = 0
                            Else
                                at = 2
                            End If
                            If dr("status") Then
                                status = 1
                            Else
                                status = 0
                            End If

                            a.Add(status)
                            a.Add(at)
                            a.Add(Convert.ToUInt32(dr("geofenceid")))
                            a.Add(dr("geofencename"))
                            a.Add(dr("data").ToString())

                            Dim polygonShape As New AspMap.Shape
                            polygonShape.ShapeType = ShapeType.mcPolygonShape

                            Dim shpPoints As New AspMap.Points()
                            Dim points() As String = dr("data").Split(";")
                            Dim values() As String

                            For i As Integer = 0 To points.Length - 1
                                values = points(i).Split(",")
                                If (values.Length = 2) Then
                                    shpPoints.AddPoint(Convert.ToDouble(values(0)), Convert.ToDouble(values(1)))
                                End If
                            Next
                            a.Add(shpPoints.Centroid.Y)
                            a.Add(shpPoints.Centroid.X)
                            aa.Add(a)
                        Catch ex As Exception
                            Response.Write(ex.Message)
                        End Try
                    End While

                    Dim jss As New Newtonsoft.Json.JsonSerializer()
                    json = JsonConvert.SerializeObject(aa, Formatting.None)

                Catch ex As Exception
                    Response.Write(ex.Message)
                Finally
                    conn.Close()
                End Try

                Response.Write(json)
                Response.ContentType = "application/json"
            End If


        Catch ex As Exception
            Response.Write(ex.Message)
        End Try
    End Sub

End Class