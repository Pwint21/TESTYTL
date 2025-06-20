Imports Newtonsoft.Json
Imports System.Data.SqlClient

Partial Class GetGroups
    Inherits SecurePageBase

    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Try
            ' Validate authentication
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.StatusCode = 401
                Response.Write("{""error"":""Unauthorized""}")
                Response.End()
                Return
            End If
            
            ' Validate user access
            Dim userid As String = SecurityHelper.ValidateAndGetUserId(Request)
            Dim role As String = SecurityHelper.ValidateAndGetUserRole(Request)
            Dim userslist As String = SecurityHelper.ValidateAndGetUsersList(Request)
            
            Dim qs As String = SecurityHelper.SanitizeForHtml(Request.QueryString("userid"))
            Dim json As String = ""
            
            Dim parameters As New Dictionary(Of String, Object)
            Dim query As String
            
            If role = "User" Then
                query = "SELECT groupid, groupname FROM vehicle_group WHERE userid = @userid ORDER BY groupname"
                parameters.Add("@userid", userid)
            ElseIf role = "SuperUser" Or role = "Operator" Then
                If qs <> "ALLUSERS" Then
                    query = "SELECT groupid, groupname FROM vehicle_group WHERE userid = @userid ORDER BY groupname"
                    parameters.Add("@userid", qs)
                Else
                    query = "SELECT groupid, groupname FROM vehicle_group WHERE userid IN (" & userslist & ") ORDER BY groupname"
                End If
            Else
                If qs <> "ALLUSERS" Then
                    query = "SELECT groupid, groupname FROM vehicle_group WHERE userid = @userid ORDER BY groupname"
                    parameters.Add("@userid", qs)
                Else
                    query = "SELECT groupid, groupname FROM vehicle_group ORDER BY groupname"
                End If
            End If
            
            Dim groupData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
            Dim aa As New ArrayList()

            For Each dr As DataRow In groupData.Rows
                Try
                    Dim a As New ArrayList()
                    a.Add(dr("groupid"))
                    a.Add(SanitizeOutput(dr("groupname").ToString().ToUpper()))
                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Group row processing error", ex, Server)
                End Try
            Next

            json = JsonConvert.SerializeObject(aa, Formatting.None)
            
            Response.Write(json)
            Response.ContentType = "application/json"
            
            AuditLogger.LogDataAccess("vehicle_group", "READ")
            
        Catch ex As Exception
            SecurityHelper.LogError("GetGroups error", ex, Server)
            Response.StatusCode = 500
            Response.Write("{""error"":""Internal server error""}")
        End Try
    End Sub
End Class