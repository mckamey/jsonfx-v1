/*global JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.Lang

	Copyright (c)2008 Stephen M. McKamey
	Created: 2008-09-04-0845
	Modified: 2008-09-05-2250
\*---------------------------------------------------------------------------*/

/* namespace JsonFx.Lang */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

if ("undefined" === typeof JsonFx.Lang) {

	/* ctor */
	JsonFx.Lang = function() {
		// create private member via closure
		var rsrc = {};

		/*void*/ this.add = function(/*object*/ r) {
			if (!r) {
				return;
			}

			// merge in the new values
			for (var k in r) {
				if (r.hasOwnProperty(k)) {
					rsrc[k] = r[k];
				}
			}
		};

		/*object*/ this.get = function(/*string*/ k, /*object*/ d) {
			if ("string" !== typeof k) {
				return d;
			}

			return rsrc.hasOwnProperty(k) ? rsrc[k] : d;
		};
	};
	/*singleton, destroy the ctor*/
	JsonFx.Lang = new JsonFx.Lang();
}
