/*global JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI Controls
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2007-06-03-1028
	Modified: 2007-06-03-1028
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*--------------------*\
	Expando-Collapso
\*--------------------*/

/*bool*/ JsonFx.UI.expando = function (/*element*/ elem, /*string*/ target) {
	var expando = elem.expando;
	if (!expando) {
		if ("string" === typeof target) {
			target = document.getElementById(target);
		}
		if (!target) {
			return false;
		}
		elem.expando = expando = new JsonFx.Effects.Engine(target);
	}
	var op;
	if (expando.hasAppliedOp()) {
		if (elem.value) {// hacky swap out for buttons
			elem.value = " \u2212 ";
			//elem.replaceChild(document.createTextNode("\u2212"), elem.firstChild);
		}
		expando.apply(null, false);
		return false;
	} else {
		op = new JsonFx.Effects.Op();
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

/*void*/ JsonFx.UI.expando_bind = function(/*element*/ elem) {
	// call after elements have been added to document
	var target = null;
	try {
		elem.style.cursor = "pointer";
	} catch (ex) {
		// IE 5.x
		elem.style.cursor = "hand";
	}
	elem.className += " jsonfx-expanded";

	elem.onclick = function (/*event*/ evt) {
		if (!target) {
			// using a closure so only lookup once
			target = document.getElementById(elem.getAttribute("for"));
			if (!target) {
				target = elem.nextSibling;
				while (target && !target.tagName) {
					target = target.nextSibling;
				}
			}
		}

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
};

/*void*/ JsonFx.UI.expando_unbind = function(/*element*/ elem) {
	if ("undefined" !== typeof elem.expando) {
		elem.expando = null;
	}
	elem.onclick = null;
	elem.ondblclick = null;
};

/*---------------*\
	Alpha Image
\*---------------*/

/*void*/ JsonFx.UI.alphaImg_bind = function(/*DOM*/ elem, /*object*/ options) {
	
//	try {
//		// IE7 doesn't like this with async loads
//		if (!elem.filters) {
//			return;
//		}
//	} catch (ex) {
//		alert(ex.message);
//		return;
//	}

	var filter, src;
	// only IE 5.x-6.0	
	if (JsonFx.UI.hasClassName(document.body, "ua-ie-6") || JsonFx.UI.hasClassName(document.body, "ua-ie-5")) {

		options = options || {};
		options.src = options.src || "about:blank";

		// add AlphaImageLoader filter
		elem.style.filter = "progid:DXImageTransform.Microsoft.AlphaImageLoader(sizingMethod='scale')";
		src = elem.src;

		// set to blank img
		elem.src = options.src;
		// copy back so can compare later
		options.src = elem.src;

		// set it back
		elem.src = src;

		// add dispose method
		elem.dispose = function() {
			elem.onpropertychange = null;
			elem.src = filter && filter.src;
			elem.style.filter = "";
			elem = filter = options = null;
		};

		// monitor changes to elem so can update filter
		elem.onpropertychange = function(/*Event*/ evt) {
			evt = evt||window.event;

			if (evt.propertyName === "src" &&
				elem &&
				elem.src !== options.src) {

				// can't populate until after added to document
				filter = filter || elem.filters.item("DXImageTransform.Microsoft.AlphaImageLoader");

				filter.src = elem.src;
				elem.src = options.src;
			}
		};

		// trigger filter call when done loading
		window.setTimeout(
			function() {
				elem.src = elem.src;
			}, 0);
	}
};

/*void*/ JsonFx.UI.alphaImg_unbind = function(/*DOM*/ elem, /*object*/ options) {
	if (elem.dispose) {
		elem.dispose();
	}
};
