/*extern JsonFx, JSON, JsonML */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-05-25-2253
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*DOM*/ elem, /*string*/ style) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	var i;
	if (elem.currentStyle) {
		// IE only
		if (style === "float") {
			style = "styleFloat";
		} else {
			// convert property name to camelCase
			style = style.split('-');
			style[0] = style[0].toLowerCase();
			for (i=1; i<style.length; i++) {
				style[i] = style[i].charAt(0).toUpperCase()+style[i].substr(1).toLowerCase();
			}
			style = style.join("");
		}
		return elem.currentStyle[style];
	}
	return null;
};

/*bool*/ JsonFx.UI.hasClassName = function(/*DOM*/ elem, /*string*/ cssClass) {
	return !!cssClass &&
		!!elem &&
		!!elem.className &&
		!!elem.className.match("\\b"+cssClass+"\\b");
};

/*DOM*/ JsonFx.UI.debugPoint = function (/*int*/ x, /*int*/ y, /*{delay,color,size,className}*/ options) {
	if (isNaN(x)) {
		x = 0;
	}
	if (isNaN(y)) {
		y = 0;
	}
	options = options || {};
	options.delay = options.delay || 1500;
	if (!options.className) {
		options.size = options.size || "4px";
		options.color = options.color || "magenta";
	}

	var pt = document.createElement("div");
	pt.style.position = "absolute";
	pt.style.left = (x-pt.offsetWidth)+"px";
	pt.style.top = (y-pt.offsetHeight)+"px";
	pt.title = "("+x+","+y+")";
	pt.className = options.className;
	if (options.size) {
		pt.style.height = pt.style.width = options.size;
		pt.style.overflow = "hidden";
	}
	document.body.appendChild(pt);

	if (options.delay > 0) {
		window.setTimeout(
			function() {
				if (pt) {
					pt.parentNode.removeChild(pt);
					pt = null;
				}
			},
			options.delay);
	}
	options = null;
	return pt;
};

/*{x,y}*/ JsonFx.UI.getScroll = function(/*element*/ elem) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	var pt = {x:0, y:0};
	elem = elem && elem.parentNode;
	while (elem) {
		if (!isNaN(elem.scrollTop)) {
			pt.y += elem.scrollTop;
		}
		if (!isNaN(elem.scrollLeft)) {
			pt.x += elem.scrollLeft;
		}
		elem = elem.parentNode;
	}
	return pt;
};

/*{x,y}*/ JsonFx.UI.getOffset = function(/*element*/ elem, /*bool*/ skipScroll) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}

	var pos,
		pt = skipScroll ? {x:0,y:0} : JsonFx.UI.getScroll(elem);

	while (elem) {
		pt.y += elem.offsetTop;
		pt.x += elem.offsetLeft;
		elem = elem.offsetParent;
//		if (elem) {
//			pos = JsonFx.UI.getStyle(elem, "position");
//			if (pos !== "static") {
//				elem = null;
//			}
//		}
	}
	return pt;
};

/*{x,y}*/ JsonFx.UI.getMidPoint = function(/*element*/ elem) {
	if (!elem) {
		return null;
	}
	var pt = JsonFx.UI.getOffset(elem, true);
	pt.x += Math.floor(0.5+elem.offsetWidth/2);
	pt.y += Math.floor(0.5+elem.offsetHeight/2);
	return pt;
};

/*{x,y}*/ JsonFx.UI.pointAdd = function(/*{x,y}*/ a, /*{x,y}*/ b) {
	if (!b) {
		return a;
	}
	if (!a) {
		return b;
	}
	return { x:(a.x+b.x), y:(a.y+b.y) };
};

/*{x,y}*/ JsonFx.UI.pointSubtract = function(/*{x,y}*/ a, /*{x,y}*/ b) {
	if (!b) {
		return a;
	}
	if (!a) {
		return b;
	}
	return { x:(a.x-b.x), y:(a.y-b.y) };
};

/*element*/ JsonFx.UI.getEventTarget = function(/*Event*/ evt) {
	evt = evt || window.event || {};
	return (evt.currentTarget || evt.srcElement || null);
};

/*{x,y}*/ JsonFx.UI.getEventPoint = function(/*Event*/ evt) {
	evt = evt || window.event || { clientX:-Infinity, clientY:-Infinity };
	if (typeof evt.pageX !== "undefined") {
		return { x:evt.pageX, y:evt.pageY };
	}
	var pt = JsonFx.UI.getScroll(JsonFx.UI.getEventTarget(evt));
	return JsonFx.UI.pointAdd(pt, { x:evt.clientX, y:evt.clientY });
};

/*{left,middle,right}*/ JsonFx.UI.getMouseButton = function(/*Event*/ evt) {
	evt = evt || window.event;
	var b, btn = {left:false,middle:false,right:false};

	if (evt) {
		if (isNaN(evt.which)) {
			// IE (can be multiple)
			b = evt.button;
			if ((b&1)!==0) {
				btn.left = true;
			}
			if ((b&2)!==0) {
				btn.right = true;
			}
			if ((b&4)!==0) {
				btn.middle = true;
			}
		} else {
			// Firefox, Opera, Safari
			switch (evt.which){
				case 1:
					btn.left = true;
					break;
				case 2:
					btn.middle = true;
					break;
				case 3:
					btn.right = true;
					break;
			}
		}
	}
	return btn;
};

/*void*/ JsonFx.UI.cancelEvent = function(/*Event*/ evt) {
	evt = evt || window.event || {};
	if (evt) {
		evt.cancelBubble = true;
	}
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

/*function*/ JsonFx.UI.combineHandlers = function (/*function*/ a, /*function*/ b) {
	if ("function" !== typeof b) {
		return a;
	}

	if ("function" !== typeof a) {
		return b;
	}

	return function(/*Event*/ evt) { a(evt); return b(evt); };
};

/*void*/ JsonFx.UI.attachHandler = function (/*DOM*/ obj, /*string*/ evtName, /*function*/ handler) {
	if (obj.addEventListener) {
		//DOM method for binding an event
		obj.addEventListener(evtName, handler, false);
	} else if (obj.attachEvent) {
		//IE exclusive method for binding an event
		obj.attachEvent("on"+evtName, handler);
	} else if ("function" === typeof obj["on"+evtName]) {
		obj["on"+evtName] = JsonFx.UI.combineHandlers(handler, obj["on"+evtName]);
	} else {
		obj["on"+evtName] = handler;
	}
};

/*void*/ JsonFx.UI.detachHandler = function (/*DOM*/ obj, /*string*/ evtName, /*function*/ handler) {
	if (obj.removeEventListener) {
		//DOM method for unbinding an event
		obj.removeEventListener(evtName, handler, false);
	} else if (obj.detachEvent) {
		//IE exclusive method for binding an event
		obj.detachEvent("on"+evtName, handler);
	} else if (handler === obj["on"+evtName]) {
		obj["on"+evtName] = null;
	} else {
		// not sure how to remove this kind...
	}
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
						r:Number("0x"+RegExp.$1),
						g:Number("0x"+RegExp.$2),
						b:Number("0x"+RegExp.$3)
					};
			} else if (hex.length === 4) {
				// e.g. "#369" === "#336699"
				return {
						r:Number("0x"+RegExp.$1+RegExp.$1),
						g:Number("0x"+RegExp.$2+RegExp.$2),
						b:Number("0x"+RegExp.$3+RegExp.$3)
					};
			}
		}
		if (hex.match(/rgb[\(](\d+),\s*(\d+),\s*(\d+)[\)]/)) {// Firefox colors
			return {
					r:Number(RegExp.$1),
					g:Number(RegExp.$2),
					b:Number(RegExp.$3)
				};
		}
	}
	return {r:NaN, g:NaN, b:NaN};
};

/*float*/ JsonFx.UI.bound = function (/*float*/ start, /*float*/ end, /*float*/ val) {
	// don't allow val to exceed bounds
	if (start < end) {
		val = Math.min(val, end);
		val = Math.max(val, start);
	} else {
		val = Math.min(val, start);
		val = Math.max(val, end);
	}
	return val;
};

/* Linear Interpolate */
/*float*/ JsonFx.UI.lerp = function (/*float*/ a, /*float*/ b, /*float*/ t, /*bool*/ bounded) {
	var val = (a * (1-t)) + (b * t);
	return bounded ? JsonFx.UI.bound(a, b, val) : val;
};

/* Integer Linear Interpolation */
/*int*/ JsonFx.UI.lerpInt = function (/*int*/ a, /*int*/ b, /*float*/ t, /*bool*/ bounded) {
	return Math.floor( 0.5 + JsonFx.UI.lerp(a, b, t, bounded) );
};

/* Reverse Linear Interpolate */
/*float*/ JsonFx.UI.revLerp = function (/*float*/ a, /*float*/ b, /*float*/ t, /*bool*/ bounded) {
	var val = (t-a)/(b-a);
	return bounded ? JsonFx.UI.bound(0, 1, val) : val;
};

/* Cubic Bezier Curve */
/*float*/ JsonFx.UI.bezier = function (/*float*/ a, /*float*/ b, /*float*/ c, /*float*/ d, /*float*/ t) {
	return a*Math.pow(1-t,3) + b*3*t*Math.pow(1-t,2) + c*3*Math.pow(t,2)*(1-t) + d*Math.pow(t,3);
};

/* Cubic Bezier Curve */
/*{x,y}*/ JsonFx.UI.bezierPoint = function (/*{x,y}*/ p0, /*{x,y}*/ p1, /*{x,y}*/ p2, /*{x,y}*/ p3, /*float*/ t) {
	return {x:JsonFx.UI.bezier(p0.x, p1.x, p2.x, p3.x, t), y:JsonFx.UI.bezier(p0.y, p1.y, p2.y, p3.y, t)};
};

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

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
			/* NOTE: using JsonFx.UI.attachHandler makes it hard to control handler order */

			// wire up binding
			if ("function" === typeof window.onload) {
				window.onload = JsonFx.UI.combineHandlers(b.bindAll, window.onload);
			} else {
				window.onload = b.bindAll;
			}

			// wire up unbinding
			if ("function" === typeof window.onunload) {
				window.onunload = JsonFx.UI.combineHandlers(b.unbindAll, window.onunload);
			} else {
				window.onunload = b.unbindAll;
			}
		}, 0);
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

/*	creates DOM elements from JsonML and binds them accordingly */
/*element*/ JsonFx.UI.bindJsonML = function(/*JsonML*/ jml) {
	if (jml) {
		return JsonML.parse(jml, JsonFx.UI.Bindings.bindOne);
	} else {
		return null;
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*element*/ JsonFx.UI.displayJsonML = function(/*JsonML*/ jml, /*element|string*/ container) {

	// either DOM element or id
	container = ("string" === typeof container) ?
		document.getElementById(container) : container;

	jml = JsonFx.UI.bindJsonML(jml);
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
	return container;
};

/*void*/ JsonFx.UI.loadJsonML = function (
	/*string*/ url,
	/*element|string*/ container,
	/*RequestOptions*/ options) {

	options = JsonFx.IO.validateOptions(options);

	var onCreate;
	if (options.onCreate) {
		onCreate = options.onCreate;
		options.onCreate = function (/*JSON*/ jml, /*object*/ cx) {
			// create callback
			onCreate(cx);

			// free closure references
			onCreate = null;
		};
	}

	var onSuccess = options.onSuccess;
	options.onSuccess = function (/*JSON*/ jml, /*object*/ cx) {
		// display UI
		JsonFx.UI.displayJsonML(jml, container);

		// success callback
		if (onSuccess) { onSuccess(cx); }

		// free closure references
		onSuccess = container = null;
	};

	var onFailure;
	if (options.onFailure) {
		onFailure = options.onFailure;
		options.onFailure = function (/*JSON*/ jml, /*object*/ cx, /*Error*/ ex) {
			ex.$error = jml;
		
			// failure callback
			onFailure(cx, ex);

			// free closure references
			onFailure = null;
		};
	}

	var onTimeout;
	if (options.onTimeout) {
		onTimeout = options.onTimeout;
		options.onTimeout = function (/*JSON*/ jml, /*object*/ cx, /*Error*/ ex) {
			// timeout callback
			onTimeout(cx, ex);

			// free closure references
			onTimeout = null;
		};
	}

	var onComplete;
	if (options.onComplete) {
		onComplete = options.onComplete;
		options.onComplete = function (/*JSON*/ jml, /*object*/ cx) {
			// complete callback
			onComplete(cx);

			// free closure references
			onComplete = 1;
		};
	}

	JsonFx.IO.sendJsonRequest(url, options);
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
	this.c = { r:NaN, g:NaN, b:NaN };
	this.bc = { r:NaN, g:NaN, b:NaN };
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
	var op = new JsonFx.UI.Animate.Op(), es, esPos, top, right, bottom, left, width, height, color, clip;

	if (elem && elem.style) {
		es = elem.style;
		esPos = JsonFx.UI.getStyle(elem, "position");

		top = parseFloat(JsonFx.UI.getStyle(elem, "top"));
		if (isFinite(top)) {
			op.top(top);
		} else if (esPos === "static") {
			op.top(0);
		}
		right = parseFloat(JsonFx.UI.getStyle(elem, "right"));
		if (isFinite(right)) {
			op.right(right);
		} else if (esPos === "static") {
			op.right(0);
		}
		bottom = parseFloat(JsonFx.UI.getStyle(elem, "bottom"));
		if (isFinite(bottom)) {
			op.right(bottom);
		} else if (esPos === "static") {
			op.bottom(0);
		}
		left = parseFloat(JsonFx.UI.getStyle(elem, "left"));
		if (isFinite(left)) {
			op.left(left);
		} else if (esPos === "static") {
			op.left(0);
		}

		// width
		if (isFinite(elem.offsetWidth)) {
			op.width(elem.offsetWidth);
		} else {
			width = parseFloat(JsonFx.UI.getStyle(elem, "width"));
			if (isFinite(width)) {
				op.width(width);
			}
		}
		// height
		if (isFinite(elem.offsetHeight)) {
			op.height(elem.offsetHeight);
		} else {
			height = parseFloat(JsonFx.UI.getStyle(elem, "height"));
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
		color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "color"));
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


/*Dictionary<string,bool>*/ JsonFx.UI.Animate.filterable =
	{
		a : true,
		abbr : true,
		acronym : true,
		address : true,
		b : true,
		bdo : true,
		big : true,
		blockquote : true,
		body : true,
		button : true,
		caption : true,
		center : true,
		cite : true,
		code : true,
		custom : true,
		dd : true,
		del : true,
		dfn : true,
		dir : true,
		div : true,
		dl : true,
		dt : true,
		em : true,
		fieldset : true,
		font : true,
		form : true,
		frame : true,
		hn : true,
		iframe : true,
		frameset : true,
		i : true,
		ins : true,
		img : true,
		input : true,
		kbd : true,
		label : true,
		legend : true,
		li : true,
		marquee : true,
		menu : true,
		nobr : true,
		ol : true,
		p : true,
		plaintext : true,
		pre : true,
		q : true,
		rt : true,
		ruby : true,
		s : true,
		samp : true,
		small : true,
		span : true,
		strike : true,
		strong : true,
		sub : true,
		sup : true,
		table : true,
		textarea : true,
		th : true,
		td : true,
		tt : true,
		u : true,
		ul : true,
		"var" : true,
		xmp : true
	};

JsonFx.UI.Animate.Engine = function(/*element*/ elem) {

	if ("string" === typeof elem) {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	var tn = elem ? elem.tagName : null;

	if (!tn) {
		throw new Error("Invalid element");
	}

	tn = tn.toLowerCase();

	var es = elem.style,
	/*JsonFx.UI.Animate.Op*/ start = null,
	/*JsonFx.UI.Animate.Op*/ op = new JsonFx.UI.Animate.Op(),
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ STEP_MIN = 0, // start
	/*const float*/ STEP_MAX = 1, // end
	/*Dictionary<string,bool>*/ filterable = JsonFx.UI.Animate.filterable;

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
				if (es.filter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasBlur()) {
			try {
				if (es.filter) {
					es.filter = userFilter;
				}
			} catch (ex2) {}
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
		if (JsonFx.UI.getStyle(elem, "display") === "none") {
			es.display = "block";
		}
		if (JsonFx.UI.getStyle(elem, "visibility") === "hidden") {
			es.visibility = "visible";
		}
	}

	/*void*/ function initFilter(op) {
		try {
			// first look up tagName to determine if
			// element can even have filters applied
			// to prevent incurring calculation cost
			if (filterable[tn] &&
				elem.filters) {
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
							} catch (ex2) { alpha = null; }
						}
					}
					if (!alpha) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
						try {
							alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
						} catch (ex3) { alpha = null; }
						if (!alpha) {
							// try IE4.0+
							es.filter += " alpha(enabled=false)";
							try {
								alpha = elem.filters.item("alpha");
							} catch (ex4) { alpha = null; }
						}
					}
				}
				if (op.hasBlur() && !blur) {
					if (elem.filters.length > 0) {
						try {
							// check IE5.5+
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex5) { blur = null; }
						if (!blur) {
							try {
								// check IE4.0+
								blur = elem.filters.item("blur");
							} catch (ex6) { blur = null; }
						}
					}
					if (!blur) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Blur(enabled=false)";
						try {
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex7) { blur = null; }
						if (!blur) {
							// try IE4.0+
							es.filter += " blur(enabled=false)";
							try {
								blur = elem.filters.item("blur");
							} catch (ex8) { blur = null; }
						}
					}
				}
			}
		} catch (ex9) {}
	}

	/*bool*/ this.hasAppliedOp = function() {
		return state;
	};

	// if newOp is null, the previous operation is reversed
	// immediate doesn't animate but applies operation
	/*void*/ this.apply = function(
		/*JsonFx.UI.Animate.Op*/ newOp,
		/*bool*/ immediate,
		/*function(cx)*/ callback,
		/*object*/ context) {

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
						if (!elem.currentStyle.hasLayout) {
							// this might have side-effects, but should be rare
							// http://msdn.microsoft.com/workshop/author/dhtml/reference/properties/haslayout.asp
							// http://www.satzansatz.de/cssd/onhavinglayout.html
							es.zoom = "1";
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
						if (!elem.currentStyle.hasLayout) {
							// this might have side-effects, but should be rare
							// http://msdn.microsoft.com/workshop/author/dhtml/reference/properties/haslayout.asp
							// http://www.satzansatz.de/cssd/onhavinglayout.html
							es.zoom = "1";
						}
						blur.pixelRadius = JsonFx.UI.lerpInt(start.bl, op.bl, step);
						blur.enabled = true;
					} catch (ex2) {
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
			var clip;
			if (op.hasClip()) {
				clip = ["auto","auto","auto","auto"];
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
					40);// ~25Hz baseline, in case fast machine
			}
		}

		// XOR: state OR immediate but not both
		// immediate forces to far end of step
		t(state^immediate ? STEP_MIN : STEP_MAX);
	};
};
