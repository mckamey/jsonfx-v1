/*extern JsonFx, ActiveXObject */
/*---------------------------------------------------------*\
	Modifications to browser global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-03-12-2349
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

///*float*/ JsonFx.JScriptVersion = ("undefined" === typeof window.ScriptEngineMajorVersion) ? NaN :
//		Number(window.ScriptEngineMajorVersion()+"."+window.ScriptEngineMinorVersion());

if ("undefined" === typeof window.global) {
	/*object*/ window.global = window;
}

if ("undefined" === typeof Object.prototype.hasOwnProperty) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
		return ("undefined" !== typeof(this[p]) && ("undefined" === typeof(this.constructor.prototype[p])));
	};
}

if ("undefined" === typeof Object.prototype.propertyIsEnumerable) {
	/*bool*/ Object.prototype.propertyIsEnumerable = function(/*object*/ p) {
		return this.hasOwnProperty(p);
	};
}

if ("undefined" === typeof Array.prototype.push) {
	/*int*/ Array.prototype.push = function(/*object*/ obj) {
		this[this.length] = obj;
		return this.length;
	};
}

if ("undefined" === typeof Array.prototype.pop) {
	/*object*/ Array.prototype.pop = function() {
		if (this.length < 1) {
			return undefined;
		}
		var obj = this[this.length-1];
		this.length--;
		return obj;
	};
}

if ("undefined" === typeof String.prototype.charCodeAt) {
	/*int*/ String.prototype.charCodeAt = function (/*int*/ i) {
		if (isNaN(i) || i<0 || i>=this.length) {
			return NaN;
		}
		return Number(this[i]-"A")+65;
	};
}

if ("undefined" === typeof Number.prototype.toPrecision) {
	/*string*/ Number.prototype.toPrecision = function(/*int*/ digits) {
		var str = this.toString();
		if (isNaN(digits) || digits < 1 || digits > 21) {
			return str;
		}
		digits += str.indexOf(".")+1;

		// note: doesn't handle scientific notation
		return str.substring(0, digits);
	};
}

if ("undefined" === typeof Object.prototype.isPrototypeOf) {
	/*bool*/ Object.prototype.isPrototypeOf = function (/*object*/ obj) {
		while ("undefined" !== typeof(obj.prototype)) {
			if (this === obj.prototype) {
				return true;
			}
			obj = obj.prototype;
		}
		return false;
	};
}

if ("undefined" === typeof window.encodeURI) {
	/*string*/ window.encodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof window.encodeURIComponent) {
	/*string*/ window.encodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof window.decodeURI) {
	/*string*/ window.decodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof window.decodeURIComponent) {
	/*string*/ window.decodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

//if ("undefined" === typeof Array.prototype.shift) {
//	/*array*/ Array.prototype.shift = function(...) {
//	};
//}

//if ("undefined" === typeof Array.prototype.unshift) {
//	/*array*/ Array.prototype.unshift = function(...) {
//	};
//}

//if ("undefined" === typeof Array.prototype.splice) {
//	/*array*/ Array.prototype.splice = function(...) {
//	};
//}

if ("undefined" === typeof String.prototype.trim) {
	/*string*/ String.prototype.trim = function () {
		return this.replace(/^\s*|\s*$/g, "");
	};
}

if ("undefined" === typeof String.prototype.contains) {
	/*string*/ String.prototype.contains = function (str) {
		return (this.indexOf(str) >= 0);
	};
}

/* ----------------------------------------------------*/

(function () {
	// wrapping in anonymous function so that the XHR-ID list
	// will be only available as a closure, as this will not
	// modify the global namespace, and it will be shared

	if ("undefined" === typeof window.XMLHttpRequest) {

		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ var xhrOCXs =
			[
				"Msxml2.XMLHTTP.6.0",
				"Msxml2.XMLHttp.5.0",
				"Msxml2.XMLHttp.4.0",
				"MSXML2.XMLHTTP.3.0",
				"MSXML2.XMLHTTP",
				"Microsoft.XMLHTTP"
			];

		// XMLHttpRequest: augment browser to have "native" XHR
		/*XMLHttpRequest*/ window.XMLHttpRequest = function() {
			while (xhrOCXs.length) {
				try {
					return new ActiveXObject(xhrOCXs[0]);
				} catch (ex) {
					// remove the failed xhrOCXs for all future requests
					xhrOCXs.shift();
				}
			}

			// all xhrOCXs failed		
			return null;
		};
	}
})();

/* ----------------------------------------------------*/

JsonFx.Timer = function() {
	/*object*/ var s = {};
	/*object*/ var t = {};

	/*void*/ JsonFx.Timer.prototype.clear = function(/*string*/ key) {
		if (key) {
			if (t[key]) {
				delete t[key];
			}
			if (s[key]) {
				delete s[key];
			}
		} else {
			t = {};
			s = {};
		}
	};

	/*void*/ JsonFx.Timer.prototype.start = function(/*string*/ key) {
		s[key] = (new Date()).valueOf();
	};

	/*void*/ JsonFx.Timer.prototype.stop = function(/*string*/ key, /*bool*/ append) {
		append = append && isFinite(t[key]);
		var time = ((new Date()).valueOf() - s[key]);
		t[key] = append ? (t[key]+time) : time;
	};

	/*int*/ JsonFx.Timer.prototype.getTime = function(/*string*/ key) {
		if (key && isFinite(t[key])) {
			return t[key];
		}
		return NaN;
	};

	/*object*/ JsonFx.Timer.prototype.getTimes = function() {
		var c = {};
		for (var key in t) {
			if (t.hasOwnProperty(key) && isFinite(t[key])) {
				c[key] = t[key];
			}
		}
		return c;
	};
};

// instantiate only one, destroying the constructor
JsonFx.Timer = new JsonFx.Timer();