<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Title="Online JBST Compiler" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="C" ContentPlaceHolderID="Content" runat="server">

<div class="step">
	<h1 class="title">Generate Ajax controls from declarative templates.</h1>
	<h2>1. JBST Template source goes here. Just click to edit. For details on the syntax see <a href="http://starterkit.jsonfx.net/jbst" target="_blank">http://starterkit.jsonfx.net/jbst</a></h2>

	<textarea id="jbst-editor"><%= this.ViewData["jbst-source"]%></textarea>
</div>

<div class="step">
	<h2>2. Convert into JavaScript. This produces a JavaScript object which can easily be bound to data e.g. <code>MyApp.MyJbstControl.bind(data);</code>.</h2>

	<div class="buttons">
		<a href="#compile" class="button button-large" onclick="JbstEditor.generate.call(this);return false;">Generate</a>
	</div>
</div>

<div class="step">
	<h2>3. Download supporting scripts. These</h2>

	<div class="buttons">
		<a href="/compiler/scripts" class="button button-large">Pretty-Print</a>
		<a href="/compiler/compacted" class="button button-large">Compacted</a>
	</div>
</div>

</asp:Content>
