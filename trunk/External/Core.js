/*---------------------------------------------------------*\
	Additions to browser global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-03-17-1241
\*---------------------------------------------------------*/

if ("undefined" === typeof String.format) {
	/*string*/ String.format = function() {
		var i, pattern, re,
			num = arguments.length,
			str = arguments[0];
		for (i=1; i<num; i++) {
			pattern = "\\{" + (i-1) + "\\}";
			re = new RegExp(pattern, "g");
			str = str.replace(re, arguments[i]);
		}
		return str;
	};
}

if ("undefined" === typeof String.prototype.trim) {
	/*string*/ String.prototype.trim = function () {
		return this.replace(/^\s*|\s*$/g, "");
	};
}

if ("undefined" === typeof String.prototype.contains) {
	/*bool*/ String.prototype.contains = function (/*string*/ str) {
		return (this.indexOf(str) >= 0);
	};
}
