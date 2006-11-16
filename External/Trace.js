/*extern JsonFx */
// =========================================================
//	class JsonFx.Trace (Singleton)
//=======================================================
if ("undefined" == typeof JsonFx) {
	JsonFx = {};
}

if ("undefined" == typeof JsonFx.Trace) {
	// Create singleton object
	JsonFx.Trace = {};

	/*element*/ JsonFx.Trace.GetTraceElement = function() {
		var trace = document.getElementById("JsonFx_Trace");
		if (!trace) {
			trace = document.createElement("div");
			trace.id = "JsonFx_Trace";
			trace.style.clear = "both";
			trace.style.padding = "0px 1em";
			trace.style.backgroundColor = "#FFFFEE";
			trace.style.border = "1px solid #993333";
			trace.style.height = "200px";
			trace.style.width = "90%";
			trace.style.overflow = "auto";
			document.body.appendChild(trace);

			var aClear = document.createElement("a");
			aClear.onclick = function(){JsonFx.Trace.Clear();return false;};
			aClear.href = "#clear";
			aClear.innerText = "Clear";
			aClear.title = "Clears the trace area.";
			trace.appendChild(aClear);

			trace.appendChild(document.createTextNode(" "));

			var aSource = document.createElement("a");
			aSource.onclick = function(){trace.innerHTML="<textarea cols='70' rows='8' onclick='this.select();'>"+document.documentElement.outerHTML+"</textarea>";return false;};
			aSource.href = "#source";
			aSource.innerText = "Source";
			aSource.title = "Shows the dynamically built source for this document.";
			trace.appendChild(aSource);
		}
		return trace;
	};
	
	/*bool*/ JsonFx.Trace.enabled = true;

	/*void*/ JsonFx.Trace.Write = function(/*string*/ sMessage) {
		if (!JsonFx.Trace.enabled) {
			return;
		}

		try {
			var trace = JsonFx.Trace.GetTraceElement();
			var preTraceMsg = document.createElement("pre");
			preTraceMsg.innerText = sMessage;
			trace.insertBefore(preTraceMsg, trace.firstChild);
		} catch (ex) {
			alert("JsonFx.Trace: "+ex.message);
		}
	};

	/*void*/ JsonFx.Trace.Clear = function() {
		try {
			var trace = JsonFx.Trace.GetTraceElement();
			document.body.removeChild(trace);
		} catch (ex) {
			alert("JsonFx.Trace: "+ex.message);
		}
	};
}