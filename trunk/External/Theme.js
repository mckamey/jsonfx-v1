var CookieKey_Theme = "CssTheme";
var StylePath = "Styles";
var StyleFile = "Style.css";

function toggleCss(elem, className, state) {
	if (typeof(elem) == 'string')
		elem = document.getElementById(elem);
	if (elem == null)
		return;
	className = ' '+className;
	if (state) {
		elem.className += className;
	} else {
		while (elem.className.lastIndexOf(className) >= 0 ) {
			elem.className = elem.className.replace(className,'');
		}
	}
}

function setTheme(themeName) {
	if (themeName == null)
		return;

	storeCookie(CookieKey_Theme, themeName);
	// returns if was successful
	if (!replaceStyleSheet('@import "'+StylePath+"/"+themeName+"/"+StyleFile+'";'))
		document.location.reload();
}

function replaceStyleSheet(content) {
	try {
		var styleCustom = document.styleSheets[document.styleSheets.length-1];
		if (styleCustom == null) {
			window.status = "replaceStyleSheet Error: No StyleSheet was found.";
			return false;
		}

		if (styleCustom.ownerNode != null) {
			// W3C
			styleCustom.ownerNode.parentNode.removeChild(styleCustom.ownerNode);
		} else if (styleCustom.owningElement != null) {
			// IE6
			styleCustom.owningElement.removeNode();
		} else if (styleCustom.removeImport) {
			// last resort, just remove content
			while (styleCustom.imports.length > 0)
				styleCustom.removeImport(0);
			while (styleCustom.rules.length > 0)
				styleCustom.removeRule(0);
		}

		if (document.createStyleSheet) {
			// IE6
			// can only create 31 styleSheets in IE6
			if (document.styleSheets.length < 31) {
				styleCustom = document.createStyleSheet();
				styleCustom.cssText = content;

				// needed to force refresh for some content
				document.body.innerHTML = document.body.innerHTML;
			}
		} else {
			// W3C
			var headTag = document.getElementsByTagName("head")[0];
			if (headTag != null){
				styleCustom = document.createElement("style");
				styleCustom.setAttribute("type", "text/css");
				var cssText = document.createTextNode(content);
				styleCustom.appendChild(cssText);
				headTag.appendChild(styleCustom);
			}
		}
		return true;
	} catch (e) {
		window.status = "replaceStyleSheet Error: "+e.message;
		return false;
	}
}

/* Clear Selection ---------------------------------------------------*/

function clearSelection() {
	try{
		if (window.getSelection)
			window.getSelection().removeAllRanges();
		else if (document.selection)
			document.selection.empty();
		//else if (document.getSelection)
		//	document.getSelection().empty();
	} catch (e) {}
}

/* Cookie Storage ----------------------------------------------------*/

function storeCookie(key, value, persist) {
	if (key == null || value == null)
		return;

	var cookie = key + "=" + value;
	if (persist) {
		var date = new Date();

		date.setTime(date.getTime() + (365*24*60*60*1000));
		date = date.toGMTString();
		cookie += "; expires=" + date +"; path=/";
	}
	cookie += "; path=/";

    document.cookie = cookie;
}

function retrieveCookie(key) {
    var value = null;
    if (document.cookie != null && document.cookie.length > 0) {
		var cookies = document.cookie.split(';');
		for (var i=0; i<cookies.length; i++) {
			var cookie = cookies[i];
			if (cookie != null && cookie.length > 0) {
				var cookieParts = cookie.split('=');
				if (cookieParts[0].indexOf(key) >= 0) {
					value = cookieParts[1];
					break;
				}
			}
		}
    }
    return value;
}

//window.onload = function(evt) {
//    var themeName = retrieveCookie(CookieKey_Theme);
//    if (themeName != null) {
//        setTheme(themeName);
//    }
//}

document.ondblclick = function(evt) {
	if (evt == null)
		evt = window.event;
	if (evt == null || !evt.ctrlKey)
		return;

	var state = (document.body.className.lastIndexOf("HideKeywords") < 0)
	toggleCss(document.body, "HideKeywords", state);
	clearSelection();
}
