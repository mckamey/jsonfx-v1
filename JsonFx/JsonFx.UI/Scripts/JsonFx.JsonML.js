/*global JsonFx, JSON, JsonML */
/*---------------------------------------------------------*\
	JsonFx.JsonML
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-05-25-2253
\*---------------------------------------------------------*/

// dependency checks
if ("undefined" === typeof window.JSON) {
	throw new Error("JsonFx.JsonML requires json2.js");
}
if ("undefined" === typeof window.JsonML) {
	throw new Error("JsonFx.JsonML requires JsonML2.js");
}
if ("undefined" === typeof JsonFx.IO) {
	throw new Error("JsonFx.JsonML requires JsonFx.IO.js");
}
if ("undefined" === typeof JsonFx.Bindings) {
	throw new Error("JsonFx.JsonML requires JsonFx.Bindings.js");
}

/* Namespace JsonFx.JsonML ----------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof window.JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*void*/ JsonFx.UI.clear = function(/*element*/ elem) {
	if (elem) {
		// unbind to prevent memory leaks
		JsonFx.Bindings.unbind(elem);

		while (elem.lastChild) {
			elem.removeChild(elem.lastChild);
		}
	}
};

/*	creates DOM elements from JsonML and binds them accordingly */
/*element*/ JsonFx.UI.bindJsonML = function(/*JsonML*/ jml) {
	if (jml) {
		return JsonML.parse(jml, JsonFx.Bindings.bindOne);
	} else {
		return null;
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*element*/ JsonFx.UI.displayJsonML = function(
	/*JsonML*/ jml,
	/*element|string*/ container,
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
