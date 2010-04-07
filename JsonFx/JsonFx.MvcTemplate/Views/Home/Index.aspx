<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">

	<%-- example showing how to directly add to the page a JBST control bound to view data --%>
	<%= Jbst.Bind("Example.congrats", this.ViewData)%>
	<p class="ReadMe">See <a href="http://help.jsonfx.net/instructions">http://help.jsonfx.net/instructions</a> for JsonFx customizations to Visual Studio.</p>

</asp:Content>
