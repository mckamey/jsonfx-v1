@ECHO OFF
ECHO.
ECHO BEGIN %0

:: Check for parameters
REM IF [%1]==[] (
REM 	GOTO HANDLE_ERROR
REM )

:: -------------------------------------------------------------------
:: Change directory to same as batch file
SETLOCAL
PUSHD "%~d0%~p0"

:: -------------------------------------------------------------------
ECHO.
"..\SolnItems\AutoVersioning\AutoVersioning.exe" "Properties\AssemblyVersion.cs"

:: -------------------------------------------------------------------
:: Compact each individual file
ECHO.

SET INFO="Copyright (c) 2006-2007 Stephen M. McKamey, http://JsonFx.net"
SET TIME="'Version 1.0.'yyMM'.'ddHH"
SET JSONML=..\JsonML
SET ROOT=Scripts
SET OUTDIR=%ROOT%\Compacted
SET CONCAT="..\SolnItems\FileConcat\FileConcat.exe"
SET COMPACTLITE="..\SolnItems\ScriptCompactor\ScriptCompactor.exe"
SET COMPACT=%COMPACTLITE% /INFO:%INFO% /TIME:%TIME%

%COMPACTLITE% /IN:"%ROOT%\JSON.js" /OUT:"%OUTDIR%\JSON.js"
%COMPACTLITE% /IN:"%ROOT%\JsonML.js" /OUT:"%OUTDIR%\JsonML.js"
FOR /F %%F IN (%ROOT%\Compact.txt) DO (
	%COMPACT% /IN:"%ROOT%\%%F" /OUT:"%OUTDIR%\%%F"
)

:: -------------------------------------------------------------------
:: Concat and compact the core set of files
ECHO.
SET CONCATLIST="%ROOT%\JsonFx.Core.js"
FOR /F %%F IN (%ROOT%\Concat.txt) DO (
	CALL :APPEND_LIST %ROOT%\%%F
)

%CONCAT% %CONCATLIST%
%COMPACT% /IN:"%ROOT%\JsonFx.Core.js" /OUT:"%OUTDIR%\JsonFx.Core.js" /WARNING

:: -------------------------------------------------------------------
:: Copy JsonML scripts over to JsonML project
ECHO.
XCOPY "JsonML.js" "%JSONML%\JsonML.js" /Y /R
XCOPY "Compacted\JsonML.js" "%JSONML%\JsonML_min.js" /Y /R

POPD
ENDLOCAL

GOTO DONE

:: -------------------------------------------------------------------
:: SubRoutine which appends param to the file list
:APPEND_LIST
SET CONCATLIST=%CONCATLIST% "%1"
GOTO :EOF

:: -------------------------------------------------------------------
:HANDLE_ERROR
ECHO Placeholder error message...
ECHO.

:: -------------------------------------------------------------------
:DONE
ECHO END %0
