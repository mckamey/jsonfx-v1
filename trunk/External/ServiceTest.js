/*extern JsonFx */
/* namespace JsonFx.IO */
if ("undefined" === typeof window.JsonFx) {
	JsonFx = {};
}
if ("undefined" === typeof JsonFx.IO) {
	JsonFx.IO = {};
}

/* namespace JsonFx.IO.ServiceTest */
JsonFx.IO.ServiceTest = {};

/*-------------------------------------------------------------------*/

/* context object */
JsonFx.IO.ServiceTest.Context = function(/*string*/ request) {
	var cx = this;
	var start = new Date().valueOf();
	cx["JSON-RPC"] = request;
	cx["Time Stamp"] = new Date(start).toString();
	cx.setResponseTime = function() {
		cx["Response Time"] = (new Date().valueOf()-start)+" ms";
		delete cx.setResponseTime;
	};
};

/* display result callback */
/*void*/ JsonFx.IO.ServiceTest.cb_displayResult = function(response, context) {
	if (!response) {
		alert("JSON-RPC failed.");
		return;
	}

	var divResponse = document.getElementById("divResponse");
	if (!divResponse) {
		// create a display area
		divResponse = document.createElement("div");
		divResponse.id = "divResponse";
		document.body.appendChild(divResponse);
	} else {
		// clear any old data
		JsonFx.UI.clear(divResponse);
	}

	if (context) {
		context.setResponseTime();
	}
	JsonFx.UI.displayJsonML(["div",JsonFx.UI.dumpData(context),["hr"],JsonFx.UI.dumpData(response)], divResponse);
};

/* creates actual method call */		
/*function*/ JsonFx.IO.ServiceTest.createServiceCall = function(/*object*/ service, /*object*/ proc) {
	if (!proc.params || proc.params.length <= 1) {
		return function() {
			var call = service.name+"."+proc.name+"(";
			if (!proc.params || proc.params.length === 0) {
				service[proc.name](JsonFx.IO.ServiceTest.cb_displayResult, new JsonFx.IO.ServiceTest.Context(call+")"));
			} else {
				var args = [];
				for (var i=0; i<proc.params.length; i++) {
					if (i > 0) {
						call += ", ";
					}
					args[i] = window.prompt("("+proc.params[i].type+") "+proc.params[i].name+" =","");
					if (args[i] !== null) {
						if (proc.params[i].type !== "str") {
							args[i] = args[i].parseJSON();
						}
						call += args[i].toJSONString();
					} else {
						call += "null";
					}
				}
				call += ")";
				service[proc.name](args[0], JsonFx.IO.ServiceTest.cb_displayResult, new JsonFx.IO.ServiceTest.Context(call));
			}
		};
	} else {
		return null;
	}
};

/* generates test buttons from the system.decribe result */
/*void*/ JsonFx.IO.ServiceTest.cb_buildTestUI = function(response, context) {
	if (!response || !response.result) {
		return;
	}
	if ("string" == typeof context.container) {
		context.container = document.getElementById(context.container);
	}
	if (!context.container) {
		context.container = document.createElement("div");
	} else {
		JsonFx.UI.clear(context.container);
	}

	// add placeholder for implicit service description
	response.result.procs.unshift({"name":"system.describe"});

	for (var i=0; i<response.result.procs.length; i++) {
		var proc = response.result.procs[i];
		var btn = ["input", {"type":"button", "class":"ServiceTest", "value":proc.name}];
		btn = btn.parseJsonML(null);

		var handler = JsonFx.IO.ServiceTest.createServiceCall(context.service, proc);
		if (handler) {
			btn.onclick = handler;
			context.container.appendChild(btn);
		}
	}
};

/*void*/ JsonFx.IO.ServiceTest.buildTestUI = function(service, container) {
	service.$describe(JsonFx.IO.ServiceTest.cb_buildTestUI, {"service":service,"container":container});
};
