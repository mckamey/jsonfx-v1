<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="<%= System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName %>">
<head runat="server">
	<meta http-equiv="Content-Type" content="application/xhtml+xml; charset=UTF-8" />

	<title>Untitled</title>

	<%-- one tag to include all the style sheets --%>
	<JsonFx:ResourceInclude ID="StyleImport" runat="server" SourceUrl="~/Styles/Styles.merge" />
</head>
<body>

	<%-- one tag to include all the client scripts --%>
	<JsonFx:ResourceInclude ID="ScriptInclude" runat="server" SourceUrl="~/Scripts/Scripts.merge" />

<form id="F" runat="server">
<div>

	<p class="HelloWorld">Hello world.</p>
	<p style="text-align:center;">View <a href="http://starterkit.jsonfx.net/instructions.aspx">http://starterkit.jsonfx.net/instructions.aspx</a> for configuration help.</p>
    
</div>
</form>

</body>
</html>
