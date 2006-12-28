/*extern JsonFx, ActiveXObject */
// ======================================================
//	class JsonFx.IO (Singleton)
//=======================================================
if ("undefined" !== typeof JsonFx) {
	JsonFx.IO = {};

	JsonFx.IO.userAgent = "JsonFx/1.0 beta";

	/* JsonRequest ----------------------------------------------------*/

	/*void*/ JsonFx.IO.PostJsonRequest = function(
		/*string*/ serviceUrl,
		/*string*/ method,
		/*object*/ params,
		/*object*/ id,
		/*function(string,object)*/ cb_responseComplete,
		/*object*/ context) {

		function cb_decodeResponse(response, context) {
			var data = response.responseText;
			if (typeof(data) === "string") {
				try {
					data = data.parseJSON();
				} catch (ex) {}
			}
			cb_responseComplete(data, context);
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

			JsonFx.IO.SendRequest(serviceUrl, rpcRequest, "POST", true, headers, cb_decodeResponse, context);
		} catch (ex) {}
	};

	/*void*/ JsonFx.IO.GetJsonRequest = function(
		/*string*/ serviceUrl,
		/*string*/ method,
		/*object*/ params,
		/*function(string,object)*/ cb_responseComplete,
		/*object*/ context) {

		function cb_decodeResponse(response, context) {
			var data = response.responseText;
			if (typeof(data) === "string") {
				try {
					data = data.parseJSON();
				} catch (ex) { }
			}
			cb_responseComplete(data, context);
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

		var headers = {
			"User-Agent" : JsonFx.IO.userAgent,
//			"Content-Type" : "application/json",
//			"Content-Length" : 0,
			"Accept" : "application/json"
		};

		JsonFx.IO.SendRequest(serviceUrl, null, "GET", true, headers, cb_decodeResponse, context);
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

	/*void*/ JsonFx.IO.SendRequest = function(
		/*string*/ url,
		/*string*/ params,
		/*string*/ HttpMethod,
		/*bool*/ bAsync,
		/*object*/ headers,
		/*function*/ cb_responseComplete,
		/*object*/ context) {

		if (!HttpMethod) {
			HttpMethod = "POST";
		}

		var request = JsonFx.IO.GetXMLHttpRequest();

		function cb_readyStateChanged() {
			if (request && request.readyState === 4 /*complete*/) {
				/* If the data was retrieved successfully */ 
				if (request.status === 200) 
				{ 
					if (cb_responseComplete) {
						cb_responseComplete(request, context);
					}
				}
				/* IE returns a status code of 0 on some occasions, so ignore this case */ 
				else if (request.status !== 0) 
				{ 
					window.alert("JsonFx.IO ERROR: " + request.statusText); 
				} 
				request = null;
			}
		}

		if (request) {
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
			} catch (ex) {
				throw new Error("HTTP Request Error: "+ex.message+"\n\n"+url);
			} finally {
				cb_readyStateChanged();
			}
		}
	};

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
