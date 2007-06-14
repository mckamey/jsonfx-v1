/*extern JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.UI.setCssUserAgent

	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-06-10-1635
	Modified: 2007-04-16-2125
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
		var R_MSIE = /(\bmsie|microsoft internet explorer)[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_Gecko = /rv[:]([0-9]+[\.]?[0-9]*).*?gecko[\/][0-9]+(\s+(\S+)[\/]([0-9]+[\.]?[0-9]*))?/;
		var R_AppleWebKit = /\bapplewebkit[\/]([0-9]+[\.]?[0-9]*).*\s+(\S+)[\/][v]?([0-9]+[\.]?[0-9]*)/;
		var R_Opera = /\bopera[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_MSPIE = /\b(mspie|microsoft pocket internet explorer)[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_iCab = /\bicab[\s\/]*([0-9]+[\.]?[0-9]*)/;
		var R_MozCompat = /[(].*?(\S+)[\/]([0-9]+[\.]?[0-9]*).*?[)]/;
		var R_Other = /^([^\/]+)[\/]([0-9]+[\.]?[0-9]*)/;
		var R_AOL = /\b(america online browser|aol)[\s\/]*([0-9]+[\.]?[0-9]*)/;

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
		} else if (R_iCab.exec(ua)) {
			name = "icab";
			ver = RegExp.$1;

			// iCab doesn't allow setting User-Agent on XHR
			JsonFx.userAgent = null;
		} else if (R_MSIE.exec(ua)) {
			name = "ie";
			ver = RegExp.$2;
		} else if (R_MSPIE.exec(ua)) {
			name = "mspie";
			ver = RegExp.$2;
		} else if (R_AppleWebKit.exec(ua)) {
			name = "webkit";
			ver = RegExp.$1;

			// also add WebKit-brand version
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
	var i;
	if (uaDetails.length && JsonFx.userAgent) {
		JsonFx.userAgent += " (";
		for (i=0; i<uaDetails.length;i ++) {
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
			return PREFIX+b+PREFIX+b+'-'+vi+PREFIX+b+'-'+vf;
		}

		// using JavaScript closures to access the parsed UA
		/*void*/ function appendCss(/*{b,v}[]*/ d) {
			var uaCss = "", ua;

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
