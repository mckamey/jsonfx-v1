/*extern JsonFx */
/*---------------------------------------------------------------------------*\
	JsonFx.UI.setCssUserAgent

	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-06-10-1635
	Modified: 2007-03-01-0313
\*---------------------------------------------------------------------------*/

/* namespace JsonFx */
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

/* namespace JsonFx.UI */
if ("undefined" === typeof JsonFx.UI) {
	JsonFx.UI = {};
}

/*	Dynamically appends CSS classes to document.body based upon user-agent.*/
/*void*/ JsonFx.UI.setCssUserAgent = function() {

	// anonymous function doesn't affect global namespace and can't be called externally
	// variables and helper functions available via JavaScript closures
	var PREFIX = " ua-";

	/*string*/ function formatCss(/*string*/ b, /*string*/ v) {
		if (!b) {
			return "";
		}

		b = b.replace(/\s*/g, '-');
		var vi = parseInt(v, 10);
		var vf = parseFloat(v);
		vf = (vf === vi && vf.toFixed) ?
			vf.toFixed(1) : vf.toString();
		vf = vf.replace(/\./g, '-');
		if (!isFinite(vi)) {
			return PREFIX+b;
		}
		return PREFIX+b+PREFIX+b+vi+PREFIX+b+vf;
	}

	/*string*/ function buildCss() {
		// Regex tested against (2006-06-11 @ 1600): http://en.wikipedia.org/wiki/User_agent
		var R_MSIE = new RegExp("(msie|microsoft internet explorer)[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var R_Gecko = new RegExp("rv[:]([0-9]+[\\.]?[0-9]*).*?gecko[/][0-9]+(\\s+(\\S+)[/]([0-9]+[\\.]?[0-9]*))?");
		var R_AppleWebKit = new RegExp("applewebkit[/]([0-9]+[\\.]?[0-9]*).*?(\\S+)[/][v]?([0-9]+[\\.]?[0-9]*)");
		var R_Opera = new RegExp("opera[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var R_MSPIE = new RegExp("(mspie|microsoft pocket internet explorer)[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var R_MozCompat = new RegExp("[(].*?(\\S+)[/]([0-9]+[\\.]?[0-9]*).*?[)]");
		var R_Other = new RegExp("^([^/]+)[/]([0-9]+[\\.]?[0-9]*)");
		var R_AOL = new RegExp("(america online browser|aol)[\\s/]*([0-9]+[\\.]?[0-9]*)");

		var ua = navigator.userAgent.toLowerCase();
		var css = PREFIX+navigator.platform.toLowerCase();
		var bName = null;// browser name
		var bVer = null;// browser version

		// aol uses multiple browsers so don't stop
		if (R_AOL.exec(ua)) {
			css += formatCss("aol", RegExp.$2);
		}

		// order is important as user-agents spoof each other	
		if (R_Opera.exec(ua)) {
			bName = "opera";
			bVer = RegExp.$1;
		} else if (R_MSIE.exec(ua)) {
			bName = "msie";
			bVer = RegExp.$2;
		} else if (R_MSPIE.exec(ua)) {
			bName = "mspie";
			bVer = RegExp.$2;
		} else if (R_AppleWebKit.exec(ua)) {
			bName = "applewebkit";
			bVer = RegExp.$1;

			// also add AppleWebKit-brand version
			css += formatCss(RegExp.$2, RegExp.$3);
		} else if (R_Gecko.exec(ua)) {
			bName = "gecko";
			bVer = RegExp.$1;

			// also add Gecko-brand version
			css += formatCss(RegExp.$3, RegExp.$4);
		} else if (R_MozCompat.exec(ua)) {
			bName = RegExp.$1;
			bVer = RegExp.$2;
		} else if (R_Other.exec(ua)) {
			bName = RegExp.$1;
			bVer = RegExp.$2;
		}

		// bVer should hold parsed version string
		if (bVer) {
			css += formatCss(bName, bVer);
		}
		
		return css;
	}

	// calculate styles immediately, loop until can apply them
	var uaCss = buildCss();

	// using JavaScript closures to access the calculated css
	/*void*/ function appendCss() {

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
	/*void*/ function appendCssLoop() {

		if (!document.body) {
			setTimeout(appendCssLoop, 100);
		} else {
			appendCss();
		}
	}

	appendCssLoop();
};