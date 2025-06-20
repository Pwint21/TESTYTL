' XSS Prevention Utilities
Imports System.Web
Imports System.Text.RegularExpressions

Public Class XSSPrevention
    
    ' HTML encode output to prevent XSS
    Public Shared Function SafeHtmlEncode(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        Return HttpUtility.HtmlEncode(input)
    End Function
    
    ' JavaScript encode for embedding in JavaScript
    Public Shared Function SafeJavaScriptEncode(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        
        ' Escape special JavaScript characters
        Dim encoded As String = input.Replace("\", "\\") _
                                     .Replace("'", "\'") _
                                     .Replace("""", "\""") _
                                     .Replace(vbCrLf, "\n") _
                                     .Replace(vbCr, "\r") _
                                     .Replace(vbTab, "\t") _
                                     .Replace("<", "\u003c") _
                                     .Replace(">", "\u003e")
        
        Return encoded
    End Function
    
    ' URL encode for URLs
    Public Shared Function SafeUrlEncode(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        Return HttpUtility.UrlEncode(input)
    End Function
    
    ' Sanitize HTML input (remove dangerous tags)
    Public Shared Function SanitizeHtml(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        
        ' Remove script tags and their content
        input = Regex.Replace(input, "<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        
        ' Remove dangerous attributes
        input = Regex.Replace(input, "\s(on\w+|javascript:|vbscript:|data:)\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase)
        
        ' Remove dangerous tags
        Dim dangerousTags As String() = {"script", "iframe", "object", "embed", "form", "input", "button", "textarea", "select", "option"}
        For Each tag As String In dangerousTags
            input = Regex.Replace(input, $"<{tag}[^>]*>.*?</{tag}>", "", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            input = Regex.Replace(input, $"<{tag}[^>]*/>", "", RegexOptions.IgnoreCase)
        Next
        
        Return input
    End Function
    
    ' Validate and sanitize user input
    Public Shared Function ValidateInput(input As String, inputType As InputType) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        
        Select Case inputType
            Case InputType.PlateNumber
                ' Allow only alphanumeric and specific characters for plate numbers
                If Not Regex.IsMatch(input, "^[A-Za-z0-9\-\s]{1,20}$") Then
                    Throw New ArgumentException("Invalid plate number format")
                End If
                
            Case InputType.Username
                ' Allow only alphanumeric and underscore for usernames
                If Not Regex.IsMatch(input, "^[A-Za-z0-9_]{1,50}$") Then
                    Throw New ArgumentException("Invalid username format")
                End If
                
            Case InputType.Email
                ' Basic email validation
                If Not Regex.IsMatch(input, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") Then
                    Throw New ArgumentException("Invalid email format")
                End If
                
            Case InputType.DateTime
                ' Validate date format
                Dim dateValue As DateTime
                If Not DateTime.TryParse(input, dateValue) Then
                    Throw New ArgumentException("Invalid date format")
                End If
                
        End Select
        
        Return SafeHtmlEncode(input)
    End Function
    
    Public Enum InputType
        PlateNumber
        Username
        Email
        DateTime
        General
    End Enum
End Class