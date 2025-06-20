Imports System.Data.SqlClient

Public Class GetExtVoltage
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim dttm As String = Request.QueryString("d")
        Dim bdt As String = Convert.ToDateTime(dttm).AddMinutes(-10).ToString("yyyy/MM/dd HH:mm:ss")
        Dim edt As String = Convert.ToDateTime(dttm).AddMinutes(10).ToString("yyyy/MM/dd HH:mm:ss")
        Dim plateno As String = Request.QueryString("plateno")
        Dim t As New DataTable

        t.Columns.Add(New DataColumn("No"))
        t.Columns.Add(New DataColumn("Plateno"))
        t.Columns.Add(New DataColumn("Date Time"))
        t.Columns.Add(New DataColumn("GPS"))
        t.Columns.Add(New DataColumn("Speed"))
        t.Columns.Add(New DataColumn("External Voltage"))

        Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.AppSettings("sqlserverconnection"))

        Dim gpsandignitioncondition As String = ""

        Dim cmd As SqlCommand = New SqlCommand("select distinct convert(varchar(19),timestamp,120) as datetime,externalbatv,gps_av,speed from vehicle_history    vht Join  vehicleTBL vt on vt.plateno=vht.plateno and " &
        "  vt.plateno ='" & plateno & "' And timestamp between '" & bdt & "' and '" & edt & "'", conn)
        'Response.Write(cmd.CommandText)
        Dim dr As SqlDataReader
        Dim r As DataRow
        Try
            conn.Open()
            dr = cmd.ExecuteReader()
            Dim i As Int64 = 1

            While dr.Read()
                r = t.NewRow
                r(0) = i.ToString()
                r(1) = plateno
                r(2) = dr("datetime")
                r(3) = dr("gps_av")
                r(4) = System.Convert.ToDouble(dr("speed")).ToString("0.00")


                If IsDBNull(dr("externalbatv")) Then
                    r(5) = "-"
                Else
                    r(5) = dr("externalbatv")
                End If

                t.Rows.Add(r)
                i = i + 1

            End While
        Catch ex As Exception
            Response.Write(ex.StackTrace)
        Finally
            conn.Close()
        End Try


        If t.Rows.Count = 0 Then
            r = t.NewRow
            r(0) = "--"
            r(1) = "--"
            r(2) = "--"
            r(3) = "--"
            r(4) = "--"
            r(5) = "--"
            t.Rows.Add(r)
        End If



        gv1.DataSource = t
        gv1.DataBind()

    End Sub

End Class