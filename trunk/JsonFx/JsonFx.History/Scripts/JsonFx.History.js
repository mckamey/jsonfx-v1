/*global JSON, JsonFx */
/*
	JsonFx.History.js

	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-06-07-1202

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
	/*object*/ h: null,

	load: function(/*DOM*/ elem, /*function*/ callback, /*bool*/ start, /*string*/ url) {

		if (!elem || "function" !== typeof callback) {
			return;
		}

		var info;
		if (!JsonFx.History.h) {
			// initialization

			JsonFx.History.h =
				{
					elem: elem,
					callback: callback,
					url: url
				};

			info = JsonFx.History.getState(JsonFx.History.h);
			if (info) {
				// previously cached page was reloaded
				callback(info);

			} else if (!elem.onload && start) {
				// IE needs a little help ensuring that
				// initial state is stored in history
				JsonFx.History.h.callback = null;

				// re-save start state
				JsonFx.History.save(start);
				// reconnect callback
				JsonFx.History.h.callback = callback;
			}
			return;

		} else {
			// onchange

			info = JsonFx.History.getState(JsonFx.History.h) || start;
			if (info) {
				callback(info);
			}
		}
	},

	/*object*/ getState: function(/*object*/ h) {

		if (!h) {
			return null;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.location || !doc.body) {
			return null;
		}

		var info = h.url ?
			doc.location.search :
			doc.body.innerHTML;

		if (info && h.url) {
			// strip query char and decode
			info = info.substr(1);
			info = decodeURIComponent(info);
		}
		if (!info) {
			return null;
		}

		return JSON.parse(info);
	},

	/*void*/ save: function(/*object*/ info) {

		var h = JsonFx.History.h;
		if (!h) {
			return;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.location || !doc.write) {
			// error just call method directly
			if ("function" === typeof h.callback) {
				h.callback(info);
			}
			return;
		}

		info = JSON.stringify(info);
		if (h.url) {
			// encode the serialized object into the query string
			doc.location.href = h.url+'?'+encodeURIComponent(info);
		} else {
			// create a new document containing the serialized object
			doc.open();
			try {
				doc.write(info);
			} finally {
				doc.close();
			}
		}
	}
};
