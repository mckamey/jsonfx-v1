/*extern JsonFx */
// ======================================================
//	class JsonFx.Utility (Singleton)
//=======================================================
if ("undefined" !== typeof JsonFx) {
	JsonFx.Utility = {};

	/* DataDump ----------------------------------------------------*/

	/*void*/ JsonFx.Utility.DumpData = function(/*elem*/parent, /*object*/result) {
		var divResult = document.createElement("ul");

		for (var prop in result) {
			if (!result.hasOwnProperty(prop)) {
				continue;
			}
			
			var divProp = document.createElement("li");
			var divName = document.createElement("b");
			divName.appendChild(document.createTextNode(prop+": "));
			var divType = document.createElement("span");
			var divValue = document.createElement("span");
			if (result[prop] === null) {
				divType.appendChild(document.createTextNode("[null] "));
				divValue.appendChild(document.createTextNode(result[prop]));
			} else if (typeof(result[prop]) == "object") {
				if (result[prop] instanceof Array) {
					divType.appendChild(document.createTextNode("[array] "));
				} else {
					divType.appendChild(document.createTextNode("["+typeof(result[prop])+"] "));
				}
				JsonFx.Utility.DumpData(divValue, result[prop]);
			} else {
				divType.appendChild(document.createTextNode("["+typeof(result[prop])+"] "));
				divValue.appendChild(document.createTextNode(result[prop]));
			}
			divProp.appendChild(divType);
			divProp.appendChild(divName);
			divProp.appendChild(divValue);
			divResult.appendChild(divProp);
		}

		parent.appendChild(divResult);
	};

	/* Script Lazy Load ----------------------------------------------------*/

	/*void*/ JsonFx.Utility.RegisterScript = function(/*string*/ sUrl) {
		if (!sUrl) {
			return;
		}

		// check if script already exists
		var aScripts = document.getElementsByTagName("script");
		for (var i=0; i<aScripts.length; i++) {
			if (aScripts[i].src == sUrl) {
				return;
			}
		}

		if (!document.body) {
			// NOTE: Google maps requires this style
		
			// hopefully still in the head
			document.write('<' + 'script src="' + sUrl + '"' +' type="text/javascript"><' + '/script>');
		} else {
			// get document head
			var documentHead = document.getElementsByTagName("head")[0];

			// append a new script element to document head
			var elScript = document.createElement("script");
			elScript.setAttribute("type","text/javascript");
			elScript.setAttribute("src", sUrl);
			documentHead.appendChild(elScript);
		}
	};
	
	/* Query String ----------------------------------------------------*/

	/*string*/ JsonFx.Utility.GetQueryParam = function (/*string*/ sQuery, /*string*/ sKey) {
		if (!sQuery || !sKey) {
			return null;
		}
		// trim off before the query
		sQuery = sQuery.substring(sQuery.lastIndexOf('?')+1);

		var aParams = sQuery.split('&');
		for (var i=0; i<aParams.length; i++) {
			var aParts = aParams[i].split('=');
			if (aParts.length >= 2 && aParts[0] == sKey) {
				return aParts[1];
			}
		}
		
		return null;
	};

	/* Callbacks ----------------------------------------------------*/

	/*void*/ JsonFx.Utility.Notify = function(/*string*/ sKey, /*object*/ sResult) {
		if (!JsonFx.Utility.aNotifyCallbacks || !JsonFx.Utility.aNotifyCallbacks[sKey]) {
			return;
		}

		JsonFx.Utility.aNotifyCallbacks[sKey](sResult);
		delete(JsonFx.Utility.aNotifyCallbacks[sKey]);
	};

	/*void*/ JsonFx.Utility.SetNotifyCallback = function(/*string*/ sKey, /*function*/ fn) {
		if (!JsonFx.Utility.aNotifyCallbacks) {
			JsonFx.Utility.aNotifyCallbacks = [];
		}
		JsonFx.Utility.aNotifyCallbacks[sKey] = fn;
	};

	/* Cookie Storage ----------------------------------------------------*/

	/*void*/ JsonFx.Utility.SetCookie = function(/*string*/ key, /*string*/ value, /*date*/ expires) {
		if (!key || !value) {
			return;
		}

		var cookie = key + "=" + escape(value);
		if (expires) {
			if (expires instanceof Date) {
				expires = expires.toGMTString();
			}
			if (typeof(expires) == "string") {
				cookie += "; expires=" + expires;
			}
		}
		cookie += "; path=/";

		if (document.cookie && document.cookie.length > 0) {
			document.cookie += ";"+cookie;
		} else {
			document.cookie = cookie;
		}
	};

	/*string*/ JsonFx.Utility.GetCookie = function(/*string*/ key) {
		if (!key || !document.cookie) {
			return null;
		}
		var start = document.cookie.indexOf(key+"=");
		if (start < 0) {
			return null;
		}
		start += key.length+1;
		var end = document.cookie.indexOf(";", start);
		if (end < 0) {
			end = document.cookie.length;
		}
		return unescape(document.cookie.substring(start, end));
	};

	/*void*/ JsonFx.Utility.ClearCookie = function(/*string*/ key) {
		JsonFx.Utility.SetCookie(key, key, new Date(2000,1,1));
	};

	/* Distance calculation ----------------------------------------------------*/

	// Based on: http://www.movable-type.co.uk/scripts/LatLong.html
	// Calculate distance (in miles) between two points specified by latitude/longitude using law of cosines
	/*number*/ JsonFx.Utility.GetDistanceMiles = function(lat1, lon1, lat2, lon2) {
		var M = 1.609344; // km per mile
		var R = 6371; // earth's mean radius in km

		// convert to radians
		lat1 = lat1 * Math.PI / 180;
		lon1 = lon1 * Math.PI / 180;
		lat2 = lat2 * Math.PI / 180;
		lon2 = lon2 * Math.PI / 180;

		var d = Math.acos(Math.sin(lat1) * Math.sin(lat2) + Math.cos(lat1) * Math.cos(lat2) * Math.cos(lon2-lon1)) * R;
		return d/M;
	};
}