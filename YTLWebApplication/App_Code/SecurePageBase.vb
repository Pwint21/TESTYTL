Imports System.Web.UI

Public Class SecurePageBase
    Inherits Page
    
    Protected Overrides Sub OnInit(e As EventArgs)
        ' Ensure HTTPS
        If Not Request.IsSecureConnection AndAlso Not Request.IsLocal Then
            Dim secureUrl As String = Request.Url.ToString().Replace("http://", "https://")
            Response.Redirect(secureUrl, True)
        End If
        
        ' Check authentication
        If Not AuthenticationHelper.IsUserAuthenticated() Then
            Response.Redirect("~/Login.aspx")
            Return
        End If
        
        ' Add security headers
        Response.Headers.Add("X-Frame-Options", "DENY")
        Response.Headers.Add("X-Content-Type-Options", "nosniff")
        Response.Headers.Add("X-XSS-Protection", "1; mode=block")
        Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
        
        ' Generate CSRF token for forms
        If TypeOf Me Is IRequiresCSRF Then
            ViewState("CSRFToken") = SecurityHelper.GenerateCSRFToken()
        End If
        
        MyBase.OnInit(e)
    End Sub
    
    Protected Overrides Sub OnPreRender(e As EventArgs)
        ' Validate session before rendering
        If Not SecurityHelper.ValidateSession() Then
            Response.Redirect("~/Login.aspx")
            Return
        End If
        
        MyBase.OnPreRender(e)
    End Sub
    
    Protected Function ValidateCSRF(token As String) As Boolean
        Return SecurityHelper.ValidateCSRFToken(token)
    End Function
    
    Protected Function SanitizeOutput(input As String) As String
        Return SecurityHelper.SanitizeForHtml(input)
    End Function
    
End Class

Public Interface IRequiresCSRF
    ' Marker interface for pages that require CSRF protection
End Interface