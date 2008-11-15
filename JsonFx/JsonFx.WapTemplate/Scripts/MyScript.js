/*global JsonFx, JsonML, Example */

/* enable valid CSS to target browsers without reverting to CSS hacks */
JsonFx.UA.setCssUserAgent();

/*
	it is a best practice to not clutter the global namespace
	creating top level objects which contain variables and functions
	allows us to simulate namespaces
*/

/* namespace Example */
if ("undefined" === typeof window.Example) {
	window.Example = {};
}

/*void*/ Example.initDisplay = function(/*DOM*/ elem) {
	if (!elem) {
		return;
	}

	// some example data
	var data = { "hostName" : window.location.host };

	// this databinds the data to the template
	var jbst = Example.congrats.dataBind( data );

	// this hydrates the resulting markup allowing dynamic behaviors to be bound to elements
	jbst = JsonML.parse(jbst, JsonFx.Bindings.bindOne);

	// add the result to the container
	if (jbst) {
		// clear the container contents
		JsonFx.UI.clear(elem);

		elem.appendChild(jbst);
	}
};

/* setup test */
JsonFx.Bindings.register(
	"div",
	"js-MessageArea",
	Example.initDisplay,
	null);
