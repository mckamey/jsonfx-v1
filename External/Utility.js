/*global JsonFx */
/*---------------------------------------------------------*\
	JsonFx Utils
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-02-2330
	Modified: 2007-01-26-1527
\*---------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* singleton JsonFx.Utils */
if ("undefined" === typeof JsonFx.Utils) {
	JsonFx.Utils = {};
}

/* Script Lazy Load ----------------------------------------------------*/

/*void*/ JsonFx.Utils.loadScript = function(/*string*/ url) {
	if (!url) {
		return;
	}

	// check if script already exists
	/*elem[]*/ var scripts = document.getElementsByTagName("script");
	for (var i=0; i<scripts.length; i++) {
		if (scripts[i].src === url) {
			return;
		}
	}

	/*element*/ var doc, head, elem;
	if (!document.body) {
		// NOTE: Google maps requires this style

		// hopefully still in the head
		doc = document;// use var so document.write doesn't get flagged by JSLint
		doc.write('<' + 'script src="' + url + '"' +' type="text/javascript"><' + '/script>');
	} else {
		// get document head
		head = document.getElementsByTagName("head")[0];

		// append a new script element to document head
		elem = document.createElement("script");
		elem.setAttribute("type","text/javascript");
		elem.setAttribute("src", url);
		head.appendChild(elem);
	}
};

/* Query String ----------------------------------------------------*/

/*string*/ JsonFx.Utils.getQueryParam = function (/*string*/ query, /*string*/ key) {
	if (!query || !key) {
		return null;
	}
	// trim off before the query
	query = query.substring(query.lastIndexOf('?')+1);

	/*string[]*/ var params = query.split('&'), parts;
	for (var i=0; i<params.length; i++) {
		parts = params[i].split('=');
		if (parts.length >= 2 && parts[0] === key) {
			return parts[1];
		}
	}
	
	return null;
};

/* Cookie Storage ----------------------------------------------------*/

/*void*/ JsonFx.Utils.setCookie = function(/*string*/ key, /*string*/ value, /*date*/ expires) {
	if (!key || !value) {
		return;
	}

	var cookie = key + "=" + escape(value);
	if (expires) {
		if (expires instanceof Date) {
			expires = expires.toGMTString();
		}
		if (typeof expires === "string") {
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

/*string*/ JsonFx.Utils.getCookie = function(/*string*/ key) {
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

/*void*/ JsonFx.Utils.clearCookie = function(/*string*/ key) {
	JsonFx.Utils.setCookie(key, key, new Date(2000,1,1));
};

/* Distance calculation ----------------------------------------------------*/

// Based on: http://www.movable-type.co.uk/scripts/LatLong.html
// Calculate distance (in miles) between two points specified by latitude/longitude using law of cosines
/*number*/ JsonFx.Utils.getDistanceMiles = function(lat1, lon1, lat2, lon2) {
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