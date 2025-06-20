Imports System.Data
Imports System.Data.SqlClient
Imports Newtonsoft.Json

Partial Class GetGeofenceAlertsData
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim oper As String = Request.QueryString("opr")

        Select Case oper.ToUpper()
            Case "0"
                GetData()
            Case "1"
                InsertData()
            Case "2"
                UpdateData()
            Case "3"
                DeleteData()
        End Select

    End Sub

    Private Sub GetData()
        Dim aa As New ArrayList()
        Dim a As ArrayList
        Dim sqlstr As String = "select gt.geofenceid,at.id,at.emaillist,at.mobileno,gt.geofencename from (select geofencename,geofenceid from geofence where accesstype='2') as gt left outer join  lafarge_private_geofence_alert_settings at on at.geofenceid=gt.geofenceid"
        Dim conn As SqlConnection = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
        Dim cmd As SqlCommand = New SqlCommand(sqlstr, conn)
        Dim dr As SqlDataReader
        Try
            Dim c As Integer = 0
            conn.Open()
            dr = cmd.ExecuteReader()
            While dr.Read()
                c += 1
                a = New ArrayList()

                If IsDBNull(dr("id")) Then
                    a.Add("0")
                Else
                    a.Add(dr("id"))
                End If

                a.Add(c)

                a.Add(dr("geofencename"))

                If IsDBNull(dr("emaillist")) Then
                    a.Add("")
                Else
                    a.Add(dr("emaillist"))
                End If

                If IsDBNull(dr("mobileno")) Then
                    a.Add("")
                Else
                    a.Add(dr("mobileno"))
                End If

                a.Add(dr("geofenceid"))

                If IsDBNull(dr("id")) Then
                    a.Add("0")
                Else
                    a.Add(dr("id"))
                End If
                aa.Add(a)
            End While
            dr.Close()
        Catch ex As Exception

        Finally
            conn.Close()
        End Try
        Dim json As String = ""
        Dim jss As New Newtonsoft.Json.JsonSerializer()
        json = JsonConvert.SerializeObject(aa, Formatting.None)
        Response.ContentType = "text/plain"
        Response.Write(json)
    End Sub

    Private Sub InsertData()
        Dim res As String = "0"
        Try
            Dim conn As SqlConnection = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim geoid As String = Request.QueryString("geoid")
            Dim email As String = Request.QueryString("eml")

            Dim mob As String = Request.QueryString("mob")
            Dim cmd As New SqlCommand("insert into lafarge_private_geofence_alert_settings (geofenceid,emaillist,mobileno,updateddatetime) values ('" & geoid & "','" & email & "','" & mob & "','" & Now.ToString("yyyy/MM/dd HH:mm:ss") & "')", conn)
            Try
                conn.Open()
                res = cmd.ExecuteNonQuery().ToString()

            Catch ex As Exception
                res = ex.Message
            Finally
                conn.Close()
            End Try
        Catch ex As Exception
            res = ex.Message
        End Try

        Response.ContentType = "text/plain"
        Response.Write(res)
    End Sub

    Private Sub UpdateData()
        Dim res As String = "0"
        Try
            Dim conn As SqlConnection = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim geoid As String = Request.QueryString("geoid")
            Dim email As String = Request.QueryString("eml")

            Dim mob As String = Request.QueryString("mob")
            Dim cmd As New SqlCommand("update lafarge_private_geofence_alert_settings set emaillist='" & email & "',mobileno='" & mob & "',updateddatetime='" & Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where id='" & geoid & "'", conn)
            Try
                conn.Open()
                res = cmd.ExecuteNonQuery.ToString()

            Catch ex As Exception
                res = ex.Message
            Finally
                conn.Close()
            End Try
        Catch ex As Exception
            res = ex.Message
        End Try

        Response.ContentType = "text/plain"
        Response.Write(res)
    End Sub

    Private Sub DeleteData()
        Try
            Dim chekitems As String = Request.QueryString("geoid")
            Dim result As Integer
            Dim res As String = "0"
            Dim ids As String() = chekitems.Split(",")
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim cmd As New SqlCommand("delete from lafarge_private_geofence_alert_settings where id=0", conn)
            Try
                conn.Open()
                Dim cnt As Int16 = ids.Length()
                For i As Int16 = 0 To cnt - 1
                    Try
                        cmd = New SqlCommand("delete from lafarge_private_geofence_alert_settings where id =" & ids(i).ToString(), conn)
                        result = cmd.ExecuteNonQuery
                        If result > 0 Then
                            res = "1"
                        End If
                    Catch ex As Exception
                        Response.Write("@@" & ex.Message)
                    End Try
                Next
            Catch ex As Exception
                Response.Write("@@@" & ex.Message)
            Finally
                conn.Close()
            End Try

            Response.ContentType = "text/plain"
            Response.Write(res)
        Catch ex As Exception
            Response.Write("@" & ex.Message)
        End Try

    End Sub

End Class
