Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports System.Collections.Generic
Partial Class GetDMS
    Inherits System.Web.UI.Page
    Public allGeo As Boolean
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Server.ScriptTimeout = Integer.MaxValue - 1

        Dim a As ArrayList
        Dim aa As New ArrayList
        Dim json As String = ""
        allGeo = False
        Dim bdt As String = Request.QueryString("bdt")
        Dim edt As String = Request.QueryString("edt")
        Dim plateno As String = Request.QueryString("plateno")
        Dim transporter As String = Request.QueryString("transporter")
        Dim geofence As String = Request.QueryString("geofence")
        Dim user As String = Request.QueryString("userid")
        Dim cond As String = ""

        If plateno <> "" And plateno <> "ALL PLATES" Then
            cond = cond & "  and plateno='" & plateno.ToString().Trim() & "' "
        End If

        If geofence = "" Or geofence = "ALL GEOFENCES" Then
            allGeo = True
        End If

        If transporter <> "" And transporter <> "ALL TRANSPORTERS" Then
            cond = cond & "  and transporter='" & transporter & "' "
        End If
        Try
            Dim locObj As New Location()
            Dim begintimestamp As String = bdt
            Dim endtimestamp As String = edt

            Dim statusdrop As String = 7

            Dim condition As String = ""

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
                '  Response.Write(ex.Message)
            Finally
                connlafarge.Close()
            End Try

            Dim r As DataRow

            Dim idlingstoptable As New DataTable
            idlingstoptable.Columns.Add(New DataColumn("plateno"))
            idlingstoptable.Columns.Add(New DataColumn("status"))
            idlingstoptable.Columns.Add(New DataColumn("bdt"))
            idlingstoptable.Columns.Add(New DataColumn("edt"))
            idlingstoptable.Columns.Add(New DataColumn("duration"))
            idlingstoptable.Columns.Add(New DataColumn("alarm"))
            idlingstoptable.Columns.Add(New DataColumn("Loc"))
            idlingstoptable.Columns.Add(New DataColumn("lat"))
            idlingstoptable.Columns.Add(New DataColumn("lon"))


            Dim idlingstoptableBack As New DataTable
            idlingstoptableBack.Columns.Add(New DataColumn("plateno"))
            idlingstoptableBack.Columns.Add(New DataColumn("status"))
            idlingstoptableBack.Columns.Add(New DataColumn("bdt"))
            idlingstoptableBack.Columns.Add(New DataColumn("edt"))
            idlingstoptableBack.Columns.Add(New DataColumn("duration"))
            idlingstoptableBack.Columns.Add(New DataColumn("alarm"))
            idlingstoptableBack.Columns.Add(New DataColumn("Loc"))
            idlingstoptableBack.Columns.Add(New DataColumn("lat"))
            idlingstoptableBack.Columns.Add(New DataColumn("lon"))


            Dim alarmTable As New DataTable
            alarmTable.Columns.Add(New DataColumn("plateno"))
            alarmTable.Columns.Add(New DataColumn("status"))
            alarmTable.Columns.Add(New DataColumn("bdt"))
            alarmTable.Columns.Add(New DataColumn("edt"))
            alarmTable.Columns.Add(New DataColumn("duration"))
            alarmTable.Columns.Add(New DataColumn("lat"))
            alarmTable.Columns.Add(New DataColumn("lon"))
            alarmTable.Columns.Add(New DataColumn("color"))

            Dim alarmTableBack As New DataTable
            alarmTableBack.Columns.Add(New DataColumn("plateno"))
            alarmTableBack.Columns.Add(New DataColumn("status"))
            alarmTableBack.Columns.Add(New DataColumn("bdt"))
            alarmTableBack.Columns.Add(New DataColumn("edt"))
            alarmTableBack.Columns.Add(New DataColumn("duration"))
            alarmTableBack.Columns.Add(New DataColumn("lat"))
            alarmTableBack.Columns.Add(New DataColumn("lon"))
            alarmTableBack.Columns.Add(New DataColumn("color"))


            Dim t As New DataTable
            t.Columns.Add(New DataColumn("chk"))
            t.Columns.Add(New DataColumn("S NO"))
            t.Columns.Add(New DataColumn("Plate NO"))
            t.Columns.Add(New DataColumn("Unit ID"))
            t.Columns.Add(New DataColumn("Transporter"))
            t.Columns.Add(New DataColumn("Source"))
            t.Columns.Add(New DataColumn("DN NO"))
            t.Columns.Add(New DataColumn("Weight Outtime"))
            t.Columns.Add(New DataColumn("Ship To Code"))
            t.Columns.Add(New DataColumn("Ship To Name"))
            t.Columns.Add(New DataColumn("ATAdt"))
            t.Columns.Add(New DataColumn("GPS Point"))
            t.Columns.Add(New DataColumn("Status"))
            t.Columns.Add(New DataColumn("Remarks"))
            t.Columns.Add(New DataColumn("DN ID"))
            t.Columns.Add(New DataColumn("Group Name"))
            t.Columns.Add(New DataColumn("ATAtm"))
            t.Columns.Add(New DataColumn("ATDdt"))
            t.Columns.Add(New DataColumn("PtoOnTime"))
            t.Columns.Add(New DataColumn("PtoOffTime"))
            t.Columns.Add(New DataColumn("IdlingStop1"))
            t.Columns.Add(New DataColumn("IdlingStopGeoName1"))
            t.Columns.Add(New DataColumn("IdlingStopDuration1"))
            t.Columns.Add(New DataColumn("IdlingStopPTO1"))
            t.Columns.Add(New DataColumn("IdlingStop2"))
            t.Columns.Add(New DataColumn("IdlingStopGeoName2"))
            t.Columns.Add(New DataColumn("IdlingStopDuration2"))
            t.Columns.Add(New DataColumn("IdlingStopPTO2"))
            t.Columns.Add(New DataColumn("BackToSource"))
            t.Columns.Add(New DataColumn("IdlingStopInfo1"))
            t.Columns.Add(New DataColumn("IdlingStopInfo2"))
            t.Columns.Add(New DataColumn("PTO1"))
            t.Columns.Add(New DataColumn("PTO2"))
            t.Columns.Add(New DataColumn("PTO1Info"))
            t.Columns.Add(New DataColumn("PTO2Info"))

            t.Columns.Add(New DataColumn("Idur1"))
            t.Columns.Add(New DataColumn("PDur1"))
            t.Columns.Add(New DataColumn("IDur2"))
            t.Columns.Add(New DataColumn("PDur2"))

            t.Columns.Add(New DataColumn("DataLost"))
            t.Columns.Add(New DataColumn("Userid"))
            t.Columns.Add(New DataColumn("BeginL"))
            t.Columns.Add(New DataColumn("EndL"))
            t.Columns.Add(New DataColumn("Driver Name"))
            t.Columns.Add(New DataColumn("DN Quatity"))
            t.Columns.Add(New DataColumn("TravellingTime"))
            t.Columns.Add(New DataColumn("Distance"))

            t.Columns.Add(New DataColumn("LoadingTime"))
            t.Columns.Add(New DataColumn("WaitingTime"))
            t.Columns.Add(New DataColumn("UnloadingTime"))

            t.Columns.Add(New DataColumn("LoadingIN"))
            t.Columns.Add(New DataColumn("Loadingout"))
            t.Columns.Add(New DataColumn("WaitingIN"))
            t.Columns.Add(New DataColumn("WaitingOUT"))


            Dim exceltable As New DataTable
            exceltable.Columns.Add(New DataColumn("Ship to Name"))
            exceltable.Columns.Add(New DataColumn("Ship To Code"))
            exceltable.Columns.Add(New DataColumn("Order No"))
            exceltable.Columns.Add(New DataColumn("Ex"))
            exceltable.Columns.Add(New DataColumn("TPT"))
            exceltable.Columns.Add(New DataColumn("VehicleNo"))
            exceltable.Columns.Add(New DataColumn("MT"))
            exceltable.Columns.Add(New DataColumn("DN No"))
            exceltable.Columns.Add(New DataColumn("Out weight BridgeDate"))
            exceltable.Columns.Add(New DataColumn("Out weight Bridge Time"))
            exceltable.Columns.Add(New DataColumn("Journey to cust"))
            exceltable.Columns.Add(New DataColumn("ATA"))
            exceltable.Columns.Add(New DataColumn("ATD"))
            exceltable.Columns.Add(New DataColumn("Time at Site"))
            exceltable.Columns.Add(New DataColumn("Back To Source"))
            exceltable.Columns.Add(New DataColumn("PtoOnTime"))
            exceltable.Columns.Add(New DataColumn("PtoOffTime"))
            exceltable.Columns.Add(New DataColumn("Stop Idling"))
            exceltable.Columns.Add(New DataColumn("Stop Or Idling Geofence"))
            exceltable.Columns.Add(New DataColumn("Duration"))
            exceltable.Columns.Add(New DataColumn("PTO1"))
            exceltable.Columns.Add(New DataColumn("Stop Idling2"))
            exceltable.Columns.Add(New DataColumn("Stop Or Idling Geofence2"))
            exceltable.Columns.Add(New DataColumn("Duration2"))
            exceltable.Columns.Add(New DataColumn("PTO2"))
            exceltable.Columns.Add(New DataColumn("PTO1Status"))
            exceltable.Columns.Add(New DataColumn("PTO2Status"))
            exceltable.Columns.Add(New DataColumn("Idur1"))
            exceltable.Columns.Add(New DataColumn("Pdur1"))
            exceltable.Columns.Add(New DataColumn("Idur2"))
            exceltable.Columns.Add(New DataColumn("pdur2"))
            exceltable.Columns.Add(New DataColumn("DataLost"))
            exceltable.Columns.Add(New DataColumn("Driver Name"))
            exceltable.Columns.Add(New DataColumn("DN Quatity"))
            exceltable.Columns.Add(New DataColumn("TravellingTime"))
            exceltable.Columns.Add(New DataColumn("Distance"))
            exceltable.Columns.Add(New DataColumn("LoadingTime"))
            exceltable.Columns.Add(New DataColumn("WaitingTime"))
            exceltable.Columns.Add(New DataColumn("UnloadingTime"))

            Dim vehicleDict As New Dictionary(Of String, String)
            Dim DriverNameDict As New Dictionary(Of String, String)
            Dim connJob As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection2"))
            Try
                Dim cmd2 As New SqlCommand("select dn_no,dn_driver,dn_qty from oss_patch_in where weight_outtime between '" & begintimestamp & "' and '" & endtimestamp & "' and dn_driver is not null  " & cond & "", connJob)
                Try
                    connJob.Open()
                    Dim drJob As SqlDataReader = cmd2.ExecuteReader()
                    While drJob.Read()
                        If Not DriverNameDict.ContainsKey(drJob("dn_no").ToString()) Then
                            If IsDBNull(drJob("dn_qty")) Then
                                DriverNameDict.Add(drJob("dn_no").ToString(), drJob("dn_driver").ToString() & ";" & "--")
                            Else
                                DriverNameDict.Add(drJob("dn_no").ToString(), drJob("dn_driver").ToString() & ";" & drJob("dn_qty"))
                            End If
                        End If
                    End While
                Catch ex As Exception

                Finally
                    connJob.Close()
                End Try
            Catch ex As Exception

            End Try

            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim cmd As New SqlCommand("select plateno,groupname from vehicleTBL", conn)
            Dim dr As SqlDataReader

            Try
                conn.Open()
                dr = cmd.ExecuteReader()
                While dr.Read()
                    Try
                        If dr("plateno").ToString().IndexOf("_") > 0 Then
                            vehicleDict.Add(dr("plateno").ToString().Split("_")(0), dr("groupname"))
                        Else
                            vehicleDict.Add(dr("plateno").ToString(), dr("groupname"))
                        End If
                    Catch ex As Exception

                    End Try
                End While

            Catch ex As Exception

            Finally
                conn.Close()
            End Try
            Dim usercond As String = ""
            If user <> "" And user <> "ALL USERS" Then
                usercond = usercond & " WHERE USERID='" & user & "' "
            End If
            Dim vehicleUserDict As New Dictionary(Of String, String)

            cmd = New SqlCommand("select userid,plateno from vehicleTBL " & usercond, conn)
            Dim dr2 As SqlDataReader
            ' Response.Write(cmd.CommandText)
            Try
                conn.Open()
                dr2 = cmd.ExecuteReader()
                While dr2.Read()
                    Try
                        If dr2("plateno").ToString().IndexOf("_") > 0 Then
                            vehicleUserDict.Add(dr2("plateno").ToString().Split("_")(0), dr2("userid"))
                        Else
                            vehicleUserDict.Add(dr2("plateno").ToString(), dr2("userid"))
                        End If
                    Catch ex As Exception
                        ' Response.Write(ex.Message)
                    End Try
                End While

            Catch ex As Exception
                ' Response.Write(ex.Message)
            Finally
                conn.Close()
            End Try


            Dim conn1 As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))

            conn = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection2"))

            cmd = New SqlCommand("select * from oss_patch_out where weight_outtime between '" & begintimestamp & "' and '" & endtimestamp & "' " & cond & " and status=7 ", conn)
            'Response.Write(cmd.CommandText)

            'For Each keys As String In vehicleUserDict.Keys
            '    Response.Write(keys)
            '    Response.Write("<br/>")
            'Next


            Try
                conn.Open()

                dr = cmd.ExecuteReader()

                Dim i As Int32 = 1
                While dr.Read()
                    Try

                        If vehicleUserDict.ContainsKey(dr("plateno")) Then
                            'Response.Write("Camehere")
                            If allGeo Or dr("destination_siteid").ToString().ToUpper() = geofence.ToUpper() Then
                                r = t.NewRow
                                r(0) = dr("patch_no")
                                r(1) = i.ToString()
                                r(2) = dr("plateno")

                                r(3) = dr("unitid")
                                If IsDBNull(dr("transporter")) Then
                                    r(4) = "--"
                                Else
                                    r(4) = dr("transporter")
                                End If

                                r(5) = dr("source_supply")
                                r(6) = dr("dn_no")


                                Dim p = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                p = p.Replace("-", "/")

                                r(7) = p



                                r(8) = dr("destination_siteid")

                                If IsDBNull(dr("destination_sitename")) Then
                                    If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                        r(9) = ShipToCodeDict.Item(dr("destination_siteid")).ToUpper()
                                    Else
                                        r(9) = "--"
                                    End If
                                Else
                                    If dr("status").ToString() <> "2" Then
                                        r(9) = dr("destination_sitename").ToString.ToUpper()
                                    Else
                                        If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                            r(9) = ShipToCodeDict.Item(dr("destination_siteid")).ToUpper()
                                        Else
                                            r(9) = "--"
                                        End If
                                    End If
                                End If

                                'If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                '    r(9) = ShipToCodeDict.Item(dr("destination_siteid"))
                                'Else
                                '    r(9) = "--"
                                'End If



                                Dim lat As Double = 0
                                Dim lon As Double = 0

                                If IsDBNull(dr("ata_date")) = False Then
                                    r(10) = dr("ata_date").ToString()
                                    r(16) = dr("ata_time").ToString()
                                Else
                                    r(10) = "--"
                                    r(16) = "--"
                                End If

                                If IsDBNull(dr("lat")) = False Then
                                    r(11) = Convert.ToDouble(dr("lat")).ToString("0.0000") & "," & Convert.ToDouble(dr("lon")).ToString("0.0000")
                                Else
                                    r(11) = "--"
                                End If
                                Dim finishDate As String = ""
                                If IsDBNull(dr("journey_back_datetime")) Then
                                    finishDate = Convert.ToDateTime(dr("weight_outtime")).AddHours(+48).ToString("yyyy/MM/dd HH:mm:ss")
                                Else
                                    finishDate = Convert.ToDateTime(dr("journey_back_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                End If

                                Try
                                    Dim obj As vehicleInfo = GetLostData(dr("plateno"), DateTime.Parse(dr("weight_outtime")), DateTime.Parse(finishDate))
                                    If obj.status Then
                                        r(39) = "Yes"
                                        r(40) = obj.userid
                                        r(41) = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd HH:mm:ss")
                                        r(42) = DateTime.Parse(finishDate).ToString("yyyy/MM/dd HH:mm:ss")
                                    Else
                                        r(39) = "No"
                                        r(40) = obj.userid
                                        r(41) = obj.bdtL
                                        r(42) = obj.edtL
                                    End If
                                Catch ex As Exception
                                    Response.Write("Out side :" & ex.Message)
                                End Try
                                Dim status As String = "Delivery Completed"
                                Try
                                    r(12) = "<span onclick=""javascript:openpage('" & dr("patch_no") & "')"" style=""cursor:pointer;text-decoration: underline; color: #000080;"" title=""Show idling information."">" & status & "</span>"

                                    If IsDBNull(dr("remarks")) = False Then
                                        r(13) = dr("remarks")
                                    Else
                                        r(13) = "--"
                                    End If
                                    r(14) = dr("dn_id")

                                    r(15) = ""

                                    If vehicleDict.ContainsKey(dr("plateno")) Then
                                        r(15) = vehicleDict(dr("plateno"))
                                    End If

                                    If IsDBNull(dr("atd_datetime")) Then
                                        r(17) = "--"
                                    Else
                                        r(17) = Convert.ToDateTime(dr("atd_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    End If

                                    If IsDBNull(dr("pto1_datetime")) Then
                                        r(18) = "--"
                                    Else
                                        r(18) = Convert.ToDateTime(dr("pto1_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    End If

                                    If IsDBNull(dr("pto2_datetime")) Then
                                        r(19) = "--"
                                    Else
                                        r(19) = Convert.ToDateTime(dr("pto2_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    End If
                                Catch ex As Exception

                                End Try
                                Try
                                    Dim rrr As DataRow
                                    Dim rpto As DataRow
                                    Dim st As Integer = 0
                                    Dim lati As Double = 0
                                    Dim lonn As Double = 0
                                    Dim prevstatus As String = "stop"
                                    Dim currentstatus As String = "stop"
                                    Dim prevpto As String = "off"
                                    Dim currentpto As String = "off"

                                    Dim tempprevtime As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                    Dim prevtime As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                    Dim currenttime As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")

                                    Dim tempprevtimepto As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                    Dim prevtimepto As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                    Dim currenttimepto As DateTime = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd  HH:mm:ss")


                                    Dim lastlat As Double = 0
                                    Dim lastlon As Double = 0


                                    Dim ts As DateTime = dr("ata_time").ToString()
                                    Dim dttm As DateTime = dr("ata_date").ToString()
                                    Dim atatime As DateTime = dttm.Add(ts.TimeOfDay)

                                    Dim bdttm As String = DateTime.Parse(dr("weight_outtime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    Dim enddttm As String = atatime.ToString("yyyy/MM/dd HH:mm:ss")

                                    If Not IsDBNull(dr("journey_back_datetime")) Then
                                        enddttm = DateTime.Parse(dr("journey_back_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    End If

                                    Dim ds As New DataSet
                                    Dim da As New SqlDataAdapter("select distinct convert(varchar(19),timestamp,120) as datetime,plateno,speed,ignition,lat,lon,alarm from vehicle_history2 where plateno ='" & dr("plateno") & "'  and timestamp between '" & bdttm & "' and '" & enddttm & "' and (gps_av='A' or (gps_av='V' and ignition='0')) order by datetime asc", conn1)
                                    da.Fill(ds)
                                    For ii As Integer = 0 To ds.Tables(0).Rows.Count - 1
                                        lastlat = ds.Tables(0).Rows(ii)("lat")
                                        lastlon = ds.Tables(0).Rows(ii)("lon")

                                        currenttime = ds.Tables(0).Rows(ii)("datetime")
                                        currenttimepto = ds.Tables(0).Rows(ii)("datetime")
                                        If ds.Tables(0).Rows(ii)("ignition") And ds.Tables(0).Rows(ii)("speed") <> 0 Then
                                            currentstatus = "moving"
                                        ElseIf ds.Tables(0).Rows(ii)("ignition") And ds.Tables(0).Rows(ii)("speed") = 0 Then
                                            currentstatus = "idle"
                                        Else
                                            currentstatus = "stop"
                                        End If

                                        If prevstatus <> currentstatus Then
                                            Dim temptime As TimeSpan = tempprevtime - prevtime
                                            Select Case prevstatus
                                                Case "stop"
                                                    If temptime.TotalMinutes > 15 Then
                                                        If (DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) <= atatime) Then 'From Source 
                                                            rrr = idlingstoptable.NewRow
                                                            rrr(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rrr(1) = "Stop"
                                                            rrr(2) = prevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(3) = tempprevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(4) = temptime.TotalMinutes.ToString("0")
                                                            rrr(5) = ds.Tables(0).Rows(ii)("alarm")
                                                            lati = ds.Tables(0).Rows(ii)("lat")
                                                            lonn = ds.Tables(0).Rows(ii)("lon")
                                                            If lati <> 0 And lonn <> 0 Then
                                                                rrr(6) = locObj.GetPublicGeofence(lati, lonn)
                                                            End If
                                                            rrr(7) = ds.Tables(0).Rows(ii)("lat")
                                                            rrr(8) = ds.Tables(0).Rows(ii)("lon")
                                                            idlingstoptable.Rows.Add(rrr)
                                                        ElseIf DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) >= DateTime.Parse(dr("atd_datetime")) Then  ' To Source
                                                            rrr = idlingstoptableBack.NewRow
                                                            rrr(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rrr(1) = "Stop"
                                                            rrr(2) = prevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(3) = tempprevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(4) = temptime.TotalMinutes.ToString("0")
                                                            rrr(5) = ds.Tables(0).Rows(ii)("alarm")
                                                            lati = ds.Tables(0).Rows(ii)("lat")
                                                            lonn = ds.Tables(0).Rows(ii)("lon")
                                                            If lati <> 0 And lonn <> 0 Then
                                                                rrr(6) = locObj.GetPublicGeofence(lati, lonn)
                                                            End If
                                                            rrr(7) = ds.Tables(0).Rows(ii)("lat")
                                                            rrr(8) = ds.Tables(0).Rows(ii)("lon")
                                                            idlingstoptableBack.Rows.Add(rrr)
                                                        End If


                                                    End If
                                                Case "moving"

                                                Case "idle"
                                                    If temptime.TotalMinutes > 15 Then
                                                        If (DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) <= atatime) Then 'From Source 
                                                            rrr = idlingstoptable.NewRow
                                                            rrr(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rrr(1) = "Idling"
                                                            rrr(2) = prevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(3) = tempprevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(4) = temptime.TotalMinutes.ToString("0")
                                                            rrr(5) = ds.Tables(0).Rows(ii)("alarm")
                                                            lati = ds.Tables(0).Rows(ii)("lat")
                                                            lonn = ds.Tables(0).Rows(ii)("lon")
                                                            If lati <> 0 And lonn <> 0 Then
                                                                rrr(6) = locObj.GetPublicGeofence(lati, lonn)
                                                            End If
                                                            rrr(7) = ds.Tables(0).Rows(ii)("lat")
                                                            rrr(8) = ds.Tables(0).Rows(ii)("lon")

                                                            idlingstoptable.Rows.Add(rrr)
                                                        ElseIf DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) >= DateTime.Parse(dr("atd_datetime")) Then  ' To Source
                                                            rrr = idlingstoptableBack.NewRow
                                                            rrr(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rrr(1) = "Idling"
                                                            rrr(2) = prevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(3) = tempprevtime.ToString("yyyy-MM-dd HH:mm:ss")
                                                            rrr(4) = temptime.TotalMinutes.ToString("0")
                                                            rrr(5) = ds.Tables(0).Rows(ii)("alarm")
                                                            lati = ds.Tables(0).Rows(ii)("lat")
                                                            lonn = ds.Tables(0).Rows(ii)("lon")
                                                            If lati <> 0 And lonn <> 0 Then
                                                                rrr(6) = locObj.GetPublicGeofence(lati, lonn)
                                                            End If
                                                            rrr(7) = ds.Tables(0).Rows(ii)("lat")
                                                            rrr(8) = ds.Tables(0).Rows(ii)("lon")

                                                            idlingstoptableBack.Rows.Add(rrr)
                                                        End If

                                                    End If
                                            End Select

                                            prevtime = currenttime
                                            prevstatus = currentstatus
                                        End If
                                        tempprevtime = currenttime




                                        Try
                                            If ds.Tables(0).Rows(ii)("alarm") Then
                                                currentpto = "on"
                                            Else
                                                currentpto = "off"
                                            End If

                                            If prevpto <> currentpto Then
                                                Dim temptime As TimeSpan = currenttimepto - tempprevtimepto
                                                Dim minutes As Integer = temptime.TotalMinutes()
                                                Try
                                                    If prevpto = "on" And minutes > 30 Then
                                                        If (DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) <= atatime) Then 'From Source 
                                                            rpto = alarmTable.NewRow
                                                            rpto(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rpto(1) = ""
                                                            rpto(2) = tempprevtimepto.ToString("yyyy/MM/dd HH:mm:ss")
                                                            rpto(3) = currenttimepto.ToString("yyyy/MM/dd HH:mm:ss")
                                                            rpto(4) = temptime.TotalMinutes.ToString("0")
                                                            rpto(5) = lastlat
                                                            rpto(6) = lastlon
                                                            Dim ptocolor As Integer = 0
                                                            Dim nameofGeo As String = locObj.GetPublicGeofence(lastlat, lastlon)
                                                            Dim destnname As String = ""

                                                            If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                                                destnname = ShipToCodeDict.Item(dr("destination_siteid"))
                                                            Else
                                                                destnname = ""
                                                            End If

                                                            If nameofGeo <> "" Then
                                                                ptocolor = 1
                                                                If nameofGeo.ToString().ToUpper() = destnname.ToString().ToUpper() Then
                                                                    ptocolor = 2
                                                                End If
                                                            End If
                                                            rpto(7) = ptocolor
                                                            alarmTable.Rows.Add(rpto)
                                                        ElseIf DateTime.Parse(ds.Tables(0).Rows(ii)("datetime")) >= DateTime.Parse(dr("atd_datetime")) Then  ' To Source
                                                            rpto = alarmTableBack.NewRow
                                                            rpto(0) = ds.Tables(0).Rows(ii)("plateno")
                                                            rpto(1) = ""
                                                            rpto(2) = tempprevtimepto.ToString("yyyy/MM/dd HH:mm:ss")
                                                            rpto(3) = currenttimepto.ToString("yyyy/MM/dd HH:mm:ss")
                                                            rpto(4) = temptime.TotalMinutes.ToString("0")
                                                            rpto(5) = lastlat
                                                            rpto(6) = lastlon
                                                            Dim ptocolor As Integer = 0
                                                            Dim nameofGeo As String = locObj.GetPublicGeofence(lastlat, lastlon)
                                                            Dim destnname As String = ""

                                                            If ShipToCodeDict.ContainsKey(dr("destination_siteid")) Then
                                                                destnname = ShipToCodeDict.Item(dr("destination_siteid"))
                                                            Else
                                                                destnname = ""
                                                            End If

                                                            If nameofGeo <> "" Then
                                                                ptocolor = 1
                                                                If nameofGeo.ToString().ToUpper() = destnname.ToString().ToUpper() Then
                                                                    ptocolor = 2
                                                                End If
                                                            End If
                                                            rpto(7) = ptocolor
                                                            alarmTableBack.Rows.Add(rpto)
                                                        End If
                                                    End If
                                                Catch ex As Exception
                                                    Response.Write("Jaffa" & ex.Message)
                                                End Try
                                                ' End If
                                                tempprevtimepto = currenttimepto

                                            End If
                                            prevtimepto = currenttimepto
                                            prevpto = currentpto

                                        Catch ex As Exception
                                            Response.Write("Jaffa" & ex.Message)
                                        End Try

                                    Next

                                Catch ex As Exception
                                    ' Response.Write("Jaffodia" & ex.Message)
                                End Try

                                Try

                                    If idlingstoptable.Rows.Count = 0 Then
                                        r(20) = "--"
                                        r(21) = "--"
                                        r(22) = "--"
                                        r(23) = "--"
                                        r(29) = "--"
                                        r(35) = "--"
                                    Else
                                        Dim IdlingStopColumn As String = ""

                                        Dim idlingDur As String = ""


                                        Dim GeoColumn As String = ""
                                        Dim DurationColumn As String = ""
                                        Dim PTOColumn As String = ""

                                        Dim idinfo As String = ""
                                        Dim loctn As String = ""
                                        For cou As Integer = 0 To idlingstoptable.Rows.Count - 1
                                            loctn = locObj.GetPublicGeofence(idlingstoptable.Rows(cou)("lat"), idlingstoptable.Rows(cou)("lon"))
                                            If loctn = "" Then
                                                loctn = locObj.GetRoadName(idlingstoptable.Rows(cou)("lat"), idlingstoptable.Rows(cou)("lon"))
                                                If loctn = "" Then
                                                    loctn = idlingstoptable.Rows(cou)("lat") & "," & idlingstoptable.Rows(cou)("lon")
                                                End If
                                            End If

                                            idinfo = idinfo & "  " & idlingstoptable.Rows(cou)("status") & " from " & Convert.ToDateTime(idlingstoptable.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & " to " & Convert.ToDateTime(idlingstoptable.Rows(cou)("edt")).ToString("yyyy/MM/dd HH:mm:ss") & " at " & loctn & "  " & vbCrLf
                                            IdlingStopColumn = IdlingStopColumn & "<span onclick=""javascript:openMapPage('" & dr("plateno") & "','" & Convert.ToDateTime(idlingstoptable.Rows(cou)("bdt")).AddMinutes(-30).ToString("yyyy/MM/dd HH:mm:ss") & "','" & Convert.ToDateTime(idlingstoptable.Rows(cou)("edt")).AddMinutes(+10).ToString("yyyy/MM/dd HH:mm:ss") & "','" & idlingstoptable.Rows(cou)("lat") & "','" & idlingstoptable.Rows(cou)("lon") & "')"" style=""cursor:pointer;text-decoration: underline; color: #000080;"" title=""Show Map."">" & idlingstoptable.Rows(cou)("status") & " from " & Convert.ToDateTime(idlingstoptable.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & "</span><br/>"
                                            idlingDur = idlingDur & "  " & idlingstoptable.Rows(cou)("duration").ToString() & " Mins  <br/>"
                                            GeoColumn = GeoColumn & idlingstoptable.Rows(cou)("Loc") & "<br/>"
                                            DurationColumn = DurationColumn & idlingstoptable.Rows(cou)("duration") & " Mins<br/>"
                                            PTOColumn = PTOColumn & idlingstoptable.Rows(cou)("alarm") & "<br/>"
                                        Next
                                        idlingstoptable.Clear()
                                        r(20) = IdlingStopColumn
                                        r(21) = GeoColumn
                                        r(22) = DurationColumn
                                        r(23) = PTOColumn
                                        r(29) = idinfo
                                        r(35) = idlingDur
                                    End If

                                Catch ex As Exception
                                    r(20) = ex.Message
                                    r(21) = "--"
                                    r(22) = "--"
                                    r(23) = "--"
                                    r(29) = "--"
                                    r(35) = "--"
                                End Try

                                ' For Journey back to Source 
                                If Not IsDBNull(dr("journey_back_datetime")) Then
                                    r(28) = DateTime.Parse(dr("journey_back_datetime")).ToString("yyyy/MM/dd  HH:mm:ss")
                                Else
                                    r(28) = "--"
                                End If
                                Try
                                    If idlingstoptableBack.Rows.Count = 0 Then
                                        r(24) = "--"
                                        r(25) = "--"
                                        r(26) = "--"
                                        r(27) = "--"
                                        r(30) = "--"
                                        r(37) = "--"
                                    Else
                                        Dim idinfo As String = ""
                                        Dim IdlingStopColumn As String = ""

                                        Dim idlingDur As String = ""

                                        Dim GeoColumn As String = ""
                                        Dim DurationColumn As String = ""
                                        Dim PTOColumn As String = ""
                                        Dim loctn As String = ""
                                        For cou As Integer = 0 To idlingstoptableBack.Rows.Count - 1
                                            loctn = locObj.GetPublicGeofence(idlingstoptableBack.Rows(cou)("lat"), idlingstoptableBack.Rows(cou)("lon"))
                                            If loctn = "" Then
                                                loctn = locObj.GetRoadName(idlingstoptableBack.Rows(cou)("lat"), idlingstoptableBack.Rows(cou)("lon"))
                                                If loctn = "" Then
                                                    loctn = idlingstoptableBack.Rows(cou)("lat") & "," & idlingstoptableBack.Rows(cou)("lon")
                                                End If
                                            End If
                                            IdlingStopColumn = IdlingStopColumn & "<span onclick=""javascript:openMapPage('" & dr("plateno") & "','" & Convert.ToDateTime(idlingstoptableBack.Rows(cou)("bdt")).AddMinutes(-30).ToString("yyyy/MM/dd HH:mm:ss") & "','" & Convert.ToDateTime(idlingstoptableBack.Rows(cou)("edt")).AddMinutes(+10).ToString("yyyy/MM/dd HH:mm:ss") & "','" & idlingstoptableBack.Rows(cou)("lat") & "','" & idlingstoptableBack.Rows(cou)("lon") & "')"" style=""cursor:pointer;text-decoration: underline; color: #000080;"" title=""Show Map."">" & idlingstoptableBack.Rows(cou)("status") & " from " & Convert.ToDateTime(idlingstoptableBack.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & "</span><br/>"
                                            idinfo = idinfo & "  " & idlingstoptableBack.Rows(cou)("status") & " from " & Convert.ToDateTime(idlingstoptableBack.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & " to " & Convert.ToDateTime(idlingstoptableBack.Rows(cou)("edt")).ToString("yyyy/MM/dd HH:mm:ss") & " at " & loctn & "   "

                                            idlingDur = idlingDur & "  " & idlingstoptableBack.Rows(cou)("duration").ToString() & " Mins <br/> "

                                            GeoColumn = GeoColumn & idlingstoptableBack.Rows(cou)("Loc") & "<br/>"
                                            DurationColumn = DurationColumn & idlingstoptableBack.Rows(cou)("duration") & " Mins<br/>"
                                            PTOColumn = PTOColumn & idlingstoptableBack.Rows(cou)("alarm") & "<br/>"
                                        Next
                                        idlingstoptableBack.Clear()
                                        r(24) = IdlingStopColumn
                                        r(25) = GeoColumn
                                        r(26) = DurationColumn
                                        r(27) = PTOColumn
                                        r(30) = idinfo
                                        r(37) = idlingDur
                                    End If

                                Catch ex As Exception
                                    r(24) = ex.Message
                                    r(25) = "--"
                                    r(26) = "--"
                                    r(27) = "--"
                                    r(30) = "--"
                                    r(37) = "--"
                                End Try


                                Try
                                    'Response.Write("  " & alarmTable.Rows.Count & " and  " & alarmTableBack.Rows.Count & " ")
                                    If alarmTable.Rows.Count = 0 Then
                                        r(31) = "--"
                                        r(33) = "--"
                                        r(36) = "--"
                                    Else
                                        Dim alarm1 As String = ""
                                        Dim ainfo1 As String = ""
                                        Dim alarmduration As String = ""
                                        For cou As Integer = 0 To alarmTable.Rows.Count - 1
                                            Dim style1 As String = "style='Color:Red;cursor:pointer;text-decoration: underline;'"
                                            If alarmTable.Rows(cou)("color") = 1 Then
                                                style1 = "style='Color:Blue;cursor:pointer;text-decoration: underline;'"
                                            ElseIf alarmTable.Rows(cou)("color") = 2 Then
                                                style1 = "style='Color:Green;cursor:pointer;text-decoration: underline;'"
                                            End If
                                            Dim dttm As TimeSpan = Convert.ToDateTime(alarmTable.Rows(cou)("edt")) - Convert.ToDateTime(alarmTable.Rows(cou)("bdt"))

                                            ainfo1 = ainfo1 & "PTO On From " & Convert.ToDateTime(alarmTable.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & "<br/>"
                                            alarmduration = alarmduration & dttm.TotalMinutes.ToString("0") & " Mins <br/>"
                                            alarm1 = alarm1 & "<span " & style1 & "  onclick=""javascript:openMapPage('" & dr("plateno") & "','" & Convert.ToDateTime(alarmTable.Rows(cou)("bdt")).AddMinutes(-30).ToString("yyyy/MM/dd HH:mm:ss") & "','" & Convert.ToDateTime(alarmTable.Rows(cou)("edt")).AddMinutes(+10).ToString("yyyy/MM/dd HH:mm:ss") & "','" & alarmTable.Rows(cou)("lat") & "','" & alarmTable.Rows(cou)("lon") & "')"" title=""Show Map""> PTO On From " & Convert.ToDateTime(alarmTable.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & " </span><br/>"
                                        Next
                                        alarmTable.Clear()
                                        r(31) = alarm1
                                        r(33) = ainfo1
                                        r(36) = alarmduration
                                    End If
                                Catch ex As Exception
                                    r(31) = ex.Message
                                    r(33) = "--"
                                    r(36) = "--"
                                End Try

                                Try
                                    If alarmTableBack.Rows.Count = 0 Then
                                        r(32) = "--"
                                        r(34) = "--"
                                        r(38) = "--"
                                    Else
                                        Dim alarm2 As String = ""
                                        Dim ainfo1 As String = ""
                                        Dim alarmduration As String = ""
                                        For cou As Integer = 0 To alarmTableBack.Rows.Count - 1
                                            Dim style1 As String = "style='Color:Red;cursor:pointer;text-decoration: underline;'"
                                            If alarmTableBack.Rows(cou)("color") = 1 Then
                                                style1 = "style='Color:Blue;cursor:pointer;text-decoration: underline;'"
                                            ElseIf alarmTableBack.Rows(cou)("color") = 2 Then
                                                style1 = "style='Color:Green;cursor:pointer;text-decoration: underline;'"
                                            End If
                                            Dim dttm As TimeSpan = Convert.ToDateTime(alarmTableBack.Rows(cou)("edt")) - Convert.ToDateTime(alarmTableBack.Rows(cou)("bdt"))

                                            ainfo1 = ainfo1 & "PTO On From " & Convert.ToDateTime(alarmTableBack.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & "<br/>"
                                            alarm2 = alarm2 & "<span " & style1 & "  onclick=""javascript:openMapPage('" & dr("plateno") & "','" & Convert.ToDateTime(alarmTableBack.Rows(cou)("bdt")).AddMinutes(-30).ToString("yyyy/MM/dd HH:mm:ss") & "','" & Convert.ToDateTime(alarmTableBack.Rows(cou)("edt")).AddMinutes(+10).ToString("yyyy/MM/dd HH:mm:ss") & "','" & alarmTableBack.Rows(cou)("lat") & "','" & alarmTableBack.Rows(cou)("lon") & "')""  title=""Show Map""> PTO On From " & Convert.ToDateTime(alarmTableBack.Rows(cou)("bdt")).ToString("yyyy/MM/dd HH:mm:ss") & "</span><br/>"
                                            alarmduration = alarmduration & dttm.TotalMinutes.ToString("0") & " Mins <br/>"

                                        Next
                                        alarmTableBack.Clear()
                                        r(32) = alarm2
                                        r(34) = ainfo1
                                        r(38) = alarmduration
                                    End If
                                Catch ex As Exception
                                    r(32) = ex.Message
                                    r(34) = "--"
                                    r(38) = "--"
                                End Try

                                If DriverNameDict.ContainsKey(dr("dn_no").ToString()) Then
                                    r(43) = DriverNameDict.Item(dr("dn_no").ToString()).Split(";")(0).ToString()
                                    r(44) = DriverNameDict.Item(dr("dn_no").ToString()).Split(";")(1).ToString()
                                Else
                                    r(43) = "--"
                                    r(44) = "--"
                                End If

                                Try
                                    Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()

                                    Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                                    If IsDBNull(atatimess) And IsDBNull(dr("weight_outtime")) Then
                                        r(45) = "--"
                                    Else
                                        r(45) = (atatimess - Convert.ToDateTime(dr("weight_outtime"))).TotalMinutes.ToString("0.00")
                                    End If
                                Catch ex As Exception
                                    ' r(45) = ex.Message
                                End Try

                                Try
                                    If IsDBNull(dr("distance")) Then
                                        r(46) = "--"
                                    Else
                                        r(46) = Convert.ToDouble(dr("distance")).ToString("0.00")
                                    End If
                                Catch ex As Exception

                                End Try

                                Try
                                    r(51) = Convert.ToDateTime(dr("weight_outtime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    If IsDBNull(dr("plant_intime")) Then
                                        r(47) = "--"
                                        r(50) = "--"
                                    Else
                                        Dim plantintime As DateTime = Convert.ToDateTime(dr("plant_intime"))
                                        Dim tim As TimeSpan = (Convert.ToDateTime(dr("weight_outtime")) - plantintime)
                                        r(47) = tim.TotalMinutes.ToString("0") & " mins"

                                        'If tim.Days > 0 Then
                                        '    r(47) = tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min"
                                        'ElseIf tim.Hours > 0 Then
                                        '    r(47) = tim.Hours & " h " & tim.Minutes & " min"
                                        'Else
                                        '    r(47) = tim.Minutes & " min"
                                        'End If
                                        r(50) = plantintime.ToString("yyyy/MM/dd HH:mm:ss")
                                    End If
                                Catch ex As Exception
                                    ' r(45) = ex.Message
                                End Try

                                Try
                                    Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()
                                    Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                                    r(52) = atatimess.ToString("yyyy/MM/dd HH:mm:ss")
                                    If IsDBNull(dr("pto1_datetime")) Then
                                        r(48) = "--"
                                        r(53) = "--"
                                    Else
                                        ' r(48) = (Convert.ToDateTime(dr("pto1_datetime")) - atatimess).TotalMinutes.ToString("0.00")

                                        Dim tim As TimeSpan = Convert.ToDateTime(dr("pto1_datetime")) - atatimess
                                        r(48) = tim.TotalMinutes.ToString("0") & " mins"
                                        'If tim.Days > 0 Then
                                        '    r(48) = tim.Days & " D " & tim.Hours & " h " & tim.Minutes & " min"
                                        'ElseIf tim.Hours > 0 Then
                                        '    r(48) = tim.Hours & " h " & tim.Minutes & " min"
                                        'Else
                                        '    r(48) = tim.Minutes & " min"
                                        'End If

                                        r(53) = Convert.ToDateTime(dr("pto1_datetime")).ToString("yyyy/MM/dd HH:mm:ss")
                                    End If
                                Catch ex As Exception
                                    ' r(45) = ex.Message
                                End Try

                                Try
                                    If IsDBNull(dr("pto1_datetime")) Or IsDBNull(dr("pto2_datetime")) Then
                                        r(49) = "--"
                                    Else
                                        r(49) = (Convert.ToDateTime(dr("pto2_datetime")) - Convert.ToDateTime(dr("pto1_datetime"))).TotalMinutes.ToString("0.00")
                                    End If

                                Catch ex As Exception
                                    ' r(45) = ex.Message
                                End Try

                                t.Rows.Add(r)

                                i = i + 1

                            End If
                        End If

                    Catch ex As Exception
                        ' Response.Write("apk " & ex.Message)
                    End Try
                End While

            Catch ex As Exception

            Finally
                conn.Close()
            End Try


            Dim excelrow As DataRow
            For i As Integer = 0 To t.Rows.Count - 1
                Try
                    a = New ArrayList()
                    excelrow = exceltable.NewRow()
                    excelrow(0) = t.DefaultView.Item(i)(9)
                    a.Add(t.DefaultView.Item(i)(9))
                    excelrow(1) = t.DefaultView.Item(i)(8)
                    a.Add(t.DefaultView.Item(i)(8))
                    excelrow(2) = ""
                    a.Add(t.DefaultView.Item(i)(14))
                    excelrow(3) = t.DefaultView.Item(i)(5)
                    a.Add(t.DefaultView.Item(i)(5))
                    excelrow(4) = t.DefaultView.Item(i)(15)
                    a.Add(t.DefaultView.Item(i)(15))
                    excelrow(5) = t.DefaultView.Item(i)(2)
                    a.Add(t.DefaultView.Item(i)(2))
                    excelrow(6) = "--"
                    a.Add("--")
                    excelrow(7) = t.DefaultView.Item(i)(6)
                    a.Add(t.DefaultView.Item(i)(6))

                    If t.DefaultView.Item(i)(16) <> "--" Then
                        excelrow(8) = Convert.ToDateTime(t.DefaultView.Item(i)(7)).ToString("yyyy/MM/dd")
                        a.Add(Convert.ToDateTime(t.DefaultView.Item(i)(7)).ToString("yyyy/MM/dd"))
                        excelrow(9) = Convert.ToDateTime(t.DefaultView.Item(i)(7)).ToString("HH:mm:ss")
                        a.Add(Convert.ToDateTime(t.DefaultView.Item(i)(7)).ToString("HH:mm:ss"))
                        Dim ts As DateTime = t.DefaultView.Item(i)(16)
                        Dim dttm As DateTime = t.DefaultView.Item(i)(10)
                        Dim atatime As DateTime = dttm.Add(ts.TimeOfDay)
                        Dim span As TimeSpan = atatime.Subtract(Convert.ToDateTime(t.DefaultView.Item(i)(7)))
                        excelrow(10) = Convert.ToInt32(span.Hours).ToString("00") & ":" & Convert.ToInt32(span.Minutes).ToString("00") & ":" & Convert.ToInt32(span.Seconds).ToString("00")
                        a.Add(Convert.ToInt32(span.Hours).ToString("00") & ":" & Convert.ToInt32(span.Minutes).ToString("00") & ":" & Convert.ToInt32(span.Seconds).ToString("00"))
                        excelrow(11) = atatime
                        a.Add(Convert.ToDateTime(atatime).ToString("yyyy/MM/dd HH:mm:ss"))
                        If t.DefaultView.Item(i)(17) <> "--" Then
                            excelrow(12) = Convert.ToDateTime(t.DefaultView.Item(i)(17)).ToString("yyyy/MM/dd HH:mm:ss")
                            span = Convert.ToDateTime(t.DefaultView.Item(i)(17)).Subtract(atatime)
                            excelrow(13) = Convert.ToInt32(span.Hours).ToString("00") & ":" & Convert.ToInt32(span.Minutes).ToString("00") & ":" & Convert.ToInt32(span.Seconds).ToString("00")
                            a.Add(Convert.ToDateTime(t.DefaultView.Item(i)(17)).ToString("yyyy/MM/dd HH:mm:ss"))
                            a.Add(Convert.ToInt32(span.Hours).ToString("00") & ":" & Convert.ToInt32(span.Minutes).ToString("00") & ":" & Convert.ToInt32(span.Seconds).ToString("00"))
                        Else
                            excelrow(12) = "--"
                            excelrow(13) = "--"
                            a.Add("--")
                            a.Add("--")
                        End If
                    Else
                        excelrow(8) = "--"
                        a.Add("--")
                        excelrow(9) = "--"
                        a.Add("--")
                        excelrow(10) = "--"
                        a.Add("--")
                        excelrow(11) = "--"
                        a.Add("--")
                        excelrow(12) = "--"
                        excelrow(13) = "--"
                        a.Add("--")
                        a.Add("--")
                    End If
                    a.Add(t.DefaultView.Item(i)(28))
                    excelrow(14) = t.DefaultView.Item(i)(28)

                    a.Add(t.DefaultView.Item(i)(18))
                    excelrow(15) = t.DefaultView.Item(i)(18)
                    a.Add(t.DefaultView.Item(i)(19))
                    excelrow(16) = t.DefaultView.Item(i)(19)



                    a.Add(t.DefaultView.Item(i)(20))
                    '18 idling dur
                    a.Add(t.DefaultView.Item(i)(35))
                    excelrow(17) = t.DefaultView.Item(i)(29)



                    a.Add(t.DefaultView.Item(i)(21))
                    excelrow(18) = t.DefaultView.Item(i)(21)






                    excelrow(19) = t.DefaultView.Item(i)(22)

                    a.Add(t.DefaultView.Item(i)(31))

                    '21 pto dur
                    a.Add(t.DefaultView.Item(i)(36))


                    excelrow(20) = t.DefaultView.Item(i)(33)

                    a.Add(t.DefaultView.Item(i)(24))
                    '23 idling dur
                    a.Add(t.DefaultView.Item(i)(37))

                    excelrow(21) = t.DefaultView.Item(i)(30)
                    a.Add(t.DefaultView.Item(i)(25))
                    excelrow(22) = t.DefaultView.Item(i)(25)

                    excelrow(23) = t.DefaultView.Item(i)(26)




                    a.Add(t.DefaultView.Item(i)(32))
                    excelrow(24) = t.DefaultView.Item(i)(34)

                    '22 idling dur
                    a.Add(t.DefaultView.Item(i)(38))

                    If t.DefaultView.Item(i)(0) <> "--" Then
                        a.Add("<span onclick=""javascript:openpage('" & t.DefaultView.Item(i)(0) & "')"" style=""cursor:pointer;text-decoration: underline; color: #000080;"" title=""Show Idling/Stop/PTO information."">Map</span>")
                    Else
                        a.Add("--")
                    End If
                    a.Add(t.DefaultView.Item(i)(39))
                    a.Add(t.DefaultView.Item(i)(40))
                    a.Add(t.DefaultView.Item(i)(41))
                    a.Add(t.DefaultView.Item(i)(42))
                    a.Add(t.DefaultView.Item(i)(43))
                    a.Add(t.DefaultView.Item(i)(44))

                    a.Add(t.DefaultView.Item(i)(45))
                    a.Add(t.DefaultView.Item(i)(46))

                    a.Add(t.DefaultView.Item(i)(50))
                    a.Add(t.DefaultView.Item(i)(47))
                    a.Add(t.DefaultView.Item(i)(51))
                    a.Add(t.DefaultView.Item(i)(52))
                    a.Add(t.DefaultView.Item(i)(48))
                    a.Add(t.DefaultView.Item(i)(53))




                    a.Add(t.DefaultView.Item(i)(49))



                    If t.DefaultView.Item(i)(31).ToString().Contains("style='Color:Green;") Then
                        excelrow(25) = "In Destination Geofence"
                    ElseIf t.DefaultView.Item(i)(31).ToString().Contains("style='Color:Blue;") Then
                        excelrow(25) = "In Public Geofence"
                    ElseIf t.DefaultView.Item(i)(31).ToString().Contains("style='Color:Red;") Then
                        excelrow(25) = "Outside Geofence"
                    Else
                        excelrow(25) = "--"
                    End If

                    If t.DefaultView.Item(i)(32).ToString().Contains("style='Color:Green;") Then
                        excelrow(26) = "In Destination Geofence"
                    ElseIf t.DefaultView.Item(i)(32).ToString().Contains("style='Color:Blue;") Then
                        excelrow(26) = "In Public Geofence"
                    ElseIf t.DefaultView.Item(i)(32).ToString().Contains("style='Color:Red;") Then
                        excelrow(26) = "Outside Geofence"
                    Else
                        excelrow(26) = "--"
                    End If

                    excelrow(27) = t.DefaultView.Item(i)(35)
                    excelrow(28) = t.DefaultView.Item(i)(36)
                    excelrow(29) = t.DefaultView.Item(i)(37)
                    excelrow(30) = t.DefaultView.Item(i)(38)
                    excelrow(31) = t.DefaultView.Item(i)(39)
                    excelrow(32) = t.DefaultView.Item(i)(43)
                    excelrow(33) = t.DefaultView.Item(i)(44)

                    excelrow(34) = t.DefaultView.Item(i)(45)
                    excelrow(35) = t.DefaultView.Item(i)(46)

                    excelrow(36) = t.DefaultView.Item(i)(47)
                    excelrow(37) = t.DefaultView.Item(i)(48)
                    excelrow(38) = t.DefaultView.Item(i)(49)


                    aa.Add(a)
                    exceltable.Rows.Add(excelrow)
                Catch ex As Exception

                End Try
            Next

            Session.Remove("exceltable")
            Session.Remove("exceltable2")

            Session("exceltable") = exceltable

        Catch ex As Exception
            '  Response.Write(ex.Message)
        End Try


        json = JsonConvert.SerializeObject(aa, Formatting.None)

        Response.ContentType = "text/plain"
        Response.Write(json)

    End Sub
    Private Function GetLostData(ByVal pno As String, ByVal begindt As String, ByVal enddt As String) As vehicleInfo
        Dim obj As New vehicleInfo
        Try
            obj.status = False
            obj.userid = ""
            obj.bdtL = ""
            obj.edtL = ""

            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            Dim ds As New DataSet
            Dim da As New SqlDataAdapter("select vh.timestamp,vh.gps_av,vh.ignition_sensor,vt.userid from (select lat,lon,timestamp,gps_av,ignition_sensor,plateno from vehicle_history where plateno='" & pno & "' and timestamp between '" & Convert.ToDateTime(begindt).ToString("yyyy/MM/dd HH:mm:ss") & "' and '" & Convert.ToDateTime(enddt).ToString("yyyy/MM/dd HH:mm:ss") & "') vh  left outer join vehicleTBL vt on vt.plateno=vh.plateno ", conn)
            da.Fill(ds)
            Dim prevstatus As String = "A"
            Dim prevdatetime As DateTime
            Dim currentstatus As String = "A"
            Dim currentdatetime As DateTime
            Dim totaldur As Integer = 0
            Dim r As DataRow

            Dim tempprevtime As DateTime
            If ds.Tables(0).Rows.Count > 0 Then
                prevdatetime = Convert.ToDateTime(ds.Tables(0).Rows(0)(0))
                prevstatus = ds.Tables(0).Rows(0)(1).ToString.ToUpper()
                For i As Integer = 0 To ds.Tables(0).Rows.Count - 1
                    currentdatetime = Convert.ToDateTime(ds.Tables(0).Rows(i)("timestamp"))
                    currentstatus = ds.Tables(0).Rows(i)("gps_av").ToString.ToUpper()
                    Try
                        If i > 1 Then
                            If ds.Tables(0).Rows(i)("ignition_sensor").ToString() = "1" And ds.Tables(0).Rows(i - 1)("ignition_sensor").ToString() = "1" And (currentdatetime - tempprevtime).Minutes > 15 Then

                                obj.status = True
                                obj.userid = ds.Tables(0).Rows(i)("userid").ToString()
                                obj.bdtL = Convert.ToDateTime(tempprevtime).ToString("yyyy/MM/dd HH:mm:ss")
                                obj.edtL = Convert.ToDateTime(currentdatetime).ToString("yyyy/MM/dd HH:mm:ss")
                                Return obj
                            End If
                            If ds.Tables(0).Rows(i)("ignition_sensor").ToString() = "0" And ds.Tables(0).Rows(i - 1)("ignition_sensor").ToString() = "0" And (currentdatetime - tempprevtime).Minutes > 120 Then
                                obj.status = True
                                obj.userid = ds.Tables(0).Rows(i)("userid").ToString()
                                obj.bdtL = Convert.ToDateTime(tempprevtime).ToString("yyyy/MM/dd HH:mm:ss")
                                obj.edtL = Convert.ToDateTime(currentdatetime).ToString("yyyy/MM/dd HH:mm:ss")
                                Return obj
                            End If
                        End If

                    Catch ex As Exception
                        '  Response.Write(ex.Message)
                    End Try

                    If prevstatus <> currentstatus Then
                        Dim temptime As TimeSpan = tempprevtime - prevdatetime  'currenttime - prevtime
                        Dim minutes As Int16 = temptime.TotalMinutes()

                        Select Case prevstatus
                            Case "V"
                                If temptime.Minutes > 15 Then
                                    Try
                                        obj.status = True
                                        obj.userid = ds.Tables(0).Rows(i)("userid").ToString()
                                        obj.bdtL = Convert.ToDateTime(prevdatetime).ToString("yyyy/MM/dd HH:mm:ss")
                                        obj.edtL = Convert.ToDateTime(currentdatetime).ToString("yyyy/MM/dd HH:mm:ss")
                                        Return obj
                                    Catch ex As Exception
                                        '  Response.Write(ex.Message)
                                    End Try
                                End If
                        End Select
                        prevdatetime = currentdatetime
                        prevstatus = currentstatus
                    End If

                    tempprevtime = currentdatetime
                Next

            End If
        Catch ex As Exception
            obj.status = False
            obj.userid = "Error " & ex.Message
            obj.bdtL = ""
            obj.edtL = ""
        End Try
        Return obj
    End Function
    Structure vehicleInfo
        Dim status As Boolean
        Dim userid As String
        Dim bdtL As String
        Dim edtL As String
    End Structure
End Class
