/*global JSON, JsonML, JsonFx, jQuery */
/*
	JsonFx_UI.js
	DOM utilities

	Created: 2006-11-11-1759
	Modified: 2009-03-03-0833

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

/*bool*/ JsonFx.UI.cancelEvent = function(/*Event*/ evt) {
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
	return false;
};

/*void*/ JsonFx.UI.addHandler = function(/*DOM*/ target, /*string*/ name, /*function*/ handler) {
	if (typeof jQuery !== "undefined") {
		jQuery(target).bind(name, handler);
	} if (target.addEventListener) {
		// DOM Level 2 model for binding events
		target.addEventListener(name, handler, false);
	} else if (target.attachEvent) {
		// IE model for binding events
		target.attachEvent("on"+name, handler);
	} else {
		// DOM Level 0 model for binding events
		var old = target["on"+name];
		target["on"+name] = ("function" === typeof old) ?
			function(/*Event*/ evt) { handler(evt); return old(evt); } :
			handler;
	}
};

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

/* JsonML utilities ---------------------------------------------*/

/* deprecated */
/*DOM*/ JsonFx.UI.bind = function(/*JBST*/ jbst, /*JSON*/ data, /*int*/ index, /*int*/ count) {
	return JsonML.BST(jbst).bind(data, index, count);
};
