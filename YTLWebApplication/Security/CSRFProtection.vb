' CSRF Protection Implementation
Public Class CSRFProtection
    
    ' Generate CSRF token
    Public Shared Function GenerateCSRFToken(session As HttpSessionState) As String
        Dim token As String = Guid.NewGuid().ToString()
        session("CSRFToken") = token
        Return token
    End Function
    
    ' Validate CSRF token
    Public Shared Function ValidateCSRFToken(session As HttpSessionState, submittedToken As String) As Boolean
        If session Is Nothing OrElse String.IsNullOrEmpty(submittedToken) Then
            Return False
        End If
        
        Dim sessionToken As String = TryCast(session("CSRFToken"), String)
        If String.IsNullOrEmpty(sessionToken) Then
            Return False
        End If
        
        Return sessionToken.Equals(submittedToken, StringComparison.Ordinal)
    End Function
    
    ' Add CSRF token to form
    Public Shared Function GetCSRFTokenHtml(session As HttpSessionState) As String
        Dim token As String = GenerateCSRFToken(session)
        Return $"<input type=""hidden"" name=""__CSRFToken"" value=""{HttpUtility.HtmlEncode(token)}"" />"
    End Function
End Class