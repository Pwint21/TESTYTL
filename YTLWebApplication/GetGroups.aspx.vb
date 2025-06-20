Imports Newtonsoft.Json
Imports System.Data.SqlClient

Partial Class GetGroups
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Try
            Dim userid As String = Request.Cookies("userinfo")("userid")
            Dim role As String = Request.Cookies("userinfo")("role")
            Dim userslist As String = Request.Cookies("userinfo")("userslist")
            Dim qs As String = ""
            qs = Request.QueryString("userid")
            Dim json As String = ""
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim cmd As SqlCommand
            If role = "User" Then
                cmd = New SqlCommand("select groupid,groupname from vehicle_group where userid='" & userid & "' order by groupname", conn)
            ElseIf role = "SuperUser" Or role = "Operator" Then
                If qs <> "ALLUSERS" Then
                    cmd = New SqlCommand("select groupid,groupname from vehicle_group where  userid ='" & qs & "' order by groupname", conn)
                Else
                    cmd = New SqlCommand("select groupid,groupname from vehicle_group where  userid in(" & userslist & ") order by groupname", conn)
                End If
            Else
                If qs <> "ALLUSERS" Then
                    cmd = New SqlCommand("select groupid,groupname from vehicle_group where userid='" & qs & "' order by groupname", conn)
                Else
                    cmd = New SqlCommand("select groupid,groupname from vehicle_group  order by groupname", conn)
                End If
            End If
            Dim dr As SqlDataReader


            Dim aa As New ArrayList()

            Try
                conn.Open()
                dr = cmd.ExecuteReader()

                While dr.Read()
                    Try
                        Dim a As New ArrayList()
                        a.Add(dr("groupid"))
                        a.Add(dr("groupname").ToString.ToUpper())
                        aa.Add(a)
                    Catch ex As Exception

                    End Try
                End While

                Dim jss As New Newtonsoft.Json.JsonSerializer()


                json = JsonConvert.SerializeObject(aa, Formatting.None)
            Catch ex As Exception

            Finally
                conn.Close()
            End Try
            Response.Write(json)
            Response.ContentType = "text/plain"
        Catch ex As Exception

        End Try

    End Sub
End Class
