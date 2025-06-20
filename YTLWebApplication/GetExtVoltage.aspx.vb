Imports System.Data.SqlClient

Public Class GetExtVoltage
    Inherits SecurePageBase

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            ' Validate authentication
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.Redirect("~/Login.aspx")
                Return
            End If
            
            ' Validate and sanitize inputs
            Dim dttm As String = SecurityHelper.SanitizeForHtml(Request.QueryString("d"))
            Dim plateno As String = SecurityHelper.SanitizeForHtml(Request.QueryString("plateno"))
            
            ' Validate date parameter
            If Not SecurityHelper.ValidateDate(dttm) Then
                Response.Write("Invalid date parameter")
                Response.End()
                Return
            End If
            
            ' Validate plate number
            If Not SecurityHelper.ValidatePlateNumber(plateno) Then
                Response.Write("Invalid plate number")
                Response.End()
                Return
            End If
            
            Dim baseDateTime As DateTime = Convert.ToDateTime(dttm)
            Dim bdt As String = baseDateTime.AddMinutes(-10).ToString("yyyy/MM/dd HH:mm:ss")
            Dim edt As String = baseDateTime.AddMinutes(10).ToString("yyyy/MM/dd HH:mm:ss")
            
            Dim t As New DataTable
            t.Columns.Add(New DataColumn("No"))
            t.Columns.Add(New DataColumn("Plateno"))
            t.Columns.Add(New DataColumn("Date Time"))
            t.Columns.Add(New DataColumn("GPS"))
            t.Columns.Add(New DataColumn("Speed"))
            t.Columns.Add(New DataColumn("External Voltage"))

            ' Use parameterized query
            Dim parameters As New Dictionary(Of String, Object)
            parameters.Add("@plateno", plateno)
            parameters.Add("@startDate", bdt)
            parameters.Add("@endDate", edt)
            
            Dim query As String = "SELECT DISTINCT CONVERT(varchar(19), timestamp, 120) as datetime, externalbatv, gps_av, speed FROM vehicle_history vht JOIN vehicleTBL vt ON vt.plateno = vht.plateno WHERE vt.plateno = @plateno AND timestamp BETWEEN @startDate AND @endDate ORDER BY datetime"
            
            Dim vehicleData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            
            Dim i As Int64 = 1
            For Each dr As DataRow In vehicleData.Rows
                Dim r As DataRow = t.NewRow
                r(0) = i.ToString()
                r(1) = SanitizeOutput(plateno)
                r(2) = SanitizeOutput(dr("datetime").ToString())
                r(3) = SanitizeOutput(dr("gps_av").ToString())
                r(4) = System.Convert.ToDouble(dr("speed")).ToString("0.00")

                If IsDBNull(dr("externalbatv")) Then
                    r(5) = "-"
                Else
                    r(5) = SanitizeOutput(dr("externalbatv").ToString())
                End If

                t.Rows.Add(r)
                i = i + 1
            Next

            If t.Rows.Count = 0 Then
                Dim r As DataRow = t.NewRow
                For j As Integer = 0 To 5
                    r(j) = "--"
                Next
                t.Rows.Add(r)
            End If

            gv1.DataSource = t
            gv1.DataBind()
            
            ' Log data access
            AuditLogger.LogDataAccess("vehicle_history", "READ")
            
        Catch ex As Exception
            SecurityHelper.LogError("GetExtVoltage error", ex, Server)
            Response.Write("An error occurred while processing your request")
        End Try
    End Sub
End Class