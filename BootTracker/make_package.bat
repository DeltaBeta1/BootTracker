@echo off
chcp 65001 >nul
echo ============================================
echo   Boot Tracker - Package Builder
echo ============================================
echo.

:: Step 1: Build
echo [1/3] Building...
call "%~dp0build.bat"
if %errorlevel% neq 0 (
    echo Build failed. Aborting package.
    pause
    exit /b 1
)

:: Step 2: Create package directory
echo.
echo [2/3] Creating package structure...
set "PKG_DIR=%~dp0package\BootTracker"
if exist "%PKG_DIR%" rmdir /s /q "%PKG_DIR%"
mkdir "%PKG_DIR%"

copy /y "%~dp0BootTracker.exe" "%PKG_DIR%\" >nul
copy /y "%~dp0sqlite3.dll" "%PKG_DIR%\" >nul
copy /y "%~dp0install.bat" "%PKG_DIR%\" >nul

:: Step 3: Create ZIP
echo [3/3] Creating distributable archive...
set "ZIP_NAME=BootTracker_Offline_%date:~0,4%%date:~5,2%%date:~8,2%.zip"
powershell -ExecutionPolicy Bypass -Command ^
    "Compress-Archive -Path '%PKG_DIR%\*' -DestinationPath '%~dp0%ZIP_NAME%' -Force"

echo.
echo ============================================
echo   Package created successfully!
echo   File: %~dp0%ZIP_NAME%
echo.
echo   Distribution contents:
echo     - BootTracker.exe
echo     - sqlite3.dll
echo     - install.bat
echo.
echo   To install: extract ZIP and run install.bat as Administrator
echo ============================================
echo.
pause
