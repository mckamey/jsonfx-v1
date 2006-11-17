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

/*string*/ JsonFx.UI.GetStyle = function(/*elem*/ el, /*string*/ style) {
	el = typeof(el)=="string" ? document.getElementById(el) : el;
	if (window.getComputedStyle) {
		// W3C DOM equivalent to currentStyle:
		return document.defaultView.getComputedStyle(el, null).getPropertyValue(style);
	}
	if (el.currentStyle) {
		// IE only
		return el.currentStyle[style];
	}
	return null;
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

// elem: DOM element or ID
// dir: true = horizontal, false = vertical
/*function*/ JsonFx.UI.Minimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) == "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem) {
		return null;
	}

	var /*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ StepMin = 0.0,
	/*const float*/ StepMax = 1.0,
	/*const float*/ StepInc = 0.05;

	var userOverflow, userHeight, userWidth, pxHeight, pxWidth;

	/*void*/ function saveShape() {
		userOverflow = elem.style.overflow;
		userHeight = elem.style.height;
		userWidth = elem.style.width;
		pxHeight = typeof(elem.offsetHeight) != "undefined" ?
				elem.offsetHeight : parseFloat(JsonFx.UI.GetStyle(elem, "height"));
		pxWidth = typeof(elem.offsetWidth) != "undefined" ?
				elem.offsetWidth : parseFloat(JsonFx.UI.GetStyle(elem, "width"));
	}

	// state: true = collapse, false = expand
	return /*void*/ function min(/*bool*/ newState) {
	
		// store the latest requested state
		if (typeof(newState) == "boolean") {
			state = newState;
		} else {
			state = !state;
		}
		if (mutex) {
			// concurrency check
			return;
		}
		mutex = true;

		if (state) {
			saveShape();
		}
		elem.style.display = "block";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (step < StepMin || step > StepMax) {
				if (state) {
					elem.style.display = "none";
				}
				elem.style.overflow = userOverflow;
				elem.style.width = userWidth;
				elem.style.height = userHeight;
				mutex = false;
				return;
			}

			elem.style.overflow = "hidden";

			if (JsonFx.UI.Dir.isHorz(dir)) {
				// horizontal, simplified lerp
				elem.style.width = Math.floor(pxWidth*step)+"px";
			}
			if (JsonFx.UI.Dir.isVert(dir)) {
				// vertical, simplified lerp
				elem.style.height = Math.floor(pxHeight*step)+"px";
			}

			setTimeout(function() {
					m(state ? (step-StepInc) : (step+StepInc));
				}, 0);
		}

		m(state ? StepMax : StepMin);
	};
};