Imports System.IO

Public Class FileUploadHelper
    
    Private Shared ReadOnly AllowedExtensions As String() = {".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"}
    Private Shared ReadOnly MaxFileSize As Integer = 5 * 1024 * 1024 ' 5MB
    
    Public Shared Function ValidateFile(fileUpload As FileUpload) As ValidationResult
        Dim result As New ValidationResult()
        
        If fileUpload Is Nothing OrElse Not fileUpload.HasFile Then
            result.IsValid = False
            result.ErrorMessage = "No file selected"
            Return result
        End If
        
        ' Check file size
        If fileUpload.PostedFile.ContentLength > MaxFileSize Then
            result.IsValid = False
            result.ErrorMessage = "File size exceeds maximum allowed size (5MB)"
            Return result
        End If
        
        ' Check file extension
        Dim extension As String = Path.GetExtension(fileUpload.FileName).ToLower()
        If Not AllowedExtensions.Contains(extension) Then
            result.IsValid = False
            result.ErrorMessage = "File type not allowed"
            Return result
        End If
        
        ' Check for malicious content
        If ContainsMaliciousContent(fileUpload.PostedFile.InputStream) Then
            result.IsValid = False
            result.ErrorMessage = "File contains potentially malicious content"
            Return result
        End If
        
        result.IsValid = True
        Return result
    End Function
    
    Public Shared Function SaveFile(fileUpload As FileUpload, uploadPath As String) As String
        Dim validation As ValidationResult = ValidateFile(fileUpload)
        If Not validation.IsValid Then
            Throw New ArgumentException(validation.ErrorMessage)
        End If
        
        ' Generate secure filename
        Dim extension As String = Path.GetExtension(fileUpload.FileName)
        Dim fileName As String = Guid.NewGuid().ToString() & extension
        Dim fullPath As String = Path.Combine(uploadPath, fileName)
        
        ' Ensure directory exists
        Directory.CreateDirectory(uploadPath)
        
        ' Save file
        fileUpload.SaveAs(fullPath)
        
        SecurityHelper.LogSecurityEvent("FILE_UPLOAD", "File uploaded: " & fileName)
        
        Return fileName
    End Function
    
    Private Shared Function ContainsMaliciousContent(stream As Stream) As Boolean
        ' Basic malicious content detection
        Dim buffer(1024) As Byte
        stream.Read(buffer, 0, buffer.Length)
        stream.Position = 0
        
        Dim content As String = System.Text.Encoding.UTF8.GetString(buffer)
        
        ' Check for script tags and other malicious patterns
        Dim maliciousPatterns As String() = {
            "<script",
            "javascript:",
            "vbscript:",
            "onload=",
            "onerror=",
            "<?php",
            "<%"
        }
        
        For Each pattern In maliciousPatterns
            If content.ToLower().Contains(pattern) Then
                Return True
            End If
        Next
        
        Return False
    End Function
    
End Class

Public Class ValidationResult
    Public Property IsValid As Boolean
    Public Property ErrorMessage As String
End Class