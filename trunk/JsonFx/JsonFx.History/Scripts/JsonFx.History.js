/*global JSON, JsonFx */
/*
	JsonFx.History.js

	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-06-03-0834

	Distributed under the terms of an MIT-style license:
	http://jsonfx.net/BuildTools/License.txt
*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* check dependencies */
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.History requires json2.js");
}

/* Utilities ----------------------------------------------------*/

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

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

JsonFx.History = {
	/*Dictionary<string, JsonFx.History>*/ history: null,

	init: function(/*DOM*/ elem, /*function*/ callback) {

		if (JsonFx.History.h) {
			// IE doesn't let us change the original onload
			JsonFx.History.changed();
			return;
		}

		// store the history for easy lookup, first init wins
		JsonFx.History.h =
			{
				elem: elem,
				callback: elem.onload ? callback : null
			};

		if (elem.onload) {
			// IE doesn't let us change the original onload
			elem.onload = JsonFx.History.changed;
		} else {
			// IE doesn't store the first state, so we re-save
			var info = JsonFx.History.getCurrent();
			if (info) {
				JsonFx.History.save(info);
			}

			// now hook up callback
			JsonFx.History.h.callback = callback;
		}
	},

	/*object*/ getCurrent: function() {

		var h = JsonFx.History.h;
		if (!h || !h.elem) {
			return null;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.body) {
			return null;
		}

		var info = doc.body.innerHTML;
		if (!info) {
			return null;
		}

		return JSON.parse(info);
	},

	/*void*/ changed: function() {

		var h = JsonFx.History.h;
		if (!h || "function" !== typeof h.callback) {
			return;
		}

		var info = JsonFx.History.getCurrent();
		if (info) {
			h.callback(info);
		}
	},

	/*void*/ save: function(/*object*/ info, /*string*/ bookmark) {

		var h = JsonFx.History.h;
		if (!h) {
			return;
		}

		if (!h.elem) {
			// doesn't support history or no binding
			if ("function" === typeof h.callback) {
				h.callback(info);
			}
			return;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.write) {
			// Opera 8 doesn't trigger onload so no history
			h.elem = null;
			if ("function" === typeof h.callback) {
				h.callback(info);
			}
			return;
		}

		// create a new document containing the serialized object
		info = JSON.stringify(info);
		doc.open();
		try {
			doc.write(info);
		} finally {
			doc.close();
		}

		// optionally replace the document fragment
		bookmark = String(bookmark||"");
		if (bookmark) {
			var loc = window.top.location.href;
			var hash = loc.indexOf('#');
			if (hash >= 0) {
				loc = loc.substr(0, hash);
			}
			window.top.location.replace(loc+'#'+bookmark);
		}
	}
};
