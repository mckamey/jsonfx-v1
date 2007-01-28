/*---------------------------------------------------------*\
	Modifications to global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-01-28-1310
\*---------------------------------------------------------*/

if (!Object.prototype.hasOwnProperty) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
//		if ("undefined" !== typeof this.constructor) {
			return this.constructor.prototype[p] === undefined;
//		} else {
//			return "function" !== typeof p;
//		}
	};
}

//if (!Array.prototype.push) {
//	/*int*/ Array.prototype.push = function(/*object*/ obj) {
//		this[this.length] = obj;
//		return this.length;
//	};
//}

//if (!Array.prototype.pop) {
//	/*object*/ Array.prototype.pop = function() {
//		if (this.length < 1) {
//			return undefined;
//		}
//		var obj = this[this.length-1];
//		delete(this[this.length-1]);
//		return obj;
//	};
//}

if (!String.prototype.trim) {
	/*string*/ String.prototype.trim = function () {
		return this.replace( /^\s*([\s\S]*?)\s*$/, "$1");
	};
}

if (!String.prototype.contains) {
	/*string*/ String.prototype.contains = function (str) {
		return (this.indexOf(str) >= 0);
	};
}