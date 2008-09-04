/*global JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.ResX

	Copyright (c)2008 Stephen M. McKamey
	Created: 2008-09-04-0845
	Modified: 2008-09-04-0845
\*---------------------------------------------------------------------------*/

/* namespace JsonFx.ResX */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

if ("undefined" === typeof JsonFx.ResX) {

	/* ctor */
	JsonFx.ResX = function() {
		// create private member via closure
		var resx = {};

		/*void*/ this.add = function(/*object*/ r) {
			if (!r) {
				return;
			}

			// merge in the new values
			for (var k in r) {
				if (r.hasOwnProperty(k)) {
					resx[k] = r[k];
				}
			}
		};

		/*object*/ this.get = function(/*string*/ k, /*object*/ d) {
			if ("string" !== typeof k) {
				return d;
			}

			return resx.hasOwnProperty(k) ? resx[k] : d;
		};
	};
	/*singleton, destroy the ctor*/
	JsonFx.ResX = new JsonFx.ResX();
}
