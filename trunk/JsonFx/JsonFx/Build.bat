@ECHO OFF
If "%1" == "" GOTO HANDLE_ERROR
If "%2" == "" GOTO HANDLE_ERROR

%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JSON.js" "%2Scripts\Compacted\JSON.js" "http://json.org/js.html" -F
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JsonML.js" "%2Scripts\Compacted\JsonML.js" "http://JsonML.org"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Core.js" "%2Scripts\Compacted\Core.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\IO.js" "%2Scripts\Compacted\IO.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\UI.js" "%2Scripts\Compacted\UI.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Utility.js" "%2Scripts\Compacted\Utility.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\UA.js" "%2Scripts\Compacted\UA.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Trace.js" "%2Scripts\Compacted\Trace.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net"

xcopy "%2Scripts\JsonML.js" "%2..\JsonML\JsonML.js" /Y /R
xcopy "%2Scripts\Compacted\JsonML.js" "%2..\JsonML\JsonML_min.js" /Y /R

%1FileConcat\FileConcat.exe "%2Scripts\JsonFx.js" "%2Scripts\Core.js" "%2Scripts\JSON.js" "%2Scripts\JsonML.js" "%2Scripts\IO.js" "%2Scripts\UI.js"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JsonFx.js" "%2Scripts\Compacted\JsonFx.js" "Copyright (c)2006-2007 Stephen M. McKamey. http://JsonFx.net" -F

GOTO DONE

:HANDLE_ERROR
echo Usage: Build.bat ScriptCompactorPath ProjectDir
echo e.g. $(ProjectDir)Build.bat $(SolutionDir)SolnItems\ScriptCompactor\ScriptCompactor.exe $(ProjectDir)
echo

:DONE
