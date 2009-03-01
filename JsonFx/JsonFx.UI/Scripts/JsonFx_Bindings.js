/*global JsonFx, JsonML, jQuery */
/*
	JsonFx_Bindings.js
	dynamic behavior binding support

	Created: 2006-11-11-1759
	Modified: 2009-02-28-1407

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

/* singleton JsonFx.Bindings */
JsonFx.Bindings = function() {

	/*object*/ var b = this;
	/*bool*/ var jQ = ("undefined" !== typeof jQuery);
	/*const string*/ var BIND = 1, UNBIND = 2;

	/*
		the structure of bindings and the implementation of add, bind, and unbind fork on presence of jQuery.
		if jQuery is present before JsonFx.Bindings, then the selector implementation is delegated to jQuery.
		otherwise a simple selector lookup structure is built to very specifically allow lookups without
		a lot of iterations.
	*/
	/*Dictionary<string,object>*/ var bindings = jQ ? [] : {};

	/*RegExp*/ var re = /^\s*([\w\-]*|[*])(?:#([\w\-]+)|\.([\w\-]+))?(?:#([\w\-]+)|\.([\w\-]+))?\s*$/;

	/*void*/ b.add = jQ ?
		// jQuery JsonFx.Bindings.add implementation
		function(/*string*/ selector, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
			if (typeof bind !== "function") {
				bind = null;
			}
			if (typeof unbind !== "function") {
				unbind = null;
			}
			if (!selector || (!bind && !unbind)) {
				return;
			}

			var binding = { selector: selector };
			binding[BIND] = bind;
			binding[UNBIND] = unbind;
			bindings.push(binding);
		} :
		// simple JsonFx.Bindings.add implementation
		function(/*string*/ selector, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
			if (typeof bind !== "function") {
				bind = null;
			}
			if (typeof unbind !== "function") {
				unbind = null;
			}
			if (!selector || (!bind && !unbind)) {
				return;
			}

			var s = selector instanceof Array ?
				selector :
				String(selector).split(',');
			while (s.length > 1) {
				b.add(s.shift(), bind, unbind);
			}
			selector = s.shift();

			s = re.exec(selector);
			if (!s) {
				// http://www.w3.org/TR/css3-selectors/#simple-selectors
				throw new Error("JsonFx.Bindings only supports simple tag, class, and id selectors. Selector: \""+selector+"\"");
			}

			s = {
				tag: (s[1]||"*").toLowerCase(),
				css: (s[3]||s[5]||"*"),
				id: (s[2]||s[4]||"")
			};

			if (s.id) {
				if (s.tag !== "*" || s.css !== "*") {
					throw new Error("JsonFx.Bindings only supports simple ID selectors. Add jQuery to enable full selector support. Selector: \""+selector+"\"");
				}
				if ("undefined" === typeof bindings["#"]) {
					/*object*/ bindings["#"] = {};
				}

				/*object*/ bindings["#"][s.id] = {};
				bindings["#"][s.id][BIND] = bind;
				bindings["#"][s.id][UNBIND] = unbind;
			} else {
				if ("undefined" === typeof bindings[s.tag]) {
					/*object*/ bindings[s.tag] = {};
				} else if (bindings[s.tag][s.css]) {
					throw new Error("A binding for "+selector+" has already been registered.");
				}

				/*object*/ bindings[s.tag][s.css] = {};
				bindings[s.tag][s.css][BIND] = bind;
				bindings[s.tag][s.css][UNBIND] = unbind;
			}
		};

	/*deprecated*/
	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
		b.add(tag+'.'+css, bind, unbind);
	};

	/*DOM*/ var performOne = jQ ?
		// jQuery performOne implementation
		function(/*DOM*/ elem, /*actionKey*/ a) {
			for (var i=0; i<bindings.length; i++) {
				if (bindings[i][a]) {
					var action = bindings[i][a];
					jQuery(elem).filter(bindings[i].selector).each(
						function() {
							try {
								elem = action(this) || this;
							} catch (ex) {
								window.alert("Error binding "+bindings[i].selector+":\n\n\""+ex.message+"\"");
							}
						});
				}
			}
			return elem;
		} :
		// simple performOne implementation
		function(/*DOM*/ elem, /*actionKey*/ a) {

			function bindSet(/*object*/ binds, /*string*/ css) {
				if (binds && binds[css] && binds[css][a]) {
					try {
						// perform action on element and
						// allow binding to replace element
						elem = binds[css][a](elem) || elem;
					} catch (ex) {
						window.alert("Error binding "+elem.tagName+"."+css+":\n\n\""+ex.message+"\"");
					}
				}
			}

			if (elem && elem.tagName) {

				// only perform on registered tags
				var tag = elem.tagName.toLowerCase();
				var allBinds = bindings["*"];
				var tagBinds = bindings[tag];

				if (tagBinds || allBinds) {

					bindSet(tagBinds, "*");
					bindSet(allBinds, "*");

					if (elem.className) {
						// for each css class in elem
						var classes = elem.className.split(/\s+/);
						for (var i=0; i<classes.length; i++) {
							var css = classes[i];
							bindSet(tagBinds, css);
							bindSet(allBinds, css);
						}
					}
				}
			}
			return elem;
		};

	var performOneID = jQ ?
		// no jQuery performOneID implementation
		null :
		// simple performOneID implementation
		function (/*DOM*/ elem, /*actionKey*/ a) {
			var action = bindings["#"][elem.id];
			action = action && action[a];
			return (action && action(elem)) || elem;
		};

	// perform a binding action on all child elements
	/*void*/ var perform = jQ ?
		// jQuery perform implementation
		function(/*DOM*/ root, /*actionKey*/ a) {
			for (var i=0; i<bindings.length; i++) {
				if (bindings[i][a]) {
					var action = bindings[i][a];
					jQuery(bindings[i].selector, root).each(
						function() {
							try {
								var elem = action(this) || this;
								if (elem !== this) {
									this.parentNode.replaceChild(elem, this);
								}
							} catch (ex) {
								window.alert("Error binding "+bindings[i].selector+":\n\n\""+ex.message+"\"");
							}
						});
				}
			}
		} :
		// simple perform implementation
		function(/*DOM*/ root, /*actionKey*/ a) {

	// TODO: add ability to bind on ID, className, tagName or any combination
	// determine how to most efficiently select the smallest set of eligible elements

			/*create a closure for replacement*/
			function queueReplacer(newer, older) {
				window.setTimeout(function() {
					if (older && older.parentNode) {
						older.parentNode.replaceChild(newer, older);
					}
					// free references
					newer = older = null;
				}, 0);
			}

			function bindTagSet(/*string*/ tag) {
				// for each element in root with tagName
				var elems = root.getElementsByTagName(tag);
				for (var i=0; i<elems.length; i++) {
					// perform action on element and
					// allow binding to replace element
					var replace = performOne(elems[i], a);
					if (replace !== elems[i] && elems[i].parentNode) {
						// queue up replacement at the end so as not to disrupt the list
						queueReplacer(replace, elems[i]);
					}
				}
			}

			if (bindings["#"]) {
				for (var id in bindings["#"]) {
					if (bindings["#"].hasOwnProperty(id)) {
						var elem = document.getElementById(id);
						if (!elem) {
							continue;
						}
						var replace = performOneID(elem, a);
						if (replace !== elem) {
							// queue up replacement at the end so as not to disrupt the list
							queueReplacer(replace, elem);
						}
					}
				}
			}
			root = root || document.body;
			if (root.getElementsByTagName) {
				if (bindings["*"]) {
					// if star rule, then must apply to all
					bindTagSet("*");
				} else {
					// only apply to tags with rules
					for (var tag in bindings) {
						if (bindings.hasOwnProperty(tag) && tag !== "*" && tag !== "#") {
							bindTagSet(tag);
						}
					}
				}
			}
		};

	// bind
	/*void*/ b.bind = function(/*DOM*/ root) {
		if (!isFinite(root.nodeType)) {
			root = null;
		}

		perform(root, BIND);
	};

	// unbind
	/*void*/ b.unbind = function(/*DOM*/ root) {
		if (!isFinite(root.nodeType)) {
			root = null;
		}

		perform(root, UNBIND);
	};

	// use bindOne as the default JBST filter
	if ("undefined" !== typeof JsonML && JsonML.BST) {
		/*DOM*/ function bindOne(/*DOM*/ elem) {
			if (performOneID && bindings["#"]) {
				elem = performOneID(elem, BIND);
			}
			elem = performOne(elem, BIND);
			return elem;
		};

		if ("function" !== typeof JsonML.BST.filter) {
			JsonML.BST.filter = bindOne;
		} else {
			JsonML.BST.filter = (function() {
				var jbstFilter = JsonML.BST.filter;
				return function(/*DOM*/ elem) {
					elem = jbstFilter(elem);
					return elem && bindOne(elem);
				};
			})();
		}
	}

	// register bind events
	if (typeof jQuery !== "undefined") {
		jQuery(b.bind);
		jQuery(window).bind("unload", b.unbind);
	} else if (window.addEventListener) {
		//DOM Level 2 model for binding events
		window.addEventListener("load", b.bind, false);
		window.addEventListener("unload", b.unbind, false);
	} else if (window.attachEvent) {
		//IE model for binding events
		window.attachEvent("onload", b.bind);
		window.attachEvent("onunload", b.unbind);
	} else {
		//DOM Level 0 model for binding events
		var onload = window.onload;
		window.onload =
			("function" === typeof onload) ?
			function(/*Event*/ evt) { b.bind(evt); return onload(evt); } :
			b.bind;

		var onunload = window.onunload;
		window.onunload =
			("function" === typeof onunload) ?
			function(/*Event*/ evt) { b.unbind(evt); return onunload(evt); } :
			b.unbind;
	}
};

// doing instead of anonymous fn for JSLint compatibility
// instantiate only one, destroying the constructor
JsonFx.Bindings = new JsonFx.Bindings();
