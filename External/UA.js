/*---------------------------------------------------------------------------*\
	UA.js
	Dynamically appends CSS classes to document.body based upon user-agent.
\*---------------------------------------------------------------------------*/

JsonFx.UI.setCssUserAgent = function() {

	// anonymous function doesn't affect global namespace and can't be called externally
	// variables and helper functions available via JavaScript closures
	var PREFIX = " ua-";

	/*string*/ function formatCss(/*string*/ browName, /*string*/ verStr) {
		if (!browName) {
			return "";
		}

		while (browName.indexOf(' ') >=0 ) {
			browName = browName.replace(' ', '-');
		}
		var verInt = parseInt(verStr, 10);
		var verFloat = parseFloat(verStr);
		verFloat = (verFloat === verInt && verFloat.toFixed) ?
			verFloat.toFixed(1) : verFloat.toString();
		verFloat = verFloat.replace('.', '-');
		if (isNaN(verInt)) {
			return PREFIX+browName;
		}
		return PREFIX+browName+PREFIX+browName+verInt+PREFIX+browName+verFloat;
	}

	/*string*/ function buildCss() {
		// Regex tested against (2006-06-11 @ 1600): http://en.wikipedia.org/wiki/User_agent
		var Regex_MSHTML = new RegExp("(msie|microsoft internet explorer)[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var Regex_Gecko = new RegExp("rv[:]([0-9]+[\\.]?[0-9]*).*?gecko[/][0-9]+(\\s+(\\S+)[/]([0-9]+[\\.]?[0-9]*))?");
		var Regex_AppleWebKit = new RegExp("applewebkit[/]([0-9]+[\\.]?[0-9]*).*?(\\S+)[/][v]?([0-9]+[\\.]?[0-9]*)");
		var Regex_Opera = new RegExp("opera[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var Regex_MSPIE = new RegExp("(mspie|microsoft pocket internet explorer)[\\s/]*([0-9]+[\\.]?[0-9]*)");
		var Regex_MozCompat = new RegExp("[(].*?(\\S+)[/]([0-9]+[\\.]?[0-9]*).*?[)]");
		var Regex_Other = new RegExp("^([^/]+)[/]([0-9]+[\\.]?[0-9]*)");
		var Regex_AOL = new RegExp("(america online browser|aol)[\\s/]*([0-9]+[\\.]?[0-9]*)");

		var ua = navigator.userAgent.toLowerCase();
		var bodyCss = PREFIX+navigator.platform.toLowerCase();
		var browserName = null;
		var browserVer = null;

		// aol uses multiple browsers so don't stop
		if (Regex_AOL.exec(ua)) {
			bodyCss += formatCss("aol", RegExp.$2);
		}

		// order is important as user-agents spoof each other	
		if (Regex_Opera.exec(ua)) {
			browserName = "opera";
			browserVer = RegExp.$1;
		} else if (Regex_MSHTML.exec(ua)) {
			browserName = "msie";
			browserVer = RegExp.$2;
		} else if (Regex_MSPIE.exec(ua)) {
			browserName = "mspie";
			browserVer = RegExp.$2;
		} else if (Regex_AppleWebKit.exec(ua)) {
			browserName = "applewebkit";
			browserVer = RegExp.$1;

			// also add AppleWebKit-brand version
			bodyCss += formatCss(RegExp.$2, RegExp.$3);
		} else if (Regex_Gecko.exec(ua)) {
			browserName = "gecko";
			browserVer = RegExp.$1;

			// also add Gecko-brand version
			bodyCss += formatCss(RegExp.$3, RegExp.$4);
		} else if (Regex_MozCompat.exec(ua)) {
			browserName = RegExp.$1;
			browserVer = RegExp.$2;
		} else if (Regex_Other.exec(ua)) {
			browserName = RegExp.$1;
			browserVer = RegExp.$2;
		}

		// browserVer should hold parsed version string
		if (browserVer) {
			bodyCss += formatCss(browserName, browserVer);
		}
		
		return bodyCss;
	}

	// calculate styles immediately, loop until can apply them
	var uaCss = buildCss();

	// using JavaScript closures to access the calculated bodyCss
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
			return;
		}

		appendCss();
	}

	appendCssLoop();
})();