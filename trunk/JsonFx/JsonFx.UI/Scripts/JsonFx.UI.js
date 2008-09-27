/*global JSON, JsonML, JsonFx */

/*---------------------------------------------------------*\
	JsonFx.UI

	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-09-27-1441
\*---------------------------------------------------------*/

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
	throw new Error("JsonFx.UI.js requires json2.js");
}
if ("undefined" === typeof window.JsonML) {
	throw new Error("JsonFx.UI.js requires JsonML2.js");
}
if ("undefined" === typeof JsonFx.IO) {
	throw new Error("JsonFx.UI.js requires JsonFx.IO.js");
}
if ("undefined" === typeof JsonFx.Bindings) {
	throw new Error("JsonFx.UI.js requires JsonFx.Bindings.js");
}

/* JsonFx.UI ----------------------------------------------------*/

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

/* JBST + JSON => JsonML => DOM */
/*DOM*/ JsonFx.UI.bindJBST = function(
	/*DOM*/ container,
	/*JBST*/ template,
	/*object*/ data,
	/*bool*/ append) {

	// ensure template is JBST
	if (!(template instanceof JsonML.BST)) {
		throw new Error("template is not a JBST");
	}

	// ensure container exists
	if ("string" === typeof container) {
		container = document.getElementById(container);
	}
	if (!container) {
		throw new Error("container is not a DOM element");
	}

	// databind JSON data to a JBST template, resulting in a JsonML representation
	var result = template.dataBind(data);

	// hydrate the resulting JsonML, binding any dynamic behaviors to elements
	result = JsonML.parse(result, JsonFx.Bindings.bindOne);

	if (!append) {
		// clear the container contents, unbinding any applied dynamic behaviors
		JsonFx.DOM.clear(container);
	}

	if (result) {
		// add the resulting DOM elements to the container
		container.appendChild(result);
	}
};

/*	creates DOM elements from JsonML and binds them accordingly */
/*DOM*/ JsonFx.UI.bindJsonML = function(/*JsonML*/ jml) {
	if (jml) {
		return JsonML.parse(jml, JsonFx.Bindings.bindOne);
	} else {
		return null;
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*DOM*/ JsonFx.UI.displayJsonML = function(
	/*JsonML*/ jml,
	/*DOM*/ container,
	/*bool*/ append) {

	// either DOM element or id
	container = ("string" === typeof container) ?
		document.getElementById(container) : container;

	jml = JsonFx.UI.bindJsonML(jml);
	if (jml) {
		if (container) {
			if (!append) {
				JsonFx.UI.clear(container);
			}
			container.appendChild(jml);
		} else if (jml.id) {
			container = document.getElementById(jml.id);
			if (container && container.parentNode) {
				// unbind to prevent memory leaks
				JsonFx.Bindings.unbind(container);
				container.parentNode.replaceChild(jml, container);
			} else {
				// can't add to document, so unbind
				JsonFx.Bindings.unbind(jml);
				jml = null;
			}
		} else if (jml.childNodes) {
			while (jml.firstChild) {
				if (!jml.firstChild.id) {
					// unbind to prevent memory leaks
					JsonFx.Bindings.unbind(jml.firstChild);
					jml.removeChild(jml.firstChild);
					continue;
				}
				container = document.getElementById(jml.firstChild.id);
				if (container && container.parentNode) {
					// unbind to prevent memory leaks
					JsonFx.Bindings.unbind(container);
					container.parentNode.replaceChild(jml.firstChild, container);
				} else {
					// unbind to prevent memory leaks
					JsonFx.Bindings.unbind(jml.firstChild);
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
	/*DOM*/ container,
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
