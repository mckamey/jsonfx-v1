@ECHO OFF
If "%1" == "" GOTO HANDLE_ERROR
If "%2" == "" GOTO HANDLE_ERROR

%1AutoVersioning\AutoVersioning.exe "%2Properties\AssemblyVersion.cs"

set copyright="Copyright (c)2006-2007 http://JsonFx.net"
set timestamp="'Version 1.0.'yyMM'.'ddHH"

%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JSON.js" "%2Scripts\Compacted\JSON.js" "" "" -F
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JsonML.js" "%2Scripts\Compacted\JsonML.js" "" ""
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Core.js" "%2Scripts\Compacted\Core.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\IO.js" "%2Scripts\Compacted\IO.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\UI.js" "%2Scripts\Compacted\UI.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\UA.js" "%2Scripts\Compacted\UA.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Utility.js" "%2Scripts\Compacted\Utility.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\ServiceTest.js" "%2Scripts\Compacted\ServiceTest.js" %copyright% %timestamp%
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\Trace.js" "%2Scripts\Compacted\Trace.js" %copyright% %timestamp%

xcopy "%2Scripts\JsonML.js" "%2..\JsonML\JsonML.js" /Y /R
xcopy "%2Scripts\Compacted\JsonML.js" "%2..\JsonML\JsonML_min.js" /Y /R

%1FileConcat\FileConcat.exe "%2Scripts\JsonFx.js" "%2Scripts\Core.js" "%2Scripts\JSON.js" "%2Scripts\JsonML.js" "%2Scripts\UA.js" "%2Scripts\IO.js" "%2Scripts\UI.js"
%1ScriptCompactor\ScriptCompactor.exe "%2Scripts\JsonFx.js" "%2Scripts\Compacted\JsonFx.js" %copyright% %timestamp% -F

GOTO DONE

:HANDLE_ERROR
echo Usage: Build.bat ScriptCompactorPath ProjectDir
echo e.g. $(ProjectDir)Build.bat $(SolutionDir)SolnItems\ScriptCompactor\ScriptCompactor.exe $(ProjectDir)
echo

:DONE
