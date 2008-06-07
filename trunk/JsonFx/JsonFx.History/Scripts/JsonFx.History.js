/*global JSON, JsonFx */
/*
	JsonFx.History.js

	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-06-07-0827

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
	/*string*/ flag: "*",

	init: function(/*DOM*/ elem, /*function*/ callback, /*bool*/ mode) {

		if (JsonFx.History.h) {
			// IE doesn't let us change the original onload
			JsonFx.History.changed();
			return;
		}

		// store the history for easy lookup, first init wins
		JsonFx.History.h =
			{
				elem: elem,
				callback: callback,
				virtual: !mode
			};

		if (elem.onload) {
			// change the onload to onchange
			elem.onload = JsonFx.History.changed;
		}

		// check if a history exists
		JsonFx.History.ensureState();
	},

	/*void*/ ensureState: function() {

		var h = JsonFx.History.h;
		if (!h || !h.elem || !h.callback) {
			return;
		}

		var info;
		if (JsonFx.History.isFlagged()) {
			// reloaded page
			info = JsonFx.History.getState();
			if (info) {
				h.callback(info);
			}
		} else if (!h.elem.onload) {
			// IE needs a little help ensuring that
			// initial state is stored in history
			var callback = JsonFx.History.h.callback;
			JsonFx.History.h.callback = null;

			// IE doesn't store the first state, so we re-save
			info = JsonFx.History.getState();
			if (info) {
				JsonFx.History.save(info);
			}

			// reconnect callback
			JsonFx.History.h.callback = callback;
		}
	},

	/*bool*/ isFlagged: function() {

		var h = JsonFx.History.h;
		if (!h || !h.elem) {
			return false;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.location || !doc.body) {
			return false;
		}

		var info = h.virtual ?
			doc.body.innerHTML :
			doc.location.search;

		if (info && !h.virtual) {
			// strip query char and decode
			info = info.substr(1);
			info = decodeURIComponent(info);
		}

		return info && (info.charAt(0) === JsonFx.History.flag);
	},

	/*object*/ getState: function() {

		var h = JsonFx.History.h;
		if (!h || !h.elem) {
			return null;
		}

		var doc = JsonFx.UI.getIFrameDocument(h.elem);
		if (!doc || !doc.location || !doc.body) {
			return null;
		}

		var info = h.virtual ?
			doc.body.innerHTML :
			doc.location.search;

		if (info && !h.virtual) {
			// strip query char and decode
			info = info.substr(1);
			info = decodeURIComponent(info);
		}
		if (info && info.charAt(0) === JsonFx.History.flag) {
			// strip cache flag
			info = info.substr(1);
		}
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

		var info = JsonFx.History.getState();
		if (info) {
			h.callback(info);
		}
	},

	/*void*/ save: function(/*object*/ info) {

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
		if (!doc || !doc.location || !doc.write) {
			// Opera 8 doesn't trigger onload so no history
			h.elem = null;
			if ("function" === typeof h.callback) {
				h.callback(info);
			}
			return;
		}

		info = JsonFx.History.flag+JSON.stringify(info);
		if (!h.virtual) {
			// encode the serialized object into the query string
			doc.location.search = '?'+encodeURIComponent(info);
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
