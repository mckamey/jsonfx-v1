/*extern JsonFx, JSON, JsonML */

/* namespace JsonFx.IO */
if ("undefined" === typeof window.JsonFx) {
	JsonFx = {};
}

// dependency checks
if ("undefined" === typeof JSON) {
	throw new Error("JsonFx.IO requires json2.js");
}

/* namespace JsonFx.ServiceTest */
JsonFx.ServiceTest = {};

/*-------------------------------------------------------------------*/

/* context object */
JsonFx.ServiceTest.Context = function(/*string*/ request, /*element*/ target) {
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

/*int*/ JsonFx.ServiceTest.dumpDataID = 0;
/*JsonML*/ JsonFx.ServiceTest.dumpData = function(/*string*/ name, /*json*/ val) {
	var c = ["span"];
	var type = typeof val;

	var a = false;
	if ("object" === type && val) {
		a = ["label", {"class":"jsonfx-expando"}];
		c.push(a);
	}
	(a?a:c).push(["span", {"class":"jsonfx-type"}, (val instanceof Array) ? "array" : type]);
	(a?a:c).push(["span", {"class":"jsonfx-name"}, name, ":"]);

	var ul;
	if ("object" === type && val) {
		ul = ["ul", {id:"JsonFx_UI_Dump_"+(JsonFx.ServiceTest.dumpDataID++),"class":"jsonfx-object"}];
		for (var pn in val) {
			if (val.hasOwnProperty(pn)) {
				ul.push(["li", JsonFx.ServiceTest.dumpData(pn, val[pn])]);
			}
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
/*void*/ JsonFx.ServiceTest.cb_displayResult = function(/*object*/ result, /*JsonFx.ServiceTest.Context*/ context, /*Error*/ error) {
	var target = null;
	if (context instanceof JsonFx.ServiceTest.Context) {
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
		[
			"div",
			JsonFx.ServiceTest.dumpData("Call Context", context),
			["hr"],
			JsonFx.ServiceTest.dumpData(label, data)
		],
		target);
};

/* creates actual method call */		
/*function*/ JsonFx.ServiceTest.createServiceCall = function(/*object*/ service, /*object*/ proc, /*element*/ target) {
	return function() {
		var call = service.proxyName+"."+proc.name+"(";
		var args = [], i;
		if (proc.params) {
			for (i=0; i<proc.params.length; i++) {
				if (i > 0) {
					call += ", ";
				}
				args[i] = window.prompt("("+proc.params[i].type+") "+proc.params[i].name+" =","");
				if (args[i] !== null) {
					if (proc.params[i].type !== "str") {
						try {
							args[i] = JSON.parse(args[i]);
						} catch (ex) {}
					}
					call += JSON.stringify(args[i]);
				} else {
					call += "null";
				}
			}
		}
		call += ")";

		var cx = new JsonFx.ServiceTest.Context(call, target);

		service.callService(
			proc.name,
			args,
			{
				onSuccess : JsonFx.ServiceTest.cb_displayResult,
				onFailure : JsonFx.ServiceTest.cb_displayResult,
				context : cx
			});
	};
};

/* generates test buttons from the system.describe result */
/*void*/ JsonFx.ServiceTest.cb_buildTestUI = function(/*object*/ result, /*object*/ context) {
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
	result.procs.unshift({name:"system.describe",summary:"Produces a Service Description for this JSON-RPC Service"});

	var proc, btn, handler;
	for (var i=0; i<result.procs.length; i++) {
		proc = result.procs[i];
		btn = ["input", {type:"button", "class":"ServiceTest", value:proc.name, title:proc.summary}];
		btn = JsonML.parse(btn, null);

		handler = JsonFx.ServiceTest.createServiceCall(context.service, proc, context.target);
		if (handler) {
			btn.onclick = handler;
			context.container.appendChild(btn);
		}
	}
};

/*void*/ JsonFx.ServiceTest.buildTestUI = function(/*JsonFx.IO.Service*/ service, /*element*/ container, /*element*/ target) {
	if (service) {
		service.$describe(
			{
				onSuccess : JsonFx.ServiceTest.cb_buildTestUI,
				onFailure : JsonFx.ServiceTest.cb_displayResult,
				context : { service:service, container:container, target:target }
			});
	}
};
