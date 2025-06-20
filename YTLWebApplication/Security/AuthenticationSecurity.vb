' Secure Authentication Implementation
Imports System.Security.Cryptography
Imports System.Text

Public Class AuthenticationSecurity
    
    ' Generate secure session token
    Public Shared Function GenerateSecureToken() As String
        Using rng As New RNGCryptoServiceProvider()
            Dim tokenBytes(31) As Byte
            rng.GetBytes(tokenBytes)
            Return Convert.ToBase64String(tokenBytes)
        End Using
    End Function
    
    ' Hash password securely
    Public Shared Function HashPassword(password As String, salt As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim saltedPassword As String = password + salt
            Dim hashedBytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword))
            Return Convert.ToBase64String(hashedBytes)
        End Using
    End Function
    
    ' Generate salt for password hashing
    Public Shared Function GenerateSalt() As String
        Using rng As New RNGCryptoServiceProvider()
            Dim saltBytes(15) As Byte
            rng.GetBytes(saltBytes)
            Return Convert.ToBase64String(saltBytes)
        End Using
    End Function
    
    ' Validate session securely
    Public Shared Function ValidateSession(context As HttpContext) As UserSession
        Try
            ' Check if session exists
            If context.Session Is Nothing OrElse context.Session("UserSession") Is Nothing Then
                Return Nothing
            End If
            
            Dim userSession As UserSession = CType(context.Session("UserSession"), UserSession)
            
            ' Check session expiry
            If userSession.ExpiryTime < DateTime.Now Then
                context.Session.Remove("UserSession")
                Return Nothing
            End If
            
            ' Validate session token
            If String.IsNullOrEmpty(userSession.Token) Then
                Return Nothing
            End If
            
            ' Update last activity
            userSession.LastActivity = DateTime.Now
            
            Return userSession
            
        Catch ex As Exception
            ' Log error and return null for security
            Return Nothing
        End Try
    End Function
    
    ' Create secure session
    Public Shared Function CreateSecureSession(userId As String, username As String, role As String) As UserSession
        Return New UserSession() With {
            .UserId = userId,
            .Username = username,
            .Role = role,
            .Token = GenerateSecureToken(),
            .CreatedTime = DateTime.Now,
            .LastActivity = DateTime.Now,
            .ExpiryTime = DateTime.Now.AddHours(8) ' 8 hour session
        }
    End Function
    
    ' Check authorization
    Public Shared Function IsAuthorized(userSession As UserSession, requiredRole As String, resourceId As String) As Boolean
        If userSession Is Nothing Then Return False
        
        ' Check role-based access
        Select Case requiredRole.ToUpper()
            Case "ADMIN"
                Return userSession.Role.ToUpper() = "ADMIN"
            Case "SUPERUSER"
                Return userSession.Role.ToUpper() = "ADMIN" OrElse userSession.Role.ToUpper() = "SUPERUSER"
            Case "USER"
                Return True ' All authenticated users
            Case Else
                Return False
        End Select
    End Function
End Class

Public Class UserSession
    Public Property UserId As String
    Public Property Username As String
    Public Property Role As String
    Public Property Token As String
    Public Property CreatedTime As DateTime
    Public Property LastActivity As DateTime
    Public Property ExpiryTime As DateTime
End Class