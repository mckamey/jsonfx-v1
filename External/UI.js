/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-02-07-2339
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

	/*hashtable[tag] of object*/ var bindings = {};

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

		if (bind || unbind) {
			tag = tag.toLowerCase();
			if ("undefined" === typeof bindings[tag]) {
				/*object*/ bindings[tag] = {};
			} else if (bindings[tag][css]) {
				throw new Error("Binding for tag=\""+tag+"\" css=\""+css+"\" has already been registered.");
			}

			/*object*/ bindings[tag][css] = {};
			bindings[tag][css][BindAction] = bind ? bind : null;
			bindings[tag][css][UnbindAction] = unbind ? unbind : null;
		}
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		if (elem && elem.tagName && elem.className) {

/*TIMER*/
//JsonFx.Timer.start(a+"_one");
/*TIMER*/

			// only perform on registered tags
			var tag = elem.tagName.toLowerCase();
			if (bindings[tag]) {
				var tagBindings = bindings[tag];
				var classes = elem.className.split(/\s+/);

				// for each css class in elem
				for (var i=0; i<classes.length; i++) {
					var css = classes[i];
					if (css && tagBindings[css] && tagBindings[css][a]) {

						// perform action on element							
						tagBindings[css][a](elem);
					}
				}
			}

/*TIMER*/
//JsonFx.Timer.stop(a+"_one", true);//48/16,46/31,62/0
/*TIMER*/

		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {

/*TIMER*/
//JsonFx.Timer.start(a+"_all");
/*TIMER*/
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in bindings) {
				if (bindings.hasOwnProperty(tag)) {

					// for each element in root with tagName
					var elems = root.getElementsByTagName(tag);
					for (var i=0; i<elems.length; i++) {
						performOne(elems[i], a);
					}
				}
			}
		}

/*TIMER*/
//JsonFx.Timer.stop(a+"_all", true);//32,31,31
/*TIMER*/
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		return performOne(elem, BindAction);
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

/*TIMER*/
//JsonFx.Timer.start("display");
/*TIMER*/

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
/*TIMER*/
//JsonFx.Timer.stop("display", true);//265,266,266
/*TIMER*/
	return container;
};

/* returns true if request was sent */
/*bool*/ JsonFx.UI.loadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {

/*TIMER*/
//JsonFx.Timer.start("load");
/*TIMER*/

	return JsonFx.IO.GetJsonRequest(url, null, null,
			function(jml,obj) {

/*TIMER*/
//JsonFx.Timer.stop("load", true);//282,281,22750(greedy regex)
/*TIMER*/
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
	None:  0x00,
	Horz:  0x01,
	Vert:  0x02,
	Fade:  0x04,
	Zoom:  0x08,
	ClipX: 0x10,
	ClipY: 0x20,
/*	Both: 0x1|0x2,*/
/*	All: 0x1|0x2|0x4|0x8,*/

	// helper method which tests for Horz
	/*bool*/ isHorz: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Horz & dir);
	},

	// helper method which tests for Vert
	/*bool*/ isVert: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Vert & dir);
	},

	// helper method which tests for Fade
	/*bool*/ isFade: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Fade & dir);
	},

	// helper method which tests for Zoom
	/*bool*/ isZoom: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Zoom & dir);
	},

	// helper method which tests for ClipX
	/*bool*/ isClipX: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.ClipX & dir);
	},

	// helper method which tests for ClipY
	/*bool*/ isClipY: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.ClipY & dir);
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

	var pxHeight = "",
		pxWidth = "",
		userHeight = "",
		userWidth = "",
		userOverflow = "",
		userPosition = "",
		userFilter = "",
		userKhtml = "",
		userMoz = "",
		userOpacity = "",
		userZoom = "",
		userClip = "",
		alpha = null;

	/*void*/ function saveShape() {
		if (elem && es) {
			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userOverflow = es.overflow;
			}
			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userHeight = es.height;
				pxHeight = !isNaN(elem.offsetHeight) ?
					elem.offsetHeight : parseFloat(JsonFx.UI.getStyle(elem, "height"));
			}
			if (JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir)) {
				userWidth = es.width;
				pxWidth = !isNaN(elem.offsetWidth) ?
					elem.offsetWidth : parseFloat(JsonFx.UI.getStyle(elem, "width"));
			}
			if (JsonFx.UI.Dir.isFade(dir)) {
				userKhtml = es["-khtml-opacity"];
				userMoz = es["-moz-opacity"];
				userOpacity = es.opacity;
				if (elem.filters && !alpha) {
					es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
					try {
						alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
					} catch (ex) { alpha = null; }
					if (!alpha) {
						es.filter += " alpha(enabled=false)";
						try {
							alpha = elem.filters.item("alpha");
						} catch (ex) { alpha = null; }
					}
				}
			}
			if (JsonFx.UI.Dir.isZoom(dir)) {
				userZoom = es.zoom;
			}
			if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userPosition = es.position;
				userClip = es.clip;
			}
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
					if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isZoom(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
						es.display = "none";
					}
					es.visibility = "hidden";
				}
				if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
					es.overflow = userOverflow;
				}
				if (JsonFx.UI.Dir.isHorz(dir)) {
					es.width = userWidth;
				}
				if (JsonFx.UI.Dir.isVert(dir)) {
					es.height = userHeight;
				}
				if (JsonFx.UI.Dir.isFade(dir)) {
					es["-khtml-opacity"] = userKhtml;
					es["-moz-opacity"] = userMoz;
					es.opacity = userOpacity;
					try {
						if (alpha) {
							alpha.enabled = false;
						}
						if (userFilter) {
							es.filter = userFilter;
						}
					} catch (ex) {
						alpha = null;
					}
				}
				if (JsonFx.UI.Dir.isZoom(dir)) {
					es.zoom = userZoom;
				}
				if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
					es.position = userPosition;
					es.clip = userClip ? userClip : "auto";
				}
				mutex = false;
				return;
			}

			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				es.overflow = "hidden";
			}

			if (JsonFx.UI.Dir.isHorz(dir) && !isNaN(pxWidth)) {
				// horizontal, simplified lerp
				es.width = Math.floor(pxWidth*step)+"px";
			}
			if (JsonFx.UI.Dir.isVert(dir) && !isNaN(pxHeight)) {
				// vertical, simplified lerp
				es.height = Math.floor(pxHeight*step)+"px";
			}
			if (JsonFx.UI.Dir.isFade(dir)) {
				// opacity, simplified lerp
				es["-khtml-opacity"] = 1.0*step;
				es["-moz-opacity"] = 1.0*step;
				es.opacity = 1.0*step;
				if (alpha) {
					try {
						alpha.opacity = Math.floor(100*step);
						alpha.enabled = true;
					} catch (ex) {
						alpha = null;
					}
				}
			}
			if (JsonFx.UI.Dir.isZoom(dir)) {
				es.zoom = Math.floor(100*step)+"%";
			}
			if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				var clip = ["auto","auto","auto","auto"];
				if (JsonFx.UI.Dir.isClipX(dir) && !isNaN(pxWidth)) {
					clip[3] = Math.ceil(pxWidth*(1-step)/2)+"px";
					clip[1] = ((pxWidth/2)+Math.floor(pxWidth*step/2))+"px";
				}
				if (JsonFx.UI.Dir.isClipY(dir) && !isNaN(pxHeight)) {
					clip[0] = Math.ceil(pxHeight*(1-step)/2)+"px";
					clip[2] = ((pxHeight/2)+Math.floor(pxHeight*step/2))+"px";
				}
				es.position = "absolute";
				es.clip = "rect("+clip.join(' ')+")";
			}

			setTimeout(
				function() {
					m(state ? (step-StepInc) : (step+StepInc));
				},
				0);
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
				next.minimize = JsonFx.UI.createMinimize(next, JsonFx.UI.Dir.Vert|JsonFx.UI.Dir.Fade);
			} else {
				return false;
			}
		}
		elem.minimized = !elem.minimized;
		next.minimize(elem.minimized);
		if (elem.minimized) {
			elem.className = elem.className.replace(/\s*jsonfx-expanded/g, " jsonfx-collapsed");
		} else {
			elem.className = elem.className.replace(/\s*jsonfx-collapsed/g, " jsonfx-expanded");
		}
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

JsonFx.UI.Bindings.register("label", "jsonfx-expando", JsonFx.UI.expandoCreate, JsonFx.UI.expandoDispose);

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

		var li = ["li"];
		var a = null;
		if ("object" === pt && pv) {
			a = ["label", {"class":"jsonfx-expando jsonfx-expanded"}];
			li.push(a);
		}
		(a?a:li).push(["span", {"class":"jsonfx-type"}, (pv instanceof Array) ? "array" : pt]);
		(a?a:li).push(["span", {"class":"jsonfx-name"}, pn]);

		if ("object" === pt) {
			li.push(JsonFx.UI.dumpData(pv));
		} else {
			li.push(["span", {"class":"jsonfx-value"}, String(pv)]);
		}
		ul.push(li);
	}

	return ul;
};
