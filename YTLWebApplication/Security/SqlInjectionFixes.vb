' Example of secure SQL query implementation
' Replace all dynamic SQL with parameterized queries

Public Class SecureDatabaseAccess
    
    ' SECURE: Parameterized query example
    Public Function GetGeofenceDataSecure(plateno As String, weightOutTime As DateTime) As List(Of GeofenceData)
        Dim visits As New List(Of GeofenceData)
        Dim conn As New SqlConnection(ConfigurationManager.AppSettings("sqlserverconnection2"))
        
        Try
            ' Use parameterized query instead of string concatenation
            Dim cmd As New SqlCommand("sp_GetbuildconGeofenceData", conn)
            cmd.CommandType = CommandType.StoredProcedure
            
            ' Validate input before using
            If String.IsNullOrWhiteSpace(plateno) OrElse plateno.Length > 20 Then
                Throw New ArgumentException("Invalid plate number")
            End If
            
            ' Use parameters to prevent SQL injection
            cmd.Parameters.Add("@weightfromtimes", SqlDbType.DateTime).Value = weightOutTime
            cmd.Parameters.Add("@plateno", SqlDbType.VarChar, 20).Value = plateno
            
            conn.Open()
            Dim dr As SqlDataReader = cmd.ExecuteReader()
            
            While dr.Read()
                Dim g As New GeofenceData()
                g.dnno = If(IsDBNull(dr("dnno")), String.Empty, dr("dnno").ToString())
                g.duration = If(IsDBNull(dr("duration")), 0, Convert.ToInt32(dr("duration")))
                g.intime = If(IsDBNull(dr("intime")), DateTime.MinValue, Convert.ToDateTime(dr("intime")))
                g.outtime = If(IsDBNull(dr("outtime")), String.Empty, Convert.ToDateTime(dr("outtime")).ToString("yyyy/MM/dd HH:mm:ss"))
                g.name = If(IsDBNull(dr("geofencename")), String.Empty, dr("geofencename").ToString().ToUpper())
                g.plate = If(IsDBNull(dr("plateno")), String.Empty, dr("plateno").ToString().ToUpper())
                visits.Add(g)
            End While
            
        Catch ex As Exception
            ' Log error securely without exposing details to user
            LogError(ex)
            Throw New ApplicationException("An error occurred while retrieving data")
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
        
        Return visits
    End Function
    
    ' Input validation helper
    Public Function ValidateUserInput(input As String, maxLength As Integer, allowedChars As String) As Boolean
        If String.IsNullOrWhiteSpace(input) Then Return False
        If input.Length > maxLength Then Return False
        
        ' Check for allowed characters only
        For Each c As Char In input
            If Not allowedChars.Contains(c) Then Return False
        Next
        
        Return True
    End Function
    
    Private Sub LogError(ex As Exception)
        ' Implement secure logging here
        ' Log to file or database without exposing sensitive information
    End Sub
End Class