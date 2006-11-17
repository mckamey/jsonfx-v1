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
	if (typeof(el)=="string") {
		el = document.getElementById(el);
	}
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
	Maximize Verb
\*-----------------*/

/*function*/ JsonFx.UI.Maximize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) == "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem) {
		return null;
	}

	var es = elem.style,
	/*bool*/ state = false;

	var userShape;

	/*void*/ function saveShape() {
		userShape = {};
		for (var s in es) {
			if (/*es.hasOwnProperty(s) &&*/ es[s]) {
				userShape[s] = es[s];
			}
		}
	}
	/*void*/ function restoreShape() {
		if (userShape) {
			for (var s in userShape) {
				if (userShape.hasOwnProperty(s)) {
					es[s] = userShape[s];
				}
			}
		}
	}

	// state: true = maixmize, false = restore
	return /*void*/ function max(/*bool*/ newState) {

		if (!state) {
			saveShape();
		}
		// store the latest requested state
		if (typeof(newState) == "boolean") {
			state = newState;
		} else {
			state = !state;
		}

		if (state) {
			es.floatStyle = "none";
			es.position = "absolute";
			es.width = JsonFx.UI.GetStyle(document.documentElement, "width");
			es.height = JsonFx.UI.GetStyle(document.documentElement, "height");
			es.top = "0px";
			es.left = "0px";
			es.bottom = "0px";
			es.right = "0px";
		} else {
			restoreShape();
		}
	};
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

/*function*/ JsonFx.UI.Minimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) == "string") {
		// either DOM element or id
		elem = document.getElementById(elem);
	}

	if (!elem) {
		return null;
	}

	var es = elem.style,
	/*bool*/ mutex = false,
	/*bool*/ state = false,
	/*const float*/ StepMin = 0.0,
	/*const float*/ StepMax = 1.0,
	/*const float*/ StepInc = 0.05;

	var userOverflow, userHeight, userWidth, pxHeight, pxWidth;

	/*void*/ function saveShape() {
		userOverflow = es.overflow;
		userHeight = es.height;
		userWidth = es.width;
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
		es.display = "block";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (step < StepMin || step > StepMax) {
				if (state) {
					es.display = "none";
				}
				es.overflow = userOverflow;
				es.width = userWidth;
				es.height = userHeight;
				mutex = false;
				return;
			}

			es.overflow = "hidden";

			if (JsonFx.UI.Dir.isHorz(dir)) {
				// horizontal, simplified lerp
				es.width = Math.floor(pxWidth*step)+"px";
			}
			if (JsonFx.UI.Dir.isVert(dir)) {
				// vertical, simplified lerp
				es.height = Math.floor(pxHeight*step)+"px";
			}

			setTimeout(function() {
					m(state ? (step-StepInc) : (step+StepInc));
				}, 0);
		}

		m(state ? StepMax : StepMin);
	};
};