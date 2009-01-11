/*global JsonFx, jQuery, JSON */
/*
	JsonFx_Bindings.js
	dynamic behavior binding support

	Created: 2006-11-11-1759
	Modified: 2009-01-10-2203

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
if ("undefined" === typeof window.jQuery) {
	throw new Error("JsonFx_Bindings.js requires jquery.js");
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

	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem,options)*/ bind, /*function(elem,options)*/ unbind, /*bool*/ overwrite) {

//		alert(tag+"."+css+":\n"+bind+"\n"+unbind);

		if (typeof css !== "string") {
			throw new Error("Binding CSS is not a string.");
		}
		if (typeof tag !== "string") {
			throw new Error("Binding tag for \""+css+"\" is not a string.");
		}
		if (bind && typeof bind !== "function") {
			throw new Error("Binding method for \""+css+"\" is not a function.");
		}
		if (unbind && typeof unbind !== "function") {
			throw new Error("Unbinding method for \""+css+"\" is not a function.");
		}

		if (bind || unbind) {
			tag = tag.toLowerCase();
			if ("undefined" === typeof bindings[tag]) {
				/*object*/ bindings[tag] = {};
			} else if (!overwrite && bindings[tag][css]) {
				throw new Error("Binding for "+tag+"."+css+" has already been registered.");
			}

			/*object*/ bindings[tag][css] = {};
			bindings[tag][css][BIND] = bind || null;
			bindings[tag][css][UNBIND] = unbind || null;
		}
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		var tag, tagBindings, classes, i, css, options, replace;
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

						// allow element to provide parameters for bindings
						options = elem.getAttribute("jsonfx:options");
						if (options && "string" === typeof options) {
							try {
								// treat string as JSON
								options = JSON.parse(options, JsonFx.jsonReviver);
							} catch (ex) { }
						}

						try {
							// perform action on element and
							// allow binding to replace element
							elem = tagBindings[css][a](elem, options) || elem;
						} catch (ex2) {
							window.alert("Error binding "+tag+"."+css+":\n\n\""+ex2.message+"\"");
						}
					}
				}
			}
		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {
		var elems, i, replace;
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in bindings) {
				if (bindings.hasOwnProperty(tag)) {

					// for each element in root with tagName
					elems = root.getElementsByTagName(tag);
					for (i=0; i<elems.length; i++) {
						// perform action on element and
						// allow binding to replace element
						replace = performOne(elems[i], a);
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
		var ttl, crsr;
		if (document.body && document.body.style) {
			// store previous values
			ttl = document.body.title;
			crsr = document.body.style.cursor;

			// setup loading visual cues
			document.body.title = "Loading...";
			document.body.style.cursor = "wait";
		}

		try {
			b.bind(document);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl || "";
				document.body.style.cursor = crsr || "";
			}
		}
	};

	// unbind
	/*void*/ b.unbindAll = function() {
		var ttl, crsr;
		if (document.body && document.body.style) {
			// store previous values
			ttl = document.body.title;
			crsr = document.body.style.cursor;

			// setup loading visual cues
			document.body.title = "Unloading...";
			document.body.style.cursor = "wait";
		}

		try {
			b.unbind(document);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl || "";
				document.body.style.cursor = crsr || "";
			}
		}
	};

	// register bind methods
	jQuery(b.bindAll);

	// register unbind methods
	jQuery(window).bind("unload", b.unbindAll)
};

// doing instead of anonymous fn for JSLint compatibility
// instantiate only one, destroying the constructor
JsonFx.Bindings = new JsonFx.Bindings();
