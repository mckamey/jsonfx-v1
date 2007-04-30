rem @ECHO OFF
If "%1" == "" GOTO HANDLE_ERROR
If "%2" == "" GOTO HANDLE_ERROR

%1AutoVersioning\AutoVersioning.exe "%2Properties\AssemblyVersion.cs"

set copyright="Copyright (c) 2006-2007 Stephen M. McKamey, http://JsonFx.net"
set timestamp="'Version 1.0.'yyMM'.'ddHH"

%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\JSON.js" /OUT:"%2Scripts\Compacted\JSON.js"
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\JsonML.js" /OUT:"%2Scripts\Compacted\JsonML.js"
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\Core.js" /OUT:"%2Scripts\Compacted\Core.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\IO.js" /OUT:"%2Scripts\Compacted\IO.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\UI.js" /OUT:"%2Scripts\Compacted\UI.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\UA.js" /OUT:"%2Scripts\Compacted\UA.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\Utility.js" /OUT:"%2Scripts\Compacted\Utility.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\ServiceTest.js" /OUT:"%2Scripts\Compacted\ServiceTest.js" /INFO:%copyright% /TIME:%timestamp%
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\Trace.js" /OUT:"%2Scripts\Compacted\Trace.js" /INFO:%copyright% /TIME:%timestamp%

xcopy "%2Scripts\JsonML.js" "%2..\JsonML\JsonML.js" /Y /R
xcopy "%2Scripts\Compacted\JsonML.js" "%2..\JsonML\JsonML_min.js" /Y /R

%1FileConcat\FileConcat.exe "%2Scripts\JsonFx.js" "%2Scripts\Core.js" "%2Scripts\JSON.js" "%2Scripts\JsonML.js" "%2Scripts\UA.js" "%2Scripts\IO.js" "%2Scripts\UI.js"
%1ScriptCompactor\ScriptCompactor.exe /IN:"%2Scripts\JsonFx.js" /OUT:"%2Scripts\Compacted\JsonFx.js" /INFO:%copyright% /TIME:%timestamp%

GOTO DONE

:HANDLE_ERROR
echo Usage: Build.bat ScriptCompactorPath ProjectDir
echo e.g. $(ProjectDir)Build.bat $(SolutionDir)SolnItems\ScriptCompactor\ScriptCompactor.exe $(ProjectDir)
echo

:DONE
