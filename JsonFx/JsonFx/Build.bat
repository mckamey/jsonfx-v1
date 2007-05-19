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
PUSHD "Scripts\"

SET INFO="Copyright (c) 2006-2007 Stephen M. McKamey, http://JsonFx.net"
SET TIME="'Version 1.0.'yyMM'.'ddHH"
SET JSONML=..\..\JsonML
SET OUTDIR=Compacted
SET CONCAT="..\..\SolnItems\FileConcat\FileConcat.exe"
SET COMPACTLITE="..\..\SolnItems\ScriptCompactor\ScriptCompactor.exe"
SET COMPACT=%COMPACTLITE% /INFO:%INFO% /TIME:%TIME%

%COMPACTLITE% /IN:"JSON.js" /OUT:"%OUTDIR%\JSON.js"
%COMPACTLITE% /IN:"JsonML.js" /OUT:"%OUTDIR%\JsonML.js"
FOR /F %%F IN (Compact.txt) DO (
	%COMPACT% /IN:"%%F" /OUT:"%OUTDIR%\%%F"
)

:: -------------------------------------------------------------------
:: Concat and compact the entire set of files
ECHO.
SET CONCATLIST="JsonFx.js"
FOR /F %%F IN (Concat.txt) DO (
	CALL :APPEND_LIST %%F
)

%CONCAT% %CONCATLIST%
%COMPACT% /IN:"JsonFx.js" /OUT:"Compacted\JsonFx.js" /WARNING

:: -------------------------------------------------------------------
:: Copy JsonML scripts over to JsonML project
ECHO.
XCOPY "JsonML.js" "%JSONML%\JsonML.js" /Y /R
XCOPY "Compacted\JsonML.js" "%JSONML%\JsonML_min.js" /Y /R

POPD
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
