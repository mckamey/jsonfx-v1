/*global JsonFx, JSON, jQuery */
/*
	JsonFx_Bindings.js
	dynamic behavior binding support

	Created: 2006-11-11-1759
	Modified: 2009-01-17-1315

	Copyright (c)2006-2009 Stephen M. McKamey
	Distributed under an open-source license: http://jsonfx.net/license
*/

/* namespace JsonFx */
if ("undefined" === typeof window.JsonFx) {
	window.JsonFx = {};
}
/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* dependency checks --------------------------------------------*/

if ("undefined" === typeof window.JSON) {
	throw new Error("JsonFx_Bindings.js requires json2.js");
}

if ("undefined" === typeof JsonFx.jsonReviver) {
	/*object*/ JsonFx.jsonReviver = function(/*string*/ key, /*object*/ value) {
		var a;
		if ("string" === typeof value) {
			a = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/.exec(value);
			if (a) {
				return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
			}
		}
		return value;
	};
}

/* singleton JsonFx.Bindings */
JsonFx.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BIND = "B", UNBIND = "U";

	/*hashtable[tag] of object*/ var bindings = {};

	// due to EMCA bug this RegExp is limited to max one of each
	// https://bugzilla.mozilla.org/show_bug.cgi?id=351349
	///*RegExp*/ var re = /^([\w\-]*|[*])(?:#([\w\-]+)|\.([\w\-]+))*$/;
	/*RegExp*/ var re = /^([\w\-]*|[*])(?:#([\w\-]+)|\.([\w\-]+))?(?:#([\w\-]+)|\.([\w\-]+))?$/;

	/*void*/ b.add = function(/*string*/ selector, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
		var s = re.exec(selector);
		if (!s) {
			// http://www.w3.org/TR/css3-selectors/#simple-selectors
			throw new Error("JsonFx.Bindings only supports simple tag, class, and id selectors. Invalid: \""+selector+"\"");
		}

		s = {
			tag: (s[1] || "*").toLowerCase(),
			id: (s[2] || ""),
			css: (s[3] || "")
		};

		if (bind && typeof bind !== "function") {
			throw new Error("Binding method for \""+selector+"\" is not a function.");
		}
		if (unbind && typeof unbind !== "function") {
			throw new Error("Unbinding method for \""+selector+"\" is not a function.");
		}

// TODO: add ability to execute on ID, className, tagName

		if (bind || unbind) {
			if ("undefined" === typeof bindings[s.tag]) {
				/*object*/ bindings[s.tag] = {};
			} else if (bindings[s.tag][s.css]) {
				throw new Error("A binding for "+selector+" has already been registered.");
			}

			/*object*/ bindings[s.tag][s.css] = {};
			bindings[s.tag][s.css][BIND] = bind || null;
			bindings[s.tag][s.css][UNBIND] = unbind || null;
		}
	};

	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
		b.add(tag+'.'+css, bind, unbind);
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		var tag, tagBindings, classes, i, css;
		if (elem && elem.tagName && elem.className) {

			// only perform on registered tags
			tag = elem.tagName.toLowerCase();
			if (bindings[tag]) {
				tagBindings = bindings[tag];
				classes = elem.className.split(/\s+/);

				// for each css class in elem
				for (i=0; i<classes.length; i++) {
					css = classes[i];
					if (css && tagBindings[css] && tagBindings[css][a]) {
						try {
							// perform action on element and
							// allow binding to replace element
							elem = tagBindings[css][a](elem) || elem;
						} catch (ex) {
							window.alert("Error binding "+tag+"."+css+":\n\n\""+ex.message+"\"");
						}
					}
				}
			}
		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in bindings) {
				if (bindings.hasOwnProperty(tag)) {

					// for each element in root with tagName
					var elems = root.getElementsByTagName(tag);
					for (var i=0; i<elems.length; i++) {
						// perform action on element and
						// allow binding to replace element
						var replace = performOne(elems[i], a);
						if (replace !== elems[i] && elems[i].parentNode) {
							elems[i].parentNode.replaceChild(replace, elems[i]);
						}
					}
				}
			}
		}
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		return performOne(elem, BIND);
	};

	// bind
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BIND);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UNBIND);
	};

	// bind
	/*void*/ b.bindAll = function() {
		b.bind(document);
	};

	// unbind
	/*void*/ b.unbindAll = function() {
		b.unbind(document);
	};

	// register bind events
	if (typeof jQuery !== "undefined") {
		jQuery(b.bindAll);
		jQuery(window).bind("unload", b.unbindAll);
	} else if (window.addEventListener) {
		//DOM Level 2 model for binding events
		window.addEventListener("load", b.bindAll, false);
		window.addEventListener("unload", b.unbindAll, false);
	} else if (window.attachEvent) {
		//IE model for binding events
		window.attachEvent("onload", b.bindAll);
		window.attachEvent("onunload", b.unbindAll);
	} else {
		//DOM Level 0 model for binding events
		var onload = window.onload;
		window.onload =
			("function" === typeof onload) ?
			function(/*Event*/ evt) { b.bindAll(evt); return onload(evt); } :
			b.bindAll;

		var onunload = window.onunload;
		window.onunload =
			("function" === typeof onunload) ?
			function(/*Event*/ evt) { b.unbindAll(evt); return onunload(evt); } :
			b.unbindAll;
	}
};

// doing instead of anonymous fn for JSLint compatibility
// instantiate only one, destroying the constructor
JsonFx.Bindings = new JsonFx.Bindings();
