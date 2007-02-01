/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-02-01-0043
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
JsonFx.UI = {};

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*elem*/ elem, /*string*/ style) {
	if (typeof(elem) === "string") {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	if (elem.currentStyle) {
		// IE only
		return elem.currentStyle[style];
	}
	return null;
};

/*function*/ JsonFx.UI.combineHandlers = function (/*function*/ handlerA, /*function*/ handlerB) {
	if ("function" === typeof handlerA) {
		if ("function" === typeof handlerB) {
			return function(/*Event*/ evt) { handlerA(evt); return handlerB(evt); };
		} else {
			return handlerA;
		}
	} else {
		return handlerB;
	}
};

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BindAction = "B", UnbindAction = "U";

	/*hashtable[tag] of array*/ var tags = {};
	/*hashtable[css] of object*/ var bindings = {};

	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
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
		if (bindings[css]) {
			throw new Error("Binding for \""+css+"\" has already been registered.");
		}
		if (bind || unbind) {
			tag = tag.toLowerCase();
			if ("undefined" === typeof tags[tag]) {
				/*array*/ tags[tag] = [];
			}
			tags[tag].push(css);

			/*object*/ bindings[css] = {};
			bindings[css][BindAction] = bind ? bind : null;
			bindings[css][UnbindAction] = unbind ? unbind : null;
		}
	};

// TODO: recreate the inner loop using just the CSS classes in elem.className
// don't check against all the classes for tag, make the tags[tag] just an
// attribute on bindings.  then the lookup for elemClassNames[i] is a hashtable
// rather than a loop.

// TODO: see if can abstract out the innermost action and use it in perform
// and in the JsonML filter bindOne

//	// perform a binding action on single element
//	/*void*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
//	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in tags) {
				if (tags.hasOwnProperty(tag)) {

					// for each element in root with tagName
					var elems = root.getElementsByTagName(tag);
					for (var j=0; j<elems.length; j++) {
						var elem = elems[j];
						if (elem.className) {

							// for each css-binding for tag
							for (var i=0; i<tags[tag].length; i++) {
								var css = tags[tag][i];
								if (bindings[css][a] &&
									elem.className.indexOf(css) >= 0) {

									// perform action on element							
									bindings[css][a](elem);
								}
							}
						}
					}
				}
			}
		}
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		if (elem && elem.tagName && elem.className) {
			var tag = elem.tagName.toLowerCase();
			if (tags.hasOwnProperty(tag)) {

				// for each css-binding for tag
				for (var i=0; i<tags[tag].length; i++) {
					var css = tags[tag][i];
					if (bindings[css][BindAction] &&
						elem.className.indexOf(css) >= 0) {

						// perform action on element							
						bindings[css][BindAction](elem);
					}
				}
			}
		}
		return elem;
	};

	// bind
	/*void*/ b.bindAll = function() {
		// store previous values
		var ttl = document.body.title;
		var crsr = document.body.style.cursor;

		// setup loading visual cues
		document.body.title = "Loading...";
		document.body.style.cursor = "wait";

		try {
			perform(document, BindAction);
		} finally {
			// restore previous values
			document.body.title = ttl ? ttl : "";
			document.body.style.cursor = crsr ? crsr : "";
		}
	};

	// unbind
	/*void*/ b.unbindAll = function() {
		// store previous values
		var ttl = document.body.title;
		var crsr = document.body.style.cursor;

		// setup loading visual cues
		document.body.title = "Unloading...";
		document.body.style.cursor = "wait";

		try {
			perform(document, UnbindAction);
		} finally {
			// restore previous values
			document.body.title = ttl ? ttl : "";
			document.body.style.cursor = crsr ? crsr : "";
		}
	};

	// bind
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BindAction);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UnbindAction);
	};

	// wire up binding
	if ("function" === typeof window.onload) {
		window.onload = JsonFx.UI.combineHandlers(b.bindAll, window.onload);
	} else {
		window.onload = b.bindAll;
	}

	// wire up unbinding
	if ("function" === typeof window.onunload) {
		window.onunload = JsonFx.UI.combineHandlers(b.unbindAll, window.onload);
	} else {
		window.onunload = b.unbindAll;
	}
};

// instantiate only one, destroying the constructor
JsonFx.UI.Bindings = new JsonFx.UI.Bindings();

/*------------------*\
	JsonML Methods
\*------------------*/

/*void*/ JsonFx.UI.clear = function(/*element*/ elem) {
	if (elem) {
		// unbind to prevent memory leaks
		JsonFx.UI.Bindings.unbind(elem);

		while (elem.lastChild) {
			elem.removeChild(elem.lastChild);
		}
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*element*/ JsonFx.UI.displayJsonML = function(/*JsonML*/ jml, /*element or string*/ container) {
	// either DOM element or id
	container = (typeof(container) !== "string") ?
		container : document.getElementById(container);

	if (jml && typeof(jml.parseJsonML) === "function") {
		jml = jml.parseJsonML(JsonFx.UI.Bindings.bindOne);
		if (jml) {
			if (container) {
				JsonFx.UI.clear(container);
				container.appendChild(jml);
			} else if (jml.id) {
				container = document.getElementById(jml.id);
				if (container && container.parentNode) {
					// unbind to prevent memory leaks
					JsonFx.UI.Bindings.unbind(container);
					container.parentNode.replaceChild(jml, container);
				} else {
					// can't add to document, so unbind
					JsonFx.UI.Bindings.unbind(jml);
					jml = null;
				}
			} else if (jml.childNodes) {
				while (jml.firstChild) {
					if (!jml.firstChild.id) {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(jml.firstChild);
						jml.removeChild(jml.firstChild);
						continue;
					}
					container = document.getElementById(jml.firstChild.id);
					if (container && container.parentNode) {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(container);
						container.parentNode.replaceChild(jml.firstChild, container);
					} else {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(jml.firstChild);
						jml.removeChild(jml.firstChild);
					}
				}
				container = null;
			}
		}
	}
	return container;
};

/* returns true if request was sent */
/*bool*/ JsonFx.UI.loadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {
	return JsonFx.IO.GetJsonRequest(url, null, null,
			function(jml,obj) {
				JsonFx.UI.displayJsonML(jml, container);
				if (callback) { callback(context); }
			}
		, null);
};

/*-----------------*\
	Maximize Verb
\*-----------------*/

/*function*/ JsonFx.UI.createMaximize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) === "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem || !elem.tagName) {
		return null;
	}

	var es = elem.style,
	/*bool*/ state = false;

	var userShape;

	/*void*/ function saveShape() {
		userShape = {};
		for (var s in es) {
			if (/*es.hasOwnProperty(s) &&*/ es[s]) {
				userShape[s] = es[s];
			}
		}
	}
	/*void*/ function restoreShape() {
		if (userShape) {
			for (var s in userShape) {
				if (userShape.hasOwnProperty(s)) {
					es[s] = userShape[s];
				}
			}
		}
	}

	// state: true = maixmize, false = restore
	return /*void*/ function max(/*bool*/ newState) {

		if (!state) {
			saveShape();
		}
		// store the latest requested state
		if (typeof(newState) === "boolean") {
			state = newState;
		} else {
			state = !state;
		}

		if (state) {
			es.floatStyle = "none";
			es.position = "absolute";
			es.width = JsonFx.UI.getStyle(document.documentElement, "width");
			es.height = JsonFx.UI.getStyle(document.documentElement, "height");
			es.top = "0px";
			es.left = "0px";
			es.bottom = "0px";
			es.right = "0px";
		} else {
			restoreShape();
		}
	};
};

/*-----------------*\
	Minimize Verb
\*-----------------*/

JsonFx.UI.Dir = {
	None: 0x0,
	Horz: 0x1,
	Vert: 0x2,
	Both: 0x1|0x2,

	// helper method which tests for Horz
	/*bool*/ isHorz: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Horz & dir);
	},

	// helper method which tests for Vert
	/*bool*/ isVert: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Vert & dir);
	}
};

/*function*/ JsonFx.UI.createMinimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) === "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem || !elem.tagName) {
		return null;
	}

	var es = elem.style,
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ StepMin = 0.0,
	/*const float*/ StepMax = 1.0,
	/*const float*/ StepInc = 0.05;

	var userOverflow = "",
		userHeight = "",
		userWidth = "",
		pxHeight = "",
		pxWidth = "";

	/*void*/ function saveShape() {
		if (elem && es) {
			userOverflow = es.overflow;
			userHeight = es.height;
			userWidth = es.width;
			pxHeight = !isNaN(elem.offsetHeight) ?
					elem.offsetHeight : parseFloat(JsonFx.UI.getStyle(elem, "height"));
			pxWidth = !isNaN(elem.offsetWidth) ?
					elem.offsetWidth : parseFloat(JsonFx.UI.getStyle(elem, "width"));
		}
	}

	// state: true = collapse, false = expand
	return /*void*/ function min(/*bool*/ newState) {

		if (!es) { return; }

		// store the latest requested state
		if (typeof(newState) === "boolean") {
			state = newState;
		} else {
			state = !state;
		}
		if (mutex) {
			// crude concurrency check
			return;
		}
		mutex = true;

		if (state) {
			saveShape();
		}
		es.display = "block";
		es.visibility = "visible";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (!es || isNaN(step)) {
				mutex = false;
				return;
			}

			if (step < StepMin || step > StepMax) {
				if (state) {
					if (JsonFx.UI.Dir.isVert(dir)) {
						es.display = "none";
					}
					es.visibility = "hidden";
				}
				es.overflow = userOverflow;
				es.width = userWidth;
				es.height = userHeight;
				mutex = false;
				return;
			}

			es.overflow = "hidden";

			if (JsonFx.UI.Dir.isHorz(dir) && !isNaN(pxWidth)) {
				// horizontal, simplified lerp
				es.width = Math.floor(pxWidth*step)+"px";
			}
			if (JsonFx.UI.Dir.isVert(dir) && !isNaN(pxHeight)) {
				// vertical, simplified lerp
				es.height = Math.floor(pxHeight*step)+"px";
			}

			setTimeout(function() {
					m(state ? (step-StepInc) : (step+StepInc));
				}, 0);
		}

		m(state ? StepMax : StepMin);
	};
};

/* DataDump ----------------------------------------------------*/

/*void*/ JsonFx.UI.expandoCreate = function(/*element*/ elem) {
	elem.style.cursor = "pointer";
	elem.minimized = false;

	elem.onclick = function (/*event*/ evt) {
		var next = elem.nextSibling;
		if (!next.minimize) {
			if (next.tagName && next.tagName.toLowerCase() === "ul") {
				next.minimize = JsonFx.UI.createMinimize(next, JsonFx.UI.Dir.Vert);
			} else {
				return false;
			}
		}
		elem.minimized = !elem.minimized;
		next.minimize(elem.minimized);
		elem.innerHTML = elem.minimized ? "[+]" : "[x]";
		return false;
	};
};
/*void*/ JsonFx.UI.expandoDispose = function(/*element*/ elem) {
	var next = elem.nextSibling;
	if (next.minimize) {
		next.minimize = null;
	}
	elem.onclick = null;
};

JsonFx.UI.Bindings.register("a", "jsonfx-close", JsonFx.UI.expandoCreate, JsonFx.UI.expandoDispose);

/*JsonML*/ JsonFx.UI.dumpData = function(/*json*/ data) {
	if (data === null) {
		return "null";
	}
	var ul = ["ul", {"class":"jsonfx-object"}];

	for (var pn in data) {
		if (!data.hasOwnProperty(pn)) {
			continue;
		}
		var pv = data[pn];
		var pt = typeof(pv);

		var li = ["li",
			["span", {"class":"jsonfx-type"}, (pv instanceof Array) ? "array" : pt],
			["span", {"class":"jsonfx-name"}, pn]];

		if ("object" === pt) {
			if (pv) {
				li.push(["a", {"class":"jsonfx-close"}, "[x]"]);
			}
			li.push(JsonFx.UI.dumpData(pv));
		} else {
			li.push(["span", {"class":"jsonfx-value"}, String(pv)]);
		}
		ul.push(li);
	}

	return ul;
};
