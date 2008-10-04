/*global JSON, JsonML, JsonFx */
/*
	JsonFx_UI.js
	DOM utilities

	Created: 2006-11-11-1759
	Modified: 2008-10-04-0035

	Copyright (c)2006-2008 Stephen M. McKamey
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
	throw new Error("JsonFx_UI.js requires json2.js");
}
if ("undefined" === typeof window.JsonML) {
	throw new Error("JsonFx_UI.js requires JsonML2.js");
}
if ("undefined" === typeof JsonFx.Bindings) {
	throw new Error("JsonFx_UI.js requires JsonFx_Bindings.js");
}

/* DOM utilities ------------------------------------------------*/

/*bool*/ JsonFx.UI.clear = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}
	// unbind to prevent memory leaks
	JsonFx.Bindings.unbind(elem);

	while (elem.lastChild) {
		elem.removeChild(elem.lastChild);
	}
};

/*bool*/ JsonFx.UI.hasClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	return elem && elem.className && cssClass &&
		!!elem.className.match(new RegExp("(^|\\s)"+cssClass+"(\\s|$)"));
};

/*void*/ JsonFx.UI.addClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className += ' '+cssClass;
};

/*void*/ JsonFx.UI.removeClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className = elem.className.replace(new RegExp("(^|\\s+)"+cssClass+"(\\s+|$)"), " ");
};

/*DOM*/ JsonFx.UI.findParent = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.parentNode;
	}

	// search up the ancestors
	while (elem) {
		if (JsonFx.UI.hasClass(elem, cssClass)) {
			return elem;
		}

		elem = elem.parentNode;
	}
	return null;
};

/*DOM*/ JsonFx.UI.findChild = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	// breadth-first search of all children
	var i, queue = [];
	
	if (skipRoot) {
		if (elem && elem.childNodes) {
			for (i=0; i<elem.childNodes.length; i++) {
				queue.push(elem.childNodes[i]);
			}
		}
	} else {
		queue.push(elem);
	}

	while (queue.length) {
		elem = queue.shift();
		if (JsonFx.UI.hasClass(elem, cssClass)) {
			return elem;
		}
		if (elem && elem.childNodes) {
			for (i=0; i<elem.childNodes.length; i++) {
				queue.push(elem.childNodes[i]);
			}
		}
	}
	return null;
};

/*DOM*/ JsonFx.UI.findPrev = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.previousSibling;
	}

	// search up siblings in order
	while (elem) {
		if (JsonFx.UI.hasClass(elem, cssClass)) {
			return elem;
		}
		elem = elem.previousSibling;
	}
	return null;
};

/*DOM*/ JsonFx.UI.findNext = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.nextSibling;
	}

	// search down siblings in order
	while (elem) {
		if (JsonFx.UI.hasClass(elem, cssClass)) {
			return elem;
		}
		elem = elem.nextSibling;
	}
	return null;
};

/* Event utilities ----------------------------------------------*/

/*void*/ JsonFx.UI.clearEvent = function(/*Event*/ evt) {
	evt = evt || window.event;
	if (evt) {
		if (evt.stopPropagation) {
			evt.stopPropagation();
			evt.preventDefault();
		} else {
			try {
				evt.cancelBubble = true;
				evt.returnValue = false;
			} catch (ex) {
				// IE6
			}
		}
	}
};

/*int*/ JsonFx.UI.getKeyCode = function(/*Event*/ evt) {
	evt = evt || window.event;
	if (!evt) {
		return -1;
	}
	return Number(evt.keyCode || evt.charCode || -1);
};

/* JsonML utilities ---------------------------------------------*/

/* JBST + JSON => JsonML => DOM */
/*DOM*/ JsonFx.UI.bind = function(
	/*JBST*/ jbst,
	/*object*/ data) {

	var jml;
	if (jbst instanceof JsonML.BST) {
		// databind JSON data to a JBST template, resulting in a JsonML representation
		jml = jbst.dataBind(data);
	} else {
		// assume template already is JsonML
		jml = jbst;
	}

	// hydrate the resulting JsonML, binding any dynamic behaviors to elements
	return JsonML.parse(jml, JsonFx.Bindings.bindOne);
};
