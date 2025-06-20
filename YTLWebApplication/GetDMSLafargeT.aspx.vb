Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json

Partial Class GetDMSLafargeT
    Inherits System.Web.UI.Page
    Public connstr As String
    Public locationDict As New Dictionary(Of Integer, List(Of Location))
    Dim sdate As String
    Dim edate As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Response.Write(GetJson())
            Response.ContentType = "text/plain"
        Catch ex As Exception

        End Try
    End Sub


    Protected Function GetJson() As String
        Dim json As String = ""
        sdate = Request.QueryString("fdt")
        edate = Request.QueryString("tdt")
        If sdate = Nothing Then
            sdate = Date.Now.AddDays(-2)
            edate = Date.Now.AddDays(-1)
        End If
        Dim ShipToCodeDict As New Dictionary(Of String, String)
        Dim connlafarge As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
        Dim cmdlafarge As New SqlCommand("select distinct(shiptocode),geofencename from geofence where accesstype='1' ", connlafarge)
        Dim drlafarge As SqlDataReader
        Try
            connlafarge.Open()
            drlafarge = cmdlafarge.ExecuteReader()
            While drlafarge.Read()
                If Not ShipToCodeDict.ContainsKey(drlafarge("shiptocode")) Then
                    ShipToCodeDict.Add(drlafarge("shiptocode"), drlafarge("geofencename"))
                End If
            End While

        Catch ex As Exception

        Finally
            connlafarge.Close()
        End Try

        Dim aa As New ArrayList()
        Dim a As ArrayList
        connstr = System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection")
        Dim conn As New SqlConnection(connstr)
        Try
            Server.ScriptTimeout = Integer.MaxValue - 1
            Dim bdt As String = Request.QueryString("fdt")
            Dim edt As String = Request.QueryString("tdt")
            Dim plateno As String = "ALL PLATES"
            Dim transporter As String = "ALL TRANSPORTERS"
            Dim geofence As String = "ALL GEOS"
            Dim user As String = Request.QueryString("userid")
            Dim cond As String = ""

            Dim DriverNameDict As New Dictionary(Of String, String)
            Dim connJob As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection2"))
            Try
                Dim cmd2 As New SqlCommand("select dn_no,dn_driver,dn_qty from oss_patch_in where weight_outtime between '" & bdt & "' and '" & edt & "' and dn_driver is not null", connJob)
                Try
                    connJob.Open()
                    Dim drJob As SqlDataReader = cmd2.ExecuteReader()
                    While drJob.Read()
                        If Not DriverNameDict.ContainsKey(drJob("dn_no").ToString()) Then
                            If IsDBNull(drJob("dn_qty")) Then
                                DriverNameDict.Add(drJob("dn_no").ToString(), drJob("dn_driver").ToString())
                            Else
                                DriverNameDict.Add(drJob("dn_no").ToString(), drJob("dn_driver").ToString())
                            End If
                        End If
                    End While
                Catch ex As Exception

                Finally
                    connJob.Close()
                End Try
            Catch ex As Exception

            End Try


            Try
                Dim begintimestamp As String = bdt
                Dim endtimestamp As String = edt
                Dim cmd As New SqlCommand
                Dim dr As SqlDataReader
                conn = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection2"))
                cmd = New SqlCommand("select * from oss_patch_out where weight_outtime between '" & begintimestamp & "' and '" & endtimestamp & "'  and status in ('7','8','12','13') ", conn)
                '   Response.Write(cmd.CommandText)
                Try
                    conn.Open()
                    dr = cmd.ExecuteReader()
                    Dim i As Int32 = 1
                    While dr.Read()
                        Try
                            a = New ArrayList()
                            a.Add("ShipToName")
                            a.Add(dr("dn_no"))
                            If IsDBNull(dr("transporter")) Then
                                a.Add("--")
                            Else
                                a.Add(dr("transporter"))
                            End If
                            a.Add(dr("plateno"))


                            If DriverNameDict.ContainsKey(dr("dn_no").ToString()) Then
                                a.Add(DriverNameDict.Item(dr("dn_no").ToString()).ToString())
                            Else
                                a.Add("--")
                            End If



                            a.Add(dr("source_supply"))

                            If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                a.Add(ShipToCodeDict.Item(dr("destination_siteid")))
                            Else
                                a.Add("--")
                            End If

                            a.Add(dr("destination_siteid"))

                            'Loading
                            Try

                                If IsDBNull(dr("plant_intime")) Then
                                    a.Add("--")
                                    a.Add("--")
                                    a.Add(Convert.ToDateTime(dr("weight_outtime")).ToString("yyyy/MM/dd"))
                                    a.Add(Convert.ToDateTime(dr("weight_outtime")).ToString("HH:mm:ss"))
                                    a.Add("--")
                                Else
                                    Dim plantintime As DateTime = Convert.ToDateTime(dr("plant_intime"))
                                    a.Add(plantintime.ToString("yyyy/MM/dd"))
                                    a.Add(plantintime.ToString("HH:mm:ss"))
                                    a.Add(Convert.ToDateTime(dr("weight_outtime")).ToString("yyyy/MM/dd"))
                                    a.Add(Convert.ToDateTime(dr("weight_outtime")).ToString("HH:mm:ss"))

                                    Dim tim As TimeSpan = (Convert.ToDateTime(dr("weight_outtime")) - plantintime)
                                    a.Add(tim.TotalMinutes.ToString("0"))
                                    'If tim.Days > 0 Then
                                    '    '  a.Add(tim.TotalMinutes)
                                    '    a.Add(tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min")
                                    'ElseIf tim.Hours > 0 Then
                                    '    a.Add(tim.Hours & " h " & tim.Minutes & " min")
                                    'Else
                                    '    a.Add(tim.Minutes & " min")
                                    'End If

                                End If
                            Catch ex As Exception
                                ' Response.Write("71." & ex.Message)
                            End Try

                            'Travelling
                            Try
                                If Not (IsDBNull(dr("ata_date")) And IsDBNull(dr("ata_time"))) Then
                                    Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()
                                    Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                                    Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("weight_outtime")))
                                    a.Add(tim.TotalMinutes.ToString("0"))
                                    'If tim.Days > 0 Then
                                    '    a.Add(tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min")
                                    'ElseIf tim.Hours > 0 Then
                                    '    a.Add(tim.Hours & " h " & tim.Minutes & " min")
                                    'Else
                                    '    a.Add(tim.Minutes & " min")
                                    'End If
                                Else
                                    a.Add("--")
                                End If

                            Catch ex As Exception
                                ' Response.Write("91." & ex.Message)
                            End Try

                            'Distance
                            Try
                                If IsDBNull(dr("distance")) Then
                                    a.Add("0.00")
                                Else
                                    a.Add(Convert.ToDouble(dr("distance")).ToString("0.00"))
                                End If
                            Catch ex As Exception

                            End Try

                            'Waiting Time
                            Try
                                If IsDBNull(dr("pto1_datetime")) Then
                                    Dim isstep As String = "0"
                                    Try
                                        If Not IsDBNull(dr("wait_start_time")) Then
                                            If Not (IsDBNull(dr("ata_date")) And IsDBNull(dr("ata_time"))) Then
                                                isstep = "1"
                                                Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()
                                                Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                                                a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("yyyy/MM/dd"))
                                                a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("HH:mm:ss"))
                                                a.Add(atatimess.ToString("yyyy/MM/dd"))
                                                a.Add(atatimess.ToString("HH:mm:ss"))
                                                Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("wait_start_time")))
                                                'If tim.Days > 0 Then
                                                '    a.Add(tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min")
                                                'ElseIf tim.Hours > 0 Then
                                                '    a.Add(tim.Hours & " h " & tim.Minutes & " min")
                                                'Else
                                                '    a.Add(tim.Minutes & " min")
                                                'End If
                                                a.Add(tim.TotalMinutes.ToString("0"))

                                            Else
                                                a.Add("--")
                                                a.Add("--")
                                                a.Add("--")
                                                a.Add("--")
                                                a.Add("--")
                                            End If
                                        Else
                                            a.Add("--")
                                            a.Add("--")
                                            a.Add("--")
                                            a.Add("--")
                                            a.Add("--")
                                        End If
                                    Catch ex As Exception
                                        Response.Write("Atstpe:" & isstep & " -- Err:" & ex.Message)
                                    End Try
                                ElseIf IsDBNull(dr("wait_start_time")) Then
                                    Try
                                        a.Add("--")
                                        a.Add("--")
                                        a.Add(Convert.ToDateTime(dr("pto1_datetime")).ToString("yyyy/MM/dd"))
                                        a.Add(Convert.ToDateTime(dr("pto1_datetime")).ToString("HH:mm:ss"))
                                        a.Add("--")

                                    Catch ex As Exception
                                        Response.Write("2." & ex.Message)
                                    End Try
                                Else
                                    Try
                                        Dim atatimess As DateTime = Convert.ToDateTime(dr("pto1_datetime").ToString())
                                        a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("yyyy/MM/dd"))
                                        a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("HH:mm:ss"))
                                        a.Add(atatimess.ToString("yyyy/MM/dd"))
                                        a.Add(atatimess.ToString("HH:mm:ss"))
                                        Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("wait_start_time")))
                                        'If tim.Days > 0 Then
                                        '    a.Add(tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min")
                                        'ElseIf tim.Hours > 0 Then
                                        '    a.Add(tim.Hours & " h " & tim.Minutes & " min")
                                        'Else
                                        '    a.Add(tim.Minutes & " min")
                                        'End If
                                        a.Add(tim.TotalMinutes.ToString("0"))

                                    Catch ex As Exception
                                        ' Response.Write("1." & ex.Message)
                                    End Try
                                End If
                            Catch ex As Exception
                                '  Response.Write("41." & ex.Message)
                            End Try


                            'Unloading
                            Try
                                If IsDBNull(dr("pto1_datetime")) Or IsDBNull(dr("pto2_datetime")) Then
                                    a.Add("--")
                                    a.Add("--")
                                    a.Add("--")
                                    a.Add("--")
                                    a.Add("--")
                                Else
                                    Dim atatimess As DateTime = Convert.ToDateTime(dr("pto1_datetime").ToString())
                                    a.Add(atatimess.ToString("yyyy/MM/dd"))
                                    a.Add(atatimess.ToString("HH:mm:ss"))
                                    a.Add(Convert.ToDateTime(dr("pto2_datetime")).ToString("yyyy/MM/dd"))
                                    a.Add(Convert.ToDateTime(dr("pto2_datetime")).ToString("HH:mm:ss"))
                                    Dim tim As TimeSpan = (Convert.ToDateTime(dr("pto2_datetime")) - atatimess)
                                    'If tim.Days > 0 Then
                                    '    a.Add(tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min")
                                    'ElseIf tim.Hours > 0 Then
                                    '    a.Add(tim.Hours & " h " & tim.Minutes & " min")
                                    'Else
                                    '    a.Add(tim.Minutes & " min")
                                    'End If
                                    a.Add(tim.TotalMinutes.ToString("0"))

                                End If

                            Catch ex As Exception
                                Response.Write("42." & ex.Message)
                            End Try

                            aa.Add(a)
                        Catch ex As Exception
                            Response.Write("31." & ex.Message)
                        End Try
                    End While

                Catch ex As Exception
                    ' Response.Write("21." & ex.Message)
                Finally
                    conn.Close()
                End Try

            Catch ex As Exception
                '  Response.Write("2." & ex.Message)
            End Try


            json = JsonConvert.SerializeObject(aa, Formatting.None)
        Catch ex As Exception
            ' Response.Write("1." & ex.Message)
        End Try

        Return json
    End Function

    Protected Sub WriteLog(ByVal message As String)
        Try
            If (message.Length > 0) Then
                Dim sw As New StreamWriter(Server.MapPath("") & "\DataLog.txt", FileMode.Append)
                sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & " - " & message)
                sw.Close()
            End If
        Catch ex As Exception

        End Try
    End Sub
End Class
