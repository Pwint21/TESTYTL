' Secure base page that all pages should inherit from
Imports System.Web.UI

Public Class SecureBasePage
    Inherits Page
    
    Protected Property CurrentUser As UserSession
    
    Protected Overrides Sub OnInit(e As EventArgs)
        ' Validate session on every page load
        CurrentUser = AuthenticationSecurity.ValidateSession(Context)
        
        If CurrentUser Is Nothing Then
            ' Redirect to login if not authenticated
            Response.Redirect("~/Login.aspx", True)
            Return
        End If
        
        ' Add security headers
        AddSecurityHeaders()
        
        ' Validate CSRF token for POST requests
        If Request.HttpMethod.ToUpper() = "POST" Then
            ValidateCSRFToken()
        End If
        
        MyBase.OnInit(e)
    End Sub
    
    Private Sub AddSecurityHeaders()
        ' Add security headers if not already present
        If Not Response.Headers.AllKeys.Contains("X-Frame-Options") Then
            Response.Headers.Add("X-Frame-Options", "DENY")
        End If
        
        If Not Response.Headers.AllKeys.Contains("X-Content-Type-Options") Then
            Response.Headers.Add("X-Content-Type-Options", "nosniff")
        End If
        
        If Not Response.Headers.AllKeys.Contains("X-XSS-Protection") Then
            Response.Headers.Add("X-XSS-Protection", "1; mode=block")
        End If
    End Sub
    
    Private Sub ValidateCSRFToken()
        Dim submittedToken As String = Request.Form("__CSRFToken")
        If Not CSRFProtection.ValidateCSRFToken(Session, submittedToken) Then
            Throw New UnauthorizedAccessException("Invalid CSRF token")
        End If
    End Sub
    
    ' Helper method to check authorization
    Protected Function CheckAuthorization(requiredRole As String, Optional resourceId As String = "") As Boolean
        Return AuthenticationSecurity.IsAuthorized(CurrentUser, requiredRole, resourceId)
    End Function
    
    ' Helper method to safely output HTML
    Protected Function SafeOutput(input As String) As String
        Return XSSPrevention.SafeHtmlEncode(input)
    End Function
    
    ' Helper method to safely output JavaScript
    Protected Function SafeJavaScript(input As String) As String
        Return XSSPrevention.SafeJavaScriptEncode(input)
    End Function
    
    ' Helper method to get CSRF token HTML
    Protected Function GetCSRFTokenHtml() As String
        Return CSRFProtection.GetCSRFTokenHtml(Session)
    End Function
End Class