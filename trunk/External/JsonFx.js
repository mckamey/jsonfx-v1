/*extern JsonFx*/
/*---------------------------------------------------------*\
	Modifications to global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-02-07-2339
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

///*float*/ JsonFx.JScriptVersion = ("undefined" === typeof window.ScriptEngineMajorVersion) ? NaN :
//		Number(window.ScriptEngineMajorVersion()+"."+window.ScriptEngineMinorVersion());

if ("undefined" === typeof(window.global)) {
	/*object*/ window.global = window;
}

if ("undefined" === typeof(Object.prototype.hasOwnProperty)) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
		return ("undefined" !== typeof(this[p]) && ("undefined" === typeof(this.constructor.prototype[p])));
	};
}

if ("undefined" === typeof(Object.prototype.propertyIsEnumerable)) {
	/*bool*/ Object.prototype.propertyIsEnumerable = function(/*object*/ p) {
		return this.hasOwnProperty(p);
	};
}

if ("undefined" === typeof(Array.prototype.push)) {
	/*int*/ Array.prototype.push = function(/*object*/ obj) {
		this[this.length] = obj;
		return this.length;
	};
}

if ("undefined" === typeof(Array.prototype.pop)) {
	/*object*/ Array.prototype.pop = function() {
		if (this.length < 1) {
			return undefined;
		}
		var obj = this[this.length-1];
		this.length--;
		return obj;
	};
}

if ("undefined" === typeof(String.prototype.charCodeAt)) {
	/*int*/ String.prototype.charCodeAt = function (/*int*/ i) {
		if (isNaN(i) || i<0 || i>=this.length) {
			return NaN;
		}
		return Number(this[i]-"A")+65;
	};
}

if ("undefined" === typeof(Number.prototype.toPrecision)) {
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

if ("undefined" === typeof(Object.prototype.isPrototypeOf)) {
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

if ("undefined" === typeof(window.encodeURI)) {
	/*string*/ window.encodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.encodeURIComponent)) {
	/*string*/ window.encodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.decodeURI)) {
	/*string*/ window.decodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.decodeURIComponent)) {
	/*string*/ window.decodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

//if ("undefined" === typeof(Array.prototype.shift)) {
//	/*array*/ Array.prototype.shift = function(...) {
//	};
//}

//if ("undefined" === typeof(Array.prototype.unshift)) {
//	/*array*/ Array.prototype.unshift = function(...) {
//	};
//}

//if ("undefined" === typeof(Array.prototype.splice)) {
//	/*array*/ Array.prototype.splice = function(...) {
//	};
//}

if ("undefined" === typeof(String.prototype.trim)) {
	/*string*/ String.prototype.trim = function () {
		return this.replace(/^\s*|\s*$/g, "");
	};
}

if ("undefined" === typeof(String.prototype.contains)) {
	/*string*/ String.prototype.contains = function (str) {
		return (this.indexOf(str) >= 0);
	};
}

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
    2007-01-10

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
    ECMAScript standard in 2007.
*/
if (!Object.prototype.toJSONString) {
    Array.prototype.toJSONString = function () {
        var a = ['['], b, i, l = this.length, v;

        function p(s) {
            if (b) {
                a.push(',');
            }
            a.push(s);
            b = true;
        }

        for (i = 0; i < l; i += 1) {
            v = this[i];
            switch (typeof v) {
            case 'undefined':
            case 'function':
            case 'unknown':
                break;
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
        a.push(']');
        return a.join('');
    };

    Boolean.prototype.toJSONString = function () {
        return String(this);
    };

    Date.prototype.toJSONString = function () {

        function f(n) {
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
        return isFinite(this) ? String(this) : "null";
    };

    Object.prototype.toJSONString = function () {
        var a = ['{'], b, i, v;

        function p(s) {
            if (b) {
                a.push(',');
            }
            a.push(i.toJSONString(), ':', s);
            b = true;
        }

        for (i in this) {
            if (this.hasOwnProperty(i)) {
                v = this[i];
                switch (typeof v) {
                case 'undefined':
                case 'function':
                case 'unknown':
                    break;
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
        a.push('}');
        return a.join('');
    };


    (function (s) {
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
            try {
                if (/^("(\\.|[^"\\\n\r])*?"|[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t])+?$/.
                        test(this)) {
                    var j = eval('(' + this + ')');
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
                        return walk('', j);
                    }
                    return j;
                }
            } catch (e) {
            }
            throw new SyntaxError("parseJSON");
        };

        s.toJSONString = function () {
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
	Modified: 2007-01-29-2154

	Released under a BSD-style open-source license:
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

	var re = /^\s*([a-zA-Z-]+[\w-]*)\s*[:]\s*((\S+\s*\S+)+)\s*$/;// styles RegExp

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
				var s = a[an];// styles
				s = s.split(";");
				for (var i=0; i<s.length; i++) {
					if (!s[i]) {
						continue;
					}
					if (s[i].match(re)) {
						var n = RegExp.$1; // style property
						var v = RegExp.$2; // style value
						if (n && v) {
							if (n === "float") {
								n = "styleFloat";
							} else {
								// convert property name to camelCase
								n = n.split('-');
								n[0] = n[0].toLowerCase();
								for (var j=1; j<n.length; j++) {
									n[j] = n[j].charAt(0).toUpperCase()+n[j].substr(1).toLowerCase();
								}
								n = n.join("");
							}
							el.style[n] = v;
						}
					}
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

		return (el && filter) ? filter(el) : el;
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

/*extern JsonFx, ActiveXObject */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2007-02-07-2339
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* singleton JsonFx.IO */
JsonFx.IO = {};

JsonFx.IO.userAgent = "JsonFx/1.0 beta";
JsonFx.IO.callTimeout = 10000;// 10 sec

/* XMLHttpRequest ----------------------------------------------------*/

/*object*/ JsonFx.IO.GetXMLHttpRequest = function () {
	if ("undefined" === typeof window.XMLHttpRequest) {
		var progIDs = [
			"Msxml2.XMLHTTP.6.0",
			"Msxml2.XMLHttp.5.0",
			"Msxml2.XMLHttp.4.0",
			"MSXML2.XMLHTTP.3.0",
			"MSXML2.XMLHTTP",
			"Microsoft.XMLHTTP" ];

		for (var i=0; i<progIDs.length; i++) {
			try {
				return new ActiveXObject(progIDs[i]);
			} catch (ex) {}
		}

		return null;
	}

	return new XMLHttpRequest();
};
/*bool*/ JsonFx.IO.supportsXMLHttp = !!JsonFx.IO.GetXMLHttpRequest();

/* returns true if request was sent */
/*bool*/ JsonFx.IO.SendRequest = function(
	/*string*/ url,
	/*string*/ params,
	/*string*/ HttpMethod,
	/*bool*/ bAsync,
	/*object*/ headers,
	/*function(response,context)*/ cb_responseSuccess,
	/*function(response,context)*/ cb_responseFail,
	/*object*/ context) {

	if (!HttpMethod) {
		HttpMethod = "POST";
	}

	var request = JsonFx.IO.GetXMLHttpRequest();
	var timeout = setTimeout(
		function() {
			if (request) {
				request.abort();
				request = null;
				cb_responseFail(null, context);
			}
		},
		JsonFx.IO.callTimeout);

	function cb_readyStateChanged() {
		if (request && request.readyState === 4 /*complete*/) {
			clearTimeout(timeout);
			var status;
			try {
				status = request.status;
			} catch (ex) {
				/* Firefox doesn't allow status to be accessed after request.abort() */
				status = 0;
			}
			if (status === 200) {
				/* data was retrieved successfully */
				if (cb_responseSuccess) {
/*TIMER*/
//JsonFx.Timer.stop("request", true);//250,250,250
/*TIMER*/
					cb_responseSuccess(request, context);
				}
			} else if (status === 0) {
				/*	IE reports status zero when aborted.
					firefox throws exception, which we set also to zero,
					suppress these as cb_responseFail has already been called. */
			} else {
				if (cb_responseFail) {
					cb_responseFail(request, context);
				}
			}
			request = null;
		}
	}

	if (!request) {
		return false;
	}

	try {
		request.onreadystatechange = cb_readyStateChanged;
		request.open(HttpMethod, url, bAsync);

		// this prevents server from sending 304 Not-Modified response
		request.setRequestHeader("If-Modified-Since", "Sat, 17 Jun 1995 00:00:00 GMT");
		request.setRequestHeader("Pragma", "no-cache");
		request.setRequestHeader("Cache-Control", "no-cache");
		if (headers) {
			for (var header in headers) {
				if (typeof(headers[header]) === "string") {
					request.setRequestHeader(header, headers[header]);
				}
			}
		} else if (HttpMethod === "POST") {
			request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		}
/*TIMER*/
//JsonFx.Timer.start("request");
/*TIMER*/
		request.send(params);
		return true;
	} catch (ex) {
		//throw new Error("HTTP Request Error: "+ex.message+"\n\n"+url);
		return false;
	} finally {
		// in case immediately returns?
		cb_readyStateChanged();
	}
};

/* JsonRequest ----------------------------------------------------*/

/*void*/ JsonFx.IO.onRequestFailed = function(/*XMLHTTP*/response, /*object*/context) {
	if (response) {
		window.alert("JsonFx.IO ERROR: "+response.statusText+" ("+response.status+")");
	} else {
		window.alert("JsonFx.IO ERROR: Timeout");
	}
};

/* returns true if request was sent */
/*bool*/ JsonFx.IO.PostJsonRequest = function(
	/*string*/ serviceUrl,
	/*string*/ method,
	/*object*/ params,
	/*object*/ id,
	/*function(data,context)*/ cb_responseSuccess,
	/*object*/ context) {

	function cb_decodeResponse(response, context) {
		var data = response.responseText;
		if (typeof(data) === "string") {
			try {
				data = data.parseJSON();
			} catch (ex) {}
		}
		cb_responseSuccess(data, context);
	}

	if (params && typeof(params) !== "object") {
		params = [ params ];
	}
	
	var rpcRequest = { "version":"1.1", "method":method, "params":params, "id":id };

	try {
		// JSON encode request
		rpcRequest = rpcRequest.toJSONString();
		
		var headers = {
			"User-Agent" : JsonFx.IO.userAgent,
			"Content-Type" : "application/json",
			"Content-Length" : rpcRequest.length,
			"Accept" : "application/json"
		};

		return JsonFx.IO.SendRequest(serviceUrl, rpcRequest, "POST", true, headers, cb_decodeResponse, JsonFx.IO.onRequestFailed, context);
	} catch (ex) {
		return false;
	}
};

/* returns true if request was sent */
/*bool*/ JsonFx.IO.GetJsonRequest = function(
	/*string*/ serviceUrl,
	/*string*/ method,
	/*object*/ params,
	/*function(data,context)*/ cb_responseSuccess,
	/*object*/ context) {

	function cb_decodeResponse(response, context) {
/*TIMER*/
//JsonFx.Timer.start("decode");
/*TIMER*/
		var data = response.responseText;
		if (typeof(data) === "string") {
			try {
				data = data.parseJSON();
			} catch (ex) { }
		}
/*TIMER*/
//JsonFx.Timer.stop("decode", true);//32,31,22500(greedy regex)
/*TIMER*/
		cb_responseSuccess(data, context);
	}

	if (method) {
		serviceUrl += "/"+encodeURIComponent(method);
	}
	if (params) {
		serviceUrl += "?";
		if (params instanceof Array)
		{
			for (var i=0; i<params.length; i++) {
				if (i > 0) {
					serviceUrl += "&";
				}
				serviceUrl += encodeURIComponent(i);
				serviceUrl += "=";
				serviceUrl += encodeURIComponent(params[i]);
			}
		} else {
			for (var param in params) {
				serviceUrl += encodeURIComponent(param);
				serviceUrl += "=";
				serviceUrl += encodeURIComponent(params[param]);
			}
		}
	}

	try {
		var headers = {
			"User-Agent" : JsonFx.IO.userAgent,
//				"Content-Type" : "application/json",
//				"Content-Length" : 0,
			"Accept" : "application/json"
		};

		return JsonFx.IO.SendRequest(serviceUrl, null, "GET", true, headers, cb_decodeResponse, JsonFx.IO.onRequestFailed, context);
	} catch (ex) {
		return false;
	}
};

/* JsonRequest ----------------------------------------------------*/

/* Base type for generated JSON Services */
if (typeof(JsonFx.IO.JsonServiceBase) === "undefined") {

	/* Ctor */
	JsonFx.IO.JsonServiceBase = function() {
	};

	/*event*/ JsonFx.IO.JsonServiceBase.prototype.onBeginRequest = null;

	/*event*/ JsonFx.IO.JsonServiceBase.prototype.onEndRequest = null;

	/*void*/ JsonFx.IO.JsonServiceBase.prototype.callService = function(
		/*string*/ method,
		/*object*/ params,
		/*function(string,object)*/ callback,
		/*object*/ context) {

		var self = this;

		if (this.onBeginRequest) {
			this.onBeginRequest(context);
		}

		JsonFx.IO.PostJsonRequest(
			this.address,
			method,
			params,
			null,
			function(data,ctxt){
				if (self.onEndRequest) {
					self.onEndRequest(ctxt);
				}
				callback(data,ctxt);
			},
			context);
	};

	/*string*/ JsonFx.IO.JsonServiceBase.prototype["system.describe"] = function(
		/*function(string,object)*/ callback,
		/*object*/ context) {

		this.callService("system.describe", null, callback, context);
	};
}

/*extern JsonFx */
/*---------------------------------------------------------*\
	JsonFx UI
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-11-1759
	Modified: 2007-02-07-2339
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
JsonFx.UI = {};

/* Utilities ----------------------------------------------------*/

/*string*/ JsonFx.UI.getStyle = function(/*elem*/ elem, /*string*/ style) {
	if (typeof(elem) === "string") {
		elem = document.getElementById(elem);
	}
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(elem, null).getPropertyValue(style);
	}
	if (elem.currentStyle) {
		// IE only
		return elem.currentStyle[style];
	}
	return null;
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

/*-------------------*\
	Binding Methods
\*-------------------*/

/* singleton JsonFx.UI.Bindings */
JsonFx.UI.Bindings = function() {

	/*object*/ var b = this;
	/*const string*/ var BindAction = "B", UnbindAction = "U";

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
			bindings[tag][css][BindAction] = bind ? bind : null;
			bindings[tag][css][UnbindAction] = unbind ? unbind : null;
		}
	};

	/*element*/ var performOne = function(/*element*/ elem, /*actionKey*/ a) {
		if (elem && elem.tagName && elem.className) {

/*TIMER*/
//JsonFx.Timer.start(a+"_one");
/*TIMER*/

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

/*TIMER*/
//JsonFx.Timer.stop(a+"_one", true);//48/16,46/31,62/0
/*TIMER*/

		}
		return elem;
	};

	// perform a binding action on child elements
	/*void*/ var perform = function(/*element*/ root, /*actionKey*/ a) {

/*TIMER*/
//JsonFx.Timer.start(a+"_all");
/*TIMER*/
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

/*TIMER*/
//JsonFx.Timer.stop(a+"_all", true);//32,31,31
/*TIMER*/
	};

	// used as JsonML filter
	/*element*/ b.bindOne = function(/*element*/ elem) {
		return performOne(elem, BindAction);
	};

	// bind
	/*void*/ b.bindAll = function() {
		// store previous values
		var ttl = document.body.title;
		var crsr = document.body.style.cursor;

		// setup loading visual cues
		document.body.title = "Loading...";
		document.body.style.cursor = "wait";

		try {
			perform(document, BindAction);
		} finally {
			// restore previous values
			document.body.title = ttl ? ttl : "";
			document.body.style.cursor = crsr ? crsr : "";
		}
	};

	// unbind
	/*void*/ b.unbindAll = function() {
		// store previous values
		var ttl = document.body.title;
		var crsr = document.body.style.cursor;

		// setup loading visual cues
		document.body.title = "Unloading...";
		document.body.style.cursor = "wait";

		try {
			perform(document, UnbindAction);
		} finally {
			// restore previous values
			document.body.title = ttl ? ttl : "";
			document.body.style.cursor = crsr ? crsr : "";
		}
	};

	// bind
	/*void*/ b.bind = function(/*element*/ root) {
		perform(root, BindAction);
	};

	// unbind
	/*void*/ b.unbind = function(/*element*/ root) {
		perform(root, UnbindAction);
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
/*element*/ JsonFx.UI.displayJsonML = function(/*JsonML*/ jml, /*element or string*/ container) {

/*TIMER*/
//JsonFx.Timer.start("display");
/*TIMER*/

	// either DOM element or id
	container = (typeof(container) !== "string") ?
		container : document.getElementById(container);

	if (jml && typeof(jml.parseJsonML) === "function") {
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
/*TIMER*/
//JsonFx.Timer.stop("display", true);//265,266,266
/*TIMER*/
	return container;
};

/* returns true if request was sent */
/*bool*/ JsonFx.UI.loadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {

/*TIMER*/
//JsonFx.Timer.start("load");
/*TIMER*/

	return JsonFx.IO.GetJsonRequest(url, null, null,
			function(jml,obj) {

/*TIMER*/
//JsonFx.Timer.stop("load", true);//282,281,22750(greedy regex)
/*TIMER*/
				JsonFx.UI.displayJsonML(jml, container);
				if (callback) { callback(context); }
			}
		, null);
};

/*-----------------*\
	Maximize Verb
\*-----------------*/

/*function*/ JsonFx.UI.createMaximize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) === "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem || !elem.tagName) {
		return null;
	}

	var es = elem.style,
	/*bool*/ state = false;

	var userShape;

	/*void*/ function saveShape() {
		userShape = {};
		for (var s in es) {
			if (/*es.hasOwnProperty(s) &&*/ es[s]) {
				userShape[s] = es[s];
			}
		}
	}
	/*void*/ function restoreShape() {
		if (userShape) {
			for (var s in userShape) {
				if (userShape.hasOwnProperty(s)) {
					es[s] = userShape[s];
				}
			}
		}
	}

	// state: true = maixmize, false = restore
	return /*void*/ function max(/*bool*/ newState) {

		if (!state) {
			saveShape();
		}
		// store the latest requested state
		if (typeof(newState) === "boolean") {
			state = newState;
		} else {
			state = !state;
		}

		if (state) {
			es.floatStyle = "none";
			es.position = "absolute";
			es.width = JsonFx.UI.getStyle(document.documentElement, "width");
			es.height = JsonFx.UI.getStyle(document.documentElement, "height");
			es.top = "0px";
			es.left = "0px";
			es.bottom = "0px";
			es.right = "0px";
		} else {
			restoreShape();
		}
	};
};

/*-----------------*\
	Minimize Verb
\*-----------------*/

JsonFx.UI.Dir = {
	None:  0x00,
	Horz:  0x01,
	Vert:  0x02,
	Fade:  0x04,
	Zoom:  0x08,
	ClipX: 0x10,
	ClipY: 0x20,
/*	Both: 0x1|0x2,*/
/*	All: 0x1|0x2|0x4|0x8,*/

	// helper method which tests for Horz
	/*bool*/ isHorz: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Horz & dir);
	},

	// helper method which tests for Vert
	/*bool*/ isVert: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Vert & dir);
	},

	// helper method which tests for Fade
	/*bool*/ isFade: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Fade & dir);
	},

	// helper method which tests for Zoom
	/*bool*/ isZoom: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Zoom & dir);
	},

	// helper method which tests for ClipX
	/*bool*/ isClipX: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.ClipX & dir);
	},

	// helper method which tests for ClipY
	/*bool*/ isClipY: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.ClipY & dir);
	}
};

/*function*/ JsonFx.UI.createMinimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) === "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem || !elem.tagName) {
		return null;
	}

	var es = elem.style,
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ StepMin = 0.0,
	/*const float*/ StepMax = 1.0,
	/*const float*/ StepInc = 0.05;

	var pxHeight = "",
		pxWidth = "",
		userHeight = "",
		userWidth = "",
		userOverflow = "",
		userPosition = "",
		userFilter = "",
		userKhtml = "",
		userMoz = "",
		userOpacity = "",
		userZoom = "",
		userClip = "",
		alpha = null;

	/*void*/ function saveShape() {
		if (elem && es) {
			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userOverflow = es.overflow;
			}
			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userHeight = es.height;
				pxHeight = !isNaN(elem.offsetHeight) ?
					elem.offsetHeight : parseFloat(JsonFx.UI.getStyle(elem, "height"));
			}
			if (JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir)) {
				userWidth = es.width;
				pxWidth = !isNaN(elem.offsetWidth) ?
					elem.offsetWidth : parseFloat(JsonFx.UI.getStyle(elem, "width"));
			}
			if (JsonFx.UI.Dir.isFade(dir)) {
				userKhtml = es["-khtml-opacity"];
				userMoz = es["-moz-opacity"];
				userOpacity = es.opacity;
				if (elem.filters && !alpha) {
					es.filter += " progid:DXImageTransform.Microsoft.Alpha(enabled=false)";
					try {
						alpha = elem.filters.item("DXImageTransform.Microsoft.Alpha");
					} catch (ex) { alpha = null; }
					if (!alpha) {
						es.filter += " alpha(enabled=false)";
						try {
							alpha = elem.filters.item("alpha");
						} catch (ex) { alpha = null; }
					}
				}
			}
			if (JsonFx.UI.Dir.isZoom(dir)) {
				userZoom = es.zoom;
			}
			if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				userPosition = es.position;
				userClip = es.clip;
			}
		}
	}

	// state: true = collapse, false = expand
	return /*void*/ function min(/*bool*/ newState) {

		if (!es) { return; }

		// store the latest requested state
		if (typeof(newState) === "boolean") {
			state = newState;
		} else {
			state = !state;
		}
		if (mutex) {
			// crude concurrency check
			return;
		}
		mutex = true;

		if (state) {
			saveShape();
		}
		es.display = "block";
		es.visibility = "visible";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (!es || isNaN(step)) {
				mutex = false;
				return;
			}

			if (step < StepMin || step > StepMax) {
				if (state) {
					if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isZoom(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
						es.display = "none";
					}
					es.visibility = "hidden";
				}
				if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
					es.overflow = userOverflow;
				}
				if (JsonFx.UI.Dir.isHorz(dir)) {
					es.width = userWidth;
				}
				if (JsonFx.UI.Dir.isVert(dir)) {
					es.height = userHeight;
				}
				if (JsonFx.UI.Dir.isFade(dir)) {
					es["-khtml-opacity"] = userKhtml;
					es["-moz-opacity"] = userMoz;
					es.opacity = userOpacity;
					if (alpha) {
						alpha.enabled = false;
					}
					if (userFilter) {
						es.filter = userFilter;
					}
				}
				if (JsonFx.UI.Dir.isZoom(dir)) {
					es.zoom = userZoom;
				}
				if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
					es.position = userPosition;
					es.clip = userClip;
				}
				mutex = false;
				return;
			}

			if (JsonFx.UI.Dir.isVert(dir) || JsonFx.UI.Dir.isHorz(dir) || JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				es.overflow = "hidden";
			}

			if (JsonFx.UI.Dir.isHorz(dir) && !isNaN(pxWidth)) {
				// horizontal, simplified lerp
				es.width = Math.floor(pxWidth*step)+"px";
			}
			if (JsonFx.UI.Dir.isVert(dir) && !isNaN(pxHeight)) {
				// vertical, simplified lerp
				es.height = Math.floor(pxHeight*step)+"px";
			}
			if (JsonFx.UI.Dir.isFade(dir)) {
				// opacity, simplified lerp
				es["-khtml-opacity"] = 1.0*step;
				es["-moz-opacity"] = 1.0*step;
				es.opacity = 1.0*step;
				if (alpha) {
					alpha.opacity = Math.floor(100*step);
					alpha.enabled = true;
				}
			}
			if (JsonFx.UI.Dir.isZoom(dir)) {
				es.zoom = Math.floor(100*step)+"%";
			}
			if (JsonFx.UI.Dir.isClipX(dir) || JsonFx.UI.Dir.isClipY(dir)) {
				var clip = ["auto","auto","auto","auto"];
				if (JsonFx.UI.Dir.isClipX(dir) && !isNaN(pxWidth)) {
					clip[3] = Math.ceil(pxWidth*(1-step)/2)+"px";
					clip[1] = ((pxWidth/2)+Math.floor(pxWidth*step/2))+"px";
				}
				if (JsonFx.UI.Dir.isClipY(dir) && !isNaN(pxHeight)) {
					clip[0] = Math.ceil(pxHeight*(1-step)/2)+"px";
					clip[2] = ((pxHeight/2)+Math.floor(pxHeight*step/2))+"px";
				}
				es.position = "absolute";
				es.clip = "rect("+clip.join(' ')+")";
			}

			setTimeout(
				function() {
					m(state ? (step-StepInc) : (step+StepInc));
				},
				0);
		}

		m(state ? StepMax : StepMin);
	};
};

/* DataDump ----------------------------------------------------*/

/*void*/ JsonFx.UI.expandoCreate = function(/*element*/ elem) {
	elem.style.cursor = "pointer";
	elem.minimized = false;

	elem.onclick = function (/*event*/ evt) {
		var next = elem.nextSibling;
		if (!next.minimize) {
			if (next.tagName && next.tagName.toLowerCase() === "ul") {
				next.minimize = JsonFx.UI.createMinimize(next, JsonFx.UI.Dir.Vert|JsonFx.UI.Dir.Fade);
			} else {
				return false;
			}
		}
		elem.minimized = !elem.minimized;
		next.minimize(elem.minimized);
		if (elem.minimized) {
			elem.className = elem.className.replace(/\s*jsonfx-expanded/g, " jsonfx-collapsed");
		} else {
			elem.className = elem.className.replace(/\s*jsonfx-collapsed/g, " jsonfx-expanded");
		}
		return false;
	};
};
/*void*/ JsonFx.UI.expandoDispose = function(/*element*/ elem) {
	var next = elem.nextSibling;
	if (next.minimize) {
		next.minimize = null;
	}
	elem.onclick = null;
};

JsonFx.UI.Bindings.register("label", "jsonfx-expando", JsonFx.UI.expandoCreate, JsonFx.UI.expandoDispose);

/*JsonML*/ JsonFx.UI.dumpData = function(/*json*/ data) {
	if (data === null) {
		return "null";
	}
	var ul = ["ul", {"class":"jsonfx-object"}];

	for (var pn in data) {
		if (!data.hasOwnProperty(pn)) {
			continue;
		}
		var pv = data[pn];
		var pt = typeof(pv);

		var li = ["li"];
		var a = null;
		if ("object" === pt && pv) {
			a = ["label", {"class":"jsonfx-expando jsonfx-expanded"}];
			li.push(a);
		}
		(a?a:li).push(["span", {"class":"jsonfx-type"}, (pv instanceof Array) ? "array" : pt]);
		(a?a:li).push(["span", {"class":"jsonfx-name"}, pn]);

		if ("object" === pt) {
			li.push(JsonFx.UI.dumpData(pv));
		} else {
			li.push(["span", {"class":"jsonfx-value"}, String(pv)]);
		}
		ul.push(li);
	}

	return ul;
};
