/*extern JsonFx, ActiveXObject */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2007-03-11-2235
\*---------------------------------------------------------*/

// XMLHttpRequest: augment browser to have "native" XHR
(function () {
	if ("undefined" === typeof window.XMLHttpRequest) {

		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ var xhrOCXs = [
			"Msxml2.XMLHTTP.6.0",
			"Msxml2.XMLHttp.5.0",
			"Msxml2.XMLHttp.4.0",
			"MSXML2.XMLHTTP.3.0",
			"MSXML2.XMLHTTP",
			"Microsoft.XMLHTTP" ];

		window.XMLHttpRequest = function() {
			while (xhrOCXs.length) {
				try {
					return new ActiveXObject(xhrOCXs[0]);
				} catch (ex) {
					// remove the failed xhrOCXs for future requests
					xhrOCXs.shift();
				}
			}

			// all xhrOCXs failed		
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

/*string*/ JsonFx.IO.userAgent = "JsonFx/1.0";
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
		onSuccess : function(XMLHttpRequest, context){},
		onFailure : function(XMLHttpRequest, context){},
		onTimeout : function(XMLHttpRequest, context){},

		// callback context
		context : object
	};
*/

/*RequestOptions*/ JsonFx.IO.onFailure = function(/*XMLHttpRequest|Error*/ obj, /*object*/ cx) {
	if (obj) {
		var name = obj.name ? obj.name : "Error";
		var msg = obj.message ? obj.message : obj.statusText;
		var code = isFinite(obj.status) ? obj.status : (isFinite(obj.code) ? obj.code : obj.number);
		window.alert("Request Failed - "+name+" ("+Number(code)+"):\n\""+msg+"\"");
	} else {
		window.alert("Request Timeout");
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
	if ("object" !== typeof options.headers) {
		options.headers = {};
		if (options.method === "POST") {
			options.headers["Content-Type"] = "application/x-www-form-urlencoded";
		}
	}
	// prevent server from sending 304 Not-Modified response
	// since we don't have a way to access the browser cache
	options.headers["If-Modified-Since"] = "Sun, 1 Jan 1995 00:00:00 GMT";
	options.headers["Cache-Control"] = "no-cache";
	options.headers.Pragma = "no-cache";

	if ("string" !== typeof options.params) {
		options.params = null;
	}
	if ("number" !== typeof options.timeout) {
		options.timeout = 10000;// 10 seconds
	}
	if ("function" !== typeof options.onSuccess) {
		options.onSuccess = null;
	}
	if ("function" !== typeof options.onFailure) {
		options.onFailure = JsonFx.IO.onFailure;
	}
	if ("function" !== typeof options.onTimeout) {
		options.onTimeout = options.onFailure;
	}
	if ("undefined" === typeof options.context) {
		options.context = null;
	}
	return options;
};

/* returns true if request was sent */
/*bool*/ JsonFx.IO.sendRequest = function(/*string*/ url, /*RequestOptions*/ options) {

	var xhr = new XMLHttpRequest();
	if (!xhr) {
		return false;
	}

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	// kill the request if takes too long
	var cancel = window.setTimeout(
		function() {
			if (xhr) {
				xhr.abort();
				xhr = null;
				if (options.onTimeout) {
					options.onTimeout(null, options.context);
				}
			}
		}, options.timeout);

	function onRSC() {
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

			if (Math.floor(status/100) === 2) {// 200-299
				// success
				if (options.onSuccess) {
//TIMER
//JsonFx.Timer.stop("request", true);//250,250,250
//TIMER
					options.onSuccess(xhr, options.context);
				}

			} else if (status === 0) {
				// timeout

				// IE reports status zero when aborted
				// Firefox throws exception, which we set to zero
				// options.onTimeout has already been called so do nothing

			} else if ("function" === typeof options.onFailure) {
				// failure
				options.onFailure(xhr, options.context);
			}
			xhr = null;
		}
	}

	try {
		xhr.onreadystatechange = onRSC;
		xhr.open(options.method, url, options.async);

		if (options.headers) {
			for (var h in options.headers) {
				if (options.headers.hasOwnProperty(h)) {
					xhr.setRequestHeader(h, options.headers[h]);
				}
			}
		}
//TIMER
//JsonFx.Timer.start("request");
//TIMER
		xhr.send(options.params);
		return true;

	} catch (ex) {
		// immediate failure
		if ("function" === typeof options.onFailure) {
			options.onFailure(ex, options.context);
		}
		return false;
	} finally {
		// in case immediately returns?
		onRSC();
	}
};

/* JsonRequest ----------------------------------------------------*/

/*bool*/ JsonFx.IO.sendJsonRequest = function(
	/*string*/ restUrl,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	options.headers["User-Agent"] = JsonFx.IO.userAgent;
	options.headers.Accept = "application/json";

	var onSuccess = options.onSuccess;
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ context) {
//TIMER
//JsonFx.Timer.start("decode");
//TIMER

		// decode response as JSON
		try {
			var json = xhr.responseText;
			json = json.parseJSON();
//TIMER
//JsonFx.Timer.stop("decode", true);//32,31,22500(greedy regex)
//TIMER
			if ("function" === typeof onSuccess) {
				onSuccess(json, context);
			}
		} catch (ex) {
			if (options.onFailure) {
				options.onFailure(ex, context);
			}
		} finally {
			// free closure references
			onSuccess = options = null;
		}
	};

	return JsonFx.IO.sendRequest(restUrl, options);
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

/* returns true if request was sent */
/*bool*/ JsonFx.IO.sendJsonRpc = function(
	/*string*/ rpcUrl,
	/*string*/ rpcMethod,
	/*object|array*/ rpcParams,
	/*RequestOptions*/ options) {

	// ensure defaults
	options = JsonFx.IO.validateOptions(options);

	options.headers["User-Agent"] = JsonFx.IO.userAgent;
	options.headers.Accept = "application/json";

	// wrap callbacks with RPC layer
	var onSuccess = options.onSuccess;
	var onFailure = options.onFailure;

	// this calls onSuccess with the results of the method (not the RPC wrapper)
	// or it calls onFailure with the error of the method (not the RPC wrapper)
	options.onSuccess = function(/*XMLHttpRequest*/ xhr, /*object*/ cx) {

 		var json = xhr.responseText;
		if ("string" === typeof json) {
			try {
				json = json.parseJSON();
			} catch (ex) {
				if ("function" === typeof onFailure) {
					onFailure(ex, cx);
				}
			}
		}

		if (json.error) {
			if ("function" === typeof onFailure) {
				onFailure(json.error, cx);
			}
		} else {
			if ("function" === typeof onSuccess) {
				onSuccess(json.result, cx);
			}
		}

		// free closure references
		onFailure = onSuccess = null;
	};

	// this calls onFailure with the RPC response
	options.onFailure = function(/*XMLHttpRequest*/ xhr, /*object*/ cx) {

		var json = xhr.responseText;
		if ("string" === typeof json) {
			try {
				json = json.parseJSON();
			} catch (ex) {
				if ("function" === typeof onFailure) {
					onFailure(ex, cx);
				}
			}
		}

		if ("function" === typeof onFailure) {
			onFailure(json, cx);
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
	return JsonFx.IO.sendRequest(rpcUrl, options);
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

			// intercept onSuccess to call onEndRequest
			var onSuccess = options.onSuccess;
			options.onSuccess = function(/*JSON*/ json, /*object*/ cx) {
				self.onEndRequest(cx);

				if ("function" === typeof onSuccess) {
					onSuccess(json, cx);
				}

				// free closure references				
				self = onSuccess = null;
			};

			// intercept onFailure to call onEndRequest
			var onFailure = options.onFailure;
			options.onFailure = function(/*JSON*/ json, /*object*/ cx) {
				self.onEndRequest(cx);

				if ("function" === typeof onFailure) {
					onFailure(json, cx);
				}

				// free closure references				
				self = onFailure = null;
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
