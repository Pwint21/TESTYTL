Imports System.IO

Public Class AuditLogger
    
    Private Shared ReadOnly LogPath As String = HttpContext.Current.Server.MapPath("~/App_Data/audit.log")
    Private Shared ReadOnly LockObject As New Object()
    
    Public Shared Sub LogUserAction(action As String, details As String, Optional userId As String = "")
        LogEvent("USER_ACTION", action, details, userId)
    End Sub
    
    Public Shared Sub LogDataAccess(tableName As String, operation As String, Optional userId As String = "")
        LogEvent("DATA_ACCESS", operation, "Table: " & tableName, userId)
    End Sub
    
    Public Shared Sub LogSystemEvent(eventType As String, details As String)
        LogEvent("SYSTEM", eventType, details)
    End Sub
    
    Private Shared Sub LogEvent(category As String, eventType As String, details As String, Optional userId As String = "")
        Try
            SyncLock LockObject
                Dim logEntry As String = String.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2} | User: {3} | IP: {4} | {5}",
                    DateTime.Now,
                    category,
                    eventType,
                    If(String.IsNullOrEmpty(userId), GetCurrentUserId(), userId),
                    GetClientIP(),
                    details)
                
                ' Ensure directory exists
                Dim logDir As String = Path.GetDirectoryName(LogPath)
                If Not Directory.Exists(logDir) Then
                    Directory.CreateDirectory(logDir)
                End If
                
                File.AppendAllText(LogPath, logEntry & Environment.NewLine)
            End SyncLock
        Catch ex As Exception
            ' Silent fail for logging to prevent application disruption
        End Try
    End Sub
    
    Private Shared Function GetCurrentUserId() As String
        Try
            If HttpContext.Current.Session IsNot Nothing AndAlso 
               HttpContext.Current.Session("userid") IsNot Nothing Then
                Return HttpContext.Current.Session("userid").ToString()
            End If
        Catch
        End Try
        Return "Anonymous"
    End Function
    
    Private Shared Function GetClientIP() As String
        Try
            Dim ip As String = HttpContext.Current.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
            If String.IsNullOrEmpty(ip) Then
                ip = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
            End If
            Return ip
        Catch
            Return "Unknown"
        End Try
    End Function
    
End Class