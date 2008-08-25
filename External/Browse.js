/*global Browse */

/* namespace Browse */
if ("undefined" === typeof Browse) {
	window.Browse = {};
}

/*int*/ Browse.getKeyCode = function(/*Event*/ evt) {
	evt = evt || window.event;
	if (!evt) {
		return -1;
	}
	return Number(evt.keyCode || evt.charCode || -1);
};

/*bool*/ Browse.clear = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}
	while (elem.lastChild) {
		elem.removeChild(elem.lastChild);
	}
};

/*bool*/ Browse.hasCssClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	return elem && elem.className && cssClass &&
		!!elem.className.match(new RegExp("(^|\\s)"+cssClass+"(\\s|$)"));
};

/*void*/ Browse.addCssClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className += ' '+cssClass;
};

/*void*/ Browse.removeCssClass = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!elem || !cssClass) {
		return;
	}

	elem.className = elem.className.replace(new RegExp("(^|\\s+)"+cssClass+"(\\s+|$)"), " ");
};

/*DOM*/ Browse.findParent = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!cssClass) {
		return false;
	}

	// search up the ancestors
	while (elem) {
		if (Browse.hasCssClass(elem, cssClass)) {
			return elem;
		}

		elem = elem.parentNode;
	}
};

/*DOM*/ Browse.findChild = function(/*DOM*/ elem, /*string*/ cssClass) {
	if (!cssClass) {
		return false;
	}

	// breadth-first search of all children
	var queue = [elem];
	while (queue.length) {
		elem = queue.shift();
		if (Browse.hasCssClass(elem, cssClass)) {
			return elem;
		}
		if (elem && elem.childNodes) {
			for (var i=0; i<elem.childNodes.length; i++) {
				queue.push(elem.childNodes[i]);
			}
		}
	}
};
