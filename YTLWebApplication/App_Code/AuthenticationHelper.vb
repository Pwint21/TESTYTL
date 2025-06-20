Imports System.Data.SqlClient
Imports System.Web.Security

Public Class AuthenticationHelper
    
    Public Shared Function AuthenticateUser(username As String, password As String) As Boolean
        ' Validate input
        If Not SecurityHelper.ValidateInput(username, "username") OrElse 
           String.IsNullOrEmpty(password) Then
            SecurityHelper.LogSecurityEvent("INVALID_LOGIN_ATTEMPT", "Invalid username or password format", username)
            Return False
        End If
        
        ' Rate limiting
        If Not SecurityHelper.CheckRateLimit("login_" & username, 5, TimeSpan.FromMinutes(15)) Then
            SecurityHelper.LogSecurityEvent("RATE_LIMIT_EXCEEDED", "Too many login attempts", username)
            Return False
        End If
        
        Try
            Using conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
                Dim query As String = "SELECT userid, password, role, userslist FROM userTBL WHERE username = @username AND active = 1"
                Using cmd As SqlCommand = SecurityHelper.CreateSafeCommand(query, conn)
                    cmd.Parameters.AddWithValue("@username", username)
                    
                    conn.Open()
                    Using dr As SqlDataReader = cmd.ExecuteReader()
                        If dr.Read() Then
                            Dim storedPassword As String = dr("password").ToString()
                            Dim hashedPassword As String = SecurityHelper.HashPassword(password)
                            
                            If storedPassword = hashedPassword Then
                                ' Create secure session
                                CreateSecureSession(dr("userid").ToString(), username, dr("role").ToString(), dr("userslist").ToString())
                                SecurityHelper.LogSecurityEvent("SUCCESSFUL_LOGIN", "User logged in successfully", username)
                                Return True
                            End If
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            SecurityHelper.LogSecurityEvent("LOGIN_ERROR", "Database error during login: " & ex.Message, username)
        End Try
        
        SecurityHelper.LogSecurityEvent("FAILED_LOGIN", "Invalid credentials", username)
        Return False
    End Function
    
    Private Shared Sub CreateSecureSession(userId As String, username As String, role As String, usersList As String)
        ' Clear any existing session
        HttpContext.Current.Session.Clear()
        
        ' Set session values
        HttpContext.Current.Session("userid") = userId
        HttpContext.Current.Session("username") = username
        HttpContext.Current.Session("role") = role
        HttpContext.Current.Session("userslist") = usersList
        HttpContext.Current.Session("logintime") = DateTime.Now
        
        ' Create authentication cookie
        Dim ticket As New FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(30), False, role)
        Dim encryptedTicket As String = FormsAuthentication.Encrypt(ticket)
        Dim authCookie As New HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
        authCookie.HttpOnly = True
        authCookie.Secure = True
        authCookie.SameSite = SameSiteMode.Strict
        HttpContext.Current.Response.Cookies.Add(authCookie)
    End Sub
    
    Public Shared Sub LogoutUser()
        Try
            Dim username As String = If(HttpContext.Current.Session("username") IsNot Nothing, 
                                      HttpContext.Current.Session("username").ToString(), "Unknown")
            
            ' Clear session
            HttpContext.Current.Session.Clear()
            HttpContext.Current.Session.Abandon()
            
            ' Clear authentication cookie
            FormsAuthentication.SignOut()
            
            SecurityHelper.LogSecurityEvent("LOGOUT", "User logged out", username)
        Catch ex As Exception
            SecurityHelper.LogSecurityEvent("LOGOUT_ERROR", "Error during logout: " & ex.Message)
        End Try
    End Sub
    
    Public Shared Function IsUserAuthenticated() As Boolean
        Return HttpContext.Current.User.Identity.IsAuthenticated AndAlso 
               SecurityHelper.ValidateSession()
    End Function
    
    Public Shared Function HasRole(requiredRole As String) As Boolean
        If Not IsUserAuthenticated() Then
            Return False
        End If
        
        Dim userRole As String = If(HttpContext.Current.Session("role") IsNot Nothing, 
                                  HttpContext.Current.Session("role").ToString(), "")
        
        Return userRole.Equals(requiredRole, StringComparison.OrdinalIgnoreCase)
    End Function
    
End Class