/*global JSON, Example */

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
	// just display raw error results
	window.alert( JSON.stringify(ex, null, "\t") );
};

/*void*/ Example.tryService = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}

	// this is a parameter to the service method
	var number = Math.PI;

	// these are the options for the service proxy
	var options = {
		onSuccess: Example.results.bind, // defined in Results.jbst
		onFailure: Example.asyncError,
		context: { elem: elem, foo: "bar" }
	};

	// call the JSON-RPC service proxy object with the method args in order and add an options object at the end
	Example.MyServiceProxy.getInfo(number, options);

	// when the request completes, the appropriate callback will get called with the return value and the context object
};
