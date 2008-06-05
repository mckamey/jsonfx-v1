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
	/*object*/ h: null,

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
				callback: callback
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

		// grab the input, assume rendered just before
		var fld = h.elem.previousSibling;
		var tag = fld && fld.tagName && fld.tagName.toLowerCase();
		var type = fld && fld.type && fld.type.toLowerCase();
		if (tag !== "input" || type !== "hidden") {
			return;
		}

		var info;
		if (fld.value) {
			if (h.elem.onload) {
				// reloaded page
				info = JsonFx.History.getState();
				if (info) {
					h.callback(info);
				}
			}
		} else {
			// first time through, set value
			fld.value = "*";

			if (!h.elem.onload) {
				var callback = JsonFx.History.h.callback;
				JsonFx.History.h.callback = null;

				// IE doesn't store the first state, so we re-save
				info = JsonFx.History.getState();
				if (info) {
					JsonFx.History.save(info);
				}

				// now hook up callback
				JsonFx.History.h.callback = callback;
			}
		}
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

		var info = doc.location.search;
		if (info && info.length) {
			info = info.substr(1);
			info = decodeURIComponent(info);
		} else {
			info = doc.body.innerHTML;
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

		info = JSON.stringify(info);
		if (doc.location.search) {
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
