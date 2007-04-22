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
JsonFx.IO.ServiceTest.Context = function(/*string*/ request, /*element*/ target) {
	var cx = this;
	var start = new Date().valueOf();
	cx.target = target;
	cx["Proxy Method"] = request;
	cx["Time Stamp"] = new Date(start).toString();
	cx.setResponseTime = function() {
		cx["Response Time"] = (new Date().valueOf()-start)+" ms";
		delete cx.setResponseTime;
	};
};

/*int*/ JsonFx.IO.ServiceTest.dumpDataID = 0;
/*JsonML*/ JsonFx.IO.ServiceTest.dumpData = function(/*string*/ name, /*json*/ val) {
	var c = ["span"];
	var type = typeof val;

	var a = false;
	if ("object" === type && val) {
		a = ["label", {"class":"jsonfx-expando"}];
		c.push(a);
	}
	(a?a:c).push(["span", {"class":"jsonfx-type"}, (val instanceof Array) ? "array" : type]);
	(a?a:c).push(["span", {"class":"jsonfx-name"}, name, ":"]);

	if ("object" === type && val) {
		var ul = ["ul", {"id":"JsonFx_UI_Dump_"+(JsonFx.IO.ServiceTest.dumpDataID++),"class":"jsonfx-object"}];
		for (var pn in val) {
			if (!val.hasOwnProperty(pn)) {
				continue;
			}
			ul.push(["li", JsonFx.IO.ServiceTest.dumpData(pn, val[pn])]);
		}

		if (a) {
			a[1]["for"] = ul[1].id;
		}
		c.push(ul);
	} else {
		c.push(["span", {"class":"jsonfx-value"}, String(val)]);
	}

	return c;
};

/* display result callback */
/*void*/ JsonFx.IO.ServiceTest.cb_displayResult = function(/*object*/ result, /*JsonFx.IO.ServiceTest.Context*/ context, /*Error*/ error) {
	var target = null;
	if (context instanceof JsonFx.IO.ServiceTest.Context) {
		target = context.target;
 		delete context.target;
		context.setResponseTime();
	}

	if ("string" === typeof target) {
		target = document.getElementById(target);
	}
	if (!target) {
		target = document.createElement("div");
		document.body.appendChild(target);
	} else {
		JsonFx.UI.clear(target);
	}

	var label = error ? "JSON-RPC Error" : "JSON-RPC Result";
	var data = error ? error : result;
	JsonFx.UI.displayJsonML(
		["div",
			JsonFx.IO.ServiceTest.dumpData("Call Context", context),
			["hr"],
			JsonFx.IO.ServiceTest.dumpData(label, data)],
			target);
};

/* creates actual method call */		
/*function*/ JsonFx.IO.ServiceTest.createServiceCall = function(/*object*/ service, /*object*/ proc, /*element*/ target) {
	return function() {
		var call = service.proxyName+"."+proc.name+"(";
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

		var cx = new JsonFx.IO.ServiceTest.Context(call, target);

		service.callService(
			proc.name,
			args,
			{
				onSuccess : JsonFx.IO.ServiceTest.cb_displayResult,
				onFailure : JsonFx.IO.ServiceTest.cb_displayResult,
				context : cx
			});
	};
};

/* generates test buttons from the system.describe result */
/*void*/ JsonFx.IO.ServiceTest.cb_buildTestUI = function(/*object*/ result, /*object*/ context) {
	if ("object" !== typeof result && result !== null) {
		return;
	}
	if ("string" === typeof context.container) {
		context.container = document.getElementById(context.container);
	}
	if (!context.container) {
		context.container = document.createElement("div");
		document.body.appendChild(context.target);
	} else {
		JsonFx.UI.clear(context.container);
	}

	// add placeholder for implicit service description
	result.procs.unshift({"name":"system.describe","summary":"Produces a Service Description for this JSON-RPC Service"});

	for (var i=0; i<result.procs.length; i++) {
		var proc = result.procs[i];
		var btn = ["input", {"type":"button", "class":"ServiceTest", "value":proc.name, "title":proc.summary}];
		btn = btn.parseJsonML(null);

		var handler = JsonFx.IO.ServiceTest.createServiceCall(context.service, proc, context.target);
		if (handler) {
			btn.onclick = handler;
			context.container.appendChild(btn);
		}
	}
};

/*void*/ JsonFx.IO.ServiceTest.buildTestUI = function(/*JsonFx.IO.Service*/ service, /*element*/ container, /*element*/ target) {
	if (service) {
		service.$describe(
			{
				onSuccess : JsonFx.IO.ServiceTest.cb_buildTestUI,
				onFailure : JsonFx.IO.ServiceTest.cb_displayResult,
				context : { "service":service, "container":container, "target":target }
			});
	}
};
