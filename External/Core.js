//=======================================================
//	modifications to global objects
//=======================================================

if (!Object.prototype.hasOwnProperty) {
	Object.prototype.hasOwnProperty = function(p) {
		return this.constructor.prototype[p] === undefined;
	};
}

String.prototype.trim = function () {
    return this.replace( /^\s*([\s\S]*?)\s*$/, "$1");
};