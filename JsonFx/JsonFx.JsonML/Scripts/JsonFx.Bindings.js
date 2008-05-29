/*global JsonFx, JSON */
/*---------------------------------------------------------*\
	JsonFx.Bindings
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-05-25-2253
\*---------------------------------------------------------*/

// dependency checks
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.Bindings requires json2.js");
}

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
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
						if (options) {
							try {
								// treat string as JSON
								options = JSON.parse(options);
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

	window.setTimeout(function() {
			/* it is important to control the handler order */

			// wire up binding
			if ("function" === typeof window.onload) {
				window.onload = function(/*Event*/ evt) { b.bindAll(evt); return window.onload(evt); };
			} else {
				window.onload = b.bindAll;
			}

			// wire up unbinding
			if ("function" === typeof window.onunload) {
				window.onunload = function(/*Event*/ evt) { b.unbindAll(evt); return window.onunload(evt); };
			} else {
				window.onunload = b.unbindAll;
			}
		}, 0);
};

// instantiate only one, destroying the constructor
JsonFx.Bindings = new JsonFx.Bindings();
