/*global JsonFx */

/* namespace JsonFx.DOM */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}
if ("undefined" === typeof JsonFx.DOM) {
	JsonFx.DOM = {};
}

/*void*/ JsonFx.DOM.clearEvent = function(/*Event*/ evt) {
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

/*int*/ JsonFx.DOM.getKeyCode = function(/*Event*/ evt) {
	evt = evt || window.event;
	if (!evt) {
		return -1;
	}
	return Number(evt.keyCode || evt.charCode || -1);
};

/*bool*/ JsonFx.DOM.clear = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}
	while (elem.lastChild) {
		elem.removeChild(elem.lastChild);
	}
};

/*bool*/ JsonFx.DOM.hasClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	return elem && elem.className && cssClass &&
		!!elem.className.match(new RegExp("(^|\\s)"+cssClass+"(\\s|$)"));
};

/*void*/ JsonFx.DOM.addClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className += ' '+cssClass;
};

/*void*/ JsonFx.DOM.removeClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className = elem.className.replace(new RegExp("(^|\\s+)"+cssClass+"(\\s+|$)"), " ");
};

/*DOM*/ JsonFx.DOM.findParent = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.parentNode;
	}

	// search up the ancestors
	while (elem) {
		if (JsonFx.DOM.hasClass(elem, cssClass)) {
			return elem;
		}

		elem = elem.parentNode;
	}
	return null;
};

/*DOM*/ JsonFx.DOM.findChild = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
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
		if (JsonFx.DOM.hasClass(elem, cssClass)) {
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

/*DOM*/ JsonFx.DOM.findPrev = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.previousSibling;
	}

	// search up siblings in order
	while (elem) {
		if (JsonFx.DOM.hasClass(elem, cssClass)) {
			return elem;
		}
		elem = elem.previousSibling;
	}
	return null;
};

/*DOM*/ JsonFx.DOM.findNext = function(/*DOM*/ elem, /*string*/ cssClass, /*bool*/ skipRoot) {
	if (!cssClass) {
		return null;
	}

	if (skipRoot) {
		elem = elem.nextSibling;
	}

	// search down siblings in order
	while (elem) {
		if (JsonFx.DOM.hasClass(elem, cssClass)) {
			return elem;
		}
		elem = elem.nextSibling;
	}
	return null;
};
