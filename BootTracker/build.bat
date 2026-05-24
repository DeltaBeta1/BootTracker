@echo off
setlocal enabledelayedexpansion
title Building Boot Tracker

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "SRC=%~dp0src"
set "OUT=%~dp0BootTracker.exe"
set "TARGET=exe"

echo ============================================
echo   Building Boot Tracker
echo ============================================
echo.

if not exist "%CSC%" (
    echo [FAIL] csc.exe not found at %CSC%
    pause
    exit /b 1
)

echo Compiler: %CSC%
echo Source:   %SRC%
echo Output:   %OUT%
echo.

:: Collect all .cs files
set "FILES="
for /r "%SRC%" %%f in (*.cs) do (
    set "FILES=!FILES! "%%f""
)

echo Source files:
for /r "%SRC%" %%f in (*.cs) do echo   %%~nxf

echo.
echo Compiling...

"%CSC%" ^
    /target:winexe ^
    /out:"%OUT%" ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Data.dll ^
    /reference:System.Xml.dll ^
    /reference:System.Windows.Forms.DataVisualization.dll ^
    /reference:Microsoft.VisualBasic.dll ^
    /nowarn:CS0414,CS0169,CS0219 ^
    %FILES%

if %errorlevel% equ 0 (
    echo.
    echo ============================================
    echo   Build SUCCESS
    echo   Output: %OUT%
    echo ============================================

    :: Copy sqlite3.dll to output
    if exist "%~dp0lib\sqlite3.dll" (
        copy /y "%~dp0lib\sqlite3.dll" "%~dp0sqlite3.dll" >nul
        echo   sqlite3.dll copied to output
    )
) else (
    echo.
    echo ============================================
    echo   Build FAILED (error code: %errorlevel%)
    echo ============================================
)

echo.
pause
