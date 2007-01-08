/*
    json.js
    2006-12-06

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

        string.parseJSON(hook)
            This method parses a JSON text to produce an object or
            array. It can throw a SyntaxError exception.

            The optional hook parameter is a function which can filter and
            transform the results. It receives each of the values, and its
            return value is used instead. If it returns what it received, then
            structure is not modified.

            Example:

            // Parse the text. If it contains any "NaN" strings, replace them
            // with the NaN value. All other values are left alone.

            myData = text.parseJSON(function (value) {
                if (value === 'NaN') {
                    return NaN;
                }
                return value;
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

        s.parseJSON = function (hook) {
            try {
                if (/^("(\\.|[^"\\\n\r])*?"|[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t])+?$/.
                        test(this)) {
                    var j = eval('(' + this + ')');
                    if (typeof hook === 'function') {
                        function walk(v) {
                            if (v && typeof v === 'object') {
                                for (var i in v) {
                                    if (v.hasOwnProperty(i)) {
                                        v[i] = walk(v[i]);
                                    }
                                }
                            }
                            return hook(v);
                        }
                        return walk(j);
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
    2007-01-07

    This file adds these methods to JavaScript:

        array.parseJsonML()

            This method produces a JSON text from an array. The
            array must not contain any cyclical references.

        string.parseJsonML()

            This method parses a JSON text to produce a tree of
            DOM elements.

    http://jsonml.org/License.htm
*/

Array.prototype.parseJsonML = function () {

	var re = /^\s*(\s*?[\w-]+)\s*[:]\s*(.+?)\s*$/;// styles regex

	//attribute name mapping
	var am = {
		"class" : "className",
		"tabindex" : "tabIndex",
		"accesskey" : "accessKey",
		"hidefocus" : "hideFocus"
	};

	//addAttributes
	/*void*/ function aa(/*element*/ el, /*object*/ a) {
		// foreach attributeName
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
			} else if (am[an.toLowerCase()]) {
				el.setAttribute(am[an.toLowerCase()], a[an]);
			} else {
				el.setAttribute(an, a[an]);
			}
		}
	}

	//appendChild
	/*void*/ function ac(/*element*/ el, /*array or string*/ c) {
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
				//eval(jml[i]);
			}
		}

		return el;
	}

	return p(this);
};

String.prototype.parseJsonML = function () {
	try {
		var jml = this.parseJSON();
		return (jml instanceof Array) ? jml.p() : null;
	} catch (ex) {
		return null;
	}
};

// ======================================================
//	namespace JsonFx
//=======================================================
var JsonFx = {};

if (!Object.prototype.hasOwnProperty) {
	Object.prototype.hasOwnProperty = function(p) {
		return this.constructor.prototype[p] === undefined;
	};
}

String.prototype.trim = function () {
    return this.replace( /^\s*([\s\S]*?)\s*$/, "$1");
};
/*extern JsonFx, ActiveXObject */
// ======================================================
//	class JsonFx.IO (Singleton)
//=======================================================
if ("undefined" !== typeof JsonFx) {
	JsonFx.IO = {};

	JsonFx.IO.userAgent = "JsonFx/1.0 beta";
	JsonFx.IO.callTimeout = 10000;// 10 sec

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
			var data = response.responseText;
			if (typeof(data) === "string") {
				try {
					data = data.parseJSON();
				} catch (ex) { }
			}
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

	/* XMLHttpRequest ----------------------------------------------------*/

	/*object*/ JsonFx.IO.GetXMLHttpRequest = function () {
		if (!window.XMLHttpRequest) {
			var progIDs = [ /*"MSXML2.XMLHTTP.4.0",*/ "MSXML2.XMLHTTP.3.0", "Msxml2.XMLHTTP", "Microsoft.XMLHTTP" ];

			for (var i=0; i<progIDs.length; i++) {
				try {
					var xmlHttp = new ActiveXObject(progIDs[i]);
					return xmlHttp;
				} catch (ex) {}
			}

			return null;
		}

		return new XMLHttpRequest();
	};

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

	/* Base class for generated JSON Services */
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
}

/*extern JsonFx */
// ======================================================
//	class JsonFx.UI (Singleton)
//=======================================================
if ("undefined" !== typeof JsonFx) {
	JsonFx.UI = {};

	/*void*/ JsonFx.UI.Clear = function(/*element*/ el) {
		if (el) {
			while (el.lastChild) {
				el.removeChild(el.lastChild);
			}
		}
	};

	/* if container is null then uses ID(s) to replace page elements */
	/*void*/ JsonFx.UI.DisplayJsonML = function(/*JsonML*/ jml, /*element or string*/ container) {
		// either DOM element or id
		container = (typeof(container) !== "string") ?
			container : document.getElementById(container);

		if (jml && typeof(jml.parseJsonML) === "function") {
			jml = jml.parseJsonML();
			if (jml) {
				if (container) {
					JsonFx.UI.Clear(container);
					container.appendChild(jml);
				} else if (jml.id) {
					container = document.getElementById(jml.id);
					if (container && container.parentNode) {
						container.parentNode.replaceChild(jml);
					}
				} else if (jml.childNodes) {
					while (jml.firstChild) {
						if (!jml.firstChild.id) {
							jml.removeChild(jml.firstChild);
							continue;
						}
						container = document.getElementById(jml.firstChild.id);
						if (container && container.parentNode) {
							container.parentNode.replaceChild(jml.firstChild, container);
						} else {
							jml.removeChild(jml.firstChild);
						}
					}
				}
			}
		}
	};

	/* returns true if request was sent */
	/*bool*/ JsonFx.UI.LoadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {
		return JsonFx.IO.GetJsonRequest(url, null, null,
				function(jml,obj) {
					JsonFx.UI.DisplayJsonML(jml,container);
					if (callback) { callback(context); }
				}
			, null);
	};

	/*string*/ JsonFx.UI.GetStyle = function(/*elem*/ el, /*string*/ style) {
		if (typeof(el) === "string") {
			el = document.getElementById(el);
		}
		if (window.getComputedStyle) {
			// W3C DOM equivalent to currentStyle:
			return document.defaultView.getComputedStyle(el, null).getPropertyValue(style);
		}
		if (el.currentStyle) {
			// IE only
			return el.currentStyle[style];
		}
		return null;
	};

	/*-----------------*\
		Maximize Verb
	\*-----------------*/

	/*function*/ JsonFx.UI.Maximize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

		if (typeof(elem) === "string") {
			// either DOM element or id
			elem = document.getElementById(elem);
		}

		if (!elem) {
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
				es.width = JsonFx.UI.GetStyle(document.documentElement, "width");
				es.height = JsonFx.UI.GetStyle(document.documentElement, "height");
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
		"None": 0,
		"Horz": 1,
		"Vert": 2,
		"Both": 3,

		"isHorz": function(dir) { return (JsonFx.UI.Dir.Horz&dir)!==0; },
		"isVert": function(dir) { return (JsonFx.UI.Dir.Vert&dir)!==0; }
	};

	/*function*/ JsonFx.UI.Minimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

		if (typeof(elem) === "string") {
			// either DOM element or id
			elem = document.getElementById(elem);
		}

		if (!elem) {
			return null;
		}

		var es = elem.style,
		/*bool*/ mutex = false,
		/*bool*/ state = false,
		/*const float*/ StepMin = 0.0,
		/*const float*/ StepMax = 1.0,
		/*const float*/ StepInc = 0.05;

		var userOverflow, userHeight, userWidth, pxHeight, pxWidth;

		/*void*/ function saveShape() {
			userOverflow = es.overflow;
			userHeight = es.height;
			userWidth = es.width;
			pxHeight = typeof(elem.offsetHeight) !== "undefined" ?
					elem.offsetHeight : parseFloat(JsonFx.UI.GetStyle(elem, "height"));
			pxWidth = typeof(elem.offsetWidth) !== "undefined" ?
					elem.offsetWidth : parseFloat(JsonFx.UI.GetStyle(elem, "width"));
		}

		// state: true = collapse, false = expand
		return /*void*/ function min(/*bool*/ newState) {
		
			// store the latest requested state
			if (typeof(newState) === "boolean") {
				state = newState;
			} else {
				state = !state;
			}
			if (mutex) {
				// concurrency check
				return;
			}
			mutex = true;

			if (state) {
				saveShape();
			}
			es.display = "block";

			// minimizeStep
			/*void*/ function m(/*float*/ step) {

				if (step < StepMin || step > StepMax) {
					if (state) {
						es.display = "none";
					}
					es.overflow = userOverflow;
					es.width = userWidth;
					es.height = userHeight;
					mutex = false;
					return;
				}

				es.overflow = "hidden";

				if (JsonFx.UI.Dir.isHorz(dir)) {
					// horizontal, simplified lerp
					es.width = Math.floor(pxWidth*step)+"px";
				}
				if (JsonFx.UI.Dir.isVert(dir)) {
					// vertical, simplified lerp
					es.height = Math.floor(pxHeight*step)+"px";
				}

				setTimeout(function() {
						m(state ? (step-StepInc) : (step+StepInc));
					}, 0);
			}

			m(state ? StepMax : StepMin);
		};
	};
}