/*extern JsonFx, ActiveXObject */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2007-03-11-1426
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.IO */
JsonFx.IO = {};

JsonFx.IO.userAgent = "JsonFx/1.0 beta";

/* XMLHttpRequest ----------------------------------------------------*/

// augment browser to have "native" XHR
(function () {
	if ("undefined" === typeof window.XMLHttpRequest) {
		// these IDs are as per MSDN documentation (including case)
		/*string[]*/ var xhrIDs = [
			"Msxml2.XMLHTTP.6.0",
			"Msxml2.XMLHttp.5.0",
			"Msxml2.XMLHttp.4.0",
			"MSXML2.XMLHTTP.3.0",
			"MSXML2.XMLHTTP",
			"Microsoft.XMLHTTP" ];

		window.XMLHttpRequest = function() {
			while (xhrIDs.length) {
				try {
					return new ActiveXObject(xhrIDs[0]);
				} catch (ex) {
					// remove the failed xhrIDs for future requests
					xhrIDs.shift();
				}
			}

			// all xhrIDs failed		
			return null;
		};
	}
})();

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

/* returns true if request was sent */
/*bool*/ JsonFx.IO.sendRequest = function(/*string*/ url, /*RequestOptions*/ options) {

	// establish defaults
	if ("undefined" === typeof options) {
		options = {};
	}
	if ("boolean" !== typeof options.async) {
		options.async = true;
	}
	if ("string" !== typeof options.method) {
		options.method = "POST";
	}
	if ("object" !== typeof options.headers) {
		options.headers = null;
	}
	if ("string" !== typeof options.params) {
		options.params = null;
	}
	if ("number" !== typeof options.timeout) {
		options.timeout = 10000;
	}
	if ("function" !== typeof options.onSuccess) {
		options.onSuccess = null;
	}
	if ("function" !== typeof options.onFailure) {
		options.onFailure = null;
	}
	if ("function" !== typeof options.onTimeout) {
		options.onTimeout = options.onFailure;
	}
	if ("undefined" === typeof options.context) {
		options.context = null;
	}

	var xhr = new XMLHttpRequest();
	if (!xhr) {
		return false;
	}

	var timeoutID = window.setTimeout(
		function() {
			if (xhr) {
				xhr.abort();
				xhr = null;
				if (options.onTimeout) {
					options.onTimeout(null, context);
				}
			}
		},
		options.timeout);

	function cb_readyStateChanged() {
		if (xhr && xhr.readyState === 4 /*complete*/) {
			window.clearTimeout(timeoutID);
			var status;
			try {
				status = Number(xhr.status);
			} catch (ex) {
				/* Firefox doesn't allow status to be accessed after xhr.abort() */
				status = 0;
			}
			if (Math.floor(status/100) === 2) {// catch all 200s
				/* data was retrieved successfully */
				if (options.onSuccess) {
//TIMER
//JsonFx.Timer.stop("request", true);//250,250,250
//TIMER
					options.onSuccess(xhr, options.context);
				}
			} else if (status === 0) {
				/*	IE reports status zero when aborted.
					Firefox throws exception, which we set also to zero,
					suppress these as onTimeout has already been called. */
			} else if (options.onFailure) {
				options.onFailure(xhr, options.context);
			}
			xhr = null;
		}
	}

	try {
		xhr.onreadystatechange = cb_readyStateChanged;
		xhr.open(options.method, url, options.async);

		// this prevents server from sending 304 Not-Modified response
		xhr.setRequestHeader("If-Modified-Since", "Sun, 1 Jan 1995 00:00:00 GMT");
		xhr.setRequestHeader("Pragma", "no-cache");
		xhr.setRequestHeader("Cache-Control", "no-cache");
		if (options.headers) {
			for (var h in options.headers) {
				if ("string" === typeof options.headers[h]) {
					xhr.setRequestHeader(h, options.headers[h]);
				}
			}
		} else if (options.method === "POST") {
			xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		}
//TIMER
//JsonFx.Timer.start("request");
//TIMER
		xhr.send(options.params);
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
/*bool*/ JsonFx.IO.postJsonRequest = function(
	/*string*/ serviceUrl,
	/*string*/ rpcMethod,
	/*object*/ rpcParams,
	/*object*/ id,
	/*function(data,context)*/ onSuccess,
	/*object*/ context) {

	function cb_decodeResponse(response, context) {
		var data = response.responseText;
		if ("string" === typeof data) {
			try {
				data = data.parseJSON();
			} catch (ex) {}
		}
		onSuccess(data, context);
	}

	if ("object" !== typeof rpcParams) {
		rpcParams = [ rpcParams ];
	}
	
	var rpcRequest = {
			"version" : "1.1",
			"method" : rpcMethod,
			"params" : rpcParams,
			"id" : id
		};

	try {
		// JSON encode request
		rpcRequest = rpcRequest.toJSONString();

		var headers = {
			"User-Agent" : JsonFx.IO.userAgent,
			"Content-Type" : "application/json",
			"Content-Length" : rpcRequest.length,
			"Accept" : "application/json"
		};

		return JsonFx.IO.sendRequest(
			serviceUrl, {
				method : "POST",
				headers : headers,
				params : rpcRequest,
				onSuccess : cb_decodeResponse,
				onFailure : JsonFx.IO.onRequestFailed,
				context : context
			});
	} catch (ex) {
		return false;
	}
};

/* returns true if request was sent */
/*bool*/ JsonFx.IO.getJsonRequest = function(
	/*string*/ serviceUrl,
	/*string*/ rpcMethod,
	/*object*/ rpcParams,
	/*function(data,context)*/ onSuccess,
	/*object*/ context) {

	function cb_decodeResponse(response, context) {
//TIMER
//JsonFx.Timer.start("decode");
//TIMER
		var data = response.responseText;
		if ("string" === typeof data) {
			try {
				data = data.parseJSON();
			} catch (ex) { }
		}
//TIMER
//JsonFx.Timer.stop("decode", true);//32,31,22500(greedy regex)
//TIMER
		onSuccess(data, context);
	}

	if (rpcMethod) {
		serviceUrl += "/"+encodeURIComponent(rpcMethod);
	}
	if (rpcParams) {
		serviceUrl += "?";
		if (rpcParams instanceof Array) {
			for (var i=0; i<rpcParams.length; i++) {
				if (i > 0) {
					serviceUrl += "&";
				}
				serviceUrl += encodeURIComponent(i);
				serviceUrl += "=";
				serviceUrl += encodeURIComponent(rpcParams[i]);
			}
		} else {
			for (var p in rpcParams) {
				serviceUrl += encodeURIComponent(p);
				serviceUrl += "=";
				serviceUrl += encodeURIComponent(rpcParams[p]);
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

		return JsonFx.IO.sendRequest(
			serviceUrl, {
				method : "GET",
				headers : headers,
				onSuccess : cb_decodeResponse,
				onFailure : JsonFx.IO.onRequestFailed,
				context : context
			});
	} catch (ex) {
		return false;
	}
};

/* JsonRequest ----------------------------------------------------*/

/* Base type for generated JSON Services */
if ("undefined" === typeof JsonFx.IO.JsonServiceBase) {

	/* Ctor */
	JsonFx.IO.JsonServiceBase = function() {
	};

	/*event*/ JsonFx.IO.JsonServiceBase.prototype.onBeginRequest = null;

	/*event*/ JsonFx.IO.JsonServiceBase.prototype.onEndRequest = null;

	/*void*/ JsonFx.IO.JsonServiceBase.prototype.callService = function(
		/*string*/ rpcMethod,
		/*object*/ rpcParams,
		/*function(string,object)*/ callback,
		/*object*/ context) {

		var self = this;

		if (this.onBeginRequest) {
			this.onBeginRequest(context);
		}

		JsonFx.IO.postJsonRequest(
			this.address,
			rpcMethod,
			rpcParams,
			null,
			function(data,ctxt){
				if (self.onEndRequest) {
					self.onEndRequest(ctxt);
				}
				if ("function" === typeof callback) {
					callback(data,ctxt);
				}
			},
			context);
	};

	/*string*/ JsonFx.IO.JsonServiceBase.prototype["system.describe"] = JsonFx.IO.JsonServiceBase.prototype.$describe = function(
		/*function(string,object)*/ callback,
		/*object*/ context) {

		this.callService("system.describe", null, callback, context);
	};
}
