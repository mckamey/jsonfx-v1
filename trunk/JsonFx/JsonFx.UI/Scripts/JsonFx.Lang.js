/*global JsonFx */
/*
	JsonFx.Lang.js
	client-size globalization support

	Created: 2008-09-04-0845
	Modified: 2008-09-05-2250

	Copyright (c)2006-2008 Stephen M. McKamey
	Distributed under an open-source license: http://jsonfx.net/license
*/

/* namespace JsonFx.Lang */
if ("undefined" === typeof window.JsonFx) {
	window.JsonFx = {};
}

if ("undefined" === typeof JsonFx.Lang) {

	/* ctor */
	JsonFx.Lang = function() {
		// create private member via closure
		var rsrc = {};
		var lang = "";

		/*void*/ this.add = function(/*object*/ r, /*string*/ c) {
			if (!r) {
				return;
			}

			if ("string" === typeof c) {
				lang = c;
			}

			// merge in the new values
			for (var k in r) {
				if (r.hasOwnProperty(k)) {
					rsrc[k] = r[k];
				}
			}
		};

		/*object*/ this.get = function(/*string*/ k) {
			if ("string" !== typeof k) {
				k = "";
			}

			// normalize key
			k = k.replace(/^\s+|\s+$/g, "");
			k = k.replace(/\s+,|,\s+/g, ",");
			k = k.toLowerCase();

			return rsrc.hasOwnProperty(k) ? rsrc[k] : "$$"+k+"$$";
		};

		/*void*/ this.getLang = function() {
			return lang;
		};
	};

	/*singleton, destroy the ctor*/
	JsonFx.Lang = new JsonFx.Lang();
}
