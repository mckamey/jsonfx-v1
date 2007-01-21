/*extern JsonFx */
//=======================================================
//	Singleton JsonFx.UI
//=======================================================

// define JsonFx namespace
if ("undefined" === typeof JsonFx) {
	window.JsonFx = {};
}

JsonFx.UI = {};

/*void*/ JsonFx.UI.Clear = function(/*element*/ el) {
	if (el) {
		while (el.lastChild) {
			el.removeChild(el.lastChild);
		}
	}
};

/* if container is null then uses ID(s) to replace page elements */
/*void*/ JsonFx.UI.DisplayJsonML = function(/*JsonML*/ jml, /*element or string*/ container) {
	// either DOM element or id
	container = (typeof(container) !== "string") ?
		container : document.getElementById(container);

	if (jml && typeof(jml.parseJsonML) === "function") {
		jml = jml.parseJsonML();
		if (jml) {
			if (container) {
				JsonFx.UI.Clear(container);
				container.appendChild(jml);
			} else if (jml.id) {
				container = document.getElementById(jml.id);
				if (container && container.parentNode) {
					container.parentNode.replaceChild(jml, container);
				}
			} else if (jml.childNodes) {
				while (jml.firstChild) {
					if (!jml.firstChild.id) {
						jml.removeChild(jml.firstChild);
						continue;
					}
					container = document.getElementById(jml.firstChild.id);
					if (container && container.parentNode) {
						container.parentNode.replaceChild(jml.firstChild, container);
					} else {
						jml.removeChild(jml.firstChild);
					}
				}
			}
		}
	}
};

/* returns true if request was sent */
/*bool*/ JsonFx.UI.LoadJsonML = function(/*string*/ url, /*element or string*/ container, /*function*/ callback, /*object*/ context) {
	return JsonFx.IO.GetJsonRequest(url, null, null,
			function(jml,obj) {
				JsonFx.UI.DisplayJsonML(jml,container);
				if (callback) { callback(context); }
			}
		, null);
};

/*string*/ JsonFx.UI.GetStyle = function(/*elem*/ el, /*string*/ style) {
	if (typeof(el) === "string") {
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

	if (typeof(elem) === "string") {
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
		if (typeof(newState) === "boolean") {
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
	None: 0x0,
	Horz: 0x1,
	Vert: 0x2,
	Both: 0x1|0x2,

	// helper method which tests for Horz
	/*bool*/ isHorz: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Horz & dir);
	},

	// helper method which tests for Vert
	/*bool*/ isVert: function (/*JsonFx.UI.Dir*/ dir) {
		return !!(JsonFx.UI.Dir.Vert & dir);
	}
};

/*function*/ JsonFx.UI.Minimize = function(/*element or string*/ elem, /*JsonFx.UI.Dir*/ dir) {

	if (typeof(elem) === "string") {
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
		pxHeight = !isNaN(elem.offsetHeight) ?
				elem.offsetHeight : parseFloat(JsonFx.UI.GetStyle(elem, "height"));
		pxWidth = !isNaN(elem.offsetWidth) ?
				elem.offsetWidth : parseFloat(JsonFx.UI.GetStyle(elem, "width"));
	}

	// state: true = collapse, false = expand
	return /*void*/ function min(/*bool*/ newState) {
	
		// store the latest requested state
		if (typeof(newState) === "boolean") {
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
		es.visibility = "visible";

		// minimizeStep
		/*void*/ function m(/*float*/ step) {

			if (step < StepMin || step > StepMax) {
				if (state) {
					if (JsonFx.UI.Dir.isVert(dir)) {
						es.display = "none";
					}
					es.visibility = "hidden";
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