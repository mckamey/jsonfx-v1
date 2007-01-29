/*extern JsonFx, ActiveXObject */
/*---------------------------------------------------------*\
	JsonFx IO
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-09-0120
	Modified: 2007-01-28-1225
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
