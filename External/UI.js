/*extern JsonFx */
if (typeof(JsonFx) == "undefined") {
	JsonFx = {};
}
if (typeof(JsonFx.UI) == "undefined") {
	JsonFx.UI = {};
}
/*void*/ JsonFx.UI.Clear = function(/*element*/ el) {
	if (el) {
		while (el.lastChild) {
			el.removeChild(el.lastChild);
		}
	}
};

/*void*/ JsonFx.UI.DisplayJsonML = function(/*JsonML*/ jml, /*element or string*/ container) {
	// either DOM element or id
	container = (typeof(container) != "string") ?
		container : document.getElementById(container);

	if (container) {
		if (jml && typeof(jml.parseJsonML) == "function") {
			jml = jml.parseJsonML();
			if (jml) {
				JsonFx.UI.Clear(container);
				container.appendChild(jml);
			}
		}
	}
};

/*void*/ JsonFx.UI.LoadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {
	JsonFx.IO.GetJsonRequest(url, null, null,
			function(jml,obj) {
				JsonFx.UI.DisplayJsonML(jml,container);
				if (callback) { callback(context); }
			}
		, null);
};

/*-----------------*\
	Minimize Verb
\*-----------------*/

	JsonFx.UI.Dir = {
		"None": 0,
		"Horz": 1,
		"Vert": 2,
		"Both": 3,
		"isHorz": function(dir) { return (JsonFx.UI.Dir.Horz&dir)!==0; },
		"isVert": function(dir) { return (JsonFx.UI.Dir.Vert&dir)!==0; }
	};

/*
	W3C DOM equivalent to currentStyle:
	http://developer.mozilla.org/en/docs/DOM:document.defaultView
	
		document.defaultView.getComputedStyle(node, "").getPropertyValue("stylename")
	or is it?
		document.defaultView().getComputedStyle(node, "").getPropertyValue("stylename")
*/


// elem: DOM element or ID
// dir: true = horizontal, false = vertical
// state: true = collapse, false = expand
/*void*/ JsonFx.UI.Minimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir, /*bool*/ state) {
	if (typeof(elem) == "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem) {
		return;
	}

	var StateKey = "JsonFx:Minimize";
	var multiple = typeof(elem[StateKey]) == "boolean";
	elem[StateKey] = state;
	if (multiple) {
		return;
	}

//	alert("elem.currentStyle.height: "+parseFloat(elem.currentStyle?elem.currentStyle.height:null)+", elem.style.height: "+parseFloat(elem.style?elem.style.height:null));
	var userOverflow = elem.style.overflow;
	var userHeight = elem.style.height;
	var userWidth = elem.style.width;
	var pxHeight = typeof(elem.offsetHeight) != "undefined" ?
			elem.offsetHeight : elem.height;
	var pxWidth = typeof(elem.offsetWidth) != "undefined" ?
			elem.offsetWidth : elem.width;

	elem.style.display = "block";

	if (!pxHeight) {
		if (!isNaN(userHeight)) {
			// user specified a fixed height
			pxHeight = userHeight;
		} else {
			// calc fixed height from auto
			pxHeight = typeof(elem.offsetHeight) != "undefined" ?
				elem.offsetHeight : elem.height;
		}
	}
	if (!pxWidth) {
		if (!isNaN(userWidth)) {
			// user specified a fixed width
			pxWidth = userWidth;
		} else {
			// calc fixed width from auto
			pxWidth = typeof(elem.offsetWidth) != "undefined" ?
				elem.offsetWidth : elem.width;
		}
	}

	if (!pxWidth) {
		pxWidth = elem.scrollWidth;
	}

	if (!pxHeight) {
		pxHeight = elem.scrollHeight;
	}

	if (pxHeight && JsonFx.UI.Dir.Horz==dir) {
		// set a fixed height as width changes
		elem.style.height = pxHeight+"px";
	}
	
	var StepMin = 0.0;
	var StepMax = 1.0;
	var StepInc = 0.1;

	// minimizeStep
	/*void*/ function m(/*number*/ step) {
		if (!elem) {
			return;
		}

		// this allows us to share state between closures
		state = elem[StateKey];

		if (step < StepMin || step > StepMax) {
			if (state) {
				elem.style.display = "none";
			}
			elem.style.overflow = userOverflow;
			elem.style.width = userWidth;
			elem.style.height = userHeight;
			elem[StateKey] = null;
			elem = null;

//			alert("userHeight: "+userHeight+", pxHeight: "+pxHeight);
//			alert(userWidth+" x "+userHeight);
			return;
		}

		if (JsonFx.UI.Dir.isHorz(dir)) {
			// horizontal
			elem.style.width = Math.floor(pxWidth*step)+"px";
		}
		if (JsonFx.UI.Dir.isVert(dir)) {
			// vertical
			elem.style.height = Math.floor(pxHeight*step)+"px";
		}

		elem.style.overflow = "hidden";
		setTimeout(function() {
				m(state?(step-StepInc):(step+StepInc));
			}, 0);
	}

	m(state?StepMax:StepMin);
};
