/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-02-14-0713
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
JsonFx.UI = {};

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*element*/ elem, /*string*/ style) {
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

/*{left,top}*/ JsonFx.UI.getOffset = function(/*element*/ elem) {
	var top=0, left=0;
	while (elem) {
		top += elem.offsetTop;
		left += elem.offsetLeft;
		elem = elem.offsetParent;
		if (elem) {
			var pos = JsonFx.UI.getStyle(elem, "position");
			if (pos && pos !== "static") {
				elem = null;
			}
		}
	}
	return { "left":left, "top":top };
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

/*float*/ JsonFx.UI.lerp = function (/*float*/ start, /*float*/ end, /*float*/ t) {
	return (start * (1-t)) + (end * t);
};

/*int*/ JsonFx.UI.lerpInt = function (/*int*/ start, /*int*/ end, /*float*/ t) {
	return Math.floor( JsonFx.UI.lerp(start, end, t) + 0.5 );
};

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BIND = "B", UNBIND = "U";

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
			bindings[tag][css][BIND] = bind ? bind : null;
			bindings[tag][css][UNBIND] = unbind ? unbind : null;
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
		return performOne(elem, BIND);
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
			perform(document, BIND);
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
			perform(document, UNBIND);
		} finally {
			// restore previous values
			document.body.title = ttl ? ttl : "";
			document.body.style.cursor = crsr ? crsr : "";
		}
	};

	// bind
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BIND);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UNBIND);
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

	/*void*/ function store() {
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
			store();
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
	/*const float*/ STEP_MIN = 0,
	/*const float*/ STEP_MAX = 1,
	/*const float*/ STEP_INC = 0.05;

	var saved = false,
		pxHeight = "",
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

	/*void*/ function store() {
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
			saved = true;
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

		if (state || !saved) {
			store();
		}
		es.display = "block";
		es.visibility = "visible";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (!es || isNaN(step)) {
				mutex = false;
				return;
			}

			if (step < STEP_MIN || step > STEP_MAX) {
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
					es.clip = userClip ? userClip : "rect(auto auto auto auto)";// works in IE/FireFox/Opera
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
				es["-khtml-opacity"] = step;
				es["-moz-opacity"] = step;
				es.opacity = step;
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
					m(state ? (step-STEP_INC) : (step+STEP_INC));
				},
				0);
		}

		m(state ? STEP_MAX : STEP_MIN);
	};
};

/* DataDump ----------------------------------------------------*/

/*void*/ JsonFx.UI.expandoBind = function(/*element*/ elem) {
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
/*void*/ JsonFx.UI.expandoUnbind = function(/*element*/ elem) {
	var next = elem.nextSibling;
	if (next.minimize) {
		next.minimize = null;
	}
	elem.onclick = null;
};

JsonFx.UI.Bindings.register("label", "jsonfx-expando", JsonFx.UI.expandoBind, JsonFx.UI.expandoUnbind);

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

/*---------------------*\
	Transform Classes
\*---------------------*/

/* namespace JsonFx.UI.Animate */
JsonFx.UI.Animate = {};

/* class JsonFx.UI.Animate.Unit -------------------------------------------- */
JsonFx.UI.Animate.Unit = function(/*int*/ size, /*string*/ unit) {
	if (unit) {
		switch (unit) {
			case "px" :
				this.size = Math.floor(size);
				this.unit = unit;
				break;
			case "%" :
			case "em" :
			case "pt" :
			case "in" :
			case "cm" :
			case "mm" :
			case "pc" :
			case "ex" :
				this.size = Number(size);
				this.unit = unit;
				break;
			default:
				throw new Error("Invalid unit.");
		}
		if (!isFinite(this.size)) {
			throw new Error("Invalid unit.");
		}
	} else {
		this.size = NaN;
		this.unit = "";
	}
};
JsonFx.UI.Animate.Unit.prototype.toString = function() {
	return this.unit ? (this.size+this.unit) : "auto";
};
/*JsonFx.UI.Animate.Unit*/ JsonFx.UI.Animate.Unit.parse = function(/*string*/ str) {
	var size = parseFloat(str);
	var unit = null;
	if (isFinite(size) && str.match(/(px|%|em|pt|in|cm|mm|pc|ex)/)) {
		unit = RegExp.$1;
	}
	return new JsonFx.UI.Animate.Unit(size, unit);
};

/* class JsonFx.UI.Animate.Op -------------------------------------------- */
JsonFx.UI.Animate.Op = function() {
	this.x = this.y = this.z = this.l = this.t = this.w = this.h =
	this.f = this.cL = this.cR = this.cT = this.cB = NaN;
	this.s = 0.05;// 20 steps
};

/*Regex*/ JsonFx.UI.Animate.Op.clipRE = /^rect[\(]([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)[\)]$/;

/*JsonFx.UI.Animate.Op*/ JsonFx.UI.Animate.Op.save = function(/*element*/ elem) {
	var op = new JsonFx.UI.Animate.Op();

	if (elem && elem.style) {
		var es = elem.style;

		var top = parseFloat(JsonFx.UI.getStyle(elem, "top"));
		if (top && isFinite(top)) {
			op.top(top);
		} else {
			op.top(0);
		}
		var left = parseFloat(JsonFx.UI.getStyle(elem, "left"));
		if (left && isFinite(left)) {
			op.left(left);
		} else {
			op.left(0);
		}

		// width
		if (isFinite(elem.offsetWidth)) {
			op.width(elem.offsetWidth);
		} else {
			var width = parseFloat(JsonFx.UI.getStyle(elem, "width"));
			if (isFinite(width)) {
				op.width(width);
			}
		}
		// height
		if (isFinite(elem.offsetHeight)) {
			op.height(elem.offsetHeight);
		} else {
			var height = parseFloat(JsonFx.UI.getStyle(elem, "height"));
			if (isFinite(height)) {
				op.height(height);
			}
		}

		// scale
		op.scale(1, 1);

		// fade
		if (!!es.zoom && isFinite(es.opacity)) {
			op.fade(es.opacity);
		} else {
			op.fade(1);
		}

		// zoom
		if (!!es.zoom && isFinite(es.zoom)) {
			op.zoom(es.zoom);
		} else {
			op.zoom(1);
		}

		// clip
		if (es.clip && es.clip.match(JsonFx.UI.Animate.Op.clipRE)) {
			if ("%" === RegExp.$2) {
				op.clipTop(RegExp.$1/100);
			} else if ("px" === RegExp.$2) {
				op.clipTop(RegExp.$1/op.h);
			} else {
				op.clipTop(0);
			}

			if ("%" === RegExp.$4) {
				op.clipRight(1-RegExp.$3/100);
			} else if ("px" === RegExp.$4) {
				op.clipRight(1-RegExp.$3/op.w);
			} else {
				op.clipRight(0);
			}

			if ("%" === RegExp.$6) {
				op.clipBottom(1-RegExp.$5/100);
			} else if ("px" === RegExp.$6) {
				op.clipBottom(1-RegExp.$5/op.h);
			} else {
				op.clipBottom(0);
			}

			if ("%" === RegExp.$8) {
				op.clipLeft(RegExp.$7/100);
			} else if ("px" === RegExp.$8) {
				op.clipLeft(RegExp.$7/op.w);
			} else {
				op.clipLeft(0);
			}
		} else {
			op.clipTop(0);
			op.clipRight(0);
			op.clipBottom(0);
			op.clipLeft(0);
		}
		es = null;
	}

	elem = null;
	return op;
};

/*void*/ JsonFx.UI.Animate.Op.prototype.fade = function(/*float*/ f) {
	if (!isFinite(f) || f<0 || f>1) {
		throw new Error("Fade is a number from 0.0 to 1.0");
	}
	this.f = Number(f);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasFade = function() {
	return isFinite(this.f);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.zoom = function(/*float*/ z) {
	if (!isFinite(z)) {
		throw new Error("Zoom is a number >= 0.01 with 1.0 being normal");
	}
	if (z < 0.01) {
		z = 0.01;
	}
	this.z = Number(z);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasZoom = function() {
	return isFinite(this.z);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.move = function(/*int*/ left, /*int*/ top) {
	this.top(top);
	this.left(left);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.left = function(/*int*/ left) {
	if (!isFinite(left)) {
		throw new Error("Left is a pixel position");
	}
	this.l = Number(left);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.top = function(/*int*/ top) {
	if (!isFinite(top)) {
		throw new Error("Top is a pixel position");
	}
	this.t = Number(top);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasMove = function() {
	return isFinite(this.t)||isFinite(this.l);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasTop = function() {
	return isFinite(this.t);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasLeft = function() {
	return isFinite(this.l);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.scaleX = function(/*int*/ x) {
	if (!isFinite(x) || x<0) {
		throw new Error("Scale X is a number >= 0.0 with 1.0 being normal");
	}
	this.x = Number(x);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.scaleY = function(/*int*/ y) {
	if (!isFinite(y) || y<0) {
		throw new Error("Scale Y is a number >= 0.0 with 1.0 being normal");
	}
	this.y = Number(y);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.scale = function(/*int*/ x, /*int*/ y) {
	this.scaleX(x);
	this.scaleY(y);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.width = function(/*int*/ width) {
	if (!isFinite(width) || width<0) {
		throw new Error("Width is a non-negative pixel size");
	}
	this.w = Number(width);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.height = function(/*int*/ height) {
	if (!isFinite(height) || height<0.0) {
		throw new Error("Height is a non-negative pixel size");
	}
	this.h = Number(height);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScaleX = function() {
	return isFinite(this.x)||isFinite(this.w);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScaleY = function() {
	return isFinite(this.y)||isFinite(this.h);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScale = function() {
	return this.hasScaleX()||this.hasScaleY();
};

/*void*/ JsonFx.UI.Animate.Op.prototype.resize = function(/*int*/ width, /*int*/ height) {
	this.width(width);
	this.height(height);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasResize = function() {
	return isFinite(this.w)||isFinite(this.h);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasWidth = function() {
	return isFinite(this.w);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasHeight = function() {
	return isFinite(this.h);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.clipTop = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Top is a number from 0.0 to 1.0");
	}
	this.cT = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipRight = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Right is a number from 0.0 to 1.0");
	}
	this.cR = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipBottom = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Bottom is a number from 0.0 to 1.0");
	}
	this.cB = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipLeft = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Left is a number from 0.0 to 1.0");
	}
	this.cL = Number(clip);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClip = function() {
	return isFinite(this.cT)||isFinite(this.cR)||isFinite(this.cB)||isFinite(this.cL);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipT = function() {
	return isFinite(this.cT);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipR = function() {
	return isFinite(this.cR);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipB = function() {
	return isFinite(this.cB);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipL = function() {
	return isFinite(this.cL);
};

/*bool*/ JsonFx.UI.Animate.Op.prototype.isHidden = function() {
	return (this.f<=0)||(this.w<=0)||(this.h<=0)||(this.x<=0)||(this.y<=0)||(this.z<=0.01);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.isRemoved = function() {
	return (this.h<=0)||(this.y<=0);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.speed = function(/*int*/ s) {
	if (!isFinite(s) || s<=0 || s>1) {
		throw new Error("Speed is a number from 0.0 to 1.0");
	}
	this.s = Math.pow(s,4.5);
};

/* class JsonFx.UI.Animate.Engine -------------------------------------------- */
JsonFx.UI.Animate.Engine = function(/*element*/ elem) {

	if (typeof(elem) === "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem || !elem.tagName) {
		throw new Error("Invalid element");
	}

	var es = elem.style,
	/*JsonFx.UI.Animate.Op*/ start = null,
	/*JsonFx.UI.Animate.Op*/ op = new JsonFx.UI.Animate.Op(),
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ STEP_MIN = 0, // start
	/*const float*/ STEP_MAX = 1; // end

	var userHeight = "",
		userWidth = "",
		userTop = "",
		userLeft = "",
		userOverflow = "",
		userPosition = "",
		userDisplay = "",
		userVisibility = "",
		userFilter = "",
		userKhtml = "",
		userMoz = "",
		userOpacity = "",
		userZoom = "",
		userClip = "",
		alpha = null;

	if (elem && es) {
		userOverflow = es.overflow;
		userPosition = es.position;
		userDisplay = es.display;
		userVisibility = es.visibility;
		userHeight = es.height;
		userWidth = es.width;
		userHeight = es.top;
		userWidth = es.left;
		userKhtml = es["-khtml-opacity"];
		userMoz = es["-moz-opacity"];
		userOpacity = es.opacity;
		userFilter = es.filter;
		userZoom = es.zoom;
		userClip = es.clip;
	}

	/*void*/ function restore() {
		es.overflow = userOverflow;
		es.position = userPosition;
		es.display = userDisplay;
		es.visibility = userVisibility;
		es.width = userWidth;
		es.height = userHeight;
		es.top = userTop;
		es.left = userLeft;
		es["-khtml-opacity"] = userKhtml;
		es["-moz-opacity"] = userMoz;
		es.opacity = userOpacity;
		try {
			if (userFilter) {
				es.filter = userFilter;
			}
		} catch (ex) {}
		es.zoom = userZoom;
		es.clip = userClip ? userClip : "rect(auto auto auto auto)";// works in IE/FireFox/Opera
	}

	/*void*/ function showElem() {
		if (!!JsonFx.UI.getStyle(elem, "display")) {
			es.display = "block";
		}
		if (!!JsonFx.UI.getStyle(elem, "visibility")) {
			es.visibility = "visible";
		}
	}

	/*void*/ function initAlpha() {
		if (elem.filters && !alpha) {
			if (elem.filters.length > 0) {
				try {
					// check IE5.5+
					alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
				} catch (ex) { alpha = null; }
				if (!alpha) {
					try {
						// check IE4.0+
						alpha = elem.filters.item("alpha");
					} catch (ex) { alpha = null; }
				}
			}
			if (!alpha) {
				// try IE5.5+
				es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
				try {
					alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
				} catch (ex) { alpha = null; }
				if (!alpha) {
					// try IE4.0+
					es.filter += " alpha(enabled=false)";
					try {
						alpha = elem.filters.item("alpha");
					} catch (ex) { alpha = null; }
				}
			}
		}
	}

	/*bool*/ this.hasAppliedOp = function() {
		return state;
	};

	// if newOp is null, the previous operation is reversed
	// immediate doesn't animate but applies operation
	/*void*/ this.apply = function(/*JsonFx.UI.Animate.Op*/ newOp, /*bool*/ immediate) {
		if (!es) { return; }

		// state: true = perform op, false = reverse op
		state = !!newOp;
		immediate = !!immediate;
		if (mutex) {
			// crude concurrency check
			return;
		}
		mutex = true;

		if (state) {
			op = newOp;
		}
		if (!start) {
			start = JsonFx.UI.Animate.Op.save(elem);
		}
		showElem();

		// minimizeStep
		/*void*/ function t(/*float*/ step) {
			if (!es || isNaN(step)) {
				mutex = false;
				return;
			}

			var esPos = JsonFx.UI.getStyle(elem, "position");

			if (step < STEP_MIN) {
				step = STEP_MIN;
			} else if (step > STEP_MAX) {
				step = STEP_MAX;
			}

			if (op.hasScale() || op.hasClip()) {
				es.overflow = "hidden";
			}

			if (op.hasWidth() && start.hasWidth()) {
				es.width = JsonFx.UI.lerpInt(start.w, op.w, step)+"px";
			} else if (op.hasScaleX() && start.hasScaleX() && start.hasWidth()) {
				es.width = Math.ceil(start.w*JsonFx.UI.lerp(start.x, op.x, step))+"px";
			}
			if (op.hasHeight() && start.hasHeight()) {
				es.height = JsonFx.UI.lerpInt(start.h, op.h, step)+"px";
			} else if (op.hasScaleY() && start.hasScaleY() && start.hasHeight()) {
				es.height = Math.ceil(start.h*JsonFx.UI.lerp(start.y, op.y, step))+"px";
			}
			if (op.hasFade() && start.hasFade()) {
				// opacity
				es["-khtml-opacity"] = es["-moz-opacity"] = es.opacity = JsonFx.UI.lerp(start.f, op.f, step);
				initAlpha();
				if (alpha) {
					try {
						alpha.opacity = JsonFx.UI.lerpInt(100*start.f, 100*op.f, step);
						alpha.enabled = true;
					} catch (ex) {
						alpha = null;
					}
				}
			}
			if (op.hasZoom() && start.hasZoom()) {
				es.zoom = JsonFx.UI.lerpInt(100*start.z, 100*op.z, step)+"%";
			}
			if (op.hasClip()) {
				var clip = ["auto","auto","auto","auto"];
				if (op.hasClipT() && start.hasClipT()) {
					clip[0] = Math.ceil(start.h*JsonFx.UI.lerp(start.cT, op.cT, step))+"px";
				}
				if (op.hasClipR() && start.hasClipR()) {
					clip[1] = Math.floor(start.w*JsonFx.UI.lerp(1-start.cR, 1-op.cR, step))+"px";
				}
				if (op.hasClipB() && start.hasClipB()) {
					clip[2] = Math.floor(start.h*JsonFx.UI.lerp(1-start.cB, 1-op.cB, step))+"px";
				}
				if (op.hasClipL() && start.hasClipL()) {
					clip[3] = Math.ceil(start.w*JsonFx.UI.lerp(start.cL, op.cL, step))+"px";
				}
				if (esPos !== "fixed") {
					es.position = "absolute";
				}
				es.clip = "rect("+clip.join(' ')+")";
			}
			if (op.hasMove()) {
				if (!esPos || esPos === "static") {
					es.position = "relative";
				}
				if (op.hasTop() && start.hasTop()) {
					es.top = JsonFx.UI.lerpInt(start.t, op.t, step)+"px";
				}
				if (op.hasLeft() && start.hasLeft()) {
					es.left = JsonFx.UI.lerpInt(start.l, op.l, step)+"px";
				}
			}

			if (step <= STEP_MIN && !state) {
				setTimeout(
					function() {
						restore();
						start = null;
						op = new JsonFx.UI.Animate.Op();
						mutex = false;
					},
					0);
			} else if (step >= STEP_MAX && state) {
				setTimeout(
					function() {
						if (op.isRemoved()) {
							es.display = "none";
						} else if (op.isHidden()) {
							es.visibility = "hidden";
						}
						mutex = false;
					},
					0);
			} else {
				setTimeout(
					function() {
						t(state ? (step+op.s) : (step-op.s));
					},
					0);
			}
		}

		// XOR: state OR immediate but not both
		// immediate forces to far end of step
		t(state^immediate ? STEP_MIN : STEP_MAX);
	};
};
