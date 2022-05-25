<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TSHelper.aspx.cs" Inherits="WebApp.Admin.TSHelper" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>

<body>
	<p>
		<br />
	</p>
	<form id="form1" runat="server">
		<p>Output Folder:
			<asp:TextBox ID="tOutPath" runat="server" Text="./ts"></asp:TextBox>
		</p>
		<p> Assembly Name(same as dll in bin):
			<asp:TextBox ID="tAssemblyName" runat="server" Text="Bugtrax"></asp:TextBox>
		</p>
		<p>
			Database Entity Classes' Namespace:
			<asp:TextBox ID="tEntityNamespace" runat="server" Text="Bugtrax.Entity"></asp:TextBox>
		</p>
		<p>
			DTO Classes' Namespace:
			<asp:TextBox ID="tDTONamespace" runat="server" Text="Bugtrax.DTO"></asp:TextBox>
		</p>
		<div>
			<asp:Button ID="bRun" runat="server" Text="Make TS Classes" OnClick="bRun_Click" />
			<asp:Label ID="lMessage" runat="server" Text="" />
		</div>
	</form>
</body>
</html>
