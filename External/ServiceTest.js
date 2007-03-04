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
JsonFx.IO.ServiceTest.Context = function(/*string*/ request, /*element*/ output) {
	var cx = this;
	var start = new Date().valueOf();
	cx.output = output;
	cx["JSON-RPC"] = request;
	cx["Time Stamp"] = new Date(start).toString();
	cx.setResponseTime = function() {
		cx["Response Time"] = (new Date().valueOf()-start)+" ms";
		delete cx.setResponseTime;
	};
};

/* display result callback */
/*void*/ JsonFx.IO.ServiceTest.cb_displayResult = function(/*object*/ response, /*JsonFx.IO.ServiceTest.Context*/ context) {
	if (!response) {
		alert("JSON-RPC failed.");
		return;
	}

	var output = null;
	if (context) {
		output = context.output;
 		delete context.output;
		context.setResponseTime();
	}

	if ("string" === typeof output) {
		output = document.getElementById(output);
	}
	if (!output) {
		output = document.createElement("div");
		document.body.appendChild(output);
	} else {
		JsonFx.UI.clear(output);
	}

	JsonFx.UI.displayJsonML(["div",JsonFx.UI.dumpData(context),["hr"],JsonFx.UI.dumpData(response)], output);
};

/* creates actual method call */		
/*function*/ JsonFx.IO.ServiceTest.createServiceCall = function(/*object*/ service, /*object*/ proc, /*element*/ output) {
	return function() {
		var call = service.name+"."+proc.name+"(";
		var args = [];
		if (proc.params) {
			for (var i=0; i<proc.params.length; i++) {
				if (i > 0) {
					call += ", ";
				}
				args[i] = window.prompt("("+proc.params[i].type+") "+proc.params[i].name+" =","");
				if (args[i] !== null) {
					if (proc.params[i].type !== "str") {
						try {
							args[i] = args[i].parseJSON();
						} catch (ex) {}
					}
					call += args[i].toJSONString();
				} else {
					call += "null";
				}
			}
		}
		call += ")";

		var context = new JsonFx.IO.ServiceTest.Context(call, output);

		service.callService(proc.name, args, JsonFx.IO.ServiceTest.cb_displayResult, context);
	};
};

/* generates test buttons from the system.decribe result */
/*void*/ JsonFx.IO.ServiceTest.cb_buildTestUI = function(/*object*/ response, /*object*/ context) {
	if (!response || !response.result) {
		return;
	}
	if ("string" === typeof context.container) {
		context.container = document.getElementById(context.container);
	}
	if (!context.container) {
		context.container = document.createElement("div");
		document.body.appendChild(context.output);
	} else {
		JsonFx.UI.clear(context.container);
	}

	// add placeholder for implicit service description
	response.result.procs.unshift({"name":"system.describe"});

	for (var i=0; i<response.result.procs.length; i++) {
		var proc = response.result.procs[i];
		var btn = ["input", {"type":"button", "class":"ServiceTest", "value":proc.name}];
		btn = btn.parseJsonML(null);

		var handler = JsonFx.IO.ServiceTest.createServiceCall(context.service, proc, context.output);
		if (handler) {
			btn.onclick = handler;
			context.container.appendChild(btn);
		}
	}
};

/*void*/ JsonFx.IO.ServiceTest.buildTestUI = function(/*JsonFx.IO.Service*/ service, /*element*/ container, /*element*/ output) {
	service.$describe(
			JsonFx.IO.ServiceTest.cb_buildTestUI,
			{ "service":service, "container":container, "output":output }
		);
};
