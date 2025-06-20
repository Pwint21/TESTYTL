Imports AspMap
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Collections.Generic
Imports Newtonsoft.Json
Partial Class GetDriverManagement
    Inherits System.Web.UI.Page
    Public connstr As String
    Public suserid As String
    Public suser As String
    Public sgroup As String
    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Try

            Dim operation As String = Request.QueryString("op")
            Select Case operation
                Case "1"
                    Dim ugData As String = Request.QueryString("ugData")
                    Dim role As String = Request.QueryString("role")
                    Dim userslist As String = Request.QueryString("userslist")

                    Response.Write(FillGrid(ugData))

                Case "2"
                    Dim userId As String = Request.QueryString("userId")
                    Dim poiname As String = Request.QueryString("poiname")
                    Dim txtDOB As String = Request.QueryString("txtDOB")
                    Dim txtrfid As String = Request.QueryString("txtrfid")
                    Dim txtPhone As String = Request.QueryString("txtPhone")
                    Dim txtAddress As String = Request.QueryString("txtAddress")
                    Dim txtLicenceno As String = Request.QueryString("txtLicenceno")
                    Dim txtIssuingdate As String = Request.QueryString("txtIssuingdate")
                    Dim txtExpiryDate As String = Request.QueryString("txtExpiryDate")
                    Dim txtFuelCardNo As String = Request.QueryString("txtFuelCardNo")
                    Dim poiid As String = Request.QueryString("poiid")
                    Dim opr As String = Request.QueryString("opr")
                    Dim ic As String = Request.QueryString("ic")
                    Dim pwd As String = Request.QueryString("pwd")
                    Dim dvrole As String = Request.QueryString("driverrole")

                    Response.Write(InsertupdateDriver(userId, poiname, txtDOB, txtrfid, txtPhone, txtAddress, txtLicenceno, txtIssuingdate, txtExpiryDate, txtFuelCardNo, poiid, opr, ic, pwd, dvrole))


                Case "3"
                    Dim chkitems() As String = Request.QueryString("chekitems").Split(",")
                    Response.Write(Activate(chkitems))
                Case "4"
                    Dim chkitems() As String = Request.QueryString("chekitems").Split(",")
                    Response.Write(InActivate(chkitems))

            End Select


            Response.ContentType = "text/plain"
        Catch ex As Exception

        End Try
    End Sub

    Public Function FillGrid(ByVal ugData As String) As String
        Dim json As String = Nothing
        Try
            Dim suserid As String = ugData
            Dim userid As String = Request.Cookies("userinfo")("userid")
            Dim role As String = Request.Cookies("userinfo")("role")
            Dim userslist As String = Request.Cookies("userinfo")("userslist")
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


            If Not suserid = "SELECT USERNAME" Then
                Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection")) 'connection.sqlConnection))
                Dim cmd As New SqlCommand


                Dim dr As SqlDataReader

                If suserid = "ALL" Then
                    cmd = New SqlCommand("select * from driver  order by drivername", conn)
                    If role = "SuperUser" Or role = "Operator" Then
                        cmd = New SqlCommand("select * from driver  where userid in (" & userslist & ")  order by drivername asc", conn)
                    End If

                Else
                    cmd.CommandText = "select * from driver where userid='" & suserid & "'"

                    cmd.Connection = conn
                End If


                conn.Open()
                '   Response.Write(cmd.CommandText)
                'Response.Write(suserid)
                dr = cmd.ExecuteReader()

                Dim drivername As String = ""

                While dr.Read
                    r = drivertable.NewRow
                    drivername = dr("drivername").ToString.ToUpper()
                    r(0) = dr("driverid")
                    r(1) = j.ToString()
                    r(2) = drivername

                    r(3) = dr("licenceno").ToString.ToUpper()
                    Try
                        r(4) = Convert.ToDateTime(dr("expirydate")).ToString("yyyy/MM/dd")
                        If Convert.ToDateTime(dr("expirydate")).ToString("yyyy/MM/dd") = "1900/01/01" Then
                            r(4) = ""
                        End If
                    Catch ex As Exception
                        r(4) = ""
                    End Try

                    r(5) = dr("phoneno").ToString()
                    r(6) = dr("address").ToString()
                    r(7) = dr("fuelcardno").ToString()
                    r(8) = dr("rfid").ToString()


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
                    'If Not IsDBNull(dr("status")) Then
                    '    If dr("status") Then

                    '    Else
                    '        r(12) = "0"
                    '    End If
                    'Else
                    '    r(12) = "0"
                    'End If

                    If Not IsDBNull(dr("driver_ic")) Then
                        r(13) = dr("driver_ic")
                    Else
                        r(13) = "-"
                    End If

                    If Not IsDBNull(dr("password")) Then
                        r(14) = dr("password")
                    Else
                        r(14) = "-"
                    End If
                    If dr("isowner") Then
                        r(15) = "OWNER"
                    Else
                        r(15) = "DRIVER"
                    End If

                    drivertable.Rows.Add(r)
                    j = j + 1
                End While

                conn.Close()

            End If

            If drivertable.Rows.Count = 0 Then
                r = drivertable.NewRow
                r(0) = "--"
                r(1) = "--"
                r(2) = "--"
                r(3) = "--"
                r(4) = "--"
                r(5) = "--"
                r(6) = "--"
                r(7) = "--"
                r(8) = "--"
                r(9) = "--"
                r(10) = "--"
                r(11) = "--"
                r(12) = "--"
                r(13) = "--"
                r(14) = "--"
                r(15) = "--"
                drivertable.Rows.Add(r)
            End If

            HttpContext.Current.Session.Remove("exceltable")
            HttpContext.Current.Session.Remove("exceltable1")
            HttpContext.Current.Session.Remove("exceltable2")
            HttpContext.Current.Session.Remove("exceltable3")
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
                    a.Add(drivertable.DefaultView.Item(j1)(0))
                    a.Add(drivertable.DefaultView.Item(j1)(1))
                    a.Add(drivertable.DefaultView.Item(j1)(2))
                    a.Add(drivertable.DefaultView.Item(j1)(3))
                    a.Add(drivertable.DefaultView.Item(j1)(4))
                    a.Add(drivertable.DefaultView.Item(j1)(5))
                    a.Add(drivertable.DefaultView.Item(j1)(6))
                    a.Add(drivertable.DefaultView.Item(j1)(7))

                    a.Add(drivertable.DefaultView.Item(j1)(8))
                    a.Add(drivertable.DefaultView.Item(j1)(9))
                    a.Add(drivertable.DefaultView.Item(j1)(10))
                    a.Add(drivertable.DefaultView.Item(j1)(11))
                    a.Add(drivertable.DefaultView.Item(j1)(0))
                    a.Add(drivertable.DefaultView.Item(j1)(12))
                    a.Add(drivertable.DefaultView.Item(j1)(3))
                    a.Add(drivertable.DefaultView.Item(j1)(4))
                    a.Add(drivertable.DefaultView.Item(j1)(6))
                    a.Add(drivertable.DefaultView.Item(j1)(7))
                    a.Add(drivertable.DefaultView.Item(j1)(8))

                    a.Add(drivertable.DefaultView.Item(j1)(13))
                    a.Add(drivertable.DefaultView.Item(j1)(14))
                    a.Add(drivertable.DefaultView.Item(j1)(15))

                    aa.Add(a)
                Catch ex As Exception

                End Try
            Next
            json = JsonConvert.SerializeObject(aa, Formatting.None)


        Catch ex As Exception
            Return ex.Message & ";" & ex.StackTrace

        End Try
        Return json
    End Function


    Public Function InsertupdateDriver(ByVal userId As String, ByVal poiname As String, ByVal txtDOB As String, ByVal txtrfid As String, ByVal txtPhone As String, ByVal txtAddress As String, ByVal txtLicenceno As String, ByVal txtIssuingdate As String, ByVal txtExpiryDate As String, ByVal txtFuelCardNo As String, ByVal poiid As String, ByVal opr As Int16, ByVal ic As String, ByVal pwd As String, ByVal driverrole As Int16) As String
        Dim strqury As String = ""
        Dim result As Integer = 0
        Dim chkqury As String = ""
        Dim cmd As New SqlCommand
        Dim dr As SqlDataReader
        Dim conn As SqlConnection = New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
        Try
            chkqury = "select * from driver where phoneno='" + txtPhone + "'"
            If txtDOB = "" Then
                txtDOB = "1900/01/01"
            End If
            If txtIssuingdate = "" Then
                txtIssuingdate = "1900/01/01"
            End If
            If txtExpiryDate = "" Then
                txtExpiryDate = "1900/01/01"
            End If
            If opr = 0 Then
                strqury = "insert into driver(rfid,userid,drivername,dateofbirth,phoneno,address,licenceno,issuingdate,expirydate,fuelcardno,driver_ic,password,isowner) "
                strqury = strqury + "values ('" & txtrfid & "','" & userId & "','" & poiname.Replace("'", "''") & "','" & txtDOB & "','" & txtPhone & "','" & txtAddress.Replace("'", "''") & "','" & txtLicenceno & "','" & txtIssuingdate & "','" & txtExpiryDate & "','" & txtFuelCardNo & "','" & ic & "','" & pwd & "'," & driverrole & ")"
            Else
                strqury = "update  driver set userid='" & userId & "',drivername='" & poiname.Replace("'", "''") & "',rfid='" & txtrfid & "',dateofbirth='" & Convert.ToDateTime(txtDOB).ToString("yyyy/MM/dd") & "',phoneno='" & txtPhone & "',address='" & txtAddress & "',licenceno='" & txtLicenceno & "',issuingdate='" & Convert.ToDateTime(txtIssuingdate).ToString("yyyy/MM/dd") & "',expirydate='" & Convert.ToDateTime(txtExpiryDate).ToString("yyyy/MM/dd") & "',fuelcardno='" & txtFuelCardNo & "',password='" & pwd & "',driver_ic='" & ic & "',isowner=" & driverrole & "  where driverid='" & poiid & "' "
            End If
            'response.write(strqury)
            If opr = 0 Then
                cmd = New SqlCommand(chkqury, conn)
                Try
                    conn.Open()
                    dr = cmd.ExecuteReader()
                    If dr.Read() Then
                        result = 99
                    Else
                        cmd = New SqlCommand(strqury, conn)
                        result = cmd.ExecuteNonQuery()
                    End If
                Catch ex As Exception

                Finally
                    conn.Close()
                End Try
            Else
                Try
                    conn.Open()
                    cmd = New SqlCommand(strqury, conn)
                    result = cmd.ExecuteNonQuery()
                Catch ex As Exception

                Finally
                    conn.Close()
                End Try
            End If



        Catch ex As Exception
            Return ex.Message & "  " & strqury
        Finally
            conn.Close()
        End Try
        Return result.ToString()
    End Function



    Public Function Activate(ByVal chekitems() As String) As Int16

        Dim result As Int16 = 0
        Dim strqury As String = ""
        Dim strargument As New StringBuilder()

        Dim cnt As Int16 = chekitems.Length()
        For i As Int16 = 0 To cnt - 1
            If i = cnt - 1 Then
                strargument.Append("'" & chekitems(i) & "'")
            Else
                strargument.Append("'" & chekitems(i) & "',")
            End If
        Next

        Try
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            strqury = "update driver set status=1 where driverid in(" & strargument.ToString() & ")"
            Dim cmd As New SqlCommand(strqury, conn)

            Try
                conn.Open()
                result = cmd.ExecuteNonQuery()
            Catch ex As Exception

            Finally
                conn.Close()
            End Try

        Catch ex As Exception

        End Try
        Return result
    End Function


    Public Function InActivate(ByVal chekitems() As String) As Int16

        Dim result As Int16 = 0
        Dim strqury As String = ""
        Dim strargument As New StringBuilder()

        Dim cnt As Int16 = chekitems.Length()
        For i As Int16 = 0 To cnt - 1
            If i = cnt - 1 Then
                strargument.Append("'" & chekitems(i) & "'")
            Else
                strargument.Append("'" & chekitems(i) & "',")
            End If
        Next

        Try
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))
            strqury = "update driver set status=0 where driverid in(" & strargument.ToString() & ")"
            Dim cmd As New SqlCommand(strqury, conn)
            Try
                conn.Open()
                result = cmd.ExecuteNonQuery()
            Catch ex As Exception
                Throw
            Finally
                conn.Close()
            End Try

        Catch ex As Exception

        End Try
        Return result
    End Function
End Class
