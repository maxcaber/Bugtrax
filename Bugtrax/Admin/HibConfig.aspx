<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HibConfig.aspx.cs" Inherits="RIFRegister.HibConfig" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            width: 122px;
        }
        .style2
        {
            width: 88px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        <table style="width: 34%;">
            <tr>
                <td class="style1">
                    Update Schema</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir2" runat="server" 
                        onclick="btnLoadActtiveDir2_Click" Text="Go!" />
                </td>
                <td rowspan="10">
                    <asp:Label ID="lblStatus" runat="server" Text="Label"></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style1">
                    Drop Tables</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir1" runat="server" 
                        onclick="btnLoadActtiveDir1_Click" Text="Go!" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    Create Tables</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir0" runat="server" 
                        onclick="btnLoadActtiveDir0_Click" Text="Go!" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    insert test data</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir3" runat="server" Text="Go!" 
                        onclick="btnLoadActtiveDir3_Click" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    CLEAN tables</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir4" runat="server" Text="Go!" 
                        onclick="btnLoadActtiveDir4_Click" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    test junk</td>
                <td class="style2">
                    <asp:Button ID="btnTest" runat="server" Text="Go!" onclick="btnTest_Click" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    &nbsp;</td>
                <td class="style2">
                    <asp:Button ID="btnLoadActtiveDir6" runat="server" Text="Go!" />
                </td>
            </tr>
            <tr>
                <td class="style1">
                    &nbsp;</td>
                <td class="style2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style1">
                    &nbsp;</td>
                <td class="style2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style1">
                    &nbsp;</td>
                <td class="style2">
                    &nbsp;</td>
            </tr>
        </table>
        
    </div>
    </form>
</body>
</html>


