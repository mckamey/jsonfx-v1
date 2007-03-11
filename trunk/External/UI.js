/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-03-02-0612
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
JsonFx.UI = {};

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*element*/ elem, /*string*/ style) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	if (elem.currentStyle) {
		// IE only
		if (style === "float") {
			style = "styleFloat";
		} else {
			// convert property name to camelCase
			style = style.split('-');
			style[0] = style[0].toLowerCase();
			for (var i=1; i<style.length; i++) {
				style[i] = style[i].charAt(0).toUpperCase()+style[i].substr(1).toLowerCase();
			}
			style = style.join("");
		}
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

/*void*/ JsonFx.UI.clearTextSelection = function() {
	if (window.getSelection && window.getSelection().removeAllRanges) {
		window.getSelection().removeAllRanges();
	} else if (document.getSelection && document.getSelection().empty) {
		document.getSelection().empty();
	} else if (document.selection && document.selection.empty) {
		document.selection.empty();
	}
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

/*element*/ JsonFx.UI.getIFrameDocument = function(/*element*/ elem) {
	if (!elem) {
		return null;
	}

	return ("undefined" !== typeof elem.contentDocument) ?
		elem.contentDocument : elem.contentWindow.document;
};

/*string*/ JsonFx.UI.toHtmlColor = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("Color components need to be numbers from 0x00 to 0xFF");
	}

	/*int*/ var c = (((Math.floor(r)<<8)+Math.floor(g))<<8)+Math.floor(b);

	/*string*/ var hex = "";
	for (var i=0; i<6; i++) {
		hex = Math.floor(c % 0x10).toString(16) + hex;
		c = Math.floor(c / 0x10);
	}

	return "#"+hex;
};

/*{r,g,b}*/ JsonFx.UI.fromHtmlColor = function(/*string*/ hex) {
	if (hex) {
		if (hex.match(/#([0-9a-fA-F]{1,2})([0-9a-fA-F]{1,2})([0-9a-fA-F]{1,2})/)) {// HEX colors
			if (hex.length === 7) {
				// e.g. "#336699"
				return {
						"r":Number("0x"+RegExp.$1),
						"g":Number("0x"+RegExp.$2),
						"b":Number("0x"+RegExp.$3)
					};
			} else if (hex.length === 4) {
				// e.g. "#369" === "#336699"
				return {
						"r":Number("0x"+RegExp.$1+RegExp.$1),
						"g":Number("0x"+RegExp.$2+RegExp.$2),
						"b":Number("0x"+RegExp.$3+RegExp.$3)
					};
			}
		}
		if (hex.match(/rgb[\(](\d+),\s*(\d+),\s*(\d+)[\)]/)) {// Firefox colors
			return {
					"r":Number(RegExp.$1),
					"g":Number(RegExp.$2),
					"b":Number(RegExp.$3)
				};
		}
	}
	return {"r":NaN, "g":NaN, "b":NaN};
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

//TIMER
//JsonFx.Timer.start(a+"_one");
//TIMER

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

//TIMER
//JsonFx.Timer.stop(a+"_one", true);//48/16,46/31,62/0
//TIMER

		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {

//TIMER
//JsonFx.Timer.start(a+"_all");
//TIMER
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

//TIMER
//JsonFx.Timer.stop(a+"_all", true);//32,31,31
//TIMER
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		return performOne(elem, BIND);
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
			perform(document, BIND);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl ? ttl : "";
				document.body.style.cursor = crsr ? crsr : "";
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
			perform(document, UNBIND);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl ? ttl : "";
				document.body.style.cursor = crsr ? crsr : "";
			}
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
	History Methods
\*------------------*/

/* singleton JsonFx.UI.History */
JsonFx.UI.History = {};

JsonFx.UI.History.h = null;
JsonFx.UI.History.onchange = null;

/*void*/ JsonFx.UI.History.add = function(/*object*/ info) {
	var h = JsonFx.UI.getIFrameDocument(JsonFx.UI.History.h);
	if (h) {
		h.location.search = encodeURIComponent(info.toJSONString());
	}
};

/*void*/ JsonFx.UI.History.changed = function(/*element*/ elem) {
	if (!JsonFx.UI.History.h) {
		JsonFx.UI.History.h = elem;
	}
	var h = JsonFx.UI.getIFrameDocument(elem);
	if (h) {
		var info = h.location.search;
		if (info) {
			if (info.charAt(0) === '?') {
				info = info.substring(1);
			}
			info = decodeURIComponent(info).parseJSON();
			if ("function" === typeof JsonFx.UI.History.onchange) {
				JsonFx.UI.History.onchange(info);
			}
		}
	}
};

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

//TIMER
//JsonFx.Timer.start("display");
//TIMER

	// either DOM element or id
	container = ("string" === typeof container) ?
		document.getElementById(container) : container;

	if (jml && ("function" === typeof jml.parseJsonML)) {
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
//TIMER
//JsonFx.Timer.stop("display", true);//265,266,266
//TIMER
	return container;
};

/* returns true if request was sent */
/*bool*/ JsonFx.UI.loadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {

//TIMER
//JsonFx.Timer.start("load");
//TIMER

	return JsonFx.IO.getJsonRequest(url, null, null,
			function(jml,obj) {

//TIMER
//JsonFx.Timer.stop("load", true);//282,281,22750(greedy regex)
//TIMER
				JsonFx.UI.displayJsonML(jml, container);
				if ("function" === typeof callback) { callback(context); }
			}
		, null);
};

/* DataDump ----------------------------------------------------*/

/*bool*/ JsonFx.UI.expando = function (/*element*/ elem, /*string*/ target) {
	var expando = elem.expando;
	if (!expando) {
		if ("string" === typeof target) {
			target = document.getElementById(target);
		}
		if (!target) {
			return false;
		}
		elem.expando = expando = new JsonFx.UI.Animate.Engine(target);
	}
	if (expando.hasAppliedOp()) {
		if (elem.value) {// hacky swap out for buttons
			elem.value = " \u2212 ";
			//elem.replaceChild(document.createTextNode("\u2212"), elem.firstChild);
		}
		expando.apply(null, false);
		return false;
	} else {
		var op = new JsonFx.UI.Animate.Op();
		op.fade(0);
		op.height(0);
		op.speed(0.65);
		if (elem.value) {// hacky swap out for buttons
			elem.value = " + ";
			//elem.replaceChild(document.createTextNode("+"), elem.firstChild);
		}
		expando.apply(op, false);
		return true;
	}
};

/*void*/ JsonFx.UI.expandoBind = function(/*element*/ elem) {
	// call after elements have been added to document
	window.setTimeout(
		function() {
			var target = document.getElementById(elem.getAttribute("for"));
			if (!target) {
				target = elem.nextSibling;
				while (target && !target.tagName) {
					target = target.nextSibling;
				}
			}
			if (target) {
				elem.style.cursor = "pointer";
				elem.className += " jsonfx-expanded";

				elem.onclick = function (/*event*/ evt) {
					if (JsonFx.UI.expando(elem, target)) {
						elem.className = elem.className.replace(/\s*jsonfx-expanded/g, " jsonfx-collapsed");
					} else {
						elem.className = elem.className.replace(/\s*jsonfx-collapsed/g, " jsonfx-expanded");
					}
					return false;
				};
				elem.ondblclick = function (/*event*/ evt) {
					if (elem.click) {
						elem.click();
					}
					window.setTimeout( function(){JsonFx.UI.clearTextSelection();}, 0);
					return false;
				};
			}
		}, 0);
};
/*void*/ JsonFx.UI.expandoUnbind = function(/*element*/ elem) {
	if ("undefined" !== typeof elem.expando) {
		elem.expando = null;
	}
	elem.onclick = null;
	elem.ondblclick = null;
};

/*int*/ JsonFx.UI.dumpDataID = 0;
/*JsonML*/ JsonFx.UI.dumpData = function(/*json*/ data) {
	if (data === null) {
		return "null";
	}
	var ul = ["ul", {"id":"JsonFx_UI_Dump_"+(JsonFx.UI.dumpDataID++),"class":"jsonfx-object"}];

	for (var pn in data) {
		if (!data.hasOwnProperty(pn)) {
			continue;
		}
		var pv = data[pn];
		var pt = typeof pv;

		var li = ["li"];
		var a = null;
		if ("object" === pt && pv) {
			a = ["label", {"class":"jsonfx-expando"}];
			li.push(a);
		}
		(a?a:li).push(["span", {"class":"jsonfx-type"}, (pv instanceof Array) ? "array" : pt]);
		(a?a:li).push(["span", {"class":"jsonfx-name"}, pn]);

		if ("object" === pt) {
			var o = JsonFx.UI.dumpData(pv);
			if (a && o[1].id) {
				a[1]["for"] = o[1].id;
			}
			li.push(o);
		} else {
			li.push(["span", {"class":"jsonfx-value"}, String(pv)]);
		}
		ul.push(li);
	}

	return ul;
};

/*---------------------*\
	Animation Classes
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
	this.x = this.y = this.w = this.h =
	this.z = this.t = this.r = this.b = this.l = 
	this.cL = this.cR = this.cT = this.cB =
	this.f = this.bl = NaN;
	this.c = { "r":NaN, "g":NaN, "b":NaN };
	this.bc = { "r":NaN, "g":NaN, "b":NaN };
	this.s = 0.05;// 20 steps
};

/*Regex*/ JsonFx.UI.Animate.Op.clipRE = /^rect[\(]([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)[\)]$/;

/*void*/ JsonFx.UI.Animate.Op.prototype.fade = function(/*float*/ f) {
	if (!isFinite(f) || f<0 || f>1) {
		throw new Error("Fade is a number from 0.0 to 1.0");
	}
	this.f = Number(f);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasFade = function() {
	return isFinite(this.f);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.blur = function(/*float*/ b) {
	if (!isFinite(b) || b<0) {
		throw new Error("Blur is a number greater than 0");
	}
	this.bl = Number(b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBlur = function() {
	return isFinite(this.bl);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.color = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("Color is a triplet of numbers from 0x00 to 0xFF");
	}
	this.c.r = Number(r);
	this.c.g = Number(g);
	this.c.b = Number(b);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.colorHtml = function(/*string*/ c) {
	c = JsonFx.UI.fromHtmlColor(c);
	this.color(c.r, c.g, c.b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasColor = function() {
	return !!this.c &&
		isFinite(this.c.r) &&
		isFinite(this.c.g) &&
		isFinite(this.c.b);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.backgroundColor = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("BackgroundColor is a triplet of numbers from 0x00 to 0xFF");
	}
	this.bc.r = Number(r);
	this.bc.g = Number(g);
	this.bc.b = Number(b);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.backgroundColorHtml = function(/*string*/ c) {
	c = JsonFx.UI.fromHtmlColor(c);
	this.backgroundColor(c.r, c.g, c.b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBackgroundColor = function() {
	return !!this.bc &&
		isFinite(this.bc.r) &&
		isFinite(this.bc.g) &&
		isFinite(this.bc.b);
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
/*void*/ JsonFx.UI.Animate.Op.prototype.top = function(/*int*/ top) {
	if (!isFinite(top)) {
		throw new Error("Top is a pixel position");
	}
	this.t = Number(top);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.right = function(/*int*/ right) {
	if (!isFinite(right)) {
		throw new Error("Right is a pixel position");
	}
	this.r = Number(right);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.bottom = function(/*int*/ bottom) {
	if (!isFinite(bottom)) {
		throw new Error("Bottom is a pixel position");
	}
	this.b = Number(bottom);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.left = function(/*int*/ left) {
	if (!isFinite(left)) {
		throw new Error("Left is a pixel position");
	}
	this.l = Number(left);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasMove = function() {
	return isFinite(this.t)||isFinite(this.r)||isFinite(this.b)||isFinite(this.l);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasTop = function() {
	return isFinite(this.t);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasRight = function() {
	return isFinite(this.r);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBottom = function() {
	return isFinite(this.b);
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

/*JsonFx.UI.Animate.Op*/ JsonFx.UI.Animate.Op.save = function(/*element*/ elem) {
	var op = new JsonFx.UI.Animate.Op();

	if (elem && elem.style) {
		var es = elem.style;
		var esPos = JsonFx.UI.getStyle(elem, "position");

		var top = parseFloat(JsonFx.UI.getStyle(elem, "top"));
		if (isFinite(top)) {
			op.top(top);
		} else if (esPos === "static") {
			op.top(0);
		}
		var right = parseFloat(JsonFx.UI.getStyle(elem, "right"));
		if (isFinite(right)) {
			op.right(right);
		} else if (esPos === "static") {
			op.right(0);
		}
		var bottom = parseFloat(JsonFx.UI.getStyle(elem, "bottom"));
		if (isFinite(bottom)) {
			op.right(bottom);
		} else if (esPos === "static") {
			op.bottom(0);
		}
		var left = parseFloat(JsonFx.UI.getStyle(elem, "left"));
		if (isFinite(left)) {
			op.left(left);
		} else if (esPos === "static") {
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
		if (!!es.opacity && isFinite(es.opacity)) {
			op.fade(es.opacity);
		} else {
			op.fade(1);
		}

		// blur
		//warning: should extract blur?
		op.blur(0);

		// zoom
		if (!!es.zoom && isFinite(es.zoom)) {
			op.zoom(es.zoom);
		} else {
			op.zoom(1);
		}

		// color
		var color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "color"));
		if (isFinite(color.r) && isFinite(color.g) && isFinite(color.b)) {
			op.color(color.r, color.g, color.b);
		} else {
			op.color(0, 0, 0);
		}

		// backgroundColor
		color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "background-color"));
		if (isFinite(color.r) && isFinite(color.g) && isFinite(color.b)) {
			op.backgroundColor(color.r, color.g, color.b);
		} else {
			op.backgroundColor(0xFF, 0xFF, 0xFF);
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

/* class JsonFx.UI.Animate.Engine -------------------------------------------- */
JsonFx.UI.Animate.Engine = function(/*element*/ elem) {

	if ("string" === typeof elem) {
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
		userRight = "",
		userBottom = "",
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
		userColor = "",
		userBGColor = "",
		alpha = null,
		blur = null;

	if (elem && es) {
		userOverflow = es.overflow;
		userPosition = es.position;
		userDisplay = es.display;
		userVisibility = es.visibility;
		userHeight = es.height;
		userWidth = es.width;
		userTop = es.top;
		userRight = es.right;
		userBottom = es.bottom;
		userLeft = es.left;
		userKhtml = es["-khtml-opacity"];
		userMoz = es["-moz-opacity"];
		userOpacity = es.opacity;
		userFilter = es.filter;
		userZoom = es.zoom;
		userClip = es.clip;
		userColor = es.color;
		userBGColor = es.backgroundColor;
	}

	/*void*/ function restore() {
//		if (!!JsonFx.UI.getStyle(elem, "display")) {
//			es.display = userDisplay;
//		}
//		if (!!JsonFx.UI.getStyle(elem, "visibility")) {
//			es.visibility = userVisibility;
//		}
		if (op.hasScale() || op.hasClip()) {
			es.overflow = userOverflow;
		}
		if (op.hasWidth()) {
			es.width = userWidth;
		}
		if (op.hasHeight()) {
			es.height = userHeight;
		}
		if (op.hasMove()) {
			es.position = userPosition;
			es.top = userTop;
			es.right = userRight;
			es.bottom = userBottom;
			es.left = userLeft;
		}
		if (op.hasFade()) {
			es["-khtml-opacity"] = userKhtml;
			es["-moz-opacity"] = userMoz;
			es.opacity = userOpacity;
			try {
				if (userFilter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasBlur()) {
			try {
				if (userFilter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasZoom()) {
			es.zoom = userZoom;
		}
		if (op.hasClip()) {
			es.clip = userClip ? userClip : "rect(auto auto auto auto)";// tested in IE/FireFox/Opera
		}
		if (op.hasColor()) {
			es.color = userColor;
		}
		if (op.hasBackgroundColor()) {
			es.backgroundColor = userBGColor;
		}
	}

	/*void*/ function showElem() {
		if (!!JsonFx.UI.getStyle(elem, "display")) {
			es.display = "block";
		}
		if (!!JsonFx.UI.getStyle(elem, "visibility")) {
			es.visibility = "visible";
		}
	}

	/*void*/ function initFilter(op) {
		try {
			if (elem.filters) {
				if (op.hasFade() && !alpha) {
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
				if (op.hasBlur() && !blur) {
					if (elem.filters.length > 0) {
						try {
							// check IE5.5+
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex) { blur = null; }
						if (!blur) {
							try {
								// check IE4.0+
								blur = elem.filters.item("blur");
							} catch (ex) { blur = null; }
						}
					}
					if (!blur) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Blur(enabled=false)";
						try {
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex) { blur = null; }
						if (!blur) {
							// try IE4.0+
							es.filter += " blur(enabled=false)";
							try {
								blur = elem.filters.item("blur");
							} catch (ex) { blur = null; }
						}
					}
				}
			}
		} catch (ex) {}
	}

	/*bool*/ this.hasAppliedOp = function() {
		return state;
	};

	// if newOp is null, the previous operation is reversed
	// immediate doesn't animate but applies operation
	/*void*/ this.apply = function(/*JsonFx.UI.Animate.Op*/ newOp, /*bool*/ immediate, /*function(cx)*/ callback, /*object*/ context) {
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
				initFilter(op);
				if (alpha) {
					try {
						// this might have side-effects, but should be rare
						if (!elem.currentStyle.hasLayout) {
							es.zoom = "100%";
						}
						alpha.opacity = JsonFx.UI.lerpInt(100*start.f, 100*op.f, step);
						alpha.enabled = true;
					} catch (ex) {
						alpha = null;
					}
				}
			}
			if (op.hasBlur()) {
				initFilter(op);
				if (blur) {
					try {
						// this might have side-effects, but should be rare
						if (!elem.currentStyle.hasLayout) {
							es.zoom = "100%";
						}
						blur.pixelRadius = JsonFx.UI.lerpInt(start.bl, op.bl, step);
						blur.enabled = true;
					} catch (ex) {
						blur = null;
					}
				}
			}
			if (op.hasZoom() && start.hasZoom()) {
				es.zoom = JsonFx.UI.lerpInt(100*start.z, 100*op.z, step)+"%";
			}
			if (op.hasBackgroundColor() && start.hasBackgroundColor()) {
				es.backgroundColor = JsonFx.UI.toHtmlColor(
					JsonFx.UI.lerpInt(start.bc.r, op.bc.r, step),
					JsonFx.UI.lerpInt(start.bc.g, op.bc.g, step),
					JsonFx.UI.lerpInt(start.bc.b, op.bc.b, step)
				);
			}
			if (op.hasColor() && start.hasColor()) {
				es.color = JsonFx.UI.toHtmlColor(
					JsonFx.UI.lerpInt(start.c.r, op.c.r, step),
					JsonFx.UI.lerpInt(start.c.g, op.c.g, step),
					JsonFx.UI.lerpInt(start.c.b, op.c.b, step)
				);
			}
			if (op.hasClip()) {
				var clip = ["auto","auto","auto","auto"];
				if (op.hasClipT() && start.hasClipT()) {
					clip[0] = Math.ceil(JsonFx.UI.lerp(start.cT, op.cT, step)*100)+"%";
				}
				if (op.hasClipR() && start.hasClipR()) {
					clip[1] = Math.floor(JsonFx.UI.lerp(1-start.cR, 1-op.cR, step)*100)+"%";
				}
				if (op.hasClipB() && start.hasClipB()) {
					clip[2] = Math.floor(JsonFx.UI.lerp(1-start.cB, 1-op.cB, step)*100)+"%";
				}
				if (op.hasClipL() && start.hasClipL()) {
					clip[3] = Math.ceil(JsonFx.UI.lerp(start.cL, op.cL, step)*100)+"%";
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
				if (op.hasRight() && start.hasRight()) {
					es.right = JsonFx.UI.lerpInt(start.r, op.r, step)+"px";
				}
				if (op.hasBottom() && start.hasBottom()) {
					es.bottom = JsonFx.UI.lerpInt(start.b, op.b, step)+"px";
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

						if ("function" === typeof callback) {
							callback(context);
						}
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

						if ("function" === typeof callback) {
							callback(context);
						}
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
