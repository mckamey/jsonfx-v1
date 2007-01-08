@ECHO OFF
If "%1" == "" GOTO HANDLE_ERROR
If "%2" == "" GOTO HANDLE_ERROR

%1 "%2Scripts\Core.js" "%2Scripts\Compacted\Core.js" "Copyright (c)2006 Stephen M. McKamey. All rights reserved."
%1 "%2Scripts\IO.js" "%2Scripts\Compacted\IO.js" "Copyright (c)2006 Stephen M. McKamey. All rights reserved."
%1 "%2Scripts\JsonML.js" "%2Scripts\Compacted\JsonML.js" "http://JsonML.org" -F
%1 "%2Scripts\UI.js" "%2Scripts\Compacted\UI.js" "Copyright (c)2006 Stephen M. McKamey. All rights reserved."
%1 "%2Scripts\JSON.js" "%2Scripts\Compacted\JSON.js" "http://json.org/js.html" -F
%1 "%2Scripts\Trace.js" "%2Scripts\Compacted\Trace.js" "Copyright (c)2006 Stephen M. McKamey. All rights reserved."
%1 "%2Scripts\Utility.js" "%2Scripts\Compacted\Utility.js" "Copyright (c)2006 Stephen M. McKamey. All rights reserved." -F

xcopy "%2Scripts\JsonML.js" "%2..\JsonML\JsonML.js" /Y /R
xcopy "%2Scripts\Compacted\JsonML.js" "%2..\JsonML\JsonML_min.js" /Y /R

GOTO DONE

:HANDLE_ERROR
echo Usage: Build.bat ScriptCompactorPath ProjectDir
echo e.g. $(ProjectDir)Build.bat $(SolutionDir)SolnItems\ScriptCompactor\ScriptCompactor.exe $(ProjectDir)
echo

:DONE
