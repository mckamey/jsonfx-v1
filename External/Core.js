/*---------------------------------------------------------*\
	Modifications to global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-01-29-0025
\*---------------------------------------------------------*/

if ("undefined" === typeof(Object.prototype.hasOwnProperty)) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
		return ("undefined" === typeof(this.constructor.prototype[p]));
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
			return;
		}
		var obj = this[this.length-1];
		this.length--;
		return obj;
	};
}

// TODO: IE5 doesn't support the lazy *? quantifier

//if ("undefined" === typeof(String.prototype.trim)) {
//	/*string*/ String.prototype.trim = function () {
//		return this.replace( /^\s*([\s\S]*?)\s*$/, "$1");
//	};
//}

if ("undefined" === typeof(String.prototype.contains)) {
	/*string*/ String.prototype.contains = function (str) {
		return (this.indexOf(str) >= 0);
	};
}