// jslint.js
// 2007-03-28
/*
Copyright (c) 2002 Douglas Crockford  (www.JSLint.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

The Software shall be used for Good, not Evil.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


/*
    JSLINT is a global function. It takes two parameters.

        var myResult = JSLINT(source, option);

    The first parameter is either a string or an array of strings. If it is a
    string, it will be split on '\n' or '\r'. If it is an array of strings, it
    is assumed that each string represents one line. The source can be a
    JavaScript text, or HTML text, or a Konfabulator text.

    The second parameter is an optional object of options which control the
    operation of JSLINT. All of the options are booleans. All are optional and
    have a default value of false.

    {
        bitwise    : true if bitwise operators should not be allowed
        browser    : true if the standard browser globals should be predefined
        cap        : true if upper case HTML should be allowed
        debug      : true if debugger statements should be allowed
        eqeqeq     : true if === should be required
        evil       : true if eval should be allowed
        laxLineEnd : true if line breaks should not be checked
        nomen      : true if names should be checked
        passfail   : true if the scan should stop on first error
        plusplus   : true if increment/decrement should not be allowed
        redef      : true if var redefinition should be allowed
        rhino      : true if the Rhino environment globals should be predefined
        undef      : true if undefined variables are errors
        white      : true if strict whitespace rules apply
        widget     : true if the Yahoo Widgets globals should be predefined
    }

    If it checks out, JSLINT returns true. Otherwise, it returns false.

    If false, you can inspect JSLINT.errors to find out the problems.
    JSLINT.errors is an array of objects containing these members:

    {
        line      : The line (relative to 0) at which the lint was found
        character : The character (relative to 0) at which the lint was found
        reason    : The problem
        evidence  : The text line in which the problem occurred
    }

    If a fatal error was found, a null will be the last element of the
    JSLINT.errors array.

    You can request a Function Report, which shows all of the functions
    and the parameters and vars that they use. This can be used to find
    implied global variables and other problems. The report is in HTML and
    can be inserted in a <body>.

        var myReport = JSLINT.report(option);

    If the option is true, then the report will be limited to only errors.
*/

String.prototype.entityify = function () {
    return this.
        replace(/&/g, '&amp;').
        replace(/</g, '&lt;').
        replace(/>/g, '&gt;');
};

String.prototype.isAlpha = function () {
    return (this >= 'a' && this <= 'z\uffff') ||
        (this >= 'A' && this <= 'Z\uffff');
};


String.prototype.isDigit = function () {
    return (this >= '0' && this <= '9');
};


// We build the application inside a function so that we produce only a single
// global variable. The function will be invoked, its return value is the JSLINT
// function itself.

var JSLINT;
JSLINT = function () {

    var anonname;

// browser contains a set of global names which are commonly provided by a
// web browser environment.

    var browser = {
        alert: true,
        blur: true,
        clearInterval: true,
        clearTimeout: true,
        close: true,
        closed: true,
        confirm: true,
        console: true,
        Debug: true,
        defaultStatus: true,
        document: true,
        event: true,
        focus: true,
        frames: true,
        history: true,
        Image: true,
        length: true,
        location: true,
        moveBy: true,
        moveTo: true,
        name: true,
        navigator: true,
        onblur: true,
        onerror: true,
        onfocus: true,
        onload: true,
        onresize: true,
        onunload: true,
        open: true,
        opener: true,
        opera: true,
        parent: true,
        print: true,
        prompt: true,
        resizeBy: true,
        resizeTo: true,
        screen: true,
        scroll: true,
        scrollBy: true,
        scrollTo: true,
        self: true,
        setInterval: true,
        setTimeout: true,
        status: true,
        top: true,
        window: true,
        XMLHttpRequest: true
    };
    var funlab;
    var funstack;
    var functions;
    var globals;
    var inblock;
    var indent;

// konfab contains the global names which are provided to a Yahoo
// (fna Konfabulator) widget.

    var konfab = {
        alert: true,
        appleScript: true,
        animator: true,
        appleScript: true,
        beep: true,
        bytesToUIString: true,
        chooseColor: true,
        chooseFile: true,
        chooseFolder: true,
        convertPathToHFS: true,
        convertPathToPlatform: true,
        closeWidget: true,
        CustomAnimation: true,
        escape: true,
        FadeAnimation: true,
        focusWidget: true,
        form: true,
        include: true,
        isApplicationRunning: true,
        iTunes: true,
        konfabulatorVersion: true,
        log: true,
        MoveAnimation: true,
        openURL: true,
        play: true,
        popupMenu: true,
        print: true,
        prompt: true,
        reloadWidget: true,
        resolvePath: true,
        resumeUpdates: true,
        RotateAnimation: true,
        runCommand: true,
        runCommandInBg: true,
        saveAs: true,
        savePreferences: true,
        showWidgetPreferences: true,
        sleep: true,
        speak: true,
        suppressUpdates: true,
        tellWidget: true,
        unescape: true,
        updateNow: true,
        yahooCheckLogin: true,
        yahooLogin: true,
        yahooLogout: true,
        COM: true,
        filesystem: true,
        preferenceGroups: true,
        preferences: true,
        screen: true,
        system: true,
        URL: true,
        XMLDOM: true,
        XMLHttpRequest: true
    };
    var lines;
    var lookahead;
    var member;
    var nexttoken;
    var noreach;
    var option;
    var prevtoken;
    var quit;
    var rhino = {
        defineClass: true,
        deserialize: true,
        gc: true,
        help: true,
        load: true,
        loadClass: true,
        print: true,
        quit: true,
        readFile: true,
        readUrl: true,
        runCommand: true,
        seal: true,
        serialize: true,
        spawn: true,
        sync: true,
        toint32: true,
        version: true
    };
    var stack;

// standard contains the global names that are provided by standard JavaScript.

    var standard = {
        Array: true,
        Boolean: true,
        Date: true,
        decodeURI: true,
        decodeURIComponent: true,
        encodeURI: true,
        encodeURIComponent: true,
        Error: true,
        escape: true,
        'eval': true,
        EvalError: true,
        Function: true,
        isFinite: true,
        isNaN: true,
        Math: true,
        Number: true,
        Object: true,
        parseInt: true,
        parseFloat: true,
        RangeError: true,
        ReferenceError: true,
        RegExp: true,
        String: true,
        SyntaxError: true,
        TypeError: true,
        unescape: true,
        URIError: true
    };
    var syntax = {};
    var token;
    var verb;
    var warnings;
    var wmode;

//  xmode is used to adapt to the exceptions in XML parsing.
//  It can have these states:
//      false   .js script file
//      "       A " attribute
//      '       A ' attribute
//      content The content of a script tag
//      CDATA   A CDATA block

    var xmode;

//  xtype identifies the type of document being analyzed.
//  It can have these states:
//      false   .js script file
//      html    .html file
//      widget  .kon Konfabulator file

    var xtype;
// token
    var tx = /^([(){}[.,:;'"~]|\](\]>)?|\?>?|==?=?|\/(\*(global|extern)*|=|)|\*[\/=]?|\+[+=]?|-[-=]?|%[=>]?|&[&=]?|\|[|=]?|>>?>?=?|<([\/=%\?]|\!(\[|--)?|<=?)?|\^=?|\!=?=?|[a-zA-Z_$][a-zA-Z0-9_$]*|[0-9]+([xX][0-9a-fA-F]+|\.[0-9]*)?([eE][+-]?[0-9]+)?)/;
// string ending in single quote
    var sx = /^((\\[^\x00-\x1f]|[^\x00-\x1f'\\])*)'/;
    var sxx = /^(([^\x00-\x1f'])*)'/;
// string ending in double quote
    var qx = /^((\\[^\x00-\x1f]|[^\x00-\x1f"\\])*)"/;
    var qxx = /^(([^\x00-\x1f"])*)"/;
// regular expression
    var rx = /^(\\[^\x00-\x1f]|\[(\\[^\x00-\x1f]|[^\x00-\x1f\\\/])*\]|[^\x00-\x1f\\\/\[])+\/[gim]*/;
// star slash
    var lx = /\*\/|\/\*/;
// global identifier
    var gx = /^([a-zA-Z_$][a-zA-Z0-9_$]*)/;
// identifier
    var ix = /^([a-zA-Z_$][a-zA-Z0-9_$]*$)/;
// global separators
    var hx = /^[\x00-\x20,]*(\*\/)?/;
// whitespace
    var wx = /^\s*(\/\/.*\r*$)?/;

// Make a new object that inherits from an existing object.

    function object(o) {
        function F() {}
        F.prototype = o;
        return new F();
    }

// Produce an error warning.


    function warning(m, x, y) {
        var l;
        var c;
        var t = typeof x === 'object' ? x : nexttoken;
        var w;
        if (typeof x === 'number') {
            l = x;
            c = y || 0;
        } else {
            if (t.id === '(end)') {
                t = token;
            }
            l = t.line || 0;
            c = t.from || 0;
        }
        w = {
            id: '(error)',
            reason: m,
            evidence: lines[l] || '',
            line: l,
            character: c
        };
        JSLINT.errors.push(w);
        if (option.passfail) {
            quit('Stopping. ', l, c);
        }
        warnings += 1;
        if (warnings === 50) {
            quit("Too many errors.", l, c);
        }
        return w;
    }

    function error(m, x, y) {
        var w = warning(m, x, y);
        quit("Stopping, unable to continue.", w.line, w.character);
    }

    quit = function quit(m, l, c) {
        warning(m + ' (' + Math.floor((l / lines.length) * 100) + '% scanned)', l, c);
        JSLINT.errors.push(null);
        throw null;
    };



// lexical analysis

    var lex = function () {
        var character;
        var from;
        var line;
        var s;

// Private lex methods

        function nextLine() {
            line += 1;
            if (line >= lines.length) {
                return false;
            }
            character = 0;
            s = lines[line];
            return true;
        }

// Produce a token object.  The token inherits from a syntax symbol.

        function it(type, value) {
            var t;
            if (type === '(punctuator)') {
                t = syntax[value];
            } else if (type === '(identifier)') {
                t = syntax[value];
                if (!t || typeof t !== 'object') {
                    t = syntax[type];
                }
            } else {
                t = syntax[type];
            }
            if (!t || typeof t !== 'object') {
                error("Unrecognized symbol: '" + value + "' " + type);
            }
            t = object(t);
            if (value || type === '(string)') {
                if (value.charAt(10) === ':' &&
                        value.substring(0, 10).toLowerCase() === 'javascript') {
                    warning("JavaScript URL.");
                }
                t.value = value;
            }
            t.line = line;
            t.character = character;
            t.from = from;
            return t;
        }

// Public lex methods

        return {
            init: function (source) {
                if (typeof source === 'string') {
                    lines = source.split('\n');
                    if (lines.length === 1) {
                        lines = lines[0].split('\r');
                    }
                } else {
                    lines = source;
                }
                line = 0;
                character = 0;
                from = 0;
                s = lines[0];
            },

// token -- this is called by advance to get the next token.

            token: function () {
                var c;
                var i;
                var l;
                var r;
                var t;

                function string(x) {
                    var a;
                    var j;
                    r = x.exec(s);
                    if (r) {
                        t = r[1];
                        l = r[0].length;
                        s = s.substr(l);
                        character += l;
                        if (xmode === 'script') {
                            if (t.indexOf('<\/') >= 0) {
                                warning(
    'Expected "...<\\/..." and instead saw "...<\/...".', nexttoken);
                            }
                        }
                        return it('(string)', r[1]);
                    } else {
                        for (j = 0; j < s.length; j += 1) {
                            a = s.charAt(j);
                            if (a < ' ') {
                                if (a === '\n' || a === '\r') {
                                    break;
                                }
                                warning("Control character in string: " +
                                        s.substring(0, j), line, character + j);
                            }
                        }
                        error("Unclosed string: " + s, line, character);
                    }
                }

                for (;;) {
                    if (!s) {
                        return it(nextLine() ? '(endline)' : '(end)', '');
                    }
                    r = wx.exec(s);
                    if (!r || !r[0]) {
                        break;
                    }
                    l = r[0].length;
                    s = s.substr(l);
                    character += l;
                    if (s) {
                        break;
                    }
                }
                from = character;
                r = tx.exec(s);
                if (r) {
                    t = r[0];
                    l = t.length;
                    s = s.substr(l);
                    character += l;
                    c = t.substr(0, 1);

//      identifier

                    if (c.isAlpha() || c === '_' || c === '$') {
                        return it('(identifier)', t);
                    }

//      number

                    if (c.isDigit()) {
                        if (nexttoken.id === '.') {
                            warning(
            "A decimal fraction should have a zero before the decimal point.",
                                nexttoken);
                        }
                        if (!isFinite(Number(t))) {
                            warning("Bad number: '" + t + "'.",
                                line, character);
                        }
                        if (s.substr(0, 1).isAlpha()) {
                            warning("Space is required after a number: '" +
                                    t + "'.", line, character);
                        }
                        if (c === '0' && t.substr(1,1).isDigit()) {
                            warning("Don't use extra leading zeros: '" +
                                    t + "'.", line, character);
                        }
                        if (t.substr(t.length - 1) === '.') {
                            warning(
    "A trailing decimal point can be confused with a dot: '" + t + "'.",
                                    line, character);
                        }
                        return it('(number)', t);
                    }

//      string

                    if (t === '"') {
                        return (xmode === '"' ||  xmode === 'string') ?
                            it('(punctuator)', t) :
                            string(xmode === 'xml' ? qxx : qx);
                    }
                    if (t === "'") {
                        return (xmode === "'" ||  xmode === 'string') ?
                            it('(punctuator)', t) :
                            string(xmode === 'xml' ? sxx : sx);
                    }

//      unbegun comment

                    if (t === '/*') {
                        for (;;) {
                            i = s.search(lx);
                            if (i >= 0) {
                                break;
                            }
                            if (!nextLine()) {
                                error("Unclosed comment.", nexttoken);
                            }
                        }
                        character += i + 2;
                        if (s.substr(i, 1) === '/') {
                            error("Nested comment.");
                        }
                        s = s.substr(i + 2);
                        return this.token();
                    }

//      /*extern

                    if (t === '/*extern' || t === '/*global') {
                        for (;;) {
                            r = hx.exec(s);
                            if (r) {
                                l = r[0].length;
                                s = s.substr(l);
                                character += l;
                                if (r[1] === '*/') {
                                    return this.token();
                                }
                            }
                            if (s) {
                                r = gx.exec(s);
                                if (r) {
                                    l = r[0].length;
                                    s = s.substr(l);
                                    character += l;
                                    globals[r[1]] = true;
                                } else {
                                    error("Bad extern identifier: '" +
                                        s + "'.", line, character);
                                }
                            } else if (!nextLine()) {
                                error("Unclosed comment.");
                            }
                        }
                    }

//      punctuator

                    return it('(punctuator)', t);
                }
                error("Unexpected token: " + (t || s.substr(0, 1)),
                    line, character);
            },

// skip -- skip past the next occurrence of a particular string.
// If the argument is empty, skip to just before the next '<' character.
// This is used to ignore HTML content. Return false if it isn't found.

            skip: function (t) {
                var i;
                if (nexttoken.id) {
                    if (!t) {
                        t = '';
                        if (nexttoken.id.substr(0, 1) === '<') {
                            lookahead.push(nexttoken);
                            return true;
                        }
                    } else if (nexttoken.id.indexOf(t) >= 0) {
                        return true;
                    }
                }
                token = nexttoken;
                nexttoken = syntax['(error)'];
                for (;;) {
                    i = s.indexOf(t || '<');
                    if (i >= 0) {
                        character += i + t.length;
                        s = s.substr(i + t.length);
                        return true;
                    }
                    if (!nextLine()) {
                        break;
                    }
                }
                return false;
            },

// regex -- this is called by parse when it sees '/' being used as a prefix.

            regex: function () {
                var l;
                var r = rx.exec(s);
                var x;
                if (r) {
                    l = r[0].length;
                    character += l;
                    s = s.substr(l);
                    x = r[1];
                    return it('(regex)', x);
                }
                error("Bad regular expression: " + s);
            }
        };
    }();

    function builtin(name) {
        return standard[name] === true ||
               globals[name] === true ||
              (option.rhino && rhino[name] === true) ||
             ((xtype === 'widget' || option.widget) && konfab[name] === true) ||
             ((xtype === 'html' || option.browser) && browser[name] === true);
    }

    function addlabel(t, type) {
        if (t) {
            if (typeof funlab[t] === 'string') {
                switch (funlab[t]) {
                case 'var':
                case 'var*':
                    if (type === 'global') {
                        funlab[t] = 'var*';
                        return;
                    }
                    break;
                case 'global':
                    if (type === 'var') {
                        warning('Var ' + t +
                            ' was used before it was declared.', token);
                        return;
                    }
                    if (type === 'var*' || type === 'global') {
                        return;
                    }
                    break;
                case 'function':
                case 'parameter':
                    if (type === 'global') {
                        return;
                    }
                    break;
                }
                warning("Identifier '" + t + "' already declared as " +
                        funlab[t], token);
            }
            funlab[t] = type;
        }
    }


// We need a peek function. If it has an argument, it peeks that much farther
// ahead. It is used to distinguish
//     for ( var i in ...
// from
//     for ( var i = ...

    function peek(i) {
        var j = 0;
        var t;
        if (nexttoken === syntax['(error)']) {
            return nexttoken;
        }
        if (typeof i === 'undefined') {
            i = 0;
        }
        while (j <= i) {
            t = lookahead[j];
            if (!t) {
                t = lookahead[j] = lex.token();
            }
            j += 1;
        }
        return t;
    }


    var badbreak = {
        ')': true,
        ']': true,
        '++': true,
        '--': true
    };

// Produce the next token. It looks for programming errors.

    function advance(id, t) {
        var l;
        switch (token.id) {
        case '(number)':
            if (nexttoken.id === '.') {
                warning(
"A dot following a number can be confused with a decimal point.", token);
            }
            break;
        case '-':
            if (nexttoken.id === '-' || nexttoken.id === '--') {
                warning("Confusing minusses.");
            }
            break;
        case '+':
            if (nexttoken.id === '+' || nexttoken.id === '++') {
                warning("Confusing plusses.");
            }
            break;
        }
        if (token.type === '(string)' || token.identifier) {
            anonname = token.value;
        }

        if (id && nexttoken.value !== id) {
            if (t) {
                if (nexttoken.id === '(end)') {
                    warning("Unmatched '" + t.id + "'.", t);
                } else {
                    warning("Expected '" + id + "' to match '" +
                            t.id + "' from line " + (t.line + 1) +
                            " and instead saw '" + nexttoken.value + "'.");
                }
            } else {
                warning("Expected '" + id + "' and instead saw '" +
                        nexttoken.value + "'.");
            }
        }
        prevtoken = token;
        token = nexttoken;
        for (;;) {
            nexttoken = lookahead.shift() || lex.token();
            if (nexttoken.id === '<![') {
                if (xtype === 'html') {
                    error("Unexpected token '<!['");
                }
                if (xmode === 'script') {
                    nexttoken = lex.token();
                    if (nexttoken.value !== 'CDATA') {
                        error("Expected 'CDATA'");
                    }
                    nexttoken = lex.token();
                    if (nexttoken.id !== '[') {
                        error("Expected '['");
                    }
                    xmode = 'CDATA';
                } else if (xmode === 'xml') {
                    lex.skip(']]>');
                } else {
                    error("Unexpected token '<!['");
                }
            } else if (nexttoken.id === ']]>') {
                if (xmode === 'CDATA') {
                    xmode = 'script';
                } else {
                    error("Unexpected token ']]>");
                }
            } else if (nexttoken.id !== '(endline)') {
                break;
            }
            if (xmode === '"' || xmode === "'") {
                error("Missing '" + xmode + "'.", token);
            }
            l = !xmode && !option.laxLineEnd &&
                (token.type === '(string)' || token.type === '(number)' ||
                token.type === '(identifier)' || badbreak[token.id]);
        }
        if (l) {
            switch (nexttoken.id) {
            case '{':
            case '}':
            case ']':
                break;
            case ')':
                switch (token.id) {
                case ')':
                case '}':
                case ']':
                    break;
                default:
                    warning("Line breaking error: ')'.", token);
                }
                break;
            default:
                warning("Line breaking error: '" + token.value + "'.",
                        token);
            }
        }
        if (xtype === 'widget' && xmode === 'script' && nexttoken.id) {
            l = nexttoken.id.charAt(0);
            if (l === '<' || l === '&') {
                nexttoken.nud = nexttoken.led = null;
                nexttoken.lbp = 0;
                nexttoken.reach = true;
            }
        }
    }


    function advanceregex() {
        nexttoken = lex.regex();
    }


    function beginfunction(i) {
        var f = {
            '(name)': i,
            '(line)': nexttoken.line + 1,
            '(context)': funlab
        };
        funstack.push(funlab);
        funlab = f;
        functions.push(funlab);
    }


    function endfunction() {
        funlab = funstack.pop();
    }


// This is the heart of JSLINT, the Pratt parser. In addition to parsing, it
// is looking for ad hoc lint patterns. We add to Pratt's model .fud, which is
// like nud except that it is only used on the first token of a statement.
// Having .fud makes it much easier to define JavaScript. I retained Pratt's
// nomenclature, even though it isn't very descriptive.

// .nud     Null denotation
// .fud     First null denotation
// .led     Left denotation
//  lbp     Left binding power
//  rbp     Right binding power

// They are key to the parsing method called Top Down Operator Precedence.

    function parse(rbp, initial) {
        var l;
        var left;
        var o;
        if (nexttoken.id && nexttoken.id === '/') {
            if ('(=:,!?[&|'.indexOf(token.id.charAt(0)) < 0) {
                warning(
"Expected to see a '(' or '=' or ':' or ',' or '[' preceding a regular expression literal, and instead saw '" +
                        token.value + "'.", token);
            }
            advanceregex();
        }
        if (nexttoken.id === '(end)') {
            warning("Unexpected early end of program", token);
        }
        advance();
        if (initial) {
            anonname = 'anonymous';
            verb = token.value;
        }
        if (initial && token.fud) {
            token.fud();
        } else {
            if (token.nud) {
                o = token.exps;
                left = token.nud();
            } else {
                if (nexttoken.type === '(number)' && token.id === '.') {
                    warning(
"A leading decimal point can be confused with a dot: ." + nexttoken.value,
                            token);
                }
                error("Expected an identifier and instead saw '" +
                        token.id + "'.", token);
            }
            while (rbp < nexttoken.lbp) {
                o = nexttoken.exps;
                advance();
                if (token.led) {
                    left = token.led(left);
                } else {
                    error("Expected an operator and instead saw '" +
                        token.id + "'.");
                }
            }
            if (initial && !o) {
                warning(
"Expected an assignment or function call and instead saw an expression.",
                        token);
            }
        }
        if (l) {
            funlab[l] = 'label';
        }
        if (left && left.id === 'eval') {
            warning("evalError", left);
        }
        return left;
    }


// Functions for conformance of style.

    function adjacent(left, right) {
        if (option.white) {
            if (left.character !== right.from) {
                warning("Unexpected space after '" + left.value + "'.");
            }
        }
    }


    function nonadjacent(left, right) {
        if (option.white) {
            if (left.character === right.from) {
                warning("Expected space after '" + left.value + "'.");
            }
        }
    }

    function indentation(bias) {
        var i;
        if (option.white) {
            i = indent + (bias || 0);
            if (nexttoken.from !== i) {
                warning("Expected '" + nexttoken.value +
                        "' to have an indentation of " + i +
                        " instead of " + nexttoken.from + ".");
            }
        }
    }



// Parasitic constructors for making the symbols that will be inherited by
// tokens.

    function symbol(s, p) {
        return syntax[s] || (syntax[s] = {
            id: s,
            lbp: p,
            value: s
        });
    }


    function delim(s) {
        return symbol(s, 0);
    }


    function stmt(s, f) {
        var x = delim(s);
        x.identifier = x.reserved = true;
        x.fud = f;
        return x;
    }


    function blockstmt(s, f) {
        var x = stmt(s, f);
        x.block = true;
        return x;
    }


    function reserveName(x) {
        var c = x.id.charAt(0);
        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
            x.identifier = x.reserved = true;
        }
        return x;
    }


    function prefix(s, f) {
        var x = symbol(s, 150);
        reserveName(x);
        x.nud = (typeof f === 'function') ? f : function () {
            if (option.plusplus && (this.id === '++' || this.id === '--')) {
                warning("Unexpected use of " + this.id + ".", this);
            }
            parse(150);
            return this;
        };
        return x;
    }


    function type(s, f) {
        var x = delim(s);
        x.type = s;
        x.nud = f;
        return x;
    }


    function reserve(s, f) {
        var x = type(s, f);
        x.identifier = x.reserved = true;
        return x;
    }


    function reservevar(s) {
        return reserve(s, function () {
            return this;
        });
    }


    function infix(s, f, p) {
        var x = symbol(s, p);
        reserveName(x);
        x.led = (typeof f === 'function') ? f : function (left) {
            nonadjacent(prevtoken, token);
            nonadjacent(token, nexttoken);
            return [this.id, left, parse(p)];
        };
        return x;
    }


    function relation(s, f) {
        var x = symbol(s, 100);
        x.led = function (left) {
            nonadjacent(prevtoken, token);
            nonadjacent(token, nexttoken);
            var right = parse(100);
            if ((left && left.id === 'NaN') || (right && right.id === 'NaN')) {
                warning("Use the isNaN function to compare with NaN.", this);
            } else if (f) {
                f.apply(this, [left, right]);
            }
            return [this.id, left, right];
        };
        return x;
    }


    function isPoorRelation(node) {
        return (node.type === '(number)' && !+node.value) ||
               (node.type === '(string)' && !node.value) ||
                node.type === 'true' ||
                node.type === 'false' ||
                node.type === 'undefined' ||
                node.type === 'null';
    }


    function assignop(s, f) {
        symbol(s, 20).exps = true;
        return infix(s, function (left) {
            nonadjacent(prevtoken, token);
            nonadjacent(token, nexttoken);
            if (left) {
                if (left.id === '.' || left.id === '[' ||
                        (left.identifier && !left.reserved)) {
                    parse(19);
                    return left;
                }
                if (left === syntax['function']) {
                    warning(
"Expected an identifier in an assignment, and instead saw a function invocation.",
                                token);
                }
            }
            error("Bad assignment.", this);
        }, 20);
    }

    function bitwise(s, f, p) {
        var x = symbol(s, p);
        reserveName(x);
        x.led = (typeof f === 'function') ? f : function (left) {
            if (option.bitwise) {
                warning("Unexpected bitwise operator '" + this.id + "'.", this);
            }
            nonadjacent(prevtoken, token);
            nonadjacent(token, nexttoken);
            return [this.id, left, parse(p)];
        };
        return x;
    }

    function bitwiseassignop(s) {
        symbol(s, 20).exps = true;
        return infix(s, function (left) {
            if (option.bitwise) {
                warning("Unexpected bitwise operator '" + this.id + "'.", this);
            }
            nonadjacent(prevtoken, token);
            nonadjacent(token, nexttoken);
            if (left) {
                if (left.id === '.' || left.id === '[' ||
                        (left.identifier && !left.reserved)) {
                    parse(19);
                    return left;
                }
                if (left === syntax['function']) {
                    warning(
"Expected an identifier in an assignment, and instead saw a function invocation.",
                                token);
                }
            }
            error("Bad assignment.", this);
        }, 20);
    }


    function suffix(s, f) {
        var x = symbol(s, 150);
        x.led = function (left) {
            if (option.plusplus) {
                warning("Unexpected use of " + this.id + ".", this);
            }
            return [f, left];
        };
        return x;
    }


    function optionalidentifier() {
        if (nexttoken.reserved) {
            warning("Expected an identifier and instead saw '" +
                nexttoken.id + "' (a reserved word).");
        }
        if (nexttoken.identifier) {
            if (option.nomen) {
                if (nexttoken.value.charAt(0) === '_' ||
                        nexttoken.value.indexOf('$') >= 0) {
                    warning("Unexpected characters in '" +
                        nexttoken.value + "'.");
                }
            }
            advance();
            return token.value;
        }
    }


    function identifier() {
        var i = optionalidentifier();
        if (i) {
            return i;
        }
        if (token.id === 'function' && nexttoken.id === '(') {
            warning("Missing name in function statement.");
        } else {
            error("Expected an identifier and instead saw '" +
                    nexttoken.value + "'.", nexttoken);
        }
    }

    function reachable(s) {
        var i = 0;
        var t;
        if (nexttoken.id !== ';' || noreach) {
            return;
        }
        for (;;) {
            t = peek(i);
            if (t.reach) {
                return;
            }
            if (t.id !== '(endline)') {
                if (t.id === 'function') {
                    warning(
"Inner functions should be listed at the top of the outer function.", t);
                    break;
                }
                warning("Unreachable '" + t.value + "' after '" + s +
                        "'.", t);
                break;
            }
            i += 1;
        }
    }


    function statement() {
        var t = nexttoken;
        if (t.id === ';') {
            warning("Unnecessary semicolon", t);
            advance(';');
            return;
        }
        if (t.identifier && !t.reserved && peek().id === ':') {
            advance();
            advance(':');
            addlabel(t.value, 'live*');
            if (!nexttoken.labelled) {
                warning("Label '" + t.value +
                        "' on unlabelable statement '" + nexttoken.value + "'.",
                        nexttoken);
            }
            if (t.value.toLowerCase() === 'javascript') {
                warning("Label '" + t.value +
                        "' looks like a javascript url.",
                        nexttoken);
            }
            nexttoken.label = t.value;
            t = nexttoken;
        }
        parse(0, true);
        if (!t.block) {
            if (nexttoken.id !== ';') {
                warning("Missing ';'", token.line,
                        token.from + token.value.length);
            } else {
                adjacent(token, nexttoken);
                advance(';');
                nonadjacent(token, nexttoken);
            }
        }
    }


    function statements() {
        while (!nexttoken.reach && nexttoken.id !== '(end)') {
            indentation();
            statement();
        }
    }


    function block(f) {
        var b = inblock;
        inblock = f;
        nonadjacent(token, nexttoken);
        var t = nexttoken;
        if (nexttoken.id === '{') {
            advance('{');
            if (nexttoken.id !== '}' || token.line !== nexttoken.line) {
                indent += 4;
                statements();
                indent -= 4;
                indentation();
            }
            advance('}', t);
        } else {
            warning("Missing '{' before '" + nexttoken.value + "'.");
            noreach = true;
            statement();
            noreach = false;
        }
        verb = null;
        inblock = b;
    }


// An identity function, used by string and number tokens.

    function idValue() {
        return this;
    }


    function countMember(m) {
        if (typeof member[m] === 'number') {
            member[m] += 1;
        } else {
            member[m] = 1;
        }
    }


// Common HTML attributes that carry scripts.

    var scriptstring = {
        onblur:      true,
        onchange:    true,
        onclick:     true,
        ondblclick:  true,
        onfocus:     true,
        onkeydown:   true,
        onkeypress:  true,
        onkeyup:     true,
        onload:      true,
        onmousedown: true,
        onmousemove: true,
        onmouseout:  true,
        onmouseover: true,
        onmouseup:   true,
        onreset:     true,
        onselect:    true,
        onsubmit:    true,
        onunload:    true
    };


// XML types. Currently we support html and widget.

    var xmltype = {
        HTML: {
            doBegin: function (n) {
                if (!option.cap) {
                    warning("HTML case error.");
                }
                xmltype.html.doBegin();
            }
        },
        html: {
            doBegin: function (n) {
                xtype = 'html';
                xmltype.html.script = false;
            },
            doTagName: function (n, p) {
                var i;
                var t = xmltype.html.tag[n];
                var x;
                if (!t) {
                    error('Unrecognized tag: <' + n + '>. ' +
                            (n === n.toLowerCase() ?
                            'Did you mean <' + n.toLowerCase() + '>?' : ''));
                }
                x = t.parent;
                if (x) {
                    if (x.indexOf(' ' + p + ' ') < 0) {
                        error('A <' + n + '> must be within <' + x + '>',
                                token);
                    }
                } else {
                    i = stack.length;
                    do {
                        if (i <= 0) {
                            error('A <' + n + '> must be within the body',
                                    token);
                        }
                        i -= 1;
                    } while (stack[i].name !== 'body');
                }
                xmltype.html.script = n === 'script';
                return t.empty;
            },
            doAttribute: function (n, a) {
                if (n === 'script') {
                    if (a === 'src') {
                        xmltype.html.script = false;
                        return 'string';
                    } else if (a === 'language') {
                        warning("The 'language' attribute is deprecated",
                                token);
                        return false;
                    }
                }
                if (a === 'href') {
                    return 'href';
                }
                return scriptstring[a] && 'script';
            },
            doIt: function (n) {
                return xmltype.html.script ? 'script' : n !== 'html' &&
                        xmltype.html.tag[n].special && 'special';
            },
            tag: {
                a:        {},
                abbr:     {},
                acronym:  {},
                address:  {},
                applet:   {},
                area:     {empty: true, parent: ' map '},
                b:        {},
                base:     {empty: true, parent: ' head '},
                bdo:      {},
                big:      {},
                blockquote: {},
                body:     {parent: ' html noframes '},
                br:       {empty: true},
                button:   {},
                canvas:   {parent: ' body p div th td '},
                caption:  {parent: ' table '},
                center:   {},
                cite:     {},
                code:     {},
                col:      {empty: true, parent: ' table colgroup '},
                colgroup: {parent: ' table '},
                dd:       {parent: ' dl '},
                del:      {},
                dfn:      {},
                dir:      {},
                div:      {},
                dl:       {},
                dt:       {parent: ' dl '},
                em:       {},
                embed:    {},
                fieldset: {},
                font:     {},
                form:     {},
                frame:    {empty: true, parent: ' frameset '},
                frameset: {parent: ' html frameset '},
                h1:       {},
                h2:       {},
                h3:       {},
                h4:       {},
                h5:       {},
                h6:       {},
                head:     {parent: ' html '},
                html:     {},
                hr:       {empty: true},
                i:        {},
                iframe:   {},
                img:      {empty: true},
                input:    {empty: true},
                ins:      {},
                kbd:      {},
                label:    {},
                legend:   {parent: ' fieldset '},
                li:       {parent: ' dir menu ol ul '},
                link:     {empty: true, parent: ' head '},
                map:      {},
                menu:     {},
                meta:     {empty: true, parent: ' head noscript '},
                noframes: {parent: ' html body '},
                noscript: {parent:
' applet blockquote body button center dd del div fieldset form frameset head html iframe ins li map noframes noscript object td th '},
                object:   {},
                ol:       {},
                optgroup: {parent: ' select '},
                option:   {parent: ' optgroup select '},
                p:        {},
                param:    {empty: true, parent: ' applet object '},
                pre:      {},
                q:        {},
                samp:     {},
                script:   {parent:
' head body p div span abbr acronym address bdo blockquote cite code del dfn em ins kbd pre samp strong table tbody td th tr var '},
                select:   {},
                small:    {},
                span:     {},
                strong:   {},
                style:    {parent: ' head ', special: true},
                sub:      {},
                sup:      {},
                table:    {},
                tbody:    {parent: ' table '},
                td:       {parent: ' tr '},
                textarea: {},
                tfoot:    {parent: ' table '},
                th:       {parent: ' tr '},
                thead:    {parent: ' table '},
                title:    {parent: ' head '},
                tr:       {parent: ' table tbody thead tfoot '},
                tt:       {},
                u:        {},
                ul:       {},
                'var':    {}
            }
        },
        widget: {
            doBegin: function (n) {
                xtype = 'widget';
            },
            doTagName: function (n, p) {
                var t = xmltype.widget.tag[n];
                if (!t) {
                    error('Unrecognized tag: <' + n + '>. ');
                }
                var x = t.parent;
                if (x.indexOf(' ' + p + ' ') < 0) {
                    error('A <' + n + '> must be within <' + x + '>',
                            token);
                }
            },
            doAttribute: function (n, a) {
                var t = xmltype.widget.tag[a];
                if (!t) {
                    error('Unrecognized attribute: <' + n + ' ' + a + '>. ');
                }
                var x = t.parent;
                if (x.indexOf(' ' + n + ' ') < 0) {
                    error('Attribute ' + a + ' does not belong in <' +
                            n + '>');
                }
                return t.script ? 'script' : a === 'name' ? 'define' : 'string';
            },
            doIt: function (n) {
                var x = xmltype.widget.tag[n];
                return x && x.script && 'script';
            },
            tag: {
                "about-box": {parent: ' widget '},
                "about-image": {parent: ' about-box '},
                "about-text": {parent: ' about-box '},
                "about-version": {parent: ' about-box '},
                action: {parent: ' widget ', script: true},
                alignment: {parent: ' image text textarea window '},
                author: {parent: ' widget '},
                autoHide: {parent: ' scrollbar '},
                bgColor: {parent: ' text textarea '},
                bgOpacity: {parent: ' text textarea '},
                checked: {parent: ' image menuItem '},
                clipRect: {parent: ' image '},
                color: {parent: ' about-text about-version shadow text textarea '},
                contextMenuItems: {parent: ' frame image text textarea window '},
                colorize: {parent: ' image '},
                columns: {parent: ' textarea '},
                company: {parent: ' widget '},
                copyright: {parent: ' widget '},
                data: {parent: ' about-text about-version text textarea '},
                debug: {parent: ' widget '},
                defaultValue: {parent: ' preference '},
                defaultTracking: {parent: ' widget '},
                description: {parent: ' preference '},
                directory: {parent: ' preference '},
                editable: {parent: ' textarea '},
                enabled: {parent: ' menuItem '},
                extension: {parent: ' preference '},
                file: {parent: ' action preference '},
                fillMode: {parent: ' image '},
                font: {parent: ' about-text about-version text textarea '},
                frame: {parent: ' frame window '},
                group: {parent: ' preference '},
                hAlign: {parent: ' frame image scrollbar text textarea '},
                height: {parent: ' frame image scrollbar text textarea window '},
                hidden: {parent: ' preference '},
                hLineSize: {parent: ' frame '},
                hOffset: {parent: ' about-text about-version frame image scrollbar shadow text textarea window '},
                hotkey: {parent: ' widget '},
                hRegistrationPoint: {parent: ' image '},
                hslAdjustment: {parent: ' image '},
                hslTinting: {parent: ' image '},
                hScrollBar: {parent: ' frame '},
                icon: {parent: ' preferenceGroup '},
                id: {parent: ' widget '},
                image: {parent: ' about-box frame window widget '},
                interval: {parent: ' action timer '},
                key: {parent: ' hotkey '},
                kind: {parent: ' preference '},
                level: {parent: ' window '},
                lines: {parent: ' textarea '},
                loadingSrc: {parent: ' image '},
                max: {parent: ' scrollbar '},
                maxLength: {parent: ' preference '},
                menuItem: {parent: ' contextMenuItems '},
                min: {parent: ' scrollbar '},
                minimumVersion: {parent: ' widget '},
                minLength: {parent: ' preference '},
                missingSrc: {parent: ' image '},
                modifier: {parent: ' hotkey '},
                name: {parent: ' frame hotkey image preference preferenceGroup scrollbar text textarea timer widget window '},
                notSaved: {parent: ' preference '},
                onClick: {parent: ' canvas frame image text textarea scrollbar ', script: true},
                onContextMenu: {parent: ' frame image text textarea window ', script: true},
                onDragDrop: {parent: ' frame image text textarea ', script: true},
                onDragEnter: {parent: ' frame image text textarea ', script: true},
                onDragExit: {parent: ' frame image text textarea ', script: true},
                onFirstDisplay: {parent: ' window ', script: true},
                onGainFocus: {parent: ' textarea window ', script: true},
                onKeyDown: {parent: ' hotkey text textarea ', script: true},
                onKeyPress: {parent: ' textarea ', script: true},
                onKeyUp: {parent: ' hotkey text textarea ', script: true},
                onImageLoaded: {parent: ' image ', script: true},
                onLoseFocus: {parent: ' textarea window ', script: true},
                onMouseDown: {parent: ' frame image text textarea ', script: true},
                onMouseDrag: {parent: ' canvas frame image scrollbar text textArea window ', script: true},
                onMouseEnter: {parent: ' frame image text textarea ', script: true},
                onMouseExit: {parent: ' frame image text textarea ', script: true},
                onMouseMove: {parent: ' frame image text ', script: true},
                onMouseUp: {parent: ' frame image text textarea ', script: true},
                onMouseWheel: {parent: ' frame ', script: true},
                onMultiClick: {parent: ' frame image text textarea window ', script: true},
                onSelect: {parent: ' menuItem ', script: true},
                onTimerFired: {parent: ' timer ', script: true},
                onValueChanged: {parent: ' scrollbar ', script: true},
                opacity: {parent: ' frame image scrollbar shadow text textarea window '},
                option: {parent: ' preference widget '},
                optionValue: {parent: ' preference '},
                order: {parent: ' preferenceGroup '},
                orientation: {parent: ' scrollbar '},
                pageSize: {parent: ' scrollbar '},
                preference: {parent: ' widget '},
                preferenceGroup: {parent: ' widget '},
                remoteAsync: {parent: ' image '},
                requiredPlatform: {parent: ' widget '},
                rotation: {parent: ' image '},
                scrollX: {parent: ' frame '},
                scrollY: {parent: ' frame '},
                secure: {parent: ' preference textarea '},
                scrollbar: {parent: ' text textarea window '},
                scrolling: {parent: ' text '},
                shadow: {parent: ' about-text about-version text window '},
                size: {parent: ' about-text about-version text textarea '},
                spellcheck: {parent: ' textarea '},
                src: {parent: ' image '},
                srcHeight: {parent: ' image '},
                srcWidth: {parent: ' image '},
                style: {parent: ' about-text about-version canvas frame image preference scrollbar text textarea window '},
                text: {parent: ' frame window '},
                textarea: {parent: ' frame window '},
                timer: {parent: ' widget '},
                thumbColor: {parent: ' scrollbar '},
                ticking: {parent: ' timer '},
                ticks: {parent: ' preference '},
                tickLabel: {parent: ' preference '},
                tileOrigin: {parent: ' image '},
                title: {parent: ' menuItem preference preferenceGroup window '},
                tooltip: {parent: ' image text textarea '},
                tracking: {parent: ' image '},
                trigger: {parent: ' action '},
                truncation: {parent: ' text '},
                type: {parent: ' preference '},
                url: {parent: ' about-box about-text about-version '},
                useFileIcon: {parent: ' image '},
                vAlign: {parent: ' frame image scrollbar text textarea '},
                value: {parent: ' preference scrollbar '},
                version: {parent: ' widget '},
                visible: {parent: ' frame image scrollbar text textarea window '},
                vLineSize: {parent: ' frame '},
                vOffset: {parent: ' about-text about-version frame image scrollbar shadow text textarea window '},
                vRegistrationPoint: {parent: ' image '},
                vScrollBar: {parent: ' frame '},
                width: {parent: ' frame image scrollbar text textarea window '},
                window: {parent: ' widget '},
                zOrder: {parent: ' frame image scrollbar text textarea '}
            }
        }
    };

    function xmlword(tag) {
        var w = nexttoken.value;
        if (!nexttoken.identifier) {
            if (nexttoken.id === '<') {
                error(tag ? "Expected &lt; and saw '<'" : "Missing '>'",
                        token);
            } else if (nexttoken.id === '(end)') {
                error("Bad structure");
            } else {
                warning("Missing quotes", token);
            }
        }
        advance();
        while (nexttoken.id === '-' || nexttoken.id === ':') {
            w += nexttoken.id;
            advance();
            if (!nexttoken.identifier) {
                error('Bad name: ' + w + nexttoken.value);
            }
            w += nexttoken.value;
            advance();
        }
        return w;
    }

    function xml() {
        var a;
        var e;
        var n;
        var q;
        var t;
        var v;
        xmode = 'xml';
        stack = null;
        for (;;) {
            switch (nexttoken.value) {
            case '<':
                if (!stack) {
                    stack = [];
                }
                advance('<');
                t = nexttoken;
                n = xmlword(true);
                t.name = n;
                if (!xtype) {
                    if (xmltype[n]) {
                        xmltype[n].doBegin();
                        n = xtype;
                        e = false;
                    } else {
                        error("Unrecognized <" + n + ">");
                    }
                } else {
                    if (option.cap && xtype === 'html') {
                        n = n.toLowerCase();
                    }
                    if (stack.length === 0) {
                        error("What the hell is this?", nexttoken);
                    }
                    e = xmltype[xtype].doTagName(n,
                            stack[stack.length - 1].type);
                }
                t.type = n;
                for (;;) {
                    if (nexttoken.id === '/') {
                        advance('/');
                        e = true;
                        break;
                    }
                    if (nexttoken.id && nexttoken.id.substr(0, 1) === '>') {
                        break;
                    }
                    a = xmlword();
                    switch (xmltype[xtype].doAttribute(n, a)) {
                    case 'script':
                        xmode = 'string';
                        advance('=');
                        q = nexttoken.id;
                        if (q !== '"' && q !== "'") {
                            error('Missing quote.');
                        }
                        xmode = q;
                        wmode = option.white;
                        option.white = false;
                        advance(q);
                        statements();
                        option.white = wmode;
                        if (nexttoken.id !== q) {
                            error('Missing close quote on script attribute.');
                        }
                        xmode = 'xml';
                        advance(q);
                        break;
                    case 'value':
                        advance('=');
                        if (!nexttoken.identifier &&
                                nexttoken.type !== '(string)' &&
                                nexttoken.type !== '(number)') {
                            error('Bad value: ' + nexttoken.value);
                        }
                        advance();
                        break;
                    case 'string':
                        advance('=');
                        if (nexttoken.type !== '(string)') {
                            error('Bad value: ' + nexttoken.value);
                        }
                        advance();
                        break;
                    case 'href':
                        advance('=');
                        if (nexttoken.type !== '(string)') {
                            error('Bad url: ' + nexttoken.value);
                        }
                        v = nexttoken.value.split(':');
                        if (v.length > 1) {
                            switch (v[0].substring(0, 4).toLowerCase()) {
                            case 'java':
                            case 'jscr':
                            case 'ecma':
                                warning('javascript url.');
                            }
                        }
                        advance();
                        break;
                    case 'define':
                        advance('=');
                        if (nexttoken.type !== '(string)') {
                            error('Bad value: ' + nexttoken.value);
                        }
                        addlabel(nexttoken.value, 'var*');
                        advance();
                        break;
                    default:
                        if (nexttoken.id === '=') {
                            advance('=');
                            if (!nexttoken.identifier &&
                                    nexttoken.type !== '(string)' &&
                                    nexttoken.type !== '(number)') {
                            }
                            advance();
                        }
                    }
                }
                switch (xmltype[xtype].doIt(n)) {
                case 'script':
                    xmode = 'script';
                    advance('>');
                    indent = nexttoken.from;
                    statements();
                    if (nexttoken.id !== '</') {
                        warning("Unexpected token.", nexttoken);
                    }
                    xmode = 'xml';
                    break;
                case 'special':
                    e = true;
                    n = '</' + t.name + '>';
                    if (!lex.skip(n)) {
                        error("Missing " + n, t);
                    }
                    break;
                default:
                    lex.skip('>');
                }
                if (!e) {
                    stack.push(t);
                }
                break;
            case '</':
                advance('</');
                n = xmlword(true);
                t = stack.pop();
                if (!t) {
                    error('Unexpected close tag: </' + n + '>');
                }
                if (t.name !== n) {
                    error('Expected </' + t.name +
                            '> and instead saw </' + n + '>');
                }
                if (nexttoken.id !== '>') {
                    error("Expected '>'");
                }
                if (stack.length > 0) {
                    lex.skip('>');
                } else {
                    advance('>');
                }
                break;
            case '<!':
                for (;;) {
                    advance();
                    if (nexttoken.id === '>') {
                        break;
                    }
                    if (nexttoken.id === '<' || nexttoken.id === '(end)') {
                        error("Missing '>'.", token);
                    }
                }
                lex.skip('>');
                break;
            case '<!--':
                lex.skip('-->');
                break;
            case '<%':
                lex.skip('%>');
                break;
            case '<?':
                for (;;) {
                    advance();
                    if (nexttoken.id === '?>') {
                        break;
                    }
                    if (nexttoken.id === '<?' || nexttoken.id === '<' ||
                            nexttoken.id === '>' || nexttoken.id === '(end)') {
                        error("Missing '?>'.", token);
                    }
                }
                lex.skip('?>');
                break;
            case '<=':
            case '<<':
            case '<<=':
                error("Expected '&lt;'.");
                break;
            case '(end)':
                return;
            }
            if (stack && stack.length === 0) {
                return;
            }
            if (!lex.skip('')) {
                t = stack.pop();
                error('Missing </' + t.name + '>', t);
            }
            advance();
        }
    }


// Build the syntax table by declaring the syntactic elements of the language.

    type('(number)', idValue);
    type('(string)', idValue);

    syntax['(identifier)'] = {
        type: '(identifier)',
        lbp: 0,
        identifier: true,
        nud: function () {
            var c;
            if (option.undef && !builtin(this.value) &&
                    xmode !== '"' && xmode !== "'") {
                c = funlab;
                while (!c[this.value]) {
                    c = c['(context)'];
                    if (!c) {
                        warning("Undefined " +
                                (nexttoken.id === '(' ? "function" : "variable") +
                                ": " + this.value, token);
                        break;
                    }
                }
            }
            addlabel(this.value, 'global');
            return this;
        },
        led: function () {
            error("Expected an operator and instead saw '" +
                nexttoken.value + "'.");
        }
    };

    type('(regex)', function () {
        return [this.id, this.value, this.flags];
    });

    delim('(endline)');
    delim('(begin)');
    delim('(end)').reach = true;
    delim('</').reach = true;
    delim('<![').reach = true;
    delim('<%');
    delim('<?');
    delim('<!');
    delim('<!--');
    delim('%>');
    delim('?>');
    delim('(error)').reach = true;
    delim('}').reach = true;
    delim(')');
    delim(']');
    delim(']]>').reach = true;
    delim('"').reach = true;
    delim("'").reach = true;
    delim(';');
    delim(':').reach = true;
    delim(',');
    reserve('else');
    reserve('case').reach = true;
    reserve('catch');
    reserve('default').reach = true;
    reserve('finally');
    reservevar('arguments');
    reservevar('eval');
    reservevar('false');
    reservevar('Infinity');
    reservevar('NaN');
    reservevar('null');
    reservevar('this');
    reservevar('true');
    reservevar('undefined');
    assignop('=', 'assign', 20);
    assignop('+=', 'assignadd', 20);
    assignop('-=', 'assignsub', 20);
    assignop('*=', 'assignmult', 20);
    assignop('/=', 'assigndiv', 20).nud = function () {
        warning(
                "A regular expression literal can be confused with '/='.");
    };
    assignop('%=', 'assignmod', 20);
    bitwiseassignop('&=', 'assignbitand', 20);
    bitwiseassignop('|=', 'assignbitor', 20);
    bitwiseassignop('^=', 'assignbitxor', 20);
    bitwiseassignop('<<=', 'assignshiftleft', 20);
    bitwiseassignop('>>=', 'assignshiftright', 20);
    bitwiseassignop('>>>=', 'assignshiftrightunsigned', 20);
    infix('?', function (left) {
        parse(10);
        advance(':');
        parse(10);
    }, 30);

    infix('||', 'or', 40);
    infix('&&', 'and', 50);
    bitwise('|', 'bitor', 70);
    bitwise('^', 'bitxor', 80);
    bitwise('&', 'bitand', 90);
    relation('==', function (left, right) {
        if (option.eqeqeq) {
            warning("Use '===' instead of '=='.", this);
        } else if (isPoorRelation(left)) {
            warning("Use '===' to compare with '" + left.value + "'.", this);
        } else if (isPoorRelation(right)) {
            warning("Use '===' to compare with '" + right.value + "'.", this);
        }
        return ['==', left, right];
    });
    relation('===');
    relation('!=', function (left, right) {
        var t = nexttoken;
        if (option.eqeqeq) {
            warning("Use '!==' instead of '!='.", t);
        } else if (isPoorRelation(left)) {
            warning("Use '!==' to compare with '" + left.value + "'.", this);
        } else if (isPoorRelation(right)) {
            warning("Use '!==' to compare with '" + right.value + "'.", this);
        }
        return ['!=', left, right];
    });
    relation('!==');
    relation('<');
    relation('>');
    relation('<=');
    relation('>=');
    bitwise('<<', 'shiftleft', 120);
    bitwise('>>', 'shiftright', 120);
    bitwise('>>>', 'shiftrightunsigned', 120);
    infix('in', 'in', 120);
    infix('instanceof', 'instanceof', 120);
    infix('+', 'addconcat', 130);
    prefix('+', 'num');
    infix('-', 'sub', 130);
    prefix('-', 'neg');
    infix('*', 'mult', 140);
    infix('/', 'div', 140);
    infix('%', 'mod', 140);

    suffix('++', 'postinc');
    prefix('++', 'preinc');
    syntax['++'].exps = true;

    suffix('--', 'postdec');
    prefix('--', 'predec');
    syntax['--'].exps = true;
    prefix('delete', function () {
        parse(0);
    }).exps = true;


    prefix('~', function () {
        if (option.bitwise) {
            warning("Unexpected bitwise operator.", this);
        }
        parse(150);
        return this;
    });
    prefix('!', 'not');
    prefix('typeof', 'typeof');
    prefix('new', function () {
        var c = parse(155);
        var i;
        if (c) {
            if (c.identifier) {
                c['new'] = true;
                switch (c.value) {
                case 'Object':
                    warning('Use the object literal notation {}.', token);
                    break;
                case 'Array':
                    warning('Use the array literal notation [].', token);
                    break;
                case 'Number':
                case 'String':
                case 'Boolean':
                    warning("Do not use the " + c.value +
                        " function as a constructor.", token);
                    break;
                case 'Function':
                    if (!option.evil) {
                        warning('The Function constructor is eval.');
                    }
                    break;
                default:
                    if (c.id !== 'function') {
                        i = c.value.substr(0, 1);
                        if (i < 'A' || i > 'Z') {
                            warning(
                    'A constructor name should start with an uppercase letter.',
                                token);
                        }
                    }
                }
            } else {
                if (c.id !== '.' && c.id !== '[' && c.id !== '(') {
                    warning('Bad constructor', token);
                }
            }
        } else {
            warning("Weird construction. Delete 'new'.", this);
        }
        adjacent(token, nexttoken);
        if (nexttoken.id === '(') {
            advance('(');
            if (nexttoken.id !== ')') {
                for (;;) {
                    parse(10);
                    if (nexttoken.id !== ',') {
                        break;
                    }
                    advance(',');
                }
            }
            advance(')');
        } else {
            warning("Missing '()' invoking a constructor.");
        }
        return syntax['function'];
    });
    syntax['new'].exps = true;

    infix('.', function (left) {
        adjacent(prevtoken, token);
        var m = identifier();
        if (typeof m === 'string') {
            countMember(m);
        }
        if (!option.evil && left && left.value === 'document' &&
                (m === 'write' || m === 'writeln')) {
            warning("document.write can be a form of eval.", left);
        }
        this.left = left;
        this.right = m;
        return this;
    }, 160);

    infix('(', function (left) {
        adjacent(prevtoken, token);
        var n = 0;
        var p = [];
        if (left && left.type === '(identifier)') {
            if (left.value.match(/^[A-Z](.*[a-z].*)?$/)) {
                if (left.value !== 'Number' && left.value !== 'String' &&
                        left.value !== 'Date') {
                    warning("Missing 'new' prefix when invoking a constructor",
                            left);
                }
            }
        }
        if (nexttoken.id !== ')') {
            for (;;) {
                p[p.length] = parse(10);
                n += 1;
                if (nexttoken.id !== ',') {
                    break;
                }
                advance(',');
            }
        }
        advance(')');
        if (typeof left === 'object') {
            if (left.value === 'parseInt' && n === 1) {
                warning("Missing radix parameter", left);
            }
            if (!option.evil) {
                if (left.value === 'eval' || left.value === 'Function') {
                    if (    p[0][0] !== "addconcat" ||
                            p[0][2].value !== ')' ||
                            p[0][1][0] !== "addconcat" ||
                            p[0][1][1].value !== '(') {
                        warning("eval is evil", left);
                    }
                } else if (p[0] && p[0].id === '(string)' &&
                       (left.value === 'setTimeout' ||
                        left.value === 'setInterval')) {
                    warning(
    "Implied eval is evil. Use a function argument instead of a string", left);
                }
            }
            if (!left.identifier && left.id !== '.' &&
                    left.id !== '[' && left.id !== '(') {
                warning('Bad invocation.', left);
            }

        }
        return syntax['function'];
    }, 155).exps = true;

    prefix('(', function () {
        var v = parse(0);
        advance(')', this);
        return v;
    });

    infix('[', function (left) {
        var e = parse(0), s;
        if (e && e.type === '(string)') {
            countMember(e.value);
            if (ix.test(e.value)) {
                s = syntax[e.value];
                if (!s || !s.reserved) {
                    warning("This is better written in dot notation.", e);
                }
            }
        }
        advance(']', this);
        this.left = left;
        this.right = e;
        return this;
    }, 160);

    prefix('[', function () {
        if (nexttoken.id === ']') {
            advance(']');
            return;
        }
        var b = token.line !== nexttoken.line;
        if (b) {
            indent += 4;
        }
        for (;;) {
            if (b) {
                indentation();
            }
            parse(10);
            if (nexttoken.id === ',') {
                adjacent(token, nexttoken);
                advance(',');
                if (nexttoken.id === ',' || nexttoken.id === ']') {
                    warning("Extra comma.", token);
                }
                nonadjacent(token, nexttoken);
            } else {
                if (b) {
                    indent -= 4;
                    indentation();
                }
                advance(']', this);
                return;
            }
        }
    }, 160);

    (function (x) {
        x.nud = function () {
            var i;
            if (nexttoken.id === '}') {
                advance('}');
                return;
            }
            var b = token.line !== nexttoken.line;
            if (b) {
                indent += 4;
            }
            for (;;) {
                if (b) {
                    indentation();
                }
                i = optionalidentifier(true);
                if (!i && (nexttoken.id === '(string)' ||
                           nexttoken.id === '(number)')) {
                    i = nexttoken.id;
                    advance();
                }
                if (!i) {
                    error("Expected an identifier or '}' and instead saw '" +
                            nexttoken.value + "'.");
                }
                if (typeof i.value === 'string') {
                    countMember(i.value);
                }
                adjacent(token, nexttoken);
                advance(':');
                nonadjacent(token, nexttoken);
                parse(10);
                if (nexttoken.id === ',') {
                    adjacent(token, nexttoken);
                    advance(',');
                    if (nexttoken.id === ',' || nexttoken.id === '}') {
                        warning("Extra comma.", token);
                    }
                    nonadjacent(token, nexttoken);
                } else {
                    if (b) {
                        indent -= 4;
                        indentation();
                    }
                    advance('}', this);
                    return;
                }
            }
        };
        x.fud = function () {
            error("Expected to see a statement and instead saw a block.");
        };
    })(delim('{'));


    function varstatement() {

// JavaScript does not have block scope. It only has function scope. So,
// declaring a variable in a block can have unexpected consequences. We
// will keep an inblock flag, which will be set when we enter a block, and
// cleared when we enter a function.

        var c, i, n, v;
        if (inblock) {
            warning("Variable '" + nexttoken.value + "' declared in a block.");
        }
        for (;;) {
            nonadjacent(token, nexttoken);
            n = identifier();
            if (!option.redef) {
                for (i = funstack.length - 1; i >= 0; i -= 1) {
                    if (funstack[i][n]) {
                        warning("Redefinition of '" + n + "'.", token);
                        break;
                    }
                }
            }
            addlabel(n, 'var');
            if (nexttoken.id === '=') {
                v = token;
                nonadjacent(token, nexttoken);
                advance('=');
                nonadjacent(token, nexttoken);
                if (nexttoken.id === '[' || nexttoken.id === '{' ||
                        nexttoken.id === 'function') {
                    if (c && option.white) {
                        warning("Give '" + v.value +
                                "' its own var statement.", v);
                        option.white = false;
                        parse(20);
                        option.white = true;
                    } else {
                        parse(20);
                    }
                    if (nexttoken.id === ',' && option.white) {
                        v = peek(0);
                        warning("Give '" + v.value +
                            "' its own var statement.", v);
                    }
                } else {
                    parse(20);
                }
            }
            if (nexttoken.id !== ',') {
                return;
            }
            adjacent(token, nexttoken);
            advance(',');
            nonadjacent(token, nexttoken);
            c = true;
        }
    }


    stmt('var', varstatement);

    stmt('new', function () {
        error("'new' should not be used as a statement.");
    });


    function functionparams() {
        var t = nexttoken;
        advance('(');
        if (nexttoken.id === ')') {
            advance(')');
            return;
        }
        for (;;) {
            addlabel(identifier(), 'parameter');
            if (nexttoken.id === ',') {
                advance(',');
            } else {
                advance(')', t);
                return;
            }
        }
    }


    blockstmt('function', function () {
        var i = identifier();
        if (inblock) {
            warning("function '" + i + "' declared in a block.", token);
        }
        addlabel(i, 'var*');
        beginfunction(i);
        addlabel(i, 'function');
        adjacent(token, nexttoken);
        functionparams();
        block(false);
        endfunction();
        if (nexttoken.id === '(' && nexttoken.line === token.line) {
            error(
"Function statements are not invocable. Wrap the function expression in parens.");
        }
    });

    prefix('function', function () {
        var i = optionalidentifier();
        if (i) {
            adjacent(token, nexttoken);
        } else {
            nonadjacent(token, nexttoken);
            i = '"' + anonname + '"';
        }
        beginfunction(i);
        addlabel(i, 'function');
        functionparams();
        block(false);
        endfunction();
    });

    blockstmt('if', function () {
        var t = nexttoken;
        advance('(');
        nonadjacent(this, t);
        parse(20);
        if (nexttoken.id === '=') {
            warning("Assignment in control part.");
            advance('=');
            parse(20);
        }
        advance(')', t);
        block(true);
        if (nexttoken.id === 'else') {
            nonadjacent(token, nexttoken);
            advance('else');
            if (nexttoken.id === 'if' || nexttoken.id === 'switch') {
                statement();
            } else {
                block(true);
            }
        }
    });

    blockstmt('try', function () {
        var b;
        block(true);
        if (nexttoken.id === 'catch') {
            advance('catch');
            beginfunction('"catch"');
            functionparams();
            block(true);
            endfunction();
            b = true;
        }
        if (nexttoken.id === 'finally') {
            advance('finally');
            beginfunction('"finally"');
            block(true);
            endfunction();
            return;
        } else if (!b) {
            error("Expected 'catch' or 'finally' and instead saw '" +
                    nexttoken.value + "'.");
        }
    });

    blockstmt('while', function () {
        var t = nexttoken;
        advance('(');
        nonadjacent(this, t);
        parse(20);
        if (nexttoken.id === '=') {
            warning("Assignment in control part.");
            advance('=');
            parse(20);
        }
        advance(')', t);
        block(true);
    }).labelled = true;

    reserve('with');

    blockstmt('switch', function () {
        var t = nexttoken;
        var g = false;
        advance('(');
        nonadjacent(this, t);
        this.condition = parse(20);
        advance(')', t);
        nonadjacent(token, nexttoken);
        t = nexttoken;
        advance('{');
        nonadjacent(token, nexttoken);
        indent += 4;
        this.cases = [];
        for (;;) {
            switch (nexttoken.id) {
            case 'case':
                switch (verb) {
                case 'break':
                case 'case':
                case 'continue':
                case 'return':
                case 'switch':
                case 'throw':
                    break;
                default:
                    warning(
                        "Expected a 'break' statement before 'case'.",
                        token);
                }
                indentation(-4);
                advance('case');
                this.cases.push(parse(20));
                g = true;
                advance(':');
                verb = 'case';
                break;
            case 'default':
                switch (verb) {
                case 'break':
                case 'continue':
                case 'return':
                case 'throw':
                    break;
                default:
                    warning(
                        "Expected a 'break' statement before 'default'.",
                        token);
                }
                indentation(-4);
                advance('default');
                g = true;
                advance(':');
                break;
            case '}':
                indent -= 4;
                indentation();
                advance('}', t);
                if (this.cases.length === 1 || this.condition.id === 'true' ||
                        this.condition.id === 'false') {
                    warning("The switch should be an if", this);
                }
                return;
            case '(end)':
                error("Missing '}'.");
                return;
            default:
                if (g) {
                    switch (token.id) {
                    case ',':
                        error("Each value should have its own case label.");
                        return;
                    case ':':
                        statements();
                        break;
                    default:
                        error("Missing ':' on a case clause.", token);
                    }
                } else {
                    error("Expected to see 'case' and instead saw '" +
                        nexttoken.value + "'.");
                }
            }
        }
    }).labelled = true;

    stmt('debugger', function () {
        if (!option.debug) {
            warning("All debugger statements should be removed.");
        }
    });

    stmt('do', function () {
        block(true);
        advance('while');
        var t = nexttoken;
        nonadjacent(token, t);
        advance('(');
        parse(20);
        advance(')', t);
    }).labelled = true;

    blockstmt('for', function () {
        var t = nexttoken;
        advance('(');
        nonadjacent(this, t);
        if (peek(nexttoken.id === 'var' ? 1 : 0).id === 'in') {
            if (nexttoken.id === 'var') {
                advance('var');
                addlabel(identifier(), 'var');
            } else {
                advance();
            }
            advance('in');
            parse(20);
            advance(')', t);
            block(true);
            return;
        } else {
            if (nexttoken.id !== ';') {
                if (nexttoken.id === 'var') {
                    advance('var');
                    varstatement();
                } else {
                    for (;;) {
                        parse(0);
                        if (nexttoken.id !== ',') {
                            break;
                        }
                        advance(',');
                    }
                }
            }
            advance(';');
            if (nexttoken.id !== ';') {
                parse(20);
            }
            advance(';');
            if (nexttoken.id === ';') {
                error("Expected to see ')' and instead saw ';'.");
            }
            if (nexttoken.id !== ')') {
                for (;;) {
                    parse(0);
                    if (nexttoken.id !== ',') {
                        break;
                    }
                    advance(',');
                }
            }
            advance(')', t);
            block(true);
        }
    }).labelled = true;


    function nolinebreak(t) {
        if (t.line !== nexttoken.line) {
            warning("Statement broken badly.", t);
        }
    }


    stmt('break', function () {
        nolinebreak(this);
        if (funlab[nexttoken.value] === 'live*') {
            advance();
        }
        reachable('break');
    });


    stmt('continue', function () {
        nolinebreak(this);
        if (funlab[nexttoken.value] === 'live*') {
            advance();
        }
        reachable('continue');
    });


    stmt('return', function () {
        nolinebreak(this);
        if (nexttoken.id !== ';' && !nexttoken.reach) {
            nonadjacent(token, nexttoken);
            parse(20);
        }
        reachable('return');
    });


    stmt('throw', function () {
        nolinebreak(this);
        nonadjacent(token, nexttoken);
        parse(20);
        reachable('throw');
    });


//  Superfluous reserved words

    reserve('abstract');
    reserve('boolean');
    reserve('byte');
    reserve('char');
    reserve('class');
    reserve('const');
    reserve('double');
    reserve('enum');
    reserve('export');
    reserve('extends');
    reserve('final');
    reserve('float');
    reserve('goto');
    reserve('implements');
    reserve('import');
    reserve('int');
    reserve('interface');
    reserve('long');
    reserve('native');
    reserve('package');
    reserve('private');
    reserve('protected');
    reserve('public');
    reserve('short');
    reserve('static');
    reserve('super');
    reserve('synchronized');
    reserve('throws');
    reserve('transient');
    reserve('void');
    reserve('volatile');


// The actual JSLINT function itself.

    var itself = function (s, o) {
        option = o || {};
        JSLINT.errors = [];
        globals = {};
        functions = [];
        xmode = false;
        xtype = '';
        stack = null;
        funlab = {};
        member = {};
        funstack = [];
        lookahead = [];
        inblock = false;
        indent = 0;
        warnings = 0;
        lex.init(s);

        prevtoken = token = nexttoken = syntax['(begin)'];
        try {
            advance();
            if (nexttoken.value.charAt(0) === '<') {
                xml();
            } else {
                statements();
            }
            advance('(end)');
        } catch (e) {
            if (e) {
                JSLINT.errors.push({
                    reason: "JSLint error: " + e.description,
                    line: nexttoken.line,
                    character: nexttoken.from,
                    evidence: nexttoken.value
                });
            }
        }
        return JSLINT.errors.length === 0;
    };


// Report generator.

    itself.report = function (option) {
        var a = [];
        var c;
        var cc;
        var f;
        var i;
        var k;
        var o = [];
        var s;

        function detail(h) {
            if (s.length) {
                o.push('<div>' + h + ':&nbsp; ' + s.sort().join(', ') +
                    '</div>');
            }
        }

        k = JSLINT.errors.length;
        if (k) {
            o.push(
                '<div id=errors>Error:<blockquote>');
            for (i = 0; i < k; i += 1) {
                c = JSLINT.errors[i];
                if (c) {
                    o.push('<p>Problem at line ' + (c.line + 1) +
                            ' character ' + (c.character + 1) +
                            ': ' + c.reason.entityify() + '</p><p><tt>' +
                            (c.evidence.length > 80 ?
                            c.evidence.substring(0, 77) + '...' :
                            c.evidence).entityify() + '</tt></p>');
                }
            }
            o.push('</blockquote></div>');
            if (!c) {
                return o.join('');
            }
        }

        if (!option) {
            for (k in member) {
                a.push(k);
            }
            if (a.length) {
                a = a.sort();
                o.push(
                 '<table><tbody><tr><th>Members</th><th>Occurrences</th></tr>');
                for (i = 0; i < a.length; i += 1) {
                    o.push('<tr><td><tt>', a[i], '</tt></td><td>', member[a[i]],
                            '</td></tr>');
                }
                o.push('</tbody></table>');
            }
            for (i = 0; i < functions.length; i += 1) {
                f = functions[i];
                for (k in f) {
                    if (f[k] === 'global') {
                        c = f['(context)'];
                        for (;;) {
                            cc = c['(context)'];
                            if (!cc) {
                                if ((!funlab[k] || funlab[k] === 'var?') &&
                                        !builtin(k)) {
                                    funlab[k] = 'var?';
                                    f[k] = 'global?';
                                }
                                break;
                            }
                            if (c[k] === 'parameter!' || c[k] === 'var!') {
                                f[k] = 'var.';
                                break;
                            }
                            if (c[k] === 'var' || c[k] === 'var*' ||
                                    c[k] === 'var!') {
                                f[k] = 'var.';
                                c[k] = 'var!';
                                break;
                            }
                            if (c[k] === 'parameter') {
                                f[k] = 'var.';
                                c[k] = 'parameter!';
                                break;
                            }
                            c = cc;
                        }
                    }
                }
            }
            s = [];
            for (k in funlab) {
                c = funlab[k];
                if (typeof c === 'string' && c.substr(0, 3) === 'var') {
                    if (c === 'var?') {
                        s.push('<tt>' + k + '</tt><small>&nbsp;(?)</small>');
                    } else {
                        s.push('<tt>' + k + '</tt>');
                    }
                } else {
                    if (c === 'global' && !builtin(k)) {
                        s.push('<tt>' + k + '</tt><small>&nbsp;(?)</small>');
                    }
                }
            }
            detail('Global');
            if (functions.length) {
                o.push('<br>Function:<ol style="padding-left:0.5in">');
            }
            for (i = 0; i < functions.length; i += 1) {
                f = functions[i];
                o.push('<li value=' +
                        f['(line)'] + '><tt>' + (f['(name)'] || '') + '</tt>');
                s = [];
                for (k in f) {
                    if (k.charAt(0) !== '(') {
                        switch (f[k]) {
                        case 'parameter':
                            s.push('<tt>' + k + '</tt>');
                            break;
                        case 'parameter!':
                            s.push('<tt>' + k +
                                    '</tt><small>&nbsp;(closure)</small>');
                            break;
                        }
                    }
                }
                detail('Parameter');
                s = [];
                for (k in f) {
                    if (k.charAt(0) !== '(') {
                        switch (f[k]) {
                        case 'var':
                            s.push('<tt>' + k +
                                    '</tt><small>&nbsp;(unused)</small>');
                            break;
                        case 'var*':
                            s.push('<tt>' + k + '</tt>');
                            break;
                        case 'var!':
                            s.push('<tt>' + k +
                                    '</tt><small>&nbsp;(closure)</small>');
                            break;
                        case 'var.':
                            s.push('<tt>' + k +
                                    '</tt><small>&nbsp;(outer)</small>');
                            break;
                        }
                    }
                }
                detail('Var');
                s = [];
                c = f['(context)'];
                for (k in f) {
                    if (k.charAt(0) !== '(' && f[k].substr(0, 6) === 'global') {
                        if (f[k] === 'global?') {
                            s.push('<tt>' + k +
                                    '</tt><small>&nbsp;(?)</small>');
                        } else {
                            s.push('<tt>' + k + '</tt>');
                        }
                    }
                }
                detail('Global');
                s = [];
                for (k in f) {
                    if (k.charAt(0) !== '(' && f[k] === 'label') {
                        s.push(k);
                    }
                }
                detail('Label');
                o.push('</li>');
            }
            if (functions.length) {
                o.push('</ol>');
            }
        }
        return o.join('');
    };

    return itself;

}();