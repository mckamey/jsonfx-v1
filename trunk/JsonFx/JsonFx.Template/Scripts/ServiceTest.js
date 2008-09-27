﻿/*global JSON, JsonFx, JsonML, Example */

/*
	it is a best practice to not clutter the global namespace
	creating top level objects which contain variables and functions
	allows us to simulate namespaces
*/

/* namespace Example */
if ("undefined" === typeof window.Example) {
	window.Example = {};
}

/*void*/ Example.asyncError = function (/*object*/ result, /*object*/ cx, /*Error*/ ex) {
	window.alert( JSON.stringify(ex, null, "\n\t") );
};

/*void*/ Example.asyncSuccess = function(/*object*/ data, /*object*/ cx) {
	if (!data) {
		return;
	}

	var elem = cx && cx.elem;
	if (elem) {
		// this databinds the data to the template
		var jbst = Example.results.dataBind( data );

		// this hydrates the resulting markup allowing dynamic behaviors to be bound to elements
		jbst = JsonML.parse(jbst, JsonFx.Bindings.bindOne);

		// add the result to the container
		if (elem && elem.parentNode && jbst) {
			elem = elem.parentNode;

			// clear the container contents
			JsonFx.DOM.clear(elem);

			elem.appendChild(jbst);
		}
	}
};

/*void*/ Example.tryService = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}

	// this is the parameter to the service method
	var number = Math.PI;

	// these are the options for the service proxy
	var options = {
		onSuccess: Example.asyncSuccess,
		onFailure: Example.asyncError,
		context: { "elem": elem, "number": number }
	};

	// call the JSON-RPC service proxy with params in order and the options at the end
	Example.MyServiceProxy.getInfo(number, options);
};