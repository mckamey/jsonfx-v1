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

SET INFO="Copyright (c)2006-2008 Stephen M. McKamey, http://JsonFx.net"
SET TIME="'Version 1.0.'yyMM'.'ddHH"
SET ROOT=Scripts
SET OUTDIR=%ROOT%\Compacted
SET CONCAT="..\SolnItems\FileConcat\FileConcat.exe"
SET COMPACTLITE="..\SolnItems\ScriptCompactor\ScriptCompactor.exe"
SET COMPACT=%COMPACTLITE% /INFO:%INFO% /TIME:%TIME%

IF NOT EXIST %COMPACTLITE% (
	GOTO DONE
)
IF NOT EXIST %CONCAT% (
	GOTO DONE
)

%COMPACTLITE% /IN:"%ROOT%\json2.js" /OUT:"%OUTDIR%\json2.js"
%COMPACTLITE% /IN:"%ROOT%\JsonML2.js" /OUT:"%OUTDIR%\JsonML2.js"
%COMPACTLITE% /IN:"%ROOT%\JBST.js" /OUT:"%OUTDIR%\JBST.js"
FOR /F %%F IN (%ROOT%\Compact.txt) DO (
	%COMPACT% /IN:"%ROOT%\%%F" /OUT:"%OUTDIR%\%%F"
)

:: -------------------------------------------------------------------
:: Concat and compact the core library
ECHO.
SET CONCATLIST="%ROOT%\JsonFx.Core.js"
FOR /F %%F IN (%ROOT%\Concat.txt) DO (
	CALL :APPEND_LIST %ROOT%\%%F
)

%CONCAT% %CONCATLIST%
%COMPACT% /IN:"%ROOT%\JsonFx.Core.js" /OUT:"%OUTDIR%\JsonFx.Core.js" /WARNING

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
