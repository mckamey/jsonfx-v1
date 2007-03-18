/*extern JsonFx */
/*---------------------------------------------------------*\
	Modifications to browser global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-03-17-1241
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
	/*int*/ Array.prototype.push = function(/*object*/ item) {
		// add item to end
		this[this.length] = item;
		return this.length;
	};
}

if ("undefined" === typeof Array.prototype.pop) {
	/*object*/ Array.prototype.pop = function() {
		if (this.length < 1) {
			return undefined;
		}

		// grab the last item
		var item = this[this.length-1];

		// reduce the length
		this.length--;
		return item;
	};
}

if ("undefined" === typeof Array.prototype.splice) {
	/*array*/ Array.prototype.splice = function (/*int*/ start, /*int*/ del) {
		// based on http://javascript.crockford.com/remedial.html
        var max = Math.max,
            min = Math.min,
            a = [], // The return value array
            e,  // element
            i = max(arguments.length-2, 0),   // insert count
            k = 0,
            l = this.length,
            n,  // new length
            v,  // delta
            x;  // shift count

        start = start || 0;
        if (start < 0) {
            start += l;
        }

        // start point
        start = max(min(start, l), 0);

        // delete count
        del = max(min((typeof del === 'number' && isFinite(del)) ? del : l, l - start), 0);

        v = i - del;
        n = l + v;
        while (k < del) {
            e = this[start + k];
            if (e !== undefined) {
                a[k] = e;
            }
            k += 1;
        }
        x = l - start - del;
        if (v < 0) {
            k = start + i;
            while (x) {
                this[k] = this[k - v];
                k += 1;
                x -= 1;
            }
            this.length = n;
        } else if (v > 0) {
            k = 1;
            while (x) {
                this[n - k] = this[l - k];
                k += 1;
                x -= 1;
            }
        }
        for (k = 0; k < i; ++k) {
            this[start + k] = arguments[k + 2];
        }
        return a;
    };
}

if ("undefined" === typeof Array.prototype.shift) {
	/*object*/ Array.prototype.shift = function() {
		return this.splice(0, 1)[0];
	};
}

if ("undefined" === typeof Array.prototype.unshift) {
	/*void*/ Array.prototype.unshift = function () {
		var a = arguments;
		if (a && a.length) {
			var i, l = this.length, al = a.length;

			// move this array items onto end of arguments list
			for (i=0; i<l; i++) {
				a[al] = a[i];
				al++;
			}

			// copy concat'ed arguments list back into this array
			for (i=0; i<al; i++) {
				this[i] = a[i];
			}
		}
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
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.encodeURIComponent) {
	/*string*/ window.encodeURIComponent = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.decodeURI) {
	/*string*/ window.decodeURI = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.decodeURIComponent) {
	/*string*/ window.decodeURIComponent = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

(function () {
	// wrapping in anonymous function so that the XHR ID list
	// will be only available as a closure, as this will not
	// modify the global namespace, and it will be shared

	if ("undefined" === typeof window.XMLHttpRequest) {

		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ var xhrOCXs = !window.ActiveXObject ? [] :
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
					return new window.ActiveXObject(xhrOCXs[0]);
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

if ("undefined" === typeof String.prototype.trim) {
	/*string*/ String.prototype.trim = function () {
		return this.replace(/^\s*|\s*$/g, "");
	};
}

if ("undefined" === typeof String.prototype.contains) {
	/*bool*/ String.prototype.contains = function (/*string*/ str) {
		return (this.indexOf(str) >= 0);
	};
}

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
/*
    json.js
    2007-03-06

    Public Domain

    This file adds these methods to JavaScript:

        array.toJSONString()
        boolean.toJSONString()
        date.toJSONString()
        number.toJSONString()
        object.toJSONString()
        string.toJSONString()
            These methods produce a JSON text from a JavaScript value.
            It must not contain any cyclical references. Illegal values
            will be excluded.

            The default conversion for dates is to an ISO string. You can
            add a toJSONString method to any date object to get a different
            representation.

        string.parseJSON(filter)
            This method parses a JSON text to produce an object or
            array. It can throw a SyntaxError exception.

            The optional filter parameter is a function which can filter and
            transform the results. It receives each of the keys and values, and
            its return value is used instead of the original value. If it
            returns what it received, then structure is not modified. If it
            returns undefined then the member is deleted.

            Example:

            // Parse the text. If a key contains the string 'date' then
            // convert the value to a date.

            myData = text.parseJSON(function (key, value) {
                return key.indexOf('date') >= 0 ? new Date(value) : value;
            });

    It is expected that these methods will formally become part of the
    JavaScript Programming Language in the Fourth Edition of the
    ECMAScript standard in 2008.
*/

// Augment the basic prototypes if they have not already been augmented.

if (!Object.prototype.toJSONString) {

    Array.prototype.toJSONString = function () {
        var a = ['['],  // The array holding the text fragments.
            b,          // A boolean indicating that a comma is required.
            i,          // Loop counter.
            l = this.length,
            v;          // The value to be stringified.

        function p(s) {

// p accumulates text fragments in an array. It inserts a comma before all
// except the first fragment.

            if (b) {
                a.push(',');
            }
            a.push(s);
            b = true;
        }

// For each value in this array...

        for (i = 0; i < l; i += 1) {
            v = this[i];
            switch (typeof v) {

// Values without a JSON representation are ignored.

            case 'undefined':
            case 'function':
            case 'unknown':
                break;

// Serialize a JavaScript object value. Ignore objects thats lack the
// toJSONString method. Due to a specification error in ECMAScript,
// typeof null is 'object', so watch out for that case.

            case 'object':
                if (v) {
                    if (typeof v.toJSONString === 'function') {
                        p(v.toJSONString());
                    }
                } else {
                    p("null");
                }
                break;

// Otherwise, serialize the value.

            default:
                p(v.toJSONString());
            }
        }

// Join all of the fragments together and return.

        a.push(']');
        return a.join('');
    };


    Boolean.prototype.toJSONString = function () {
        return String(this);
    };


    Date.prototype.toJSONString = function () {

// Ultimately, this method will be equivalent to the date.toISOString method.

        function f(n) {

// Format integers to have at least two digits.

            return n < 10 ? '0' + n : n;
        }

        return '"' + this.getFullYear() + '-' +
                f(this.getMonth() + 1) + '-' +
                f(this.getDate()) + 'T' +
                f(this.getHours()) + ':' +
                f(this.getMinutes()) + ':' +
                f(this.getSeconds()) + '"';
    };


    Number.prototype.toJSONString = function () {

// JSON numbers must be finite. Encode non-finite numbers as null.

        return isFinite(this) ? String(this) : "null";
    };


    Object.prototype.toJSONString = function () {
        var a = ['{'],  // The array holding the text fragments.
            b,          // A boolean indicating that a comma is required.
            k,          // The current key.
            v;          // The current value.

        function p(s) {

// p accumulates text fragment pairs in an array. It inserts a comma before all
// except the first fragment pair.

            if (b) {
                a.push(',');
            }
            a.push(k.toJSONString(), ':', s);
            b = true;
        }

// Iterate through all of the keys in the object, ignoring the proto chain.

        for (k in this) {
            if (this.hasOwnProperty(k)) {
                v = this[k];
                switch (typeof v) {

// Values without a JSON representation are ignored.

                case 'undefined':
                case 'function':
                case 'unknown':
                    break;

// Serialize a JavaScript object value. Ignore objects that lack the
// toJSONString method. Due to a specification error in ECMAScript,
// typeof null is 'object', so watch out for that case.

                case 'object':
                    if (v) {
                        if (typeof v.toJSONString === 'function') {
                            p(v.toJSONString());
                        }
                    } else {
                        p("null");
                    }
                    break;
                default:
                    p(v.toJSONString());
                }
            }
        }

// Join all of the fragments together and return.

        a.push('}');
        return a.join('');
    };


    (function (s) {

// Augment String.prototype. We do this in an immediate anonymous function to
// avoid defining global variables.

// m is a table of character substitutions.

        var m = {
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '"' : '\\"',
            '\\': '\\\\'
        };


        s.parseJSON = function (filter) {

// Parsing happens in three stages. In the first stage, we run the text against
// a regular expression which looks for non-JSON characters. We are especially
// concerned with '()' and 'new' because they can cause invocation, and '='
// because it can cause mutation. But just to be safe, we will reject all
// unexpected characters.

            try {
                if (/^("(\\.|[^"\\\n\r])*?"|[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t])+?$/.
                        test(this)) {

// In the second stage we use the eval function to compile the text into a
// JavaScript structure. The '{' operator is subject to a syntactic ambiguity
// in JavaScript: it can begin a block or an object literal. We wrap the text
// in parens to eliminate the ambiguity.

                    var j = eval('(' + this + ')');

// In the optional third stage, we recursively walk the new structure, passing
// each name/value pair to a filter function for possible transformation.

                    if (typeof filter === 'function') {

                        function walk(k, v) {
                            if (v && typeof v === 'object') {
                                for (var i in v) {
                                    if (v.hasOwnProperty(i)) {
                                        v[i] = walk(i, v[i]);
                                    }
                                }
                            }
                            return filter(k, v);
                        }

                        j = walk('', j);
                    }
                    return j;
                }
            } catch (e) {

// Fall through if the regexp test fails.

            }
            throw new SyntaxError("parseJSON");
        };


        s.toJSONString = function () {

// If the string contains no control characters, no quote characters, and no
// backslash characters, then we can simply slap some quotes around it.
// Otherwise we must also replace the offending characters with safe
// sequences.

            if (/["\\\x00-\x1f]/.test(this)) {
                return '"' + this.replace(/([\x00-\x1f\\"])/g, function(a, b) {
                    var c = m[b];
                    if (c) {
                        return c;
                    }
                    c = b.charCodeAt();
                    return '\\u00' +
                        Math.floor(c / 16).toString(16) +
                        (c % 16).toString(16);
                }) + '"';
            }
            return '"' + this + '"';
        };
    })(String.prototype);
}
/*
	JsonML.js

	Created: 2006-11-09-0116
	Modified: 2007-02-15-2226

	Released under an open-source license:
	http://jsonml.org/License.htm

    This file adds these methods to JavaScript:

        string.parseJsonML(filter)

            This method produces a tree of DOM elements from a (JSON text) JsonML tree.

        array.parseJsonML(filter)

            This method produces a tree of DOM elements from a JsonML tree. The
            array must not contain any cyclical references.

            The optional filter parameter is a function which can filter and
            transform the results. It receives each of the DOM nodes, and
            its return value is used instead of the original value. If it
            returns what it received, then structure is not modified. If it
            returns undefined then the member is deleted.

			This is useful for binding unobtrusive JavaScript to the generated
			DOM elements.

            Example:

            // Parses the structure. If an element has a specific CSS value then
            // takes appropriate action: Remove from results, add special event
            // handlers, or bind to a custom component.

            var myUI = myUITemplate.parseJsonML(function (elem) {
				if (elem.className.indexOf("Remove-Me") >= 0) {
					// this will remove from resulting DOM tree
					return undefined;
				}

				if (elem.tagName && elem.tagName.toLowerCase() === "a" &&
					elem.className.indexOf("External-Link") >= 0) {
					// this is the equivalent of target="_blank"
					elem.onclick = function(evt) {
						window.open(elem.href); return false;
					};

				} else if (elem.className.indexOf("Fancy-Widgit") >= 0) {
					// bind to a custom component
					FancyWidgit.bindDOM(elem);
				}
				return elem;
			});

*/

/*element*/ Array.prototype.parseJsonML = function (/*element function(element)*/ filter) {

	//attribute name mapping
	var am = {
		"tabindex" : "tabIndex",
		"accesskey" : "accessKey",
		"hidefocus" : "hideFocus"
	};

	//addAttributes
	/*void*/ function aa(/*element*/ el, /*Object*/ a) {
		// for each attributeName
		for (var an in a) {
			if (!an || typeof(a[an]) !== "string") {
				continue;
			}
			if (an.toLowerCase() === "style") {
				if ("undefined" !== typeof el.style.cssText) {
					el.style.cssText = a[an];
				} else {
					el.style = a[an];
				}
			} else if (an.toLowerCase() === "class") {
				el.className = a[an];
			} else if (am[an.toLowerCase()]) {
				el.setAttribute(am[an.toLowerCase()], a[an]);
			} else {
				el.setAttribute(an, a[an]);
			}
		}
	}

	//appendChild
	/*void*/ function ac(/*element*/ el, /*Array or String*/ c) {
		if (c) {
			if (el.tagName.toLowerCase() === "table" && el.tBodies) {
				// in IE must explicitly nest TDs in TBODY
				var ct = c.tagName ? c.tagName.toLowerCase() : null;// child tagName
				if (ct && ct!=="tbody" && ct!=="thead") {
					// insert in last tbody
					var tb = el.tBodies.length>0 ? el.tBodies[el.tBodies.length-1] : null;// tBody
					if (!tb) {
						tb = document.createElement("tbody");
						el.appendChild(tb);
					}
					tb.appendChild(c);
				}
			} else {
				el.appendChild(c);
			}
		}
	}

	//parseJsonML
	/*element*/ function p(/*JsonML*/ jml) {
		if (!jml) {
			return null;
		}
		if (typeof(jml) === "string") {
			return document.createTextNode(jml);
		}

		if (!(jml instanceof Array) || jml.length < 1 || typeof(jml[0]) !== "string") {
			throw new Error("parseJsonML");
		}

		var t = jml[0]; // tagName
		var x = (t.toLowerCase() === "script"); // check for scripts
		var el = x ? null : document.createElement(t);

		for (var i=1; i<jml.length; i++) {
			if (!x) {
				if (jml[i] instanceof Array || typeof(jml[i]) === "string") {
					// append children
					ac(el, p(jml[i]));
				} else if (typeof(jml[i]) === "object") {
					// add attributes
					aa(el, jml[i]);
				}
			//} else if (typeof(jml[i]) === "string") {
				/*	JSLint: "eval is evil"
					uncomment at your own risk, executes script elements */
				//eval( "(" + jml[i] + ")" );
			}
		}

		return (el && "function" === typeof filter) ? filter(el) : el;
	}

	return p(this);
};

/*element*/ String.prototype.parseJsonML = function (/*function(element)*/ filter) {
	try {
		var jml = this.parseJSON();
	} catch (ex) {
		return null;
	}
	return (jml instanceof Array) ? jml.parseJsonML(filter) : null;
};

/*extern JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.UI.setCssUserAgent

	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-06-10-1635
	Modified: 2007-03-16-0123
\*---------------------------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*string*/ JsonFx.userAgent = "JsonFx/1.0";

(function() {
	// anonymous function doesn't affect global namespace and can't be called externally
	// variables and helper functions only available via JavaScript closures

	/*string*/ function parse() {
		/*{b,v}[]*/ var details = [];

		// Regex tested against (2006-06-11 @ 1600): http://en.wikipedia.org/wiki/User_agent
		var R_MSIE = /(msie|microsoft internet explorer)[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_Gecko = /rv[:]([0-9]+[\.]?[0-9]*).*?gecko[\/][0-9]+(\s+(\S+)[\/]([0-9]+[\.]?[0-9]*))?/;
		var R_AppleWebKit = /applewebkit[\/]([0-9]+[\.]?[0-9]*).*?(\S+)[\/][v]?([0-9]+[\.]?[0-9]*)/;
		var R_Opera = /opera[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_MSPIE = /(mspie|microsoft pocket internet explorer)[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_MozCompat = /[(].*?(\S+)[\/]([0-9]+[\.]?[0-9]*).*?[)]/;
		var R_Other = /^([^\/]+)[\/]([0-9]+[\.]?[0-9]*)/;
		var R_AOL = /(america online browser|aol)[\s\/]*([0-9]+[\.]?[0-9]*)/;

		var ua = navigator.userAgent.toLowerCase();
		var name = null;// browser name
		var ver = null;// browser version

		// aol uses multiple browsers so don't stop
		if (R_AOL.exec(ua)) {
			details.push( { b : "aol", v : RegExp.$2 } );
		}

		// order is important as user-agents spoof each other	
		if (R_Opera.exec(ua)) {
			name = "opera";
			ver = RegExp.$1;
		} else if (R_MSIE.exec(ua)) {
			name = "msie";
			ver = RegExp.$2;
		} else if (R_MSPIE.exec(ua)) {
			name = "mspie";
			ver = RegExp.$2;
		} else if (R_AppleWebKit.exec(ua)) {
			name = "applewebkit";
			ver = RegExp.$1;

			// also add AppleWebKit-brand version
			details.push( { b : RegExp.$2, v : RegExp.$3 } );
		} else if (R_Gecko.exec(ua)) {
			name = "gecko";
			ver = RegExp.$1;

			// also add Gecko-brand version
			details.push( { b : RegExp.$3, v : RegExp.$4 } );
		} else if (R_MozCompat.exec(ua)) {
			name = RegExp.$1;
			ver = RegExp.$2;
		} else if (R_Other.exec(ua)) {
			name = RegExp.$1;
			ver = RegExp.$2;
		}

		// name = browser, ver = parsed version string
		details.push( { b : name, v : ver } );
		
		return details;
	}

	// calculate styles immediately, loop until can apply them
	/*string*/ var uaDetails = parse();
	if (uaDetails.length) {
		JsonFx.userAgent += " (";
		for (var i=0; i<uaDetails.length;i ++) {
			if (i>0) {
				JsonFx.userAgent += "; ";
			}
			JsonFx.userAgent += uaDetails[i].b+"/"+uaDetails[i].v;
		}
		JsonFx.userAgent += ")";
	}

	/*	Dynamically appends CSS classes to document.body based upon user-agent.*/
	/*void*/ JsonFx.UI.setCssUserAgent = function() {

		/*string*/ function formatCss(/*string*/ b, /*string*/ v) {
			/*const string*/ var PREFIX = " ua-";

			if (!b) {
				return "";
			}

			b = b.replace(/\s+/g, '-');
			var vi = parseInt(v, 10);
			var vf = parseFloat(v);
			vf = (vf === vi && vf.toFixed) ?
				vf.toFixed(1) : vf.toString();
			vf = vf.replace(/\./g, '-');
			if (!isFinite(vi)) {
				return PREFIX+b;
			}
			return PREFIX+b+PREFIX+b+vi+PREFIX+b+vf;
		}

		// using JavaScript closures to access the parsed UA
		/*void*/ function appendCss(/*{b,v}[]*/ d) {
			var uaCss = "";

			while (d.length) {
				ua = d.pop();
				uaCss += formatCss(ua.b, ua.v);
			}

			// assign user-agent classes
			if (document.body.className) {
				document.body.className += uaCss;
			} else {
				document.body.className += uaCss.substring(1);
			}

			// DEBUG
			//alert("\""+document.body.className+"\"");
		}

		// using setTimeout to poll until body exists
		/*void*/ function appendCssPoll() {

			if (!document.body) {
				setTimeout(appendCssPoll, 100);
			} else {
				appendCss(uaDetails);
			}
		}

		appendCssPoll();
	};
})();

/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2007-03-12-2349
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.IO */
JsonFx.IO = {};

/*bool*/ JsonFx.IO.hasAjax = !!(new XMLHttpRequest());

/*
	RequestOptions = {
		// HTTP Options
		async : bool,
		method : string,
		headers : Dictionary<string, string>,
		timeout : number,
		params : string,

		// callbacks
		onCreate : function(XMLHttpRequest, context){},
		onSuccess : function(XMLHttpRequest, context){},
		onFailure : function(XMLHttpRequest, context, Error){},
		onTimeout : function(XMLHttpRequest, context, Error){},
		onComplete : function(XMLHttpRequest, context){},

		// callback context
		context : object
	};
*/

/*RequestOptions*/ JsonFx.IO.onFailure = function(/*XMLHttpRequest|JSON*/ obj, /*object*/ cx, /*error*/ ex) {
	if (ex) {
		var name = ex.name ? ex.name : "Error";
		var msg = ex.message ? ex.message : "";
		var code = isFinite(ex.code) ? Number(ex.code) : Number(ex.number);

		if (isFinite(code)) {
			name += " ("+code+")";
		}

		window.alert("Request Failed - "+name+":\n\""+msg+"\"");
	}
};

/*RequestOptions*/ JsonFx.IO.validateOptions = function(/*RequestOptions*/ options) {
	// establish defaults
	if ("object" !== typeof options) {
		options = {};
	}
	if ("boolean" !== typeof options.async) {
		options.async = true;
	}
	if ("string" !== typeof options.method) {
		options.method = "POST";
	}
	if ("string" !== typeof options.params) {
		options.params = null;
	}
	if ("object" !== typeof options.headers) {
		options.headers = {};
		if (options.method === "POST" && options.params) {
			options.headers["Content-Type"] = "application/x-www-form-urlencoded";
		}
	}
	// prevent server from sending 304 Not-Modified response
	// since we don't have a way to access the browser cache
	options.headers["If-Modified-Since"] = "Sun, 1 Jan 1995 00:00:00 GMT";
	options.headers["Cache-Control"] = "no-cache";
	options.headers.Pragma = "no-cache";

	if ("number" !== typeof options.timeout) {
		options.timeout = 10000;// 10 seconds
	}
	if ("function" !== typeof options.onCreate) {
		options.onCreate = null;
	}
	if ("function" !== typeof options.onSuccess) {
		options.onSuccess = null;
	}
	if ("function" !== typeof options.onFailure) {
		options.onFailure = JsonFx.IO.onFailure;
	}
	if ("function" !== typeof options.onTimeout) {
		options.onTimeout = null;
	}
	if ("function" !== typeof options.onComplete) {
		options.onComplete = null;
	}
	if ("undefined" === typeof options.context) {
		options.context = null;
	}
	return options;
};

/*void*/ JsonFx.IO.sendRequest = function(
	/*string*/ url,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	var xhr = new XMLHttpRequest();

	if (options.onCreate) {
		// create
		options.onCreate(xhr, options.context);
	}

	if (!xhr) {
		if (options.onFailure) {
			// immediate failure: xhr wasn't created
			options.onFailure(xhr, options.context, new Error("XMLHttpRequest not supported"));
		}
		if (options.onComplete) {
			// complete
			options.onComplete(xhr, options.context);
		}
		return;
	}

	// kill off request if takes too long
	var cancel = window.setTimeout(
		function () {
			if (xhr) {
				xhr.onreadystatechange = function(){};
				xhr.abort();
				xhr = null;
			}
			if (options.onTimeout) {
				// timeout-specific handler
				options.onTimeout(xhr, options.context, new Error("Request Timeout"));
			} else if (options.onFailure) {
				// general-failure handler
				options.onFailure(xhr, options.context, new Error("Request Timeout"));
			}
			if (options.onComplete) {
				// complete
				options.onComplete(xhr, options.context);
			}
		}, options.timeout);

	function onRSC() {
		/*
			var readyStates = [
					"uninitialized",
					"loading",
					"loaded",
					"interactive",
					"complete"
				];

			try { document.body.appendChild(document.createTextNode((xhr?readyStates[xhr.readyState]:"null")+";")); } catch (ex) {}
		*/
		if (xhr && xhr.readyState === 4 /*complete*/) {

			// stop the timeout
			window.clearTimeout(cancel);

			// check the status
			var status = 0;
			try {
				status = Number(xhr.status);
			} catch (ex) {
				// Firefox doesn't allow status to be accessed after xhr.abort()
			}

			if (status === 0) {
				// timeout

				// IE reports status zero when aborted
				// Firefox throws exception, which we set to zero
				// options.onTimeout has already been called so do nothing
				// timeout calls onComplete
				return;

			} else if (Math.floor(status/100) === 2) {// 200-299
				// success
				if (options.onSuccess) {
					options.onSuccess(xhr, options.context);
				}

			} else if (options.onFailure) { // status not 200-299
				// failure
				var ex = new Error(xhr.statusText);
				ex.code = status;
				options.onFailure(xhr, options.context, ex);
			}

			if (options.onComplete) { // all
				// complete
				options.onComplete(xhr, options.context);
			}
			xhr = null;
		}
	}

	try {
		xhr.onreadystatechange = onRSC;
		xhr.open(options.method, url, options.async);

		if (options.headers) {
			try {// Opera 8.0 doesn't have xhr.setRequestHeader
				for (var h in options.headers) {
					if (options.headers.hasOwnProperty(h)) {
						xhr.setRequestHeader(h, options.headers[h]);
					}
				}
			} catch (ex) {
				alert("xhr.setRequestHeader: "+(ex.message?ex.message:ex));
			}
		}
		xhr.send(options.params);

	} catch (ex) {
		// immediate failure: exception thrown
		if (options.onFailure) {
			options.onFailure(xhr, options.context, ex);
		}

	} finally {
		// in case immediately returns?
		onRSC();
	}
};

/* JsonRequest ----------------------------------------------------*/

/*void*/ JsonFx.IO.sendJsonRequest = function (
	/*string*/ restUrl,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	options.headers["User-Agent"] = JsonFx.userAgent;
	options.headers.Accept = "application/json";

	var onSuccess = options.onSuccess;
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ context) {

		// decode response as JSON
		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? json.parseJSON() : null;

			if ("function" === typeof onSuccess) {
				onSuccess(json, context);
			}
		} catch (ex) {
			if (options.onFailure) {
				options.onFailure(xhr, context, ex);
			}
		} finally {
			// free closure references
			onSuccess = options = null;
		}
	};

	if (options.onFailure) {
		var onFailure = options.onFailure;
		options.onFailure = function (/*XMLHttpRequest*/ xhr, /*object*/ context, /*Error*/ ex) {

			onFailure((xhr ? xhr.responseText : null), context, ex);

			// free closure references
			onFailure = null;
		};
	}

	JsonFx.IO.sendRequest(restUrl, options);
};

/* JSON-RPC ----------------------------------------------------*/

/*string*/ JsonFx.IO.jsonRpcPathEncode = function (/*string*/ rpcMethod, /*object|array*/ rpcParams) {
	var enc = encodeURIComponent;
	var rpcUrl = "/";
	if (rpcMethod) {
		rpcUrl += enc(rpcMethod);
	}
	if ("object" === typeof rpcParams) {
		rpcUrl += "?";
		if (rpcParams instanceof Array) {
			for (var i=0; i<rpcParams.length; i++) {
				if (i > 0) {
					rpcUrl += "&";
				}
				rpcUrl += enc(i);
				rpcUrl += "=";
				rpcUrl += enc(rpcParams[i]);
			}
		} else {
			for (var p in rpcParams) {
				rpcUrl += enc(p);
				rpcUrl += "=";
				rpcUrl += enc(rpcParams[p]);
			}
		}
	}
};

/*void*/ JsonFx.IO.sendJsonRpc = function(
	/*string*/ rpcUrl,
	/*string*/ rpcMethod,
	/*object|array*/ rpcParams,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	options.headers["User-Agent"] = JsonFx.userAgent;
	options.headers.Accept = "application/json";

	// wrap callbacks with RPC layer
	var onSuccess = options.onSuccess;
	var onFailure = options.onFailure;

	// this calls onSuccess with the results of the method (not the RPC wrapper)
	// or it calls onFailure with the error of the method (not the RPC wrapper)
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ cx) {

		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? json.parseJSON() : null;

			if (json.error) {
				if (onFailure) {
					onFailure(json, cx, json.error);
				}
			} else {
				if (onSuccess) {
					onSuccess(json.result, cx);
				}
			}

		} catch (ex) {
			if (onFailure) {
				onFailure(json, cx, ex);
			}
		}

		// free closure references
		onFailure = onSuccess = null;
	};

	// this calls onFailure with the RPC response
	options.onFailure = function(/*XMLHttpRequest*/ xhr, /*object*/ cx, /*Error*/ ex) {

		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? json.parseJSON() : null;

			if (onFailure) {
				onFailure(json, cx, ex);
			}
		} catch (ex2) {
			if (onFailure) {
				onFailure(json, cx, ex?ex:ex2);
			}
		}

		// free closure references
		onFailure = null;
	};

	if ("object" !== typeof rpcParams) {// must be object or array, else wrap in one
		rpcParams = [ rpcParams ];
	}

	if (options.method === "GET") {
		// GET RPC is encoded as part the URL
		rpcUrl += JsonFx.IO.jsonRpcPathEncode(rpcMethod, rpcParams);

	} else {
		// POST RPC is encoded as a JSON body
		var rpcRequest = {
				"version" : "1.1",
				"method" : rpcMethod,
				"params" : rpcParams
			};

		try {
			// JSON encode request object
			rpcRequest = rpcRequest.toJSONString();
		} catch (ex) {
			// if violates JSON, then fail
			return false;
		}

		options.params = rpcRequest;
		options.headers["Content-Type"] = "application/json";
		options.headers["Content-Length"] = rpcRequest.length;
	}
	JsonFx.IO.sendRequest(rpcUrl, options);
};

/* JsonRpcService ----------------------------------------------------*/

/* Base type for generated JSON Services */
if ("undefined" === typeof JsonFx.IO.JsonRpcService) {

	/* Ctor */
	JsonFx.IO.JsonRpcService = function() {};

	/*event*/ JsonFx.IO.JsonRpcService.prototype.onBeginRequest = null;

	/*event*/ JsonFx.IO.JsonRpcService.prototype.onEndRequest = null;

	/*void*/ JsonFx.IO.JsonRpcService.prototype.callService = function(
		/*string*/ rpcMethod,
		/*object*/ rpcParams,
		/*RequestOptions*/ options) {

		// ensure defaults
		options = JsonFx.IO.validateOptions(options);

		if ("function" === typeof this.onEndRequest) {
			var self = this;

			// intercept onComplete to call onEndRequest
			var onComplete = options.onComplete;
			options.onComplete = function(/*JSON*/ json, /*object*/ cx) {
				self.onEndRequest(cx);

				if (onComplete) {
					onComplete(json, cx);
				}

				// free closure references				
				self = onComplete = null;
			};
		}

		if ("function" === typeof this.onBeginRequest) {
			this.onBeginRequest(options.context);
		}

		JsonFx.IO.sendJsonRpc(this.address, rpcMethod, rpcParams, options);
	};

	// service description is callable via two methods
	/*string*/ JsonFx.IO.JsonRpcService.prototype["system.describe"] = JsonFx.IO.JsonRpcService.prototype.$describe = function(
		/*RequestOptions*/ options) {

		this.callService("system.describe", null, options);
	};
}

/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-03-12-2349
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*element*/ elem, /*string*/ style) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	if (elem.currentStyle) {
		// IE only
		if (style === "float") {
			style = "styleFloat";
		} else {
			// convert property name to camelCase
			style = style.split('-');
			style[0] = style[0].toLowerCase();
			for (var i=1; i<style.length; i++) {
				style[i] = style[i].charAt(0).toUpperCase()+style[i].substr(1).toLowerCase();
			}
			style = style.join("");
		}
		return elem.currentStyle[style];
	}
	return null;
};

/*{left,top}*/ JsonFx.UI.getOffset = function(/*element*/ elem) {
	var top=0, left=0;
	while (elem) {
		top += elem.offsetTop;
		left += elem.offsetLeft;
		elem = elem.offsetParent;
		if (elem) {
			var pos = JsonFx.UI.getStyle(elem, "position");
			if (pos && pos !== "static") {
				elem = null;
			}
		}
	}
	return { "left":left, "top":top };
};

/*void*/ JsonFx.UI.clearTextSelection = function() {
	if (window.getSelection && window.getSelection().removeAllRanges) {
		window.getSelection().removeAllRanges();
	} else if (document.getSelection && document.getSelection().empty) {
		document.getSelection().empty();
	} else if (document.selection && document.selection.empty) {
		document.selection.empty();
	}
};

/*function*/ JsonFx.UI.combineHandlers = function (/*function*/ handlerA, /*function*/ handlerB) {
	if ("function" === typeof handlerA) {
		if ("function" === typeof handlerB) {
			return function(/*Event*/ evt) { handlerA(evt); return handlerB(evt); };
		} else {
			return handlerA;
		}
	} else {
		return handlerB;
	}
};

/*element*/ JsonFx.UI.getIFrameDocument = function(/*element*/ elem) {
	if (!elem) {
		return null;
	}

	return ("undefined" !== typeof elem.contentDocument) ?
		elem.contentDocument : elem.contentWindow.document;
};

/*string*/ JsonFx.UI.toHtmlColor = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("Color components need to be numbers from 0x00 to 0xFF");
	}

	/*int*/ var c = (((Math.floor(r)<<8)+Math.floor(g))<<8)+Math.floor(b);

	/*string*/ var hex = "";
	for (var i=0; i<6; i++) {
		hex = Math.floor(c % 0x10).toString(16) + hex;
		c = Math.floor(c / 0x10);
	}

	return "#"+hex;
};

/*{r,g,b}*/ JsonFx.UI.fromHtmlColor = function(/*string*/ hex) {
	if (hex) {
		if (hex.match(/#([0-9a-fA-F]{1,2})([0-9a-fA-F]{1,2})([0-9a-fA-F]{1,2})/)) {// HEX colors
			if (hex.length === 7) {
				// e.g. "#336699"
				return {
						"r":Number("0x"+RegExp.$1),
						"g":Number("0x"+RegExp.$2),
						"b":Number("0x"+RegExp.$3)
					};
			} else if (hex.length === 4) {
				// e.g. "#369" === "#336699"
				return {
						"r":Number("0x"+RegExp.$1+RegExp.$1),
						"g":Number("0x"+RegExp.$2+RegExp.$2),
						"b":Number("0x"+RegExp.$3+RegExp.$3)
					};
			}
		}
		if (hex.match(/rgb[\(](\d+),\s*(\d+),\s*(\d+)[\)]/)) {// Firefox colors
			return {
					"r":Number(RegExp.$1),
					"g":Number(RegExp.$2),
					"b":Number(RegExp.$3)
				};
		}
	}
	return {"r":NaN, "g":NaN, "b":NaN};
};

/*float*/ JsonFx.UI.lerp = function (/*float*/ start, /*float*/ end, /*float*/ t) {
	return (start * (1-t)) + (end * t);
};

/*int*/ JsonFx.UI.lerpInt = function (/*int*/ start, /*int*/ end, /*float*/ t) {
	return Math.floor( JsonFx.UI.lerp(start, end, t) + 0.5 );
};

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BIND = "B", UNBIND = "U";

	/*hashtable[tag] of object*/ var bindings = {};

	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem)*/ bind, /*function(elem)*/ unbind) {
		if (typeof css !== "string") {
			throw new Error("Binding CSS is not a string.");
		}
		if (typeof tag !== "string") {
			throw new Error("Binding tag for \""+css+"\" is not a string.");
		}
		if (bind && typeof bind !== "function") {
			throw new Error("Binding method for \""+css+"\" is not a function.");
		}
		if (unbind && typeof unbind !== "function") {
			throw new Error("Unbinding method for \""+css+"\" is not a function.");
		}

		if (bind || unbind) {
			tag = tag.toLowerCase();
			if ("undefined" === typeof bindings[tag]) {
				/*object*/ bindings[tag] = {};
			} else if (bindings[tag][css]) {
				throw new Error("Binding for tag=\""+tag+"\" css=\""+css+"\" has already been registered.");
			}

			/*object*/ bindings[tag][css] = {};
			bindings[tag][css][BIND] = bind ? bind : null;
			bindings[tag][css][UNBIND] = unbind ? unbind : null;
		}
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		if (elem && elem.tagName && elem.className) {

			// only perform on registered tags
			var tag = elem.tagName.toLowerCase();
			if (bindings[tag]) {
				var tagBindings = bindings[tag];
				var classes = elem.className.split(/\s+/);

				// for each css class in elem
				for (var i=0; i<classes.length; i++) {
					var css = classes[i];
					if (css && tagBindings[css] && tagBindings[css][a]) {

						// perform action on element							
						tagBindings[css][a](elem);
					}
				}
			}
		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in bindings) {
				if (bindings.hasOwnProperty(tag)) {

					// for each element in root with tagName
					var elems = root.getElementsByTagName(tag);
					for (var i=0; i<elems.length; i++) {
						performOne(elems[i], a);
					}
				}
			}
		}
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		return performOne(elem, BIND);
	};

	// bind
	/*void*/ b.bindAll = function() {
		var ttl, crsr;
		if (document.body && document.body.style) {
			// store previous values
			ttl = document.body.title;
			crsr = document.body.style.cursor;

			// setup loading visual cues
			document.body.title = "Loading...";
			document.body.style.cursor = "wait";
		}

		try {
			perform(document, BIND);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl ? ttl : "";
				document.body.style.cursor = crsr ? crsr : "";
			}
		}
	};

	// unbind
	/*void*/ b.unbindAll = function() {
		var ttl, crsr;
		if (document.body && document.body.style) {
			// store previous values
			ttl = document.body.title;
			crsr = document.body.style.cursor;

			// setup loading visual cues
			document.body.title = "Unloading...";
			document.body.style.cursor = "wait";
		}

		try {
			perform(document, UNBIND);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl ? ttl : "";
				document.body.style.cursor = crsr ? crsr : "";
			}
		}
	};

	// bind
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BIND);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UNBIND);
	};

	// wire up binding
	if ("function" === typeof window.onload) {
		window.onload = JsonFx.UI.combineHandlers(b.bindAll, window.onload);
	} else {
		window.onload = b.bindAll;
	}

	// wire up unbinding
	if ("function" === typeof window.onunload) {
		window.onunload = JsonFx.UI.combineHandlers(b.unbindAll, window.onload);
	} else {
		window.onunload = b.unbindAll;
	}
};

// instantiate only one, destroying the constructor
JsonFx.UI.Bindings = new JsonFx.UI.Bindings();

/*------------------*\
	History Methods
\*------------------*/

/* singleton JsonFx.UI.History */
JsonFx.UI.History = {};

JsonFx.UI.History.h = null;
JsonFx.UI.History.onchange = null;

/*void*/ JsonFx.UI.History.add = function(/*object*/ info) {
	var h = JsonFx.UI.getIFrameDocument(JsonFx.UI.History.h);
	if (h) {
		h.location.search = encodeURIComponent(info.toJSONString());
	}
};

/*void*/ JsonFx.UI.History.changed = function(/*element*/ elem) {
	if (!JsonFx.UI.History.h) {
		JsonFx.UI.History.h = elem;
	}
	var h = JsonFx.UI.getIFrameDocument(elem);
	if (h) {
		var info = h.location.search;
		if (info) {
			if (info.charAt(0) === '?') {
				info = info.substring(1);
			}
			info = decodeURIComponent(info).parseJSON();
			if ("function" === typeof JsonFx.UI.History.onchange) {
				JsonFx.UI.History.onchange(info);
			}
		}
	}
};

/*------------------*\
	JsonML Methods
\*------------------*/

/*void*/ JsonFx.UI.clear = function(/*element*/ elem) {
	if (elem) {
		// unbind to prevent memory leaks
		JsonFx.UI.Bindings.unbind(elem);

		while (elem.lastChild) {
			elem.removeChild(elem.lastChild);
		}
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*element*/ JsonFx.UI.displayJsonML = function(/*JsonML*/ jml, /*element|string*/ container) {

	// either DOM element or id
	container = ("string" === typeof container) ?
		document.getElementById(container) : container;

	if (jml && ("function" === typeof jml.parseJsonML)) {
		jml = jml.parseJsonML(JsonFx.UI.Bindings.bindOne);
		if (jml) {
			if (container) {
				JsonFx.UI.clear(container);
				container.appendChild(jml);
			} else if (jml.id) {
				container = document.getElementById(jml.id);
				if (container && container.parentNode) {
					// unbind to prevent memory leaks
					JsonFx.UI.Bindings.unbind(container);
					container.parentNode.replaceChild(jml, container);
				} else {
					// can't add to document, so unbind
					JsonFx.UI.Bindings.unbind(jml);
					jml = null;
				}
			} else if (jml.childNodes) {
				while (jml.firstChild) {
					if (!jml.firstChild.id) {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(jml.firstChild);
						jml.removeChild(jml.firstChild);
						continue;
					}
					container = document.getElementById(jml.firstChild.id);
					if (container && container.parentNode) {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(container);
						container.parentNode.replaceChild(jml.firstChild, container);
					} else {
						// unbind to prevent memory leaks
						JsonFx.UI.Bindings.unbind(jml.firstChild);
						jml.removeChild(jml.firstChild);
					}
				}
				container = null;
			}
		}
	}
	return container;
};

/*void*/ JsonFx.UI.loadJsonML = function (
	/*string*/ url,
	/*element|string*/ container,
	/*RequestOptions*/ options) {

	options = JsonFx.IO.validateOptions(options);
	options.method = "GET";

	if (options.onCreate) {
		var onCreate = options.onCreate;
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

	if (options.onFailure) {
		var onFailure = options.onFailure;
		options.onFailure = function (/*JSON*/ jml, /*object*/ cx, /*Error*/ ex) {
			// failure callback
			onFailure(cx, ex);

			// free closure references
			onFailure = null;
		};
	}

	if (options.onTimeout) {
		var onTimeout = options.onTimeout;
		options.onTimeout = function (/*JSON*/ jml, /*object*/ cx, /*Error*/ ex) {
			// timeout callback
			onTimeout(cx, ex);

			// free closure references
			onTimeout = null;
		};
	}

	if (options.onComplete) {
		var onComplete = options.onComplete;
		options.onComplete = function (/*JSON*/ jml, /*object*/ cx) {
			// complete callback
			onComplete(cx);

			// free closure references
			onComplete = 1;
		};
	}

	JsonFx.IO.sendJsonRequest(url, options);
};

/* DataDump ----------------------------------------------------*/

/*bool*/ JsonFx.UI.expando = function (/*element*/ elem, /*string*/ target) {
	var expando = elem.expando;
	if (!expando) {
		if ("string" === typeof target) {
			target = document.getElementById(target);
		}
		if (!target) {
			return false;
		}
		elem.expando = expando = new JsonFx.UI.Animate.Engine(target);
	}
	if (expando.hasAppliedOp()) {
		if (elem.value) {// hacky swap out for buttons
			elem.value = " \u2212 ";
			//elem.replaceChild(document.createTextNode("\u2212"), elem.firstChild);
		}
		expando.apply(null, false);
		return false;
	} else {
		var op = new JsonFx.UI.Animate.Op();
		op.fade(0);
		op.height(0);
		op.speed(0.65);
		if (elem.value) {// hacky swap out for buttons
			elem.value = " + ";
			//elem.replaceChild(document.createTextNode("+"), elem.firstChild);
		}
		expando.apply(op, false);
		return true;
	}
};

/*void*/ JsonFx.UI.expandoBind = function(/*element*/ elem) {
	// call after elements have been added to document
	window.setTimeout(
		function() {
			var target = document.getElementById(elem.getAttribute("for"));
			if (!target) {
				target = elem.nextSibling;
				while (target && !target.tagName) {
					target = target.nextSibling;
				}
			}
			if (target) {
				try {
					elem.style.cursor = "pointer";
				} catch (ex) {
					elem.style.cursor = "hand";
				}
				elem.className += " jsonfx-expanded";

				elem.onclick = function (/*event*/ evt) {
					if (JsonFx.UI.expando(elem, target)) {
						elem.className = elem.className.replace(/\s*jsonfx-expanded/g, " jsonfx-collapsed");
					} else {
						elem.className = elem.className.replace(/\s*jsonfx-collapsed/g, " jsonfx-expanded");
					}
					return false;
				};
				elem.ondblclick = function (/*event*/ evt) {
					if (elem.click) {
						elem.click();
					}
					window.setTimeout( function(){JsonFx.UI.clearTextSelection();}, 0);
					return false;
				};
			}
		}, 0);
};
/*void*/ JsonFx.UI.expandoUnbind = function(/*element*/ elem) {
	if ("undefined" !== typeof elem.expando) {
		elem.expando = null;
	}
	elem.onclick = null;
	elem.ondblclick = null;
};

/*int*/ JsonFx.UI.dumpDataID = 0;
/*JsonML*/ JsonFx.UI.dumpData = function(/*string*/ name, /*json*/ val) {
	var c = ["span"];
	var type = typeof val;

	var a = false;
	if ("object" === type && val) {
		a = ["label", {"class":"jsonfx-expando"}];
		c.push(a);
	}
	(a?a:c).push(["span", {"class":"jsonfx-type"}, (val instanceof Array) ? "array" : type]);
	(a?a:c).push(["span", {"class":"jsonfx-name"}, name]);

	if ("object" === type && val) {
		var ul = ["ul", {"id":"JsonFx_UI_Dump_"+(JsonFx.UI.dumpDataID++),"class":"jsonfx-object"}];
		for (var pn in val) {
			if (!val.hasOwnProperty(pn)) {
				continue;
			}
			ul.push(["li", JsonFx.UI.dumpData(pn, val[pn])]);
		}

		if (a) {
			a[1]["for"] = ul[1].id;
		}
		c.push(ul);
	} else {
		c.push(["span", {"class":"jsonfx-value"}, String(val)]);
	}

	return c;
};

/*---------------------*\
	Animation Classes
\*---------------------*/

/* namespace JsonFx.UI.Animate */
JsonFx.UI.Animate = {};

/* class JsonFx.UI.Animate.Unit -------------------------------------------- */
JsonFx.UI.Animate.Unit = function(/*int*/ size, /*string*/ unit) {
	if (unit) {
		switch (unit) {
			case "px" :
				this.size = Math.floor(size);
				this.unit = unit;
				break;
			case "%" :
			case "em" :
			case "pt" :
			case "in" :
			case "cm" :
			case "mm" :
			case "pc" :
			case "ex" :
				this.size = Number(size);
				this.unit = unit;
				break;
			default:
				throw new Error("Invalid unit.");
		}
		if (!isFinite(this.size)) {
			throw new Error("Invalid unit.");
		}
	} else {
		this.size = NaN;
		this.unit = "";
	}
};
JsonFx.UI.Animate.Unit.prototype.toString = function() {
	return this.unit ? (this.size+this.unit) : "auto";
};
/*JsonFx.UI.Animate.Unit*/ JsonFx.UI.Animate.Unit.parse = function(/*string*/ str) {
	var size = parseFloat(str);
	var unit = null;
	if (isFinite(size) && str.match(/(px|%|em|pt|in|cm|mm|pc|ex)/)) {
		unit = RegExp.$1;
	}
	return new JsonFx.UI.Animate.Unit(size, unit);
};

/* class JsonFx.UI.Animate.Op -------------------------------------------- */
JsonFx.UI.Animate.Op = function() {
	this.x = this.y = this.w = this.h =
	this.z = this.t = this.r = this.b = this.l = 
	this.cL = this.cR = this.cT = this.cB =
	this.f = this.bl = NaN;
	this.c = { "r":NaN, "g":NaN, "b":NaN };
	this.bc = { "r":NaN, "g":NaN, "b":NaN };
	this.s = 0.05;// 20 steps
};

/*Regex*/ JsonFx.UI.Animate.Op.clipRE = /^rect[\(]([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)\s*([0-9\.]*)(auto|px|%)[\)]$/;

/*void*/ JsonFx.UI.Animate.Op.prototype.fade = function(/*float*/ f) {
	if (!isFinite(f) || f<0 || f>1) {
		throw new Error("Fade is a number from 0.0 to 1.0");
	}
	this.f = Number(f);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasFade = function() {
	return isFinite(this.f);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.blur = function(/*float*/ b) {
	if (!isFinite(b) || b<0) {
		throw new Error("Blur is a number greater than 0");
	}
	this.bl = Number(b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBlur = function() {
	return isFinite(this.bl);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.color = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("Color is a triplet of numbers from 0x00 to 0xFF");
	}
	this.c.r = Number(r);
	this.c.g = Number(g);
	this.c.b = Number(b);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.colorHtml = function(/*string*/ c) {
	c = JsonFx.UI.fromHtmlColor(c);
	this.color(c.r, c.g, c.b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasColor = function() {
	return !!this.c &&
		isFinite(this.c.r) &&
		isFinite(this.c.g) &&
		isFinite(this.c.b);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.backgroundColor = function(/*int*/ r, /*int*/ g, /*int*/ b) {
	if (!isFinite(r) || r<0x00 || r>0xFF ||
		!isFinite(g) || g<0x00 || g>0xFF ||
		!isFinite(b) || b<0x00 || b>0xFF) {
		throw new Error("BackgroundColor is a triplet of numbers from 0x00 to 0xFF");
	}
	this.bc.r = Number(r);
	this.bc.g = Number(g);
	this.bc.b = Number(b);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.backgroundColorHtml = function(/*string*/ c) {
	c = JsonFx.UI.fromHtmlColor(c);
	this.backgroundColor(c.r, c.g, c.b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBackgroundColor = function() {
	return !!this.bc &&
		isFinite(this.bc.r) &&
		isFinite(this.bc.g) &&
		isFinite(this.bc.b);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.zoom = function(/*float*/ z) {
	if (!isFinite(z)) {
		throw new Error("Zoom is a number >= 0.01 with 1.0 being normal");
	}
	if (z < 0.01) {
		z = 0.01;
	}
	this.z = Number(z);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasZoom = function() {
	return isFinite(this.z);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.move = function(/*int*/ left, /*int*/ top) {
	this.top(top);
	this.left(left);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.top = function(/*int*/ top) {
	if (!isFinite(top)) {
		throw new Error("Top is a pixel position");
	}
	this.t = Number(top);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.right = function(/*int*/ right) {
	if (!isFinite(right)) {
		throw new Error("Right is a pixel position");
	}
	this.r = Number(right);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.bottom = function(/*int*/ bottom) {
	if (!isFinite(bottom)) {
		throw new Error("Bottom is a pixel position");
	}
	this.b = Number(bottom);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.left = function(/*int*/ left) {
	if (!isFinite(left)) {
		throw new Error("Left is a pixel position");
	}
	this.l = Number(left);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasMove = function() {
	return isFinite(this.t)||isFinite(this.r)||isFinite(this.b)||isFinite(this.l);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasTop = function() {
	return isFinite(this.t);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasRight = function() {
	return isFinite(this.r);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasBottom = function() {
	return isFinite(this.b);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasLeft = function() {
	return isFinite(this.l);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.scaleX = function(/*int*/ x) {
	if (!isFinite(x) || x<0) {
		throw new Error("Scale X is a number >= 0.0 with 1.0 being normal");
	}
	this.x = Number(x);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.scaleY = function(/*int*/ y) {
	if (!isFinite(y) || y<0) {
		throw new Error("Scale Y is a number >= 0.0 with 1.0 being normal");
	}
	this.y = Number(y);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.scale = function(/*int*/ x, /*int*/ y) {
	this.scaleX(x);
	this.scaleY(y);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.width = function(/*int*/ width) {
	if (!isFinite(width) || width<0) {
		throw new Error("Width is a non-negative pixel size");
	}
	this.w = Number(width);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.height = function(/*int*/ height) {
	if (!isFinite(height) || height<0.0) {
		throw new Error("Height is a non-negative pixel size");
	}
	this.h = Number(height);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScaleX = function() {
	return isFinite(this.x)||isFinite(this.w);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScaleY = function() {
	return isFinite(this.y)||isFinite(this.h);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasScale = function() {
	return this.hasScaleX()||this.hasScaleY();
};

/*void*/ JsonFx.UI.Animate.Op.prototype.resize = function(/*int*/ width, /*int*/ height) {
	this.width(width);
	this.height(height);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasResize = function() {
	return isFinite(this.w)||isFinite(this.h);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasWidth = function() {
	return isFinite(this.w);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasHeight = function() {
	return isFinite(this.h);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.clipTop = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Top is a number from 0.0 to 1.0");
	}
	this.cT = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipRight = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Right is a number from 0.0 to 1.0");
	}
	this.cR = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipBottom = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Bottom is a number from 0.0 to 1.0");
	}
	this.cB = Number(clip);
};
/*void*/ JsonFx.UI.Animate.Op.prototype.clipLeft = function(/*float*/ clip) {
	if (!isFinite(clip)) {
		throw new Error("Clip Left is a number from 0.0 to 1.0");
	}
	this.cL = Number(clip);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClip = function() {
	return isFinite(this.cT)||isFinite(this.cR)||isFinite(this.cB)||isFinite(this.cL);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipT = function() {
	return isFinite(this.cT);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipR = function() {
	return isFinite(this.cR);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipB = function() {
	return isFinite(this.cB);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.hasClipL = function() {
	return isFinite(this.cL);
};

/*bool*/ JsonFx.UI.Animate.Op.prototype.isHidden = function() {
	return (this.f<=0)||(this.w<=0)||(this.h<=0)||(this.x<=0)||(this.y<=0)||(this.z<=0.01);
};
/*bool*/ JsonFx.UI.Animate.Op.prototype.isRemoved = function() {
	return (this.h<=0)||(this.y<=0);
};

/*void*/ JsonFx.UI.Animate.Op.prototype.speed = function(/*int*/ s) {
	if (!isFinite(s) || s<=0 || s>1) {
		throw new Error("Speed is a number from 0.0 to 1.0");
	}
	this.s = Math.pow(s,4.5);
};

/*JsonFx.UI.Animate.Op*/ JsonFx.UI.Animate.Op.save = function(/*element*/ elem) {
	var op = new JsonFx.UI.Animate.Op();

	if (elem && elem.style) {
		var es = elem.style;
		var esPos = JsonFx.UI.getStyle(elem, "position");

		var top = parseFloat(JsonFx.UI.getStyle(elem, "top"));
		if (isFinite(top)) {
			op.top(top);
		} else if (esPos === "static") {
			op.top(0);
		}
		var right = parseFloat(JsonFx.UI.getStyle(elem, "right"));
		if (isFinite(right)) {
			op.right(right);
		} else if (esPos === "static") {
			op.right(0);
		}
		var bottom = parseFloat(JsonFx.UI.getStyle(elem, "bottom"));
		if (isFinite(bottom)) {
			op.right(bottom);
		} else if (esPos === "static") {
			op.bottom(0);
		}
		var left = parseFloat(JsonFx.UI.getStyle(elem, "left"));
		if (isFinite(left)) {
			op.left(left);
		} else if (esPos === "static") {
			op.left(0);
		}

		// width
		if (isFinite(elem.offsetWidth)) {
			op.width(elem.offsetWidth);
		} else {
			var width = parseFloat(JsonFx.UI.getStyle(elem, "width"));
			if (isFinite(width)) {
				op.width(width);
			}
		}
		// height
		if (isFinite(elem.offsetHeight)) {
			op.height(elem.offsetHeight);
		} else {
			var height = parseFloat(JsonFx.UI.getStyle(elem, "height"));
			if (isFinite(height)) {
				op.height(height);
			}
		}

		// scale
		op.scale(1, 1);

		// fade
		if (!!es.opacity && isFinite(es.opacity)) {
			op.fade(es.opacity);
		} else {
			op.fade(1);
		}

		// blur
		//warning: should extract blur?
		op.blur(0);

		// zoom
		if (!!es.zoom && isFinite(es.zoom)) {
			op.zoom(es.zoom);
		} else {
			op.zoom(1);
		}

		// color
		var color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "color"));
		if (isFinite(color.r) && isFinite(color.g) && isFinite(color.b)) {
			op.color(color.r, color.g, color.b);
		} else {
			op.color(0, 0, 0);
		}

		// backgroundColor
		color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "background-color"));
		if (isFinite(color.r) && isFinite(color.g) && isFinite(color.b)) {
			op.backgroundColor(color.r, color.g, color.b);
		} else {
			op.backgroundColor(0xFF, 0xFF, 0xFF);
		}

		// clip
		if (es.clip && es.clip.match(JsonFx.UI.Animate.Op.clipRE)) {
			if ("%" === RegExp.$2) {
				op.clipTop(RegExp.$1/100);
			} else if ("px" === RegExp.$2) {
				op.clipTop(RegExp.$1/op.h);
			} else {
				op.clipTop(0);
			}

			if ("%" === RegExp.$4) {
				op.clipRight(1-RegExp.$3/100);
			} else if ("px" === RegExp.$4) {
				op.clipRight(1-RegExp.$3/op.w);
			} else {
				op.clipRight(0);
			}

			if ("%" === RegExp.$6) {
				op.clipBottom(1-RegExp.$5/100);
			} else if ("px" === RegExp.$6) {
				op.clipBottom(1-RegExp.$5/op.h);
			} else {
				op.clipBottom(0);
			}

			if ("%" === RegExp.$8) {
				op.clipLeft(RegExp.$7/100);
			} else if ("px" === RegExp.$8) {
				op.clipLeft(RegExp.$7/op.w);
			} else {
				op.clipLeft(0);
			}
		} else {
			op.clipTop(0);
			op.clipRight(0);
			op.clipBottom(0);
			op.clipLeft(0);
		}
		es = null;
	}

	elem = null;
	return op;
};

/* class JsonFx.UI.Animate.Engine -------------------------------------------- */


/*Dictionary<string,bool>*/ JsonFx.UI.Animate.filterable =
	{
		a : true,
		abbr : true,
		acronym : true,
		address : true,
		b : true,
		bdo : true,
		big : true,
		blockquote : true,
		body : true,
		button : true,
		caption : true,
		center : true,
		cite : true,
		code : true,
		custom : true,
		dd : true,
		del : true,
		dfn : true,
		dir : true,
		div : true,
		dl : true,
		dt : true,
		em : true,
		fieldset : true,
		font : true,
		form : true,
		frame : true,
		hn : true,
		iframe : true,
		frameset : true,
		i : true,
		ins : true,
		img : true,
		input : true,
		kbd : true,
		label : true,
		legend : true,
		li : true,
		marquee : true,
		menu : true,
		nobr : true,
		ol : true,
		p : true,
		plaintext : true,
		pre : true,
		q : true,
		rt : true,
		ruby : true,
		s : true,
		samp : true,
		small : true,
		span : true,
		strike : true,
		strong : true,
		sub : true,
		sup : true,
		table : true,
		textarea : true,
		th : true,
		td : true,
		tt : true,
		u : true,
		ul : true,
		"var" : true,
		xmp : true
	};

JsonFx.UI.Animate.Engine = function(/*element*/ elem) {

	if ("string" === typeof elem) {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	var tn = elem ? elem.tagName : null;

	if (!tn) {
		throw new Error("Invalid element");
	}

	tn = tn.toLowerCase();

	var es = elem.style,
	/*JsonFx.UI.Animate.Op*/ start = null,
	/*JsonFx.UI.Animate.Op*/ op = new JsonFx.UI.Animate.Op(),
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ STEP_MIN = 0, // start
	/*const float*/ STEP_MAX = 1, // end
	/*Dictionary<string,bool>*/ filerable = JsonFx.UI.Animate.filterable;

	var userHeight = "",
		userWidth = "",
		userTop = "",
		userRight = "",
		userBottom = "",
		userLeft = "",
		userOverflow = "",
		userPosition = "",
		userDisplay = "",
		userVisibility = "",
		userFilter = "",
		userKhtml = "",
		userMoz = "",
		userOpacity = "",
		userZoom = "",
		userClip = "",
		userColor = "",
		userBGColor = "",
		alpha = null,
		blur = null;

	if (elem && es) {
		userOverflow = es.overflow;
		userPosition = es.position;
		userDisplay = es.display;
		userVisibility = es.visibility;
		userHeight = es.height;
		userWidth = es.width;
		userTop = es.top;
		userRight = es.right;
		userBottom = es.bottom;
		userLeft = es.left;
		userKhtml = es["-khtml-opacity"];
		userMoz = es["-moz-opacity"];
		userOpacity = es.opacity;
		userFilter = es.filter;
		userZoom = es.zoom;
		userClip = es.clip;
		userColor = es.color;
		userBGColor = es.backgroundColor;
	}

	/*void*/ function restore() {
//		if (!!JsonFx.UI.getStyle(elem, "display")) {
//			es.display = userDisplay;
//		}
//		if (!!JsonFx.UI.getStyle(elem, "visibility")) {
//			es.visibility = userVisibility;
//		}
		if (op.hasScale() || op.hasClip()) {
			es.overflow = userOverflow;
		}
		if (op.hasWidth()) {
			es.width = userWidth;
		}
		if (op.hasHeight()) {
			es.height = userHeight;
		}
		if (op.hasMove()) {
			es.position = userPosition;
			es.top = userTop;
			es.right = userRight;
			es.bottom = userBottom;
			es.left = userLeft;
		}
		if (op.hasFade()) {
			es["-khtml-opacity"] = userKhtml;
			es["-moz-opacity"] = userMoz;
			es.opacity = userOpacity;
			try {
				if (userFilter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasBlur()) {
			try {
				if (userFilter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasZoom()) {
			es.zoom = userZoom;
		}
		if (op.hasClip()) {
			es.clip = userClip ? userClip : "rect(auto auto auto auto)";// tested in IE/FireFox/Opera
		}
		if (op.hasColor()) {
			es.color = userColor;
		}
		if (op.hasBackgroundColor()) {
			es.backgroundColor = userBGColor;
		}
	}

	/*void*/ function showElem() {
		if (!!JsonFx.UI.getStyle(elem, "display")) {
			es.display = "block";
		}
		if (!!JsonFx.UI.getStyle(elem, "visibility")) {
			es.visibility = "visible";
		}
	}

	/*void*/ function initFilter(op) {
		try {
			// first look up tagName to determine if
			// element can even have filters applied
			// to prevent incurring calculation cost
			if (filerable[tn] &&
				elem.filters) {
				if (op.hasFade() && !alpha) {
					if (elem.filters.length > 0) {
						try {
							// check IE5.5+
							alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
						} catch (ex) { alpha = null; }
						if (!alpha) {
							try {
								// check IE4.0+
								alpha = elem.filters.item("alpha");
							} catch (ex) { alpha = null; }
						}
					}
					if (!alpha) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
						try {
							alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
						} catch (ex) { alpha = null; }
						if (!alpha) {
							// try IE4.0+
							es.filter += " alpha(enabled=false)";
							try {
								alpha = elem.filters.item("alpha");
							} catch (ex) { alpha = null; }
						}
					}
				}
				if (op.hasBlur() && !blur) {
					if (elem.filters.length > 0) {
						try {
							// check IE5.5+
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex) { blur = null; }
						if (!blur) {
							try {
								// check IE4.0+
								blur = elem.filters.item("blur");
							} catch (ex) { blur = null; }
						}
					}
					if (!blur) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Blur(enabled=false)";
						try {
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex) { blur = null; }
						if (!blur) {
							// try IE4.0+
							es.filter += " blur(enabled=false)";
							try {
								blur = elem.filters.item("blur");
							} catch (ex) { blur = null; }
						}
					}
				}
			}
		} catch (ex) {}
	}

	/*bool*/ this.hasAppliedOp = function() {
		return state;
	};

	// if newOp is null, the previous operation is reversed
	// immediate doesn't animate but applies operation
	/*void*/ this.apply = function(
		/*JsonFx.UI.Animate.Op*/ newOp,
		/*bool*/ immediate,
		/*function(cx)*/ callback,
		/*object*/ context) {

		if (!es) { return; }

		// state: true = perform op, false = reverse op
		state = !!newOp;
		immediate = !!immediate;
		if (mutex) {
			// crude concurrency check
			return;
		}
		mutex = true;

		if (state) {
			op = newOp;
		}
		if (!start) {
			start = JsonFx.UI.Animate.Op.save(elem);
		}
		showElem();

		// minimizeStep
		/*void*/ function t(/*float*/ step) {
			if (!es || isNaN(step)) {
				mutex = false;
				return;
			}

			var esPos = JsonFx.UI.getStyle(elem, "position");

			if (step < STEP_MIN) {
				step = STEP_MIN;
			} else if (step > STEP_MAX) {
				step = STEP_MAX;
			}

			if (op.hasScale() || op.hasClip()) {
				es.overflow = "hidden";
			}

			if (op.hasWidth() && start.hasWidth()) {
				es.width = JsonFx.UI.lerpInt(start.w, op.w, step)+"px";
			} else if (op.hasScaleX() && start.hasScaleX() && start.hasWidth()) {
				es.width = Math.ceil(start.w*JsonFx.UI.lerp(start.x, op.x, step))+"px";
			}
			if (op.hasHeight() && start.hasHeight()) {
				es.height = JsonFx.UI.lerpInt(start.h, op.h, step)+"px";
			} else if (op.hasScaleY() && start.hasScaleY() && start.hasHeight()) {
				es.height = Math.ceil(start.h*JsonFx.UI.lerp(start.y, op.y, step))+"px";
			}
			if (op.hasFade() && start.hasFade()) {
				// opacity
				es["-khtml-opacity"] = es["-moz-opacity"] = es.opacity = JsonFx.UI.lerp(start.f, op.f, step);
				initFilter(op);
				if (alpha) {
					try {
						if (!elem.currentStyle.hasLayout) {
							// this might have side-effects, but should be rare
							// http://msdn.microsoft.com/workshop/author/dhtml/reference/properties/haslayout.asp
							// http://www.satzansatz.de/cssd/onhavinglayout.html
							es.zoom = "1";
						}
						alpha.opacity = JsonFx.UI.lerpInt(100*start.f, 100*op.f, step);
						alpha.enabled = true;
					} catch (ex) {
						alpha = null;
					}
				}
			}
			if (op.hasBlur()) {
				initFilter(op);
				if (blur) {
					try {
						if (!elem.currentStyle.hasLayout) {
							// this might have side-effects, but should be rare
							// http://msdn.microsoft.com/workshop/author/dhtml/reference/properties/haslayout.asp
							// http://www.satzansatz.de/cssd/onhavinglayout.html
							es.zoom = "1";
						}
						blur.pixelRadius = JsonFx.UI.lerpInt(start.bl, op.bl, step);
						blur.enabled = true;
					} catch (ex) {
						blur = null;
					}
				}
			}
			if (op.hasZoom() && start.hasZoom()) {
				es.zoom = JsonFx.UI.lerpInt(100*start.z, 100*op.z, step)+"%";
			}
			if (op.hasBackgroundColor() && start.hasBackgroundColor()) {
				es.backgroundColor = JsonFx.UI.toHtmlColor(
					JsonFx.UI.lerpInt(start.bc.r, op.bc.r, step),
					JsonFx.UI.lerpInt(start.bc.g, op.bc.g, step),
					JsonFx.UI.lerpInt(start.bc.b, op.bc.b, step)
				);
			}
			if (op.hasColor() && start.hasColor()) {
				es.color = JsonFx.UI.toHtmlColor(
					JsonFx.UI.lerpInt(start.c.r, op.c.r, step),
					JsonFx.UI.lerpInt(start.c.g, op.c.g, step),
					JsonFx.UI.lerpInt(start.c.b, op.c.b, step)
				);
			}
			if (op.hasClip()) {
				var clip = ["auto","auto","auto","auto"];
				if (op.hasClipT() && start.hasClipT()) {
					clip[0] = Math.ceil(JsonFx.UI.lerp(start.cT, op.cT, step)*100)+"%";
				}
				if (op.hasClipR() && start.hasClipR()) {
					clip[1] = Math.floor(JsonFx.UI.lerp(1-start.cR, 1-op.cR, step)*100)+"%";
				}
				if (op.hasClipB() && start.hasClipB()) {
					clip[2] = Math.floor(JsonFx.UI.lerp(1-start.cB, 1-op.cB, step)*100)+"%";
				}
				if (op.hasClipL() && start.hasClipL()) {
					clip[3] = Math.ceil(JsonFx.UI.lerp(start.cL, op.cL, step)*100)+"%";
				}
				if (esPos !== "fixed") {
					es.position = "absolute";
				}
				es.clip = "rect("+clip.join(' ')+")";
			}
			if (op.hasMove()) {
				if (!esPos || esPos === "static") {
					es.position = "relative";
				}
				if (op.hasTop() && start.hasTop()) {
					es.top = JsonFx.UI.lerpInt(start.t, op.t, step)+"px";
				}
				if (op.hasRight() && start.hasRight()) {
					es.right = JsonFx.UI.lerpInt(start.r, op.r, step)+"px";
				}
				if (op.hasBottom() && start.hasBottom()) {
					es.bottom = JsonFx.UI.lerpInt(start.b, op.b, step)+"px";
				}
				if (op.hasLeft() && start.hasLeft()) {
					es.left = JsonFx.UI.lerpInt(start.l, op.l, step)+"px";
				}
			}

			if (step <= STEP_MIN && !state) {
				setTimeout(
					function() {
						restore();
						start = null;
						op = new JsonFx.UI.Animate.Op();
						mutex = false;

						if ("function" === typeof callback) {
							callback(context);
						}
					},
					0);
			} else if (step >= STEP_MAX && state) {
				setTimeout(
					function() {
						if (op.isRemoved()) {
							es.display = "none";
						} else if (op.isHidden()) {
							es.visibility = "hidden";
						}
						mutex = false;

						if ("function" === typeof callback) {
							callback(context);
						}
					},
					0);
			} else {
				setTimeout(
					function() {
						t(state ? (step+op.s) : (step-op.s));
					},
					0);
			}
		}

		// XOR: state OR immediate but not both
		// immediate forces to far end of step
		t(state^immediate ? STEP_MIN : STEP_MAX);
	};
};
