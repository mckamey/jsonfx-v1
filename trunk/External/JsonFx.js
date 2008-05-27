/*
    http://www.JSON.org/json2.js
    2008-05-25

    Public Domain.

    NO WARRANTY EXPRESSED OR IMPLIED. USE AT YOUR OWN RISK.

    See http://www.JSON.org/js.html

    This file creates a global JSON object containing two methods: stringify
    and parse.

        JSON.stringify(value, replacer, space)
            value       any JavaScript value, usually an object or array.

            replacer    an optional parameter that determines how object
                        values are stringified for objects without a toJSON
                        method. It can be a function or an array.

            space       an optional parameter that specifies the indentation
                        of nested structures. If it is omitted, the text will
                        be packed without extra whitespace. If it is a number,
                        it will specify the number of spaces to indent at each
                        level. If it is a string (such as '\t' or '&nbsp;'),
                        it contains the characters used to indent at each level.

            This method produces a JSON text from a JavaScript value.

            When an object value is found, if the object contains a toJSON
            method, its toJSON method will be called and the result will be
            stringified. A toJSON method does not serialize: it returns the
            value represented by the name/value pair that should be serialized,
            or undefined if nothing should be serialized. The toJSON method
            will be passed the key associated with the value, and this will be
            bound to the object holding the key.

            For example, this would serialize Dates as ISO strings.

                Date.prototype.toJSON = function (key) {
                    function f(n) {
                        // Format integers to have at least two digits.
                        return n < 10 ? '0' + n : n;
                    }

                    return this.getUTCFullYear()   + '-' +
                         f(this.getUTCMonth() + 1) + '-' +
                         f(this.getUTCDate())      + 'T' +
                         f(this.getUTCHours())     + ':' +
                         f(this.getUTCMinutes())   + ':' +
                         f(this.getUTCSeconds())   + 'Z';
                };

            You can provide an optional replacer method. It will be passed the
            key and value of each member, with this bound to the containing
            object. The value that is returned from your method will be
            serialized. If your method returns undefined, then the member will
            be excluded from the serialization.

            If the replacer parameter is an array, then it will be used to
            select the members to be serialized. It filters the results such
            that only members with keys listed in the replacer array are
            stringified.

            Values that do not have JSON representations, such as undefined or
            functions, will not be serialized. Such values in objects will be
            dropped; in arrays they will be replaced with null. You can use
            a replacer function to replace those with JSON values.
            JSON.stringify(undefined) returns undefined.

            The optional space parameter produces a stringification of the
            value that is filled with line breaks and indentation to make it
            easier to read.

            If the space parameter is a non-empty string, then that string will
            be used for indentation. If the space parameter is a number, then
            the indentation will be that many spaces.

            Example:

            text = JSON.stringify(['e', {pluribus: 'unum'}]);
            // text is '["e",{"pluribus":"unum"}]'


            text = JSON.stringify(['e', {pluribus: 'unum'}], null, '\t');
            // text is '[\n\t"e",\n\t{\n\t\t"pluribus": "unum"\n\t}\n]'

            text = JSON.stringify([new Date()], function (key, value) {
                return this[key] instanceof Date ?
                    'Date(' + this[key] + ')' : value;
            });
            // text is '["Date(---current time---)"]'


        JSON.parse(text, reviver)
            This method parses a JSON text to produce an object or array.
            It can throw a SyntaxError exception.

            The optional reviver parameter is a function that can filter and
            transform the results. It receives each of the keys and values,
            and its return value is used instead of the original value.
            If it returns what it received, then the structure is not modified.
            If it returns undefined then the member is deleted.

            Example:

            // Parse the text. Values that look like ISO date strings will
            // be converted to Date objects.

            myData = JSON.parse(text, function (key, value) {
                var a;
                if (typeof value === 'string') {
                    a =
/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/.exec(value);
                    if (a) {
                        return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4],
                            +a[5], +a[6]));
                    }
                }
                return value;
            });

            myData = JSON.parse('["Date(09/09/2001)"]', function (key, value) {
                var d;
                if (typeof value === 'string' &&
                        value.slice(0, 5) === 'Date(' &&
                        value.slice(-1) === ')') {
                    d = new Date(value.slice(5, -1));
                    if (d) {
                        return d;
                    }
                }
                return value;
            });


    This is a reference implementation. You are free to copy, modify, or
    redistribute.

    This code should be minified before deployment.
    See http://javascript.crockford.com/jsmin.html

    USE YOUR OWN COPY. IT IS EXTREMELY UNWISE TO LOAD CODE FROM SERVERS YOU DO
    NOT CONTROL.
*/

/*jslint evil: true */

/*global JSON */

if (!this.JSON) {

// Create a JSON object only if one does not already exist. We create the
// object in a closure to avoid creating global variables.

    JSON = function () {

        function f(n) {
            // Format integers to have at least two digits.
            return n < 10 ? '0' + n : n;
        }

        Date.prototype.toJSON = function (key) {

            return this.getUTCFullYear()   + '-' +
                 f(this.getUTCMonth() + 1) + '-' +
                 f(this.getUTCDate())      + 'T' +
                 f(this.getUTCHours())     + ':' +
                 f(this.getUTCMinutes())   + ':' +
                 f(this.getUTCSeconds())   + 'Z';
        };

        var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
            escapeable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
            gap,
            indent,
            meta = {    // table of character substitutions
                '\b': '\\b',
                '\t': '\\t',
                '\n': '\\n',
                '\f': '\\f',
                '\r': '\\r',
                '"' : '\\"',
                '\\': '\\\\'
            },
            rep;


        function quote(string) {

// If the string contains no control characters, no quote characters, and no
// backslash characters, then we can safely slap some quotes around it.
// Otherwise we must also replace the offending characters with safe escape
// sequences.

            escapeable.lastIndex = 0;
            return escapeable.test(string) ?
                '"' + string.replace(escapeable, function (a) {
                    var c = meta[a];
                    if (typeof c === 'string') {
                        return c;
                    }
                    return '\\u' + ('0000' +
                            (+(a.charCodeAt(0))).toString(16)).slice(-4);
                }) + '"' :
                '"' + string + '"';
        }


        function str(key, holder) {

// Produce a string from holder[key].

            var i,          // The loop counter.
                k,          // The member key.
                v,          // The member value.
                length,
                mind = gap,
                partial,
                value = holder[key];

// If the value has a toJSON method, call it to obtain a replacement value.

            if (value && typeof value === 'object' &&
                    typeof value.toJSON === 'function') {
                value = value.toJSON(key);
            }

// If we were called with a replacer function, then call the replacer to
// obtain a replacement value.

            if (typeof rep === 'function') {
                value = rep.call(holder, key, value);
            }

// What happens next depends on the value's type.

            switch (typeof value) {
            case 'string':
                return quote(value);

            case 'number':

// JSON numbers must be finite. Encode non-finite numbers as null.

                return isFinite(value) ? String(value) : 'null';

            case 'boolean':
            case 'null':

// If the value is a boolean or null, convert it to a string. Note:
// typeof null does not produce 'null'. The case is included here in
// the remote chance that this gets fixed someday.

                return String(value);

// If the type is 'object', we might be dealing with an object or an array or
// null.

            case 'object':

// Due to a specification blunder in ECMAScript, typeof null is 'object',
// so watch out for that case.

                if (!value) {
                    return 'null';
                }

// Make an array to hold the partial results of stringifying this object value.

                gap += indent;
                partial = [];

// If the object has a dontEnum length property, we'll treat it as an array.

                if (typeof value.length === 'number' &&
                        !(value.propertyIsEnumerable('length'))) {

// The object is an array. Stringify every element. Use null as a placeholder
// for non-JSON values.

                    length = value.length;
                    for (i = 0; i < length; i += 1) {
                        partial[i] = str(i, value) || 'null';
                    }

// Join all of the elements together, separated with commas, and wrap them in
// brackets.

                    v = partial.length === 0 ? '[]' :
                        gap ? '[\n' + gap +
                                partial.join(',\n' + gap) + '\n' +
                                    mind + ']' :
                              '[' + partial.join(',') + ']';
                    gap = mind;
                    return v;
                }

// If the replacer is an array, use it to select the members to be stringified.

                if (rep && typeof rep === 'object') {
                    length = rep.length;
                    for (i = 0; i < length; i += 1) {
                        k = rep[i];
                        if (typeof k === 'string') {
                            v = str(k, value, rep);
                            if (v) {
                                partial.push(quote(k) + (gap ? ': ' : ':') + v);
                            }
                        }
                    }
                } else {

// Otherwise, iterate through all of the keys in the object.

                    for (k in value) {
                        if (Object.hasOwnProperty.call(value, k)) {
                            v = str(k, value, rep);
                            if (v) {
                                partial.push(quote(k) + (gap ? ': ' : ':') + v);
                            }
                        }
                    }
                }

// Join all of the member texts together, separated with commas,
// and wrap them in braces.

                v = partial.length === 0 ? '{}' :
                    gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
                            mind + '}' : '{' + partial.join(',') + '}';
                gap = mind;
                return v;
            }
        }

// Return the JSON object containing the stringify and parse methods.

        return {
            stringify: function (value, replacer, space) {

// The stringify method takes a value and an optional replacer, and an optional
// space parameter, and returns a JSON text. The replacer can be a function
// that can replace values, or an array of strings that will select the keys.
// A default replacer method can be provided. Use of the space parameter can
// produce text that is more easily readable.

                var i;
                gap = '';
                indent = '';

// If the space parameter is a number, make an indent string containing that
// many spaces.

                if (typeof space === 'number') {
                    for (i = 0; i < space; i += 1) {
                        indent += ' ';
                    }

// If the space parameter is a string, it will be used as the indent string.

                } else if (typeof space === 'string') {
                    indent = space;
                }

// If there is a replacer, it must be a function or an array.
// Otherwise, throw an error.

                rep = replacer;
                if (replacer && typeof replacer !== 'function' &&
                        (typeof replacer !== 'object' ||
                         typeof replacer.length !== 'number')) {
                    throw new Error('JSON.stringify');
                }

// Make a fake root object containing our value under the key of ''.
// Return the result of stringifying the value.

                return str('', {'': value});
            },


            parse: function (text, reviver) {

// The parse method takes a text and an optional reviver function, and returns
// a JavaScript value if the text is a valid JSON text.

                var j;

                function walk(holder, key) {

// The walk method is used to recursively walk the resulting structure so
// that modifications can be made.

                    var k, v, value = holder[key];
                    if (value && typeof value === 'object') {
                        for (k in value) {
                            if (Object.hasOwnProperty.call(value, k)) {
                                v = walk(value, k);
                                if (v !== undefined) {
                                    value[k] = v;
                                } else {
                                    delete value[k];
                                }
                            }
                        }
                    }
                    return reviver.call(holder, key, value);
                }


// Parsing happens in four stages. In the first stage, we replace certain
// Unicode characters with escape sequences. JavaScript handles many characters
// incorrectly, either silently deleting them, or treating them as line endings.

                cx.lastIndex = 0;
                if (cx.test(text)) {
                    text = text.replace(cx, function (a) {
                        return '\\u' + ('0000' +
                                (+(a.charCodeAt(0))).toString(16)).slice(-4);
                    });
                }

// In the second stage, we run the text against regular expressions that look
// for non-JSON patterns. We are especially concerned with '()' and 'new'
// because they can cause invocation, and '=' because it can cause mutation.
// But just to be safe, we want to reject all unexpected forms.

// We split the second stage into 4 regexp operations in order to work around
// crippling inefficiencies in IE's and Safari's regexp engines. First we
// replace the JSON backslash pairs with '@' (a non-JSON character). Second, we
// replace all simple value tokens with ']' characters. Third, we delete all
// open brackets that follow a colon or comma or that begin the text. Finally,
// we look to see that the remaining characters are only whitespace or ']' or
// ',' or ':' or '{' or '}'. If that is so, then the text is safe for eval.

                if (/^[\],:{}\s]*$/.
test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, '@').
replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').
replace(/(?:^|:|,)(?:\s*\[)+/g, ''))) {

// In the third stage we use the eval function to compile the text into a
// JavaScript structure. The '{' operator is subject to a syntactic ambiguity
// in JavaScript: it can begin a block or an object literal. We wrap the text
// in parens to eliminate the ambiguity.

                    j = eval('(' + text + ')');

// In the optional fourth stage, we recursively walk the new structure, passing
// each name/value pair to a reviver function for possible transformation.

                    return typeof reviver === 'function' ?
                        walk({'': j}, '') : j;
                }

// If the text is not JSON parseable, then a SyntaxError is thrown.

                throw new SyntaxError('JSON.parse');
            }
        };
    }();
}

/*extern JSON, JsonML */
/*
	JsonML2.js

	Created: 2006-11-09-0116
	Modified: 2008-05-25-2326

	Released under an open-source license:
	http://jsonml.org/License.htm

    This file adds these methods to JavaScript:

        JsonML.parse(string|array, filter)

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

            var myUI = JsonML.parse(myUITemplate, function (elem) {
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

if ("undefined" === typeof JsonML) {
	window.JsonML = {};
}

JsonML.parse = function(/*JsonML*/ jml, /*element function(element)*/ filter) {

	//attribute name mapping
	var am = {
		rowspan : "rowSpan",
		colspan : "colSpan",
		cellpadding : "cellPadding",
		cellspacing : "cellSpacing",
		tabindex : "tabIndex",
		accesskey : "accessKey",
		hidefocus : "hideFocus",
		usemap : "useMap",
		maxlength : "maxLength",
		readonly : "readOnly",
		contenteditable : "contentEditable"
		// can add more attributes here as needed
	};

	//addAttributes
	/*void*/ function aa(/*element*/ el, /*Object*/ a) {
		// for each attributeName
		for (var an in a) {
			if (an && "string" === typeof a[an]) {
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
	}

	//appendChild
	/*void*/ function ac(/*element*/ el, /*Array or String*/ c) {
		var ct, tb;
		if (c) {
			if (el.tagName.toLowerCase() === "table" && el.tBodies) {
				// in IE must explicitly nest TDs in TBODY
				ct = c.tagName ? c.tagName.toLowerCase() : null;// child tagName
				if (ct && ct!=="tbody" && ct!=="thead") {
					// insert in last tbody
					tb = el.tBodies.length>0 ? el.tBodies[el.tBodies.length-1] : null;// tBody
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

	//JsonML.parse
	/*element*/ function p(/*JsonML*/ jml) {
		if (!jml) {
			return null;
		}
		if (typeof(jml) === "string") {
			return document.createTextNode(jml);
		}

		if (!(jml instanceof Array) || jml.length < 1 || "string" !== typeof jml[0]) {
			throw new Error("JsonML.parse");
		}

		var t = jml[0]; // tagName
		var x = (t.toLowerCase() === "script"); // check for scripts
		var el = x ? null : document.createElement(t);

		for (var i=1; i<jml.length; i++) {
			if (!x) {
				if (jml[i] instanceof Array || "string" === typeof jml[i]) {
					// append children
					ac(el, p(jml[i]));
				} else if ("object" === typeof jml[i]) {
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

	if (jml instanceof Array) {
		return p(jml);
	} else if ("string" === typeof jml) {

		try {
			jml = JSON.parse(jml);
		} catch (ex) {
			return null;
		}

		if (jml instanceof Array) {
			return JsonML.parse(jml, filter);
		}
	}
	return null;
};

/*extern JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.UI.setCssUserAgent

	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-06-10-1635
	Modified: 2007-06-17-1302
\*---------------------------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*Hastable*/ JsonFx.userAgent = {
		jsonfx : "1.0"
	};

/*Hastable*/ JsonFx.parseUserAgent = function(/*string*/ ua) {
	/*Hastable*/ var fxua = {};

	if (!ua) {
		return fxua;
	}
	ua = ua.toLowerCase();

	// RegExp tested against (2007-06-17 @ 1235):
	// http://www.useragentstring.com/pages/useragentstring.php
	// http://www.user-agents.org
	// http://en.wikipedia.org/wiki/User_agent
	var R_All = /\S+[\/][v]?\d+(\.\d+)*/g;
	var R_AOL = /\b(america online browser|aol)[\s\/]*(\d+(\.\d+)*)/;
	var R_MSIE = /(\bmsie|microsoft internet explorer)[\s\/]*(\d+(\.\d+)*)/;
	var R_Gecko = /rv[:](\d+(\.\d+)*).*?gecko[\/]\d+/;
	var R_Opera = /\bopera[\s\/]*(\d+(\.\d+)*)/;
	var R_MSPIE = /\b(mspie|microsoft pocket internet explorer)[\s\/]*(\d+(\.\d+)*)/;
	var R_iCab = /\bicab[\s\/]*(\d+(\.\d+)*)/;

	// do this first for all (covers most browser types)
	var i, s, b, raw = ua.match(R_All);
	if (raw) {
		for (i=0; i<raw.length; i++) {
			s = raw[i].indexOf('/');
			b = raw[i].substring(0, s);
			if (b && b !== "mozilla") {
				// shorten this common browser
				if (b === "applewebkit") {
					b = "webkit";
				}
				fxua[b] = raw[i].substr(s+1);
			}
		}
	}

	// aol uses multiple engines so continue checking
	if (R_AOL.exec(ua)) {
		fxua.aol = RegExp.$2;
	}

	// order is important as user-agents spoof each other	
	if (R_Opera.exec(ua)) {
		fxua.opera = RegExp.$1;
	} else if (R_iCab.exec(ua)) {
		fxua.icab = RegExp.$1;
	} else if (R_MSIE.exec(ua)) {
		fxua.ie = RegExp.$2;
	} else if (R_MSPIE.exec(ua)) {
		fxua.mspie = RegExp.$2;
	} else if (R_Gecko.exec(ua)) {
		fxua.gecko = RegExp.$1;
	}
	return fxua;
};

/*void*/ JsonFx.formatCssUserAgent = function (/*Hastable*/ fxua) {
	/*string*/ function format(/*string*/ b, /*string*/ v) {
		/*const string*/ var PREFIX = " ua-", i;

		/*string*/ var css = PREFIX+b;
		if (v) {
			v = v.replace(/\./g, '-');
			i = v.indexOf('-');
			while (i > 0) {
				// loop through chopping last '-' to end off
				// concat result onto return string
				css += PREFIX+b+'-'+v.substring(0, i);
				i = v.indexOf('-', i+1);
			}
			css += PREFIX+b+'-'+v;
		}
		return css;
	}

	var uaCss = "";

	for (var b in fxua) {
		if (b && fxua.hasOwnProperty(b)) {
			JsonFx.userAgent[b] = fxua[b];
			uaCss += format(b, fxua[b]);
		}
	}

	// assign user-agent classes
	return uaCss;
};

(function() {
	// anonymous function doesn't affect global namespace and can't be called externally
	// variables and helper functions only available via JavaScript closures

	// calculate userAgent immediately, poll until can apply them
	/*Hastable*/ var fxua = JsonFx.parseUserAgent(navigator.userAgent);

	/*	Dynamically appends CSS classes to document.body based upon user-agent.*/
	/*void*/ JsonFx.UI.setCssUserAgent = function() {

		// using JavaScript closures to access the parsed UA
		// using setTimeout to poll until body exists
		/*void*/ function appendCssPoll() {

			if (!document.body) {
				setTimeout(appendCssPoll, 10);
			} else {
				document.body.className += JsonFx.formatCssUserAgent(fxua);
			}
		}

		appendCssPoll();
	};
})();

/*extern JsonFx, JSON */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2008-05-25-2253
\*---------------------------------------------------------*/

// dependency checks
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.IO requires json2.js");
}

/* ----------------------------------------------------*/

(function () {
	// wrapping in anonymous function so that the XHR ID list
	// will be only available as a closure, as this will not
	// modify the global namespace, and it will be shared
	var XHR_OCXs;

	if ("undefined" === typeof window.XMLHttpRequest) {

		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ XHR_OCXs = !window.ActiveXObject ? [] :
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
			while (XHR_OCXs.length) {
				try {
					return new window.ActiveXObject(XHR_OCXs[0]);
				} catch (ex) {
					// remove the failed XHR_OCXs for all future requests
					XHR_OCXs.shift();
				}
			}

			// all XHR_OCXs failed		
			return null;
		};
	}
})();

/* ----------------------------------------------------*/

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
	var name, msg, code;
	if (ex) {
		name = ex.name ? ex.name : "Error";
		msg = ex.message ? ex.message : "";
		code = isFinite(ex.code) ? Number(ex.code) : Number(ex.number);

		if (isFinite(code)) {
			name += " ("+code+")";
		}

		window.alert("Request Failed - "+name+":\r\n\""+msg+"\"\r\n"+obj);
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
	} else {
		options.method = options.method.toUpperCase();
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
		options.timeout = 60000;// 60 seconds
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
		var status, ex;
		if (xhr && xhr.readyState === 4 /*complete*/) {

			// stop the timeout
			window.clearTimeout(cancel);

			// check the status
			status = 0;
			try {
				status = Number(xhr.status);
			} catch (ex2) {
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
				ex = new Error(xhr.statusText);
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
			for (var h in options.headers) {
				if (options.headers.hasOwnProperty(h) && options.headers[h]) {
					try {// Opera 8.0.0 doesn't have xhr.setRequestHeader
						xhr.setRequestHeader(h, options.headers[h]);
					} catch (ex) { }
				}
			}
		}

		if (options.method === "POST" && !options.params) {
			options.params = "";
		}
		xhr.send(options.params);

	} catch (ex2) {
		// immediate failure: exception thrown
		if (options.onFailure) {
			options.onFailure(xhr, options.context, ex2);
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

	options.headers.Accept = "application/json, application/jsonml+json";

	var onSuccess = options.onSuccess;
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ context) {

		// decode response as JSON
		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? JSON.parse(json) : null;

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

	var onFailure = null;
	if (options.onFailure) {
		onFailure = options.onFailure;
		options.onFailure = function (/*XMLHttpRequest*/ xhr, /*object*/ context, /*Error*/ ex) {

			onFailure((xhr&&xhr.responseText), context, ex);

			// free closure references
			onFailure = null;
		};
	}

	JsonFx.IO.sendRequest(restUrl, options);
};

/* JSON-RPC ----------------------------------------------------*/

/*string*/ JsonFx.IO.jsonRpcPathEncode = function (/*string*/ rpcMethod, /*object|array*/ rpcParams) {
	var i, enc = encodeURIComponent, rpcUrl = "/";
	if (rpcMethod && rpcMethod !== "system.describe") {
		rpcUrl += enc(rpcMethod);
	}
	if ("object" === typeof rpcParams) {
		rpcUrl += "?";
		if (rpcParams instanceof Array) {
			for (i=0; i<rpcParams.length; i++) {
				if (i > 0) {
					rpcUrl += "&";
				}
				rpcUrl += enc(i);
				rpcUrl += "=";
				rpcUrl += enc(rpcParams[i]);
			}
		} else {
			for (var p in rpcParams) {
				if (rpcParams.hasOwnProperty(p)) {
					rpcUrl += enc(p);
					rpcUrl += "=";
					rpcUrl += enc(rpcParams[p]);
				}
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

	options.headers.Accept = "application/json, application/jsonml+json";

	// wrap callbacks with RPC layer
	var onSuccess = options.onSuccess;
	var onFailure = options.onFailure;

	// this calls onSuccess with the results of the method (not the RPC wrapper)
	// or it calls onFailure with the error of the method (not the RPC wrapper)
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ cx) {

		var json = xhr ? xhr.responseText : null;
		try {
			json = ("string" === typeof json) ? JSON.parse(json) : null;

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
			json = ("string" === typeof json) ? JSON.parse(json) : null;

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

	var rpcRequest;
	if (options.method === "GET") {
		// GET RPC is encoded as part the URL
		rpcUrl += JsonFx.IO.jsonRpcPathEncode(rpcMethod, rpcParams);

	} else {
		// POST RPC is encoded as a JSON body
		rpcRequest = {
				version : "1.1",
				method : rpcMethod,
				params : rpcParams
			};

		try {
			// JSON encode request object
			rpcRequest = JSON.stringify(rpcRequest);
		} catch (ex) {
			// if violates JSON, then fail
			if (onFailure) {
				onFailure(rpcRequest, options.context, ex);
			}
			return;
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

		var self = this, onComplete = null;
		if ("function" === typeof this.onEndRequest) {
			// intercept onComplete to call onEndRequest
			onComplete = options.onComplete;
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

/*extern JsonFx, JSON, JsonML */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-05-25-2253
\*---------------------------------------------------------*/

// dependency checks
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.UI requires json2.js");
}
if ("undefined" === typeof JsonML) {
	throw new Error("JsonFx.UI requires JsonML2.js");
}

/* Utilities ----------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*DOM*/ elem, /*string*/ style) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	var i;
	if (elem.currentStyle) {
		// IE only
		if (style === "float") {
			style = "styleFloat";
		} else {
			// convert property name to camelCase
			style = style.split('-');
			style[0] = style[0].toLowerCase();
			for (i=1; i<style.length; i++) {
				style[i] = style[i].charAt(0).toUpperCase()+style[i].substr(1).toLowerCase();
			}
			style = style.join("");
		}
		return elem.currentStyle[style];
	}
	return null;
};

/*bool*/ JsonFx.UI.hasClassName = function(/*DOM*/ elem, /*string*/ cssClass) {
	return !!cssClass &&
		!!elem &&
		!!elem.className &&
		!!elem.className.match("\\b"+cssClass+"\\b");
};

/*DOM*/ JsonFx.UI.debugPoint = function (/*int*/ x, /*int*/ y, /*{delay,color,size,className}*/ options) {
	if (isNaN(x)) {
		x = 0;
	}
	if (isNaN(y)) {
		y = 0;
	}
	options = options || {};
	options.delay = options.delay || 1500;
	if (!options.className) {
		options.size = options.size || "4px";
		options.color = options.color || "magenta";
	}

	var pt = document.createElement("div");
	pt.style.position = "absolute";
	pt.style.left = (x-pt.offsetWidth)+"px";
	pt.style.top = (y-pt.offsetHeight)+"px";
	pt.title = "("+x+","+y+")";
	pt.className = options.className;
	if (options.size) {
		pt.style.height = pt.style.width = options.size;
		pt.style.overflow = "hidden";
	}
	document.body.appendChild(pt);

	if (options.delay > 0) {
		window.setTimeout(
			function() {
				if (pt) {
					pt.parentNode.removeChild(pt);
					pt = null;
				}
			},
			options.delay);
	}
	options = null;
	return pt;
};

/*{x,y}*/ JsonFx.UI.getScroll = function(/*element*/ elem) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}
	var pt = {x:0, y:0};
	elem = elem && elem.parentNode;
	while (elem) {
		if (!isNaN(elem.scrollTop)) {
			pt.y += elem.scrollTop;
		}
		if (!isNaN(elem.scrollLeft)) {
			pt.x += elem.scrollLeft;
		}
		elem = elem.parentNode;
	}
	return pt;
};

/*{x,y}*/ JsonFx.UI.getOffset = function(/*element*/ elem, /*bool*/ skipScroll) {
	if ("string" === typeof elem) {
		elem = document.getElementById(elem);
	}

	var pos,
		pt = skipScroll ? {x:0,y:0} : JsonFx.UI.getScroll(elem);

	while (elem) {
		pt.y += elem.offsetTop;
		pt.x += elem.offsetLeft;
		elem = elem.offsetParent;
//		if (elem) {
//			pos = JsonFx.UI.getStyle(elem, "position");
//			if (pos !== "static") {
//				elem = null;
//			}
//		}
	}
	return pt;
};

/*{x,y}*/ JsonFx.UI.getMidPoint = function(/*element*/ elem) {
	if (!elem) {
		return null;
	}
	var pt = JsonFx.UI.getOffset(elem, true);
	pt.x += Math.floor(0.5+elem.offsetWidth/2);
	pt.y += Math.floor(0.5+elem.offsetHeight/2);
	return pt;
};

/*{x,y}*/ JsonFx.UI.pointAdd = function(/*{x,y}*/ a, /*{x,y}*/ b) {
	if (!b) {
		return a;
	}
	if (!a) {
		return b;
	}
	return { x:(a.x+b.x), y:(a.y+b.y) };
};

/*{x,y}*/ JsonFx.UI.pointSubtract = function(/*{x,y}*/ a, /*{x,y}*/ b) {
	if (!b) {
		return a;
	}
	if (!a) {
		return b;
	}
	return { x:(a.x-b.x), y:(a.y-b.y) };
};

/*element*/ JsonFx.UI.getEventTarget = function(/*Event*/ evt) {
	evt = evt || window.event || {};
	return (evt.currentTarget || evt.srcElement || null);
};

/*{x,y}*/ JsonFx.UI.getEventPoint = function(/*Event*/ evt) {
	evt = evt || window.event || { clientX:-Infinity, clientY:-Infinity };
	if (typeof evt.pageX !== "undefined") {
		return { x:evt.pageX, y:evt.pageY };
	}
	var pt = JsonFx.UI.getScroll(JsonFx.UI.getEventTarget(evt));
	return JsonFx.UI.pointAdd(pt, { x:evt.clientX, y:evt.clientY });
};

/*{left,middle,right}*/ JsonFx.UI.getMouseButton = function(/*Event*/ evt) {
	evt = evt || window.event;
	var b, btn = {left:false,middle:false,right:false};

	if (evt) {
		if (isNaN(evt.which)) {
			// IE (can be multiple)
			b = evt.button;
			if ((b&1)!==0) {
				btn.left = true;
			}
			if ((b&2)!==0) {
				btn.right = true;
			}
			if ((b&4)!==0) {
				btn.middle = true;
			}
		} else {
			// Firefox, Opera, Safari
			switch (evt.which){
				case 1:
					btn.left = true;
					break;
				case 2:
					btn.middle = true;
					break;
				case 3:
					btn.right = true;
					break;
			}
		}
	}
	return btn;
};

/*void*/ JsonFx.UI.cancelEvent = function(/*Event*/ evt) {
	evt = evt || window.event || {};
	if (evt) {
		evt.cancelBubble = true;
	}
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

/*function*/ JsonFx.UI.combineHandlers = function (/*function*/ a, /*function*/ b) {
	if ("function" !== typeof b) {
		return a;
	}

	if ("function" !== typeof a) {
		return b;
	}

	return function(/*Event*/ evt) { a(evt); return b(evt); };
};

/*void*/ JsonFx.UI.attachHandler = function (/*DOM*/ obj, /*string*/ evtName, /*function*/ handler) {
	if (obj.addEventListener) {
		//DOM method for binding an event
		obj.addEventListener(evtName, handler, false);
	} else if (obj.attachEvent) {
		//IE exclusive method for binding an event
		obj.attachEvent("on"+evtName, handler);
	} else if ("function" === typeof obj["on"+evtName]) {
		obj["on"+evtName] = JsonFx.UI.combineHandlers(handler, obj["on"+evtName]);
	} else {
		obj["on"+evtName] = handler;
	}
};

/*void*/ JsonFx.UI.detachHandler = function (/*DOM*/ obj, /*string*/ evtName, /*function*/ handler) {
	if (obj.removeEventListener) {
		//DOM method for unbinding an event
		obj.removeEventListener(evtName, handler, false);
	} else if (obj.detachEvent) {
		//IE exclusive method for binding an event
		obj.detachEvent("on"+evtName, handler);
	} else if (handler === obj["on"+evtName]) {
		obj["on"+evtName] = null;
	} else {
		// not sure how to remove this kind...
	}
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
						r:Number("0x"+RegExp.$1),
						g:Number("0x"+RegExp.$2),
						b:Number("0x"+RegExp.$3)
					};
			} else if (hex.length === 4) {
				// e.g. "#369" === "#336699"
				return {
						r:Number("0x"+RegExp.$1+RegExp.$1),
						g:Number("0x"+RegExp.$2+RegExp.$2),
						b:Number("0x"+RegExp.$3+RegExp.$3)
					};
			}
		}
		if (hex.match(/rgb[\(](\d+),\s*(\d+),\s*(\d+)[\)]/)) {// Firefox colors
			return {
					r:Number(RegExp.$1),
					g:Number(RegExp.$2),
					b:Number(RegExp.$3)
				};
		}
	}
	return {r:NaN, g:NaN, b:NaN};
};

/*float*/ JsonFx.UI.bound = function (/*float*/ start, /*float*/ end, /*float*/ val) {
	// don't allow val to exceed bounds
	if (start < end) {
		val = Math.min(val, end);
		val = Math.max(val, start);
	} else {
		val = Math.min(val, start);
		val = Math.max(val, end);
	}
	return val;
};

/* Linear Interpolate */
/*float*/ JsonFx.UI.lerp = function (/*float*/ a, /*float*/ b, /*float*/ t, /*bool*/ bounded) {
	var val = (a * (1-t)) + (b * t);
	return bounded ? JsonFx.UI.bound(a, b, val) : val;
};

/* Integer Linear Interpolation */
/*int*/ JsonFx.UI.lerpInt = function (/*int*/ a, /*int*/ b, /*float*/ t, /*bool*/ bounded) {
	return Math.floor( 0.5 + JsonFx.UI.lerp(a, b, t, bounded) );
};

/* Reverse Linear Interpolate */
/*float*/ JsonFx.UI.revLerp = function (/*float*/ a, /*float*/ b, /*float*/ t, /*bool*/ bounded) {
	var val = (t-a)/(b-a);
	return bounded ? JsonFx.UI.bound(0, 1, val) : val;
};

/* Cubic Bezier Curve */
/*float*/ JsonFx.UI.bezier = function (/*float*/ a, /*float*/ b, /*float*/ c, /*float*/ d, /*float*/ t) {
	return a*Math.pow(1-t,3) + b*3*t*Math.pow(1-t,2) + c*3*Math.pow(t,2)*(1-t) + d*Math.pow(t,3);
};

/* Cubic Bezier Curve */
/*{x,y}*/ JsonFx.UI.bezierPoint = function (/*{x,y}*/ p0, /*{x,y}*/ p1, /*{x,y}*/ p2, /*{x,y}*/ p3, /*float*/ t) {
	return {x:JsonFx.UI.bezier(p0.x, p1.x, p2.x, p3.x, t), y:JsonFx.UI.bezier(p0.y, p1.y, p2.y, p3.y, t)};
};

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BIND = "B", UNBIND = "U";

	/*hashtable[tag] of object*/ var bindings = {};

	/*void*/ b.register = function(/*string*/ tag, /*string*/ css, /*function(elem,options)*/ bind, /*function(elem,options)*/ unbind, /*bool*/ overwrite) {

//		alert(tag+"."+css+":\n"+bind+"\n"+unbind);

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
			} else if (!overwrite && bindings[tag][css]) {
				throw new Error("Binding for "+tag+"."+css+" has already been registered.");
			}

			/*object*/ bindings[tag][css] = {};
			bindings[tag][css][BIND] = bind || null;
			bindings[tag][css][UNBIND] = unbind || null;
		}
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		var tag, tagBindings, classes, i, css, options, replace;
		if (elem && elem.tagName && elem.className) {

			// only perform on registered tags
			tag = elem.tagName.toLowerCase();
			if (bindings[tag]) {
				tagBindings = bindings[tag];
				classes = elem.className.split(/\s+/);

				// for each css class in elem
				for (i=0; i<classes.length; i++) {
					css = classes[i];
					if (css && tagBindings[css] && tagBindings[css][a]) {

						// allow element to provide parameters for bindings
						options = elem.getAttribute("jsonfx:options");
						if (options) {
							try {
								// treat string as JSON
								options = JSON.parse(options);
							} catch (ex) { }
						}

						try {
							// perform action on element and
							// allow binding to replace element
							elem = tagBindings[css][a](elem, options) || elem;
						} catch (ex2) {
							window.alert("Error binding "+tag+"."+css+":\n\n\""+ex2.message+"\"");
						}
					}
				}
			}
		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {
		var elems, i, replace;
		if (root && root.getElementsByTagName) {

			// for each registered tag
			for (var tag in bindings) {
				if (bindings.hasOwnProperty(tag)) {

					// for each element in root with tagName
					elems = root.getElementsByTagName(tag);
					for (i=0; i<elems.length; i++) {
						// perform action on element and
						// allow binding to replace element
						replace = performOne(elems[i], a);
						if (replace !== elems[i] && elems[i].parentNode) {
							elems[i].parentNode.replaceChild(replace, elems[i]);
						}
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
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BIND);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UNBIND);
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
			b.bind(document);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl || "";
				document.body.style.cursor = crsr || "";
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
			b.unbind(document);
		} finally {
			if (document.body && document.body.style) {
				// restore previous values
				document.body.title = ttl || "";
				document.body.style.cursor = crsr || "";
			}
		}
	};

	window.setTimeout(function() {
			/* NOTE: using JsonFx.UI.attachHandler makes it hard to control handler order */

			// wire up binding
			if ("function" === typeof window.onload) {
				window.onload = JsonFx.UI.combineHandlers(b.bindAll, window.onload);
			} else {
				window.onload = b.bindAll;
			}

			// wire up unbinding
			if ("function" === typeof window.onunload) {
				window.onunload = JsonFx.UI.combineHandlers(b.unbindAll, window.onunload);
			} else {
				window.onunload = b.unbindAll;
			}
		}, 0);
};

// instantiate only one, destroying the constructor
JsonFx.UI.Bindings = new JsonFx.UI.Bindings();

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

/*	creates DOM elements from JsonML and binds them accordingly */
/*element*/ JsonFx.UI.bindJsonML = function(/*JsonML*/ jml) {
	if (jml) {
		return JsonML.parse(jml, JsonFx.UI.Bindings.bindOne);
	} else {
		return null;
	}
};

/*	if container is null then uses ID(s) to replace page elements
	returns the container element if one was specified */
/*element*/ JsonFx.UI.displayJsonML = function(/*JsonML*/ jml, /*element|string*/ container) {

	// either DOM element or id
	container = ("string" === typeof container) ?
		document.getElementById(container) : container;

	jml = JsonFx.UI.bindJsonML(jml);
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
	this.c = { r:NaN, g:NaN, b:NaN };
	this.bc = { r:NaN, g:NaN, b:NaN };
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
	var op = new JsonFx.UI.Animate.Op(), es, esPos, top, right, bottom, left, width, height, color, clip;

	if (elem && elem.style) {
		es = elem.style;
		esPos = JsonFx.UI.getStyle(elem, "position");

		top = parseFloat(JsonFx.UI.getStyle(elem, "top"));
		if (isFinite(top)) {
			op.top(top);
		} else if (esPos === "static") {
			op.top(0);
		}
		right = parseFloat(JsonFx.UI.getStyle(elem, "right"));
		if (isFinite(right)) {
			op.right(right);
		} else if (esPos === "static") {
			op.right(0);
		}
		bottom = parseFloat(JsonFx.UI.getStyle(elem, "bottom"));
		if (isFinite(bottom)) {
			op.right(bottom);
		} else if (esPos === "static") {
			op.bottom(0);
		}
		left = parseFloat(JsonFx.UI.getStyle(elem, "left"));
		if (isFinite(left)) {
			op.left(left);
		} else if (esPos === "static") {
			op.left(0);
		}

		// width
		if (isFinite(elem.offsetWidth)) {
			op.width(elem.offsetWidth);
		} else {
			width = parseFloat(JsonFx.UI.getStyle(elem, "width"));
			if (isFinite(width)) {
				op.width(width);
			}
		}
		// height
		if (isFinite(elem.offsetHeight)) {
			op.height(elem.offsetHeight);
		} else {
			height = parseFloat(JsonFx.UI.getStyle(elem, "height"));
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
		color = JsonFx.UI.fromHtmlColor(JsonFx.UI.getStyle(elem, "color"));
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
	/*Dictionary<string,bool>*/ filterable = JsonFx.UI.Animate.filterable;

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
				if (es.filter) {
					es.filter = userFilter;
				}
			} catch (ex) {}
		}
		if (op.hasBlur()) {
			try {
				if (es.filter) {
					es.filter = userFilter;
				}
			} catch (ex2) {}
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
		if (JsonFx.UI.getStyle(elem, "display") === "none") {
			es.display = "block";
		}
		if (JsonFx.UI.getStyle(elem, "visibility") === "hidden") {
			es.visibility = "visible";
		}
	}

	/*void*/ function initFilter(op) {
		try {
			// first look up tagName to determine if
			// element can even have filters applied
			// to prevent incurring calculation cost
			if (filterable[tn] &&
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
							} catch (ex2) { alpha = null; }
						}
					}
					if (!alpha) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
						try {
							alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
						} catch (ex3) { alpha = null; }
						if (!alpha) {
							// try IE4.0+
							es.filter += " alpha(enabled=false)";
							try {
								alpha = elem.filters.item("alpha");
							} catch (ex4) { alpha = null; }
						}
					}
				}
				if (op.hasBlur() && !blur) {
					if (elem.filters.length > 0) {
						try {
							// check IE5.5+
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex5) { blur = null; }
						if (!blur) {
							try {
								// check IE4.0+
								blur = elem.filters.item("blur");
							} catch (ex6) { blur = null; }
						}
					}
					if (!blur) {
						// try IE5.5+
						es.filter += " progid:DXImageTransform.Microsoft.Blur(enabled=false)";
						try {
							blur = elem.filters.item("DXImageTransform.Microsoft.Blur");
						} catch (ex7) { blur = null; }
						if (!blur) {
							// try IE4.0+
							es.filter += " blur(enabled=false)";
							try {
								blur = elem.filters.item("blur");
							} catch (ex8) { blur = null; }
						}
					}
				}
			}
		} catch (ex9) {}
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
					} catch (ex2) {
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
			var clip;
			if (op.hasClip()) {
				clip = ["auto","auto","auto","auto"];
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
					40);// ~25Hz baseline, in case fast machine
			}
		}

		// XOR: state OR immediate but not both
		// immediate forces to far end of step
		t(state^immediate ? STEP_MIN : STEP_MAX);
	};
};

/*extern JSON, JsonFx */
/*
	JsonFx.History.js

	Copyright (c)2006-2008 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2008-05-21-2244

	Distributed under the terms of an MIT-style license:
	http://jsonfx.net/BuildTools/License.txt
*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/* check dependencies */
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.History requires json2.js");
}

/* Utilities ----------------------------------------------------*/

/*DOM*/ JsonFx.UI.getIFrameDocument = function(/*DOM*/ elem) {
	if (!elem) {
		return null;
	}

	if ("undefined" !== typeof elem.contentDocument) {
		// W3C
		return elem.contentDocument;
	} else if ("undefined" !== typeof elem.contentWindow) {
		// Microsoft
		return elem.contentWindow.document;
	} else if ("undefined" !== typeof elem.document) {
		// deprecated
		return elem.document;
	}
	// not available
	return elem;
};

/* JsonFx.History -----------------------------------------------*/

/* singleton JsonFx.History */
JsonFx.History = {

	/*DOM*/ h: null,
	/*function(object)*/ onchange: null,

	/*void*/ push: function(/*object*/ info) {
		if (!JsonFx.History.h) {
			// doesn't support history or no binding
			if ("function" === typeof JsonFx.History.onchange) {
				JsonFx.History.onchange(info);
			}
			return;
		}
		var h = JsonFx.UI.getIFrameDocument(JsonFx.History.h);
		if (h && h.location) {
			h.location.search = '?'+encodeURIComponent(JSON.stringify(info));
		} else {
			// Opera 8 doesn't trigger onload so no history
			JsonFx.History.h = null;
		}
	},

	/*void*/ changed: function(/*DOM*/ elem) {
		if (!JsonFx.History.h) {
			// first time through is just for binding
			JsonFx.History.h = elem;
			return;
		}
		var h, info;
		if ("function" === typeof JsonFx.History.onchange) {
			h = JsonFx.UI.getIFrameDocument(elem);
			if (h) {
				info = h.location.search;
				if (info) {
					info = info.substring(info.indexOf('?')+1);
					info = decodeURIComponent(info);
					if (info) {
						info = JSON.parse(info);
					}
					JsonFx.History.onchange(info);
				}
			}
		}
	}
};

/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI Controls
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2007-06-03-1028
	Modified: 2007-06-03-1028
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*--------------------*\
	Expando-Collapso
\*--------------------*/

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
	var op;
	if (expando.hasAppliedOp()) {
		if (elem.value) {// hacky swap out for buttons
			elem.value = " \u2212 ";
			//elem.replaceChild(document.createTextNode("\u2212"), elem.firstChild);
		}
		expando.apply(null, false);
		return false;
	} else {
		op = new JsonFx.UI.Animate.Op();
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

/*void*/ JsonFx.UI.expando_bind = function(/*element*/ elem) {
	// call after elements have been added to document
	var target = null;
	try {
		elem.style.cursor = "pointer";
	} catch (ex) {
		// IE 5.x
		elem.style.cursor = "hand";
	}
	elem.className += " jsonfx-expanded";

	elem.onclick = function (/*event*/ evt) {
		if (!target) {
			// using a closure so only lookup once
			target = document.getElementById(elem.getAttribute("for"));
			if (!target) {
				target = elem.nextSibling;
				while (target && !target.tagName) {
					target = target.nextSibling;
				}
			}
		}

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
};

/*void*/ JsonFx.UI.expando_unbind = function(/*element*/ elem) {
	if ("undefined" !== typeof elem.expando) {
		elem.expando = null;
	}
	elem.onclick = null;
	elem.ondblclick = null;
};

/*---------------*\
	Alpha Image
\*---------------*/

/*void*/ JsonFx.UI.alphaImg_bind = function(/*DOM*/ elem, /*object*/ options) {
	
//	try {
//		// IE7 doesn't like this with async loads
//		if (!elem.filters) {
//			return;
//		}
//	} catch (ex) {
//		alert(ex.message);
//		return;
//	}

	var filter, src;
	// only IE 5.x-6.0	
	if (JsonFx.UI.hasClassName(document.body, "ua-ie-6") || JsonFx.UI.hasClassName(document.body, "ua-ie-5")) {

		options = options || {};
		options.src = options.src || "about:blank";

		// add AlphaImageLoader filter
		elem.style.filter = "progid:DXImageTransform.Microsoft.AlphaImageLoader(sizingMethod='scale')";
		src = elem.src;

		// set to blank img
		elem.src = options.src;
		// copy back so can compare later
		options.src = elem.src;

		// set it back
		elem.src = src;

		// add dispose method
		elem.dispose = function() {
			elem.onpropertychange = null;
			elem.src = filter && filter.src;
			elem.style.filter = "";
			elem = filter = options = null;
		};

		// monitor changes to elem so can update filter
		elem.onpropertychange = function(/*Event*/ evt) {
			evt = evt||window.event;

			if (evt.propertyName === "src" &&
				elem.src !== options.src) {

				// can't populate until after added to document
				filter = filter || elem.filters.item("DXImageTransform.Microsoft.AlphaImageLoader");

				filter.src = elem.src;
				elem.src = options.src;
			}
		};

		// trigger filter call when done loading
		window.setTimeout(
			function() {
				elem.src = elem.src;
			}, 0);
	}
};

/*void*/ JsonFx.UI.alphaImg_unbind = function(/*DOM*/ elem, /*object*/ options) {
	if (elem.dispose) {
		elem.dispose();
	}
};
