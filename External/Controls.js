/*extern JsonFx */
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
		elem.expando = expando = new JsonFx.UI.Animate.Engine(target);
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
		op = new JsonFx.UI.Animate.Op();
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

/*void*/ JsonFx.UI.expandoUnbind = function(/*element*/ elem) {
	if ("undefined" !== typeof elem.expando) {
		elem.expando = null;
	}
	elem.onclick = null;
	elem.ondblclick = null;
};

