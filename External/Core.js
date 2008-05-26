/*---------------------------------------------------------*\
	Modifications to browser global objects
	Copyright (c)2006-2007 Stephen M. McKamey
	Created: 2006-11-14-0928
	Modified: 2007-03-17-1241
\*---------------------------------------------------------*/

///*float*/ var JScriptVersion =
//	("undefined" === typeof window.ScriptEngineMajorVersion) ? NaN :
//	Number(window.ScriptEngineMajorVersion()+"."+window.ScriptEngineMinorVersion());

if ("undefined" === typeof window.global) {
	/*object*/ window.global = window;
}

if ("undefined" === typeof Object.prototype.hasOwnProperty) {
	/*bool*/ Object.prototype.hasOwnProperty = function(/*object*/ p) {
		return ("undefined" !== typeof(this[p]) && ("undefined" === typeof(this.constructor.prototype[p])));
	};
}

if ("undefined" === typeof Object.prototype.propertyIsEnumerable) {
	/*bool*/ Object.prototype.propertyIsEnumerable = function(/*object*/ p) {
		return this.hasOwnProperty(p);
	};
}

if ("undefined" === typeof Array.prototype.push) {
	/*int*/ Array.prototype.push = function(/*object*/ item) {
		// add item to end
		this[this.length] = item;
		return this.length;
	};
}

if ("undefined" === typeof Array.prototype.pop) {
	/*object*/ Array.prototype.pop = function() {
		if (this.length < 1) {
			return undefined;
		}

		// grab the last item
		var item = this[this.length-1];

		// reduce the length
		this.length--;
		return item;
	};
}

if ("undefined" === typeof Array.prototype.splice) {
	/*array*/ Array.prototype.splice = function (/*int*/ start, /*int*/ del) {
		// based on http://javascript.crockford.com/remedial.html
        var max = Math.max,
            min = Math.min,
            a = [], // The return value array
            e,  // element
            i = max(arguments.length-2, 0),   // insert count
            k = 0,
            l = this.length,
            n,  // new length
            v,  // delta
            x;  // shift count

        start = start || 0;
        if (start < 0) {
            start += l;
        }

        // start point
        start = max(min(start, l), 0);

        // delete count
        del = max(min((typeof del === 'number' && isFinite(del)) ? del : l, l - start), 0);

        v = i - del;
        n = l + v;
        while (k < del) {
            e = this[start + k];
            if (e !== undefined) {
                a[k] = e;
            }
            k += 1;
        }
        x = l - start - del;
        if (v < 0) {
            k = start + i;
            while (x) {
                this[k] = this[k - v];
                k += 1;
                x -= 1;
            }
            this.length = n;
        } else if (v > 0) {
            k = 1;
            while (x) {
                this[n - k] = this[l - k];
                k += 1;
                x -= 1;
            }
        }
        for (k = 0; k < i; ++k) {
            this[start + k] = arguments[k + 2];
        }
        return a;
    };
}

if ("undefined" === typeof Array.prototype.shift) {
	/*object*/ Array.prototype.shift = function() {
		return this.splice(0, 1)[0];
	};
}

if ("undefined" === typeof Array.prototype.unshift) {
	/*void*/ Array.prototype.unshift = function () {
		var a = arguments, i, l, al;
		if (a && a.length) {
			l = this.length;
			al = a.length;

			// move this array items onto end of arguments list
			for (i=0; i<l; i++) {
				a[al] = a[i];
				al++;
			}

			// copy concat'ed arguments list back into this array
			for (i=0; i<al; i++) {
				this[i] = a[i];
			}
		}
    };
}

if ("undefined" === typeof String.prototype.charCodeAt) {
	/*int*/ String.prototype.charCodeAt = function (/*int*/ i) {
		if (isNaN(i) || i<0 || i>=this.length) {
			return NaN;
		}
		return Number(this[i]-"A")+65;
	};
}

if ("undefined" === typeof Number.prototype.toPrecision) {
	/*string*/ Number.prototype.toPrecision = function(/*int*/ digits) {
		var str = this.toString();
		if (isNaN(digits) || digits < 1 || digits > 21) {
			return str;
		}
		digits += str.indexOf(".")+1;

		// note: doesn't handle scientific notation
		return str.substring(0, digits);
	};
}

if ("undefined" === typeof Object.prototype.isPrototypeOf) {
	/*bool*/ Object.prototype.isPrototypeOf = function (/*object*/ obj) {
		while ("undefined" !== typeof(obj.prototype)) {
			if (this === obj.prototype) {
				return true;
			}
			obj = obj.prototype;
		}
		return false;
	};
}

if ("undefined" === typeof window.encodeURI) {
	/*string*/ window.encodeURI = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.encodeURIComponent) {
	/*string*/ window.encodeURIComponent = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.decodeURI) {
	/*string*/ window.decodeURI = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

if ("undefined" === typeof window.decodeURIComponent) {
	/*string*/ window.decodeURIComponent = function (/*string*/ str) {
		// placeholder method, not yet implemented
		return str;
	};
}

/* ----------------------------------------------------*/

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
