/*---------------------------------------------------------*\
	Modifications to global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-01-29-2139
\*---------------------------------------------------------*/

if ("undefined" === typeof(global)) {
	window.global = window;
}

if ("undefined" === typeof(Object.prototype.hasOwnProperty)) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
		return ("undefined" !== typeof(this[p]) && ("undefined" === typeof(this.constructor.prototype[p])));
	};
}

if ("undefined" === typeof(Object.prototype.propertyIsEnumerable)) {
	/*bool*/ Object.prototype.propertyIsEnumerable = function(/*object*/ p) {
		return this.hasOwnProperty(p);
	};
}

if ("undefined" === typeof(Array.prototype.push)) {
	/*int*/ Array.prototype.push = function(/*object*/ obj) {
		this[this.length] = obj;
		return this.length;
	};
}

if ("undefined" === typeof(Array.prototype.pop)) {
	/*object*/ Array.prototype.pop = function() {
		if (this.length < 1) {
			return undefined;
		}
		var obj = this[this.length-1];
		this.length--;
		return obj;
	};
}

if ("undefined" === typeof(Number.prototype.toPrecision)) {
	/*string*/ toPrecision = function(/*int*/ digits) {
		var str = this.toString();
		if (isNaN(digits) || digits < 1 || digits > 21) {
			return str;
		}
		digits += str.indexOf(".")+1;

		// note: doesn't handle scientific notation
		return str.substring(0, digits);
	};
}

if ("undefined" === typeof(window.encodeURI)) {
	/*string*/ window.encodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.encodeURIComponent)) {
	/*string*/ window.encodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.decodeURI)) {
	/*string*/ window.decodeURI = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(window.decodeURIComponent)) {
	/*string*/ window.decodeURIComponent = function (/*string*/ str) {
		// placeholder method
		return str;
	};
}

if ("undefined" === typeof(String.prototype.charCodeAt)) {
	/*int*/ String.prototype.charCodeAt = function (/*int*/ i) {
		if (isNaN(i) || i<0 || i>=this.length) {
			return NaN;
		}
		return Number(this[i]-"A")+65;
	};
}

//if ("undefined" === typeof(Array.prototype.splice)) {
//	/*array*/ Array.prototype.splice = function(...) {
//	};
//}

//if ("undefined" === typeof(Array.prototype.shift)) {
//	/*array*/ Array.prototype.shift = function(...) {
//	};
//}

//if ("undefined" === typeof(Array.prototype.unshift)) {
//	/*array*/ Array.prototype.unshift = function(...) {
//	};
//}

if ("undefined" === typeof(Object.prototype.isProtoTypeOf)) {
	/*bool*/ Object.prototype.isProtoTypeOf = function (/*object*/ obj) {
		while ("undefined" !== typeof(obj.prototype)) {
			if (this === obj.prototype) {
				return true;
			}
			obj = obj.prototype;
		}
		return false;
	};
}

if ("undefined" === typeof(String.prototype.trim)) {
	/*string*/ String.prototype.trim = function () {
		return this.replace(/^\s*|\s*$/g, "");
	};
}

if ("undefined" === typeof(String.prototype.contains)) {
	/*string*/ String.prototype.contains = function (str) {
		return (this.indexOf(str) >= 0);
	};
}