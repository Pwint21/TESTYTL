Imports System.Data
Imports System.Data.SqlClient
Imports Newtonsoft.Json

Partial Class GetGeofenceAlertsData
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
            
            Dim oper As String = SecurityHelper.SanitizeForHtml(Request.QueryString("opr"))
            
            ' Validate CSRF token for state-changing operations
            If oper = "1" OrElse oper = "2" OrElse oper = "3" Then
                If Not ValidateCSRF(Request.Form("__CSRFToken")) Then
                    Response.StatusCode = 403
                    Response.Write("{""error"":""Invalid request""}")
                    Response.End()
                    Return
                End If
            End If

            Select Case oper.ToUpper()
                Case "0"
                    GetData()
                Case "1"
                    InsertData()
                Case "2"
                    UpdateData()
                Case "3"
                    DeleteData()
                Case Else
                    Response.StatusCode = 400
                    Response.Write("{""error"":""Invalid operation""}")
            End Select

        Catch ex As Exception
            SecurityHelper.LogError("GetGeofenceAlertsData error", ex, Server)
            Response.StatusCode = 500
            Response.Write("{""error"":""Internal server error""}")
        End Try
    End Sub

    Private Sub GetData()
        Try
            Dim aa As New ArrayList()
            Dim a As ArrayList
            
            Dim parameters As New Dictionary(Of String, Object)
            Dim query As String = "SELECT gt.geofenceid, at.id, at.emaillist, at.mobileno, gt.geofencename FROM (SELECT geofencename, geofenceid FROM geofence WHERE accesstype = @accesstype) as gt LEFT OUTER JOIN lafarge_private_geofence_alert_settings at ON at.geofenceid = gt.geofenceid"
            parameters.Add("@accesstype", "2")
            
            Dim alertData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            
            Dim c As Integer = 0
            For Each dr As DataRow In alertData.Rows
                c += 1
                a = New ArrayList()

                If IsDBNull(dr("id")) Then
                    a.Add("0")
                Else
                    a.Add(dr("id"))
                End If

                a.Add(c)
                a.Add(SanitizeOutput(dr("geofencename").ToString()))

                If IsDBNull(dr("emaillist")) Then
                    a.Add("")
                Else
                    a.Add(SanitizeOutput(dr("emaillist").ToString()))
                End If

                If IsDBNull(dr("mobileno")) Then
                    a.Add("")
                Else
                    a.Add(SanitizeOutput(dr("mobileno").ToString()))
                End If

                a.Add(dr("geofenceid"))

                If IsDBNull(dr("id")) Then
                    a.Add("0")
                Else
                    a.Add(dr("id"))
                End If
                aa.Add(a)
            Next
            
            Dim json As String = JsonConvert.SerializeObject(aa, Formatting.None)
            Response.ContentType = "application/json"
            Response.Write(json)
            
            AuditLogger.LogDataAccess("geofence_alerts", "READ")
            
        Catch ex As Exception
            SecurityHelper.LogError("GetData error", ex, Server)
            Response.Write("{""error"":""Data retrieval failed""}")
        End Try
    End Sub

    Private Sub InsertData()
        Dim res As String = "0"
        Try
            ' Validate and sanitize inputs
            Dim geoid As String = SecurityHelper.SanitizeForHtml(Request.QueryString("geoid"))
            Dim email As String = SecurityHelper.SanitizeForHtml(Request.QueryString("eml"))
            Dim mob As String = SecurityHelper.SanitizeForHtml(Request.QueryString("mob"))
            
            ' Validate geofence ID
            Dim geoIdInt As Integer
            If Not Integer.TryParse(geoid, geoIdInt) OrElse geoIdInt <= 0 Then
                Response.Write("{""error"":""Invalid geofence ID""}")
                Return
            End If
            
            ' Validate email format if provided
            If Not String.IsNullOrEmpty(email) AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") Then
                Response.Write("{""error"":""Invalid email format""}")
                Return
            End If
            
            ' Validate mobile number if provided
            If Not String.IsNullOrEmpty(mob) AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(mob, "^[0-9+\-\s()]{7,20}$") Then
                Response.Write("{""error"":""Invalid mobile number""}")
                Return
            End If
            
            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@geofenceid", geoid)
            parameters.Add("@emaillist", email)
            parameters.Add("@mobileno", mob)
            parameters.Add("@updateddatetime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))
            
            Dim query As String = "INSERT INTO lafarge_private_geofence_alert_settings (geofenceid, emaillist, mobileno, updateddatetime) VALUES (@geofenceid, @emaillist, @mobileno, @updateddatetime)"
            
            res = DatabaseHelper.ExecuteNonQuery(query, parameters).ToString()
            
            AuditLogger.LogUserAction("GEOFENCE_ALERT_INSERT", "Geofence ID: " & geoid)
            
        Catch ex As Exception
            SecurityHelper.LogError("InsertData error", ex, Server)
            res = "0"
        End Try

        Response.ContentType = "application/json"
        Response.Write("{""result"":" & res & "}")
    End Sub

    Private Sub UpdateData()
        Dim res As String = "0"
        Try
            ' Validate and sanitize inputs
            Dim alertId As String = SecurityHelper.SanitizeForHtml(Request.QueryString("geoid"))
            Dim email As String = SecurityHelper.SanitizeForHtml(Request.QueryString("eml"))
            Dim mob As String = SecurityHelper.SanitizeForHtml(Request.QueryString("mob"))
            
            ' Validate alert ID
            Dim alertIdInt As Integer
            If Not Integer.TryParse(alertId, alertIdInt) OrElse alertIdInt <= 0 Then
                Response.Write("{""error"":""Invalid alert ID""}")
                Return
            End If
            
            ' Validate email format if provided
            If Not String.IsNullOrEmpty(email) AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") Then
                Response.Write("{""error"":""Invalid email format""}")
                Return
            End If
            
            ' Validate mobile number if provided
            If Not String.IsNullOrEmpty(mob) AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(mob, "^[0-9+\-\s()]{7,20}$") Then
                Response.Write("{""error"":""Invalid mobile number""}")
                Return
            End If
            
            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@emaillist", email)
            parameters.Add("@mobileno", mob)
            parameters.Add("@updateddatetime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))
            parameters.Add("@id", alertId)
            
            Dim query As String = "UPDATE lafarge_private_geofence_alert_settings SET emaillist = @emaillist, mobileno = @mobileno, updateddatetime = @updateddatetime WHERE id = @id"
            
            res = DatabaseHelper.ExecuteNonQuery(query, parameters).ToString()
            
            AuditLogger.LogUserAction("GEOFENCE_ALERT_UPDATE", "Alert ID: " & alertId)
            
        Catch ex As Exception
            SecurityHelper.LogError("UpdateData error", ex, Server)
            res = "0"
        End Try

        Response.ContentType = "application/json"
        Response.Write("{""result"":" & res & "}")
    End Sub

    Private Sub DeleteData()
        Try
            Dim chekitems As String = SecurityHelper.SanitizeForHtml(Request.QueryString("geoid"))
            Dim result As Integer = 0
            Dim res As String = "0"
            
            If String.IsNullOrEmpty(chekitems) Then
                Response.Write("{""error"":""No items selected""}")
                Return
            End If
            
            Dim ids As String() = chekitems.Split(","c)
            
            ' Validate all IDs
            For Each id As String In ids
                Dim numericId As Integer
                If Not Integer.TryParse(id.Trim(), numericId) OrElse numericId <= 0 Then
                    Response.Write("{""error"":""Invalid ID format""}")
                    Return
                End If
            Next
            
            ' Delete each item using parameterized queries
            For i As Integer = 0 To ids.Length - 1
                Try
                    Dim parameters As New Dictionary(Of String, Object)
                    parameters.Add("@id", ids(i).Trim())
                    
                    Dim query As String = "DELETE FROM lafarge_private_geofence_alert_settings WHERE id = @id"
                    result = DatabaseHelper.ExecuteNonQuery(query, parameters)
                    
                    If result > 0 Then
                        res = "1"
                    End If
                Catch ex As Exception
                    SecurityHelper.LogError("Delete item error", ex, Server)
                End Try
            Next
            
            AuditLogger.LogUserAction("GEOFENCE_ALERT_DELETE", "Deleted IDs: " & chekitems)

            Response.ContentType = "application/json"
            Response.Write("{""result"":" & res & "}")
            
        Catch ex As Exception
            SecurityHelper.LogError("DeleteData error", ex, Server)
            Response.Write("{""error"":""Delete operation failed""}")
        End Try
    End Sub
End Class