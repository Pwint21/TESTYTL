<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GetExtVoltage.aspx.vb" Inherits="YTLWebApplication.GetExtVoltage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="cssfiles/css3-buttons.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript" src="jsfiles/jquery.js"></script>
    <title>Polling</title>
    <style type="text/css" media="screen">
        @import "cssfiles/demo_page.css";
        @import "cssfiles/demo_table_jui.css";
        @import "cssfiles/themes/redmond/jquery-ui-1.8.4.custom.css";
        @import "cssfiles/ColVis.css";
    </style>
    <link type="text/css" href="cssfiles/jquery-ui.css" rel="stylesheet" />
    <link href="cssfiles/css3-buttons.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        #fw_container {
            width: 996px;
        }

        table.display td {
            padding: 2px 2px;
        }

        .fg-toolbar {
            font-size: 10px;
        }

        .MyButton {
            text-align: left;
            float: left;
            width: 350px;
        }

        .ColVis {
            display: none;
        }
    </style>
    <script type="text/javascript" language="javascript" src="jsfiles/jquery.js"></script>
    <script type="text/javascript" language="javascript" src="jsfiles/jquery-ui-1.8.20.custom.min.js"></script>
    <script type="text/javascript" language="javascript" src="jsfiles/jquery.dataTables.js"></script>
    <script type="text/javascript" language="javascript" src="jsfiles/ColVis.js"></script>
    <script type="text/javascript" language="javascript" src="jsfiles/FixedColumns.js"></script>
    <script type="text/javascript">
        var oTable;


        $(document).ready(function () {

            var oTable = $('#gv1').dataTable({
                "bProcessing": false,
                "bJQueryUI": true,
                "bServerSide": false,
                "sScrollY": "250px",
                "bPaginate": false,
                "bScrollCollapse": true,
                "bInfo": true,
                "bAutoWidth": false,
                "aaSorting": [[1, "desc"]],
                "fnDrawCallback": function (oSettings) {
                    if (oSettings.bSorted || oSettings.bFiltered) {
                        if (oSettings.aoColumns[0].bVisible == true) {
                            for (var i = 0, iLen = oSettings.aiDisplay.length; i < iLen; i++) {
                                $('td:eq(0)', oSettings.aoData[oSettings.aiDisplay[i]].nTr).html(i + 1);
                            }
                        }
                    }
                }

            });

    </script>
</head>
<body id="dt_example" style="margin: 0px;">
    <form id="form1" runat="server">
        <table>
            <br />
            <tr>
                <td align="left">
                    <div id="fw_container">
                        <asp:GridView ID="gv1" runat="server" AllowPaging="True" Width="100%" PageSize="20"
                            AutoGenerateColumns="False" HeaderStyle-Font-Size="12px" HeaderStyle-ForeColor="#FFFFFF"
                            HeaderStyle-BackColor="#465AE8" HeaderStyle-Font-Bold="True" Font-Bold="False"
                            Font-Overline="False" EnableViewState="False" HeaderStyle-Height="22px" HeaderStyle-HorizontalAlign="Center"
                            BorderColor="#F0F0F0">
                            <PagerSettings PageButtonCount="5" />
                            <PagerStyle Font-Bold="True" Font-Names="Verdana" Font-Size="Small" HorizontalAlign="Center"
                                VerticalAlign="Middle" BackColor="White" Font-Italic="False" Font-Overline="False"
                                Font-Strikeout="False" />
                            <Columns>
                                <asp:BoundField DataField="No" HeaderText="No">
                                    <ItemStyle Width="35px" HorizontalAlign="center" />
                                </asp:BoundField>
                                 <asp:BoundField DataField="Plateno" HeaderText="Plateno">
                                    <ItemStyle HorizontalAlign="Left"  Width="160px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Date Time" HeaderText="Date Time" HtmlEncode="False">
                                    <ItemStyle Width="130px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="GPS" HeaderText="GPS">
                                    <ItemStyle HorizontalAlign="Center"  Width="70px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Speed" HeaderText="Speed">
                                    <ItemStyle HorizontalAlign="Right"  Width="70px" />
                                </asp:BoundField>                               
                                <asp:BoundField DataField="External Voltage" HeaderText="External Voltage">
                                    <ItemStyle HorizontalAlign="Right"  Width="130px"  />
                                </asp:BoundField>                               
                            </Columns>
                            <AlternatingRowStyle BackColor="Lavender" />
                        </asp:GridView>
                    </div>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
