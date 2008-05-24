/*extern JSON, JsonFx */
/*---------------------------------------------------------*\
	JsonFx.History.js
	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-06-03-1433
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* check dependencies */
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.History requires json2.js");
}

/* Utilities ----------------------------------------------------*/

/*DOM*/ JsonFx.UI.getIFrameDocument = function(/*DOM*/ elem) {
	if (!elem) {
		return null;
	}

	if ("undefined" !== typeof elem.contentDocument) {
		// W3C
		return elem.contentDocument;
	} else if ("undefined" !== typeof elem.contentWindow) {
		// Microsoft
		return elem.contentWindow.document;
	} else if ("undefined" !== typeof elem.document) {
		// deprecated
		return elem.document;
	}
	// not available
	return elem;
};

/* JsonFx.History -----------------------------------------------*/

/* singleton JsonFx.History */
JsonFx.History = {

	/*DOM*/ h: null,
	/*function(object)*/ onchange: null,

	/*void*/ add: function(/*object*/ info) {
		if (!JsonFx.History.h) {
			// doesn't support history or no binding
			if ("function" === typeof JsonFx.History.onchange) {
				JsonFx.History.onchange(info);
			}
			return;
		}
		var h = JsonFx.UI.getIFrameDocument(JsonFx.History.h);
		if (h && h.location) {
			info = '?'+encodeURIComponent(JSON.stringify(info));
			h.location.href = h.location.href.replace(/[?].*$/, info);
		} else {
			// Opera 8 doesn't trigger onload so no history
			JsonFx.History.h = null;
		}
	},

	/*void*/ changed: function(/*DOM*/ elem) {
		if (!JsonFx.History.h) {
			// first time through is just for binding
			JsonFx.History.h = elem;
			return;
		}
		var h, info;
		if ("function" === typeof JsonFx.History.onchange) {
			h = JsonFx.UI.getIFrameDocument(elem);
			if (h) {
				info = h.location.search;
				if (info) {
					info = info.substring(info.indexOf('?')+1);
					info = decodeURIComponent(info);
					if (info) {
						info = JSON.parse(info);
					}
					JsonFx.History.onchange(info);
				}
			}
		}
	}
};
