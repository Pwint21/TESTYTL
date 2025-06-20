Imports AspMap
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json

Partial Class GetDriverManagement
    Inherits SecurePageBase
    Implements IRequiresCSRF
    
    Public connstr As String
    Public suserid As String
    Public suser As String
    Public sgroup As String
    
    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Try
            ' Validate authentication
            If Not AuthenticationHelper.IsUserAuthenticated() Then
                Response.StatusCode = 401
                Response.End()
                Return
            End If
            
            ' Validate CSRF token for state-changing operations
            Dim operation As String = SecurityHelper.SanitizeForHtml(Request.QueryString("op"))
            If operation = "2" OrElse operation = "3" OrElse operation = "4" Then
                If Not ValidateCSRF(Request.Form("__CSRFToken")) Then
                    Response.StatusCode = 403
                    Response.Write("Invalid request")
                    Response.End()
                    Return
                End If
            End If
            
            ' Log the operation
            AuditLogger.LogUserAction("DRIVER_MANAGEMENT", "Operation: " & operation)
            
            Select Case operation
                Case "1"
                    Dim ugData As String = SecurityHelper.SanitizeForHtml(Request.QueryString("ugData"))
                    Response.Write(FillGrid(ugData))

                Case "2"
                    ' Validate and sanitize all inputs
                    Dim userId As String = SecurityHelper.ValidateAndGetUserId(Request)
                    Dim poiname As String = SecurityHelper.SanitizeForHtml(Request.QueryString("poiname"))
                    Dim txtDOB As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtDOB"))
                    Dim txtrfid As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtrfid"))
                    Dim txtPhone As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtPhone"))
                    Dim txtAddress As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtAddress"))
                    Dim txtLicenceno As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtLicenceno"))
                    Dim txtIssuingdate As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtIssuingdate"))
                    Dim txtExpiryDate As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtExpiryDate"))
                    Dim txtFuelCardNo As String = SecurityHelper.SanitizeForHtml(Request.QueryString("txtFuelCardNo"))
                    Dim poiid As String = SecurityHelper.SanitizeForHtml(Request.QueryString("poiid"))
                    Dim opr As String = SecurityHelper.SanitizeForHtml(Request.QueryString("opr"))
                    Dim ic As String = SecurityHelper.SanitizeForHtml(Request.QueryString("ic"))
                    Dim pwd As String = Request.QueryString("pwd") ' Don't sanitize password, just validate
                    Dim dvrole As String = SecurityHelper.SanitizeForHtml(Request.QueryString("driverrole"))
                    
                    ' Validate inputs
                    If Not ValidateDriverInputs(poiname, txtPhone, txtLicenceno, ic) Then
                        Response.Write("0")
                        Response.End()
                        Return
                    End If
                    
                    Response.Write(InsertupdateDriver(userId, poiname, txtDOB, txtrfid, txtPhone, txtAddress, txtLicenceno, txtIssuingdate, txtExpiryDate, txtFuelCardNo, poiid, opr, ic, pwd, dvrole))

                Case "3"
                    Dim chkitems As String = SecurityHelper.SanitizeForHtml(Request.QueryString("chekitems"))
                    If ValidateDriverIds(chkitems) Then
                        Response.Write(Activate(chkitems.Split(","c)))
                    Else
                        Response.Write("0")
                    End If
                    
                Case "4"
                    Dim chkitems As String = SecurityHelper.SanitizeForHtml(Request.QueryString("chekitems"))
                    If ValidateDriverIds(chkitems) Then
                        Response.Write(InActivate(chkitems.Split(","c)))
                    Else
                        Response.Write("0")
                    End If
            End Select

            Response.ContentType = "application/json"
            
        Catch ex As Exception
            SecurityHelper.LogError("GetDriverManagement error", ex, Server)
            Response.StatusCode = 500
            Response.Write("0")
        End Try
    End Sub
    
    Private Function ValidateDriverInputs(poiname As String, txtPhone As String, txtLicenceno As String, ic As String) As Boolean
        ' Validate driver name
        If String.IsNullOrWhiteSpace(poiname) OrElse poiname.Length > 100 Then
            Return False
        End If
        
        ' Validate phone number
        If Not String.IsNullOrEmpty(txtPhone) AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(txtPhone, "^[0-9+\-\s()]{7,20}$") Then
            Return False
        End If
        
        ' Validate license number
        If Not String.IsNullOrEmpty(txtLicenceno) AndAlso txtLicenceno.Length > 50 Then
            Return False
        End If
        
        ' Validate IC
        If Not String.IsNullOrEmpty(ic) AndAlso ic.Length > 20 Then
            Return False
        End If
        
        Return True
    End Function
    
    Private Function ValidateDriverIds(chkitems As String) As Boolean
        If String.IsNullOrEmpty(chkitems) Then Return False
        
        Dim ids As String() = chkitems.Split(","c)
        For Each id As String In ids
            Dim numericId As Integer
            If Not Integer.TryParse(id.Trim(), numericId) OrElse numericId <= 0 Then
                Return False
            End If
        Next
        
        Return True
    End Function

    Public Function FillGrid(ByVal ugData As String) As String
        Dim json As String = Nothing
        Try
            ' Validate user access
            Dim userid As String = SecurityHelper.ValidateAndGetUserId(Request)
            Dim role As String = SecurityHelper.ValidateAndGetUserRole(Request)
            Dim userslist As String = SecurityHelper.ValidateAndGetUsersList(Request)
            
            Dim r As DataRow
            Dim j As Int32 = 1

            Dim drivertable As New DataTable
            drivertable.Columns.Add(New DataColumn("chk"))
            drivertable.Columns.Add(New DataColumn("sno"))
            drivertable.Columns.Add(New DataColumn("drivername"))
            drivertable.Columns.Add(New DataColumn("Licence No"))
            drivertable.Columns.Add(New DataColumn("Exp Date"))
            drivertable.Columns.Add(New DataColumn("Phone No"))
            drivertable.Columns.Add(New DataColumn("Address"))
            drivertable.Columns.Add(New DataColumn("Fuel Card No"))
            drivertable.Columns.Add(New DataColumn("rfid"))
            drivertable.Columns.Add(New DataColumn("dob"))
            drivertable.Columns.Add(New DataColumn("Issuingdate"))
            drivertable.Columns.Add(New DataColumn("userid"))
            drivertable.Columns.Add(New DataColumn("status"))
            drivertable.Columns.Add(New DataColumn("driveric"))
            drivertable.Columns.Add(New DataColumn("pwd"))
            drivertable.Columns.Add(New DataColumn("driverrole"))

            If ugData <> "SELECT USERNAME" Then
                ' Use parameterized queries
                Dim parameters As New Dictionary(Of String, Object)
                Dim query As String
                
                If ugData = "ALL" Then
                    If role = "SuperUser" Or role = "Operator" Then
                        query = "SELECT * FROM driver WHERE userid IN (" & userslist & ") ORDER BY drivername ASC"
                    Else
                        query = "SELECT * FROM driver ORDER BY drivername"
                    End If
                Else
                    query = "SELECT * FROM driver WHERE userid = @userid ORDER BY drivername"
                    parameters.Add("@userid", ugData)
                End If

                Dim driverData As DataTable = DatabaseHelper.ExecuteQuery(query, parameters)
                
                For Each dr As DataRow In driverData.Rows
                    r = drivertable.NewRow
                    Dim drivername As String = SecurityHelper.SanitizeForHtml(dr("drivername").ToString().ToUpper())
                    
                    r(0) = dr("driverid")
                    r(1) = j.ToString()
                    r(2) = drivername
                    r(3) = SecurityHelper.SanitizeForHtml(dr("licenceno").ToString().ToUpper())
                    
                    Try
                        r(4) = Convert.ToDateTime(dr("expirydate")).ToString("yyyy/MM/dd")
                        If Convert.ToDateTime(dr("expirydate")).ToString("yyyy/MM/dd") = "1900/01/01" Then
                            r(4) = ""
                        End If
                    Catch ex As Exception
                        r(4) = ""
                    End Try

                    r(5) = SecurityHelper.SanitizeForHtml(dr("phoneno").ToString())
                    r(6) = SecurityHelper.SanitizeForHtml(dr("address").ToString())
                    r(7) = SecurityHelper.SanitizeForHtml(dr("fuelcardno").ToString())
                    r(8) = SecurityHelper.SanitizeForHtml(dr("rfid").ToString())

                    Try
                        r(9) = Convert.ToDateTime(dr("dateofbirth")).ToString("yyyy/MM/dd")
                        If Convert.ToDateTime(dr("dateofbirth")).ToString("yyyy/MM/dd") = "1900/01/01" Then
                            r(9) = ""
                        End If
                    Catch ex As Exception
                        r(9) = ""
                    End Try

                    Try
                        r(10) = Convert.ToDateTime(dr("issuingdate")).ToString("yyyy/MM/dd")
                        If Convert.ToDateTime(dr("issuingdate")).ToString("yyyy/MM/dd") = "1900/01/01" Then
                            r(10) = ""
                        End If
                    Catch ex As Exception
                        r(10) = ""
                    End Try

                    r(11) = dr("userid")
                    r(12) = "1"
                    If Not IsDBNull(dr("status")) Then
                        If dr("status") = False Then
                            r(12) = "0"
                        End If
                    End If

                    If Not IsDBNull(dr("driver_ic")) Then
                        r(13) = SecurityHelper.SanitizeForHtml(dr("driver_ic").ToString())
                    Else
                        r(13) = "-"
                    End If

                    r(14) = "***" ' Never expose passwords
                    
                    If Not IsDBNull(dr("isowner")) AndAlso dr("isowner") Then
                        r(15) = "OWNER"
                    Else
                        r(15) = "DRIVER"
                    End If

                    drivertable.Rows.Add(r)
                    j = j + 1
                Next
            End If

            If drivertable.Rows.Count = 0 Then
                r = drivertable.NewRow
                For i As Integer = 0 To 15
                    r(i) = "--"
                Next
                drivertable.Rows.Add(r)
            End If

            ' Store sanitized data for Excel export
            HttpContext.Current.Session.Remove("exceltable")
            Dim exceltable As New DataTable
            exceltable = drivertable.Copy()
            exceltable.Columns.Remove("userid")
            exceltable.Columns.Remove("chk")
            exceltable.Columns.Remove("sno")
            HttpContext.Current.Session("exceltable") = exceltable

            Dim aa As New ArrayList
            Dim a As ArrayList

            For j1 As Integer = 0 To drivertable.Rows.Count - 1
                Try
                    a = New ArrayList
                    For col As Integer = 0 To 21
                        If col < drivertable.Columns.Count Then
                            a.Add(drivertable.DefaultView.Item(j1)(col))
                        Else
                            a.Add("--")
                        End If
                    Next
                    aa.Add(a)
                Catch ex As Exception
                    SecurityHelper.LogError("Grid row processing error", ex, Server)
                End Try
            Next
            
            json = JsonConvert.SerializeObject(aa, Formatting.None)

        Catch ex As Exception
            SecurityHelper.LogError("FillGrid error", ex, Server)
            Return "{""error"":""Data retrieval failed""}"
        End Try
        
        Return json
    End Function

    Public Function InsertupdateDriver(ByVal userId As String, ByVal poiname As String, ByVal txtDOB As String, ByVal txtrfid As String, ByVal txtPhone As String, ByVal txtAddress As String, ByVal txtLicenceno As String, ByVal txtIssuingdate As String, ByVal txtExpiryDate As String, ByVal txtFuelCardNo As String, ByVal poiid As String, ByVal opr As String, ByVal ic As String, ByVal pwd As String, ByVal driverrole As String) As String
        
        Try
            ' Validate inputs
            If Not ValidateDriverInputs(poiname, txtPhone, txtLicenceno, ic) Then
                Return "0"
            End If
            
            ' Validate password if provided
            If Not String.IsNullOrEmpty(pwd) AndAlso Not PasswordHelper.ValidatePasswordStrength(pwd) Then
                Return "0"
            End If
            
            ' Hash password if provided
            Dim hashedPassword As String = ""
            If Not String.IsNullOrEmpty(pwd) Then
                hashedPassword = PasswordHelper.HashPassword(pwd)
            End If
            
            ' Set default dates
            If String.IsNullOrEmpty(txtDOB) Then txtDOB = "1900/01/01"
            If String.IsNullOrEmpty(txtIssuingdate) Then txtIssuingdate = "1900/01/01"
            If String.IsNullOrEmpty(txtExpiryDate) Then txtExpiryDate = "1900/01/01"
            
            Dim parameters As New Dictionary(Of String, Object)
            Dim query As String
            Dim result As Integer = 0
            
            If opr = "0" Then ' Insert
                ' Check for duplicate phone number
                Dim checkParams As New Dictionary(Of String, Object)
                checkParams.Add("@phoneno", txtPhone)
                Dim checkQuery As String = "SELECT COUNT(*) FROM driver WHERE phoneno = @phoneno"
                Dim count As Integer = CInt(DatabaseHelper.ExecuteScalar(checkQuery, checkParams))
                
                If count > 0 Then
                    Return "99" ' Duplicate found
                End If
                
                query = "INSERT INTO driver (rfid, userid, drivername, dateofbirth, phoneno, address, licenceno, issuingdate, expirydate, fuelcardno, driver_ic, password, isowner) VALUES (@rfid, @userid, @drivername, @dateofbirth, @phoneno, @address, @licenceno, @issuingdate, @expirydate, @fuelcardno, @driver_ic, @password, @isowner)"
                
                parameters.Add("@rfid", txtrfid)
                parameters.Add("@userid", userId)
                parameters.Add("@drivername", poiname)
                parameters.Add("@dateofbirth", txtDOB)
                parameters.Add("@phoneno", txtPhone)
                parameters.Add("@address", txtAddress)
                parameters.Add("@licenceno", txtLicenceno)
                parameters.Add("@issuingdate", txtIssuingdate)
                parameters.Add("@expirydate", txtExpiryDate)
                parameters.Add("@fuelcardno", txtFuelCardNo)
                parameters.Add("@driver_ic", ic)
                parameters.Add("@password", hashedPassword)
                parameters.Add("@isowner", If(driverrole = "1", 1, 0))
                
                AuditLogger.LogUserAction("DRIVER_INSERT", "New driver: " & poiname)
                
            Else ' Update
                query = "UPDATE driver SET userid = @userid, drivername = @drivername, rfid = @rfid, dateofbirth = @dateofbirth, phoneno = @phoneno, address = @address, licenceno = @licenceno, issuingdate = @issuingdate, expirydate = @expirydate, fuelcardno = @fuelcardno, driver_ic = @driver_ic, isowner = @isowner"
                
                If Not String.IsNullOrEmpty(hashedPassword) Then
                    query &= ", password = @password"
                    parameters.Add("@password", hashedPassword)
                End If
                
                query &= " WHERE driverid = @driverid"
                
                parameters.Add("@userid", userId)
                parameters.Add("@drivername", poiname)
                parameters.Add("@rfid", txtrfid)
                parameters.Add("@dateofbirth", Convert.ToDateTime(txtDOB).ToString("yyyy/MM/dd"))
                parameters.Add("@phoneno", txtPhone)
                parameters.Add("@address", txtAddress)
                parameters.Add("@licenceno", txtLicenceno)
                parameters.Add("@issuingdate", Convert.ToDateTime(txtIssuingdate).ToString("yyyy/MM/dd"))
                parameters.Add("@expirydate", Convert.ToDateTime(txtExpiryDate).ToString("yyyy/MM/dd"))
                parameters.Add("@fuelcardno", txtFuelCardNo)
                parameters.Add("@driver_ic", ic)
                parameters.Add("@isowner", If(driverrole = "1", 1, 0))
                parameters.Add("@driverid", poiid)
                
                AuditLogger.LogUserAction("DRIVER_UPDATE", "Updated driver ID: " & poiid)
            End If
            
            result = DatabaseHelper.ExecuteNonQuery(query, parameters)
            Return result.ToString()
            
        Catch ex As Exception
            SecurityHelper.LogError("InsertupdateDriver error", ex, Server)
            Return "0"
        End Try
    End Function

    Public Function Activate(ByVal chekitems() As String) As Int16
        Try
            Dim parameters As New Dictionary(Of String, Object)
            Dim placeholders As New List(Of String)
            
            For i As Integer = 0 To chekitems.Length - 1
                Dim paramName As String = "@id" & i
                placeholders.Add(paramName)
                parameters.Add(paramName, chekitems(i))
            Next
            
            Dim query As String = "UPDATE driver SET status = 1 WHERE driverid IN (" & String.Join(",", placeholders) & ")"
            Dim result As Integer = DatabaseHelper.ExecuteNonQuery(query, parameters)
            
            AuditLogger.LogUserAction("DRIVER_ACTIVATE", "Activated drivers: " & String.Join(",", chekitems))
            
            Return CShort(result)
            
        Catch ex As Exception
            SecurityHelper.LogError("Activate error", ex, Server)
            Return 0
        End Try
    End Function

    Public Function InActivate(ByVal chekitems() As String) As Int16
        Try
            Dim parameters As New Dictionary(Of String, Object)
            Dim placeholders As New List(Of String)
            
            For i As Integer = 0 To chekitems.Length - 1
                Dim paramName As String = "@id" & i
                placeholders.Add(paramName)
                parameters.Add(paramName, chekitems(i))
            Next
            
            Dim query As String = "UPDATE driver SET status = 0 WHERE driverid IN (" & String.Join(",", placeholders) & ")"
            Dim result As Integer = DatabaseHelper.ExecuteNonQuery(query, parameters)
            
            AuditLogger.LogUserAction("DRIVER_DEACTIVATE", "Deactivated drivers: " & String.Join(",", chekitems))
            
            Return CShort(result)
            
        Catch ex As Exception
            SecurityHelper.LogError("InActivate error", ex, Server)
            Return 0
        End Try
    End Function
End Class