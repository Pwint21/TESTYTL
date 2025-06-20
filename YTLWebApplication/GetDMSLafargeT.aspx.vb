Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json

Partial Class GetDMSLafargeT
    Inherits SecurePageBase
    Implements IRequiresCSRF
    
    Public connstr As String
    Public locationDict As New Dictionary(Of Integer, List(Of Location))
    Dim sdate As String
    Dim edate As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            ' Validate user authentication and authorization
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.StatusCode = 401
                Response.End()
                Return
            End If
            
            ' Log data access
            AuditLogger.LogDataAccess("DMS_Data", "READ")
            
            Response.Write(GetJson())
            Response.ContentType = "application/json"
        Catch ex As Exception
            SecurityHelper.LogError("GetDMSLafargeT error", ex, Server)
            Response.StatusCode = 500
            Response.Write("{""error"":""Internal server error""}")
        End Try
    End Sub

    Protected Function GetJson() As String
        Dim json As String = ""
        
        Try
            ' Validate and sanitize date inputs
            sdate = SecurityHelper.SanitizeForHtml(Request.QueryString("fdt"))
            edate = SecurityHelper.SanitizeForHtml(Request.QueryString("tdt"))
            
            If Not SecurityHelper.ValidateDate(sdate) OrElse Not SecurityHelper.ValidateDate(edate) Then
                sdate = Date.Now.AddDays(-2).ToString("yyyy/MM/dd")
                edate = Date.Now.AddDays(-1).ToString("yyyy/MM/dd")
            End If
            
            ' Validate user access
            Dim userId As String = SecurityHelper.ValidateAndGetUserId(Request)
            Dim usersList As String = SecurityHelper.ValidateAndGetUsersList(Request)
            
            Dim ShipToCodeDict As New Dictionary(Of String, String)
            
            ' Use parameterized queries
            Dim parameters As New Dictionary(Of String, Object)
            Dim query As String = "SELECT DISTINCT shiptocode, geofencename FROM geofence WHERE accesstype = @accesstype"
            parameters.Add("@accesstype", "1")
            
            Dim geofenceData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            
            For Each row As DataRow In geofenceData.Rows
                Dim shipToCode As String = row("shiptocode").ToString()
                If Not ShipToCodeDict.ContainsKey(shipToCode) Then
                    ShipToCodeDict.Add(shipToCode, SecurityHelper.SanitizeForHtml(row("geofencename").ToString()))
                End If
            Next

            Dim aa As New ArrayList()
            Dim a As ArrayList
            
            Server.ScriptTimeout = 300 ' Limit script timeout
            
            ' Validate query parameters
            Dim bdt As String = SecurityHelper.SanitizeForHtml(Request.QueryString("fdt"))
            Dim edt As String = SecurityHelper.SanitizeForHtml(Request.QueryString("tdt"))
            
            If Not SecurityHelper.ValidateDate(bdt) OrElse Not SecurityHelper.ValidateDate(edt) Then
                Throw New ArgumentException("Invalid date parameters")
            End If
            
            Dim DriverNameDict As New Dictionary(Of String, String)
            
            ' Use parameterized query for driver data
            Dim driverParams As New Dictionary(Of String, Object)
            driverParams.Add("@startDate", bdt)
            driverParams.Add("@endDate", edt)
            
            Dim driverQuery As String = "SELECT dn_no, dn_driver, dn_qty FROM oss_patch_in WHERE weight_outtime BETWEEN @startDate AND @endDate AND dn_driver IS NOT NULL"
            Dim driverData As DataTable = DatabaseHelper.ExecuteQuery(driverQuery, driverParams)
            
            For Each row As DataRow In driverData.Rows
                Dim dnNo As String = row("dn_no").ToString()
                If Not DriverNameDict.ContainsKey(dnNo) Then
                    DriverNameDict.Add(dnNo, SecurityHelper.SanitizeForHtml(row("dn_driver").ToString()))
                End If
            Next

            ' Use parameterized query for main data
            Dim mainParams As New Dictionary(Of String, Object)
            mainParams.Add("@startDate", bdt)
            mainParams.Add("@endDate", edt)
            
            Dim mainQuery As String = "SELECT * FROM oss_patch_out WHERE weight_outtime BETWEEN @startDate AND @endDate AND status IN ('7','8','12','13')"
            Dim mainData As DataTable = DatabaseHelper.ExecuteQuery(mainQuery, mainParams)
            
            Dim i As Int32 = 1
            For Each dr As DataRow In mainData.Rows
                Try
                    a = New ArrayList()
                    a.Add(SecurityHelper.SanitizeForHtml("ShipToName"))
                    a.Add(SecurityHelper.SanitizeForHtml(dr("dn_no").ToString()))
                    
                    If IsDBNull(dr("transporter")) Then
                        a.Add("--")
                    Else
                        a.Add(SecurityHelper.SanitizeForHtml(dr("transporter").ToString()))
                    End If
                    
                    a.Add(SecurityHelper.SanitizeForHtml(dr("plateno").ToString()))

                    If DriverNameDict.ContainsKey(dr("dn_no").ToString()) Then
                        a.Add(DriverNameDict.Item(dr("dn_no").ToString()))
                    Else
                        a.Add("--")
                    End If

                    a.Add(SecurityHelper.SanitizeForHtml(dr("source_supply").ToString()))

                    If ShipToCodeDict.ContainsKey(dr("destination_siteid").ToString()) Then
                        a.Add(ShipToCodeDict.Item(dr("destination_siteid").ToString()))
                    Else
                        a.Add("--")
                    End If

                    a.Add(SecurityHelper.SanitizeForHtml(dr("destination_siteid").ToString()))

                    ' Process loading times safely
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
                        End If
                    Catch ex As Exception
                        SecurityHelper.LogError("Date processing error", ex, Server)
                        a.Add("--")
                        a.Add("--")
                        a.Add("--")
                        a.Add("--")
                        a.Add("--")
                    End Try

                    ' Process travelling time safely
                    Try
                        If Not (IsDBNull(dr("ata_date")) And IsDBNull(dr("ata_time"))) Then
                            Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()
                            Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                            Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("weight_outtime")))
                            a.Add(tim.TotalMinutes.ToString("0"))
                        Else
                            a.Add("--")
                        End If
                    Catch ex As Exception
                        SecurityHelper.LogError("Travel time processing error", ex, Server)
                        a.Add("--")
                    End Try

                    ' Process distance safely
                    Try
                        If IsDBNull(dr("distance")) Then
                            a.Add("0.00")
                        Else
                            a.Add(Convert.ToDouble(dr("distance")).ToString("0.00"))
                        End If
                    Catch ex As Exception
                        a.Add("0.00")
                    End Try

                    ' Process waiting time safely
                    Try
                        If IsDBNull(dr("pto1_datetime")) Then
                            If Not IsDBNull(dr("wait_start_time")) Then
                                If Not (IsDBNull(dr("ata_date")) And IsDBNull(dr("ata_time"))) Then
                                    Dim tsss As String = (Convert.ToDateTime(dr("ata_date")).ToString("yyyy/MM/dd") & " " & dr("ata_time").ToString()).ToString()
                                    Dim atatimess As DateTime = Convert.ToDateTime(tsss)
                                    a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("yyyy/MM/dd"))
                                    a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("HH:mm:ss"))
                                    a.Add(atatimess.ToString("yyyy/MM/dd"))
                                    a.Add(atatimess.ToString("HH:mm:ss"))
                                    Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("wait_start_time")))
                                    a.Add(tim.TotalMinutes.ToString("0"))
                                Else
                                    For j As Integer = 0 To 4
                                        a.Add("--")
                                    Next
                                End If
                            Else
                                For j As Integer = 0 To 4
                                    a.Add("--")
                                Next
                            End If
                        ElseIf IsDBNull(dr("wait_start_time")) Then
                            a.Add("--")
                            a.Add("--")
                            a.Add(Convert.ToDateTime(dr("pto1_datetime")).ToString("yyyy/MM/dd"))
                            a.Add(Convert.ToDateTime(dr("pto1_datetime")).ToString("HH:mm:ss"))
                            a.Add("--")
                        Else
                            Dim atatimess As DateTime = Convert.ToDateTime(dr("pto1_datetime").ToString())
                            a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("yyyy/MM/dd"))
                            a.Add(Convert.ToDateTime(dr("wait_start_time")).ToString("HH:mm:ss"))
                            a.Add(atatimess.ToString("yyyy/MM/dd"))
                            a.Add(atatimess.ToString("HH:mm:ss"))
                            Dim tim As TimeSpan = (atatimess - Convert.ToDateTime(dr("wait_start_time")))
                            a.Add(tim.TotalMinutes.ToString("0"))
                        End If
                    Catch ex As Exception
                        SecurityHelper.LogError("Waiting time processing error", ex, Server)
                        For j As Integer = 0 To 4
                            a.Add("--")
                        Next
                    End Try

                    ' Process unloading time safely
                    Try
                        If IsDBNull(dr("pto1_datetime")) Or IsDBNull(dr("pto2_datetime")) Then
                            For j As Integer = 0 To 4
                                a.Add("--")
                            Next
                        Else
                            Dim atatimess As DateTime = Convert.ToDateTime(dr("pto1_datetime").ToString())
                            a.Add(atatimess.ToString("yyyy/MM/dd"))
                            a.Add(atatimess.ToString("HH:mm:ss"))
                            a.Add(Convert.ToDateTime(dr("pto2_datetime")).ToString("yyyy/MM/dd"))
                            a.Add(Convert.ToDateTime(dr("pto2_datetime")).ToString("HH:mm:ss"))
                            Dim tim As TimeSpan = (Convert.ToDateTime(dr("pto2_datetime")) - atatimess)
                            a.Add(tim.TotalMinutes.ToString("0"))
                        End If
                    Catch ex As Exception
                        SecurityHelper.LogError("Unloading time processing error", ex, Server)
                        For j As Integer = 0 To 4
                            a.Add("--")
                        Next
                    End Try

                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Row processing error", ex, Server)
                End Try
            Next

            json = JsonConvert.SerializeObject(aa, Formatting.None)
            
        Catch ex As Exception
            SecurityHelper.LogError("GetJson error", ex, Server)
            json = "{""error"":""Data processing failed""}"
        End Try

        Return json
    End Function

    Protected Sub WriteLog(ByVal message As String)
        Try
            If Not String.IsNullOrEmpty(message) AndAlso message.Length <= 1000 Then
                Dim sanitizedMessage As String = SecurityHelper.SanitizeForHtml(message)
                AuditLogger.LogSystemEvent("DATA_LOG", sanitizedMessage)
            End If
        Catch ex As Exception
            ' Silent fail for logging
        End Try
    End Sub
End Class