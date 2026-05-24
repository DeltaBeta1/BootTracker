@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: Run with admin privileges check
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo This installer requires administrator privileges.
    echo Please right-click and select "Run as administrator".
    pause
    exit /b 1
)

set "INSTALL_DIR=%ProgramFiles%\BootTracker"
set "START_MENU=%ProgramData%\Microsoft\Windows\Start Menu\Programs\BootTracker"

echo ============================================
echo   Boot Tracker - Offline Installer
echo ============================================
echo.
echo Install directory: %INSTALL_DIR%
echo.

:: Create directories
echo [1/4] Creating directories...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%START_MENU%" mkdir "%START_MENU%"

:: Copy files
echo [2/4] Copying application files...
copy /y "%~dp0BootTracker.exe" "%INSTALL_DIR%\BootTracker.exe" >nul
copy /y "%~dp0sqlite3.dll" "%INSTALL_DIR%\sqlite3.dll" >nul
echo   Done.

:: Create Start Menu shortcut
echo [3/4] Creating Start Menu shortcut...
set "SHORTCUT=%START_MENU%\Boot Tracker.lnk"
powershell -ExecutionPolicy Bypass -Command ^
    "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%SHORTCUT%'); $s.TargetPath = '%INSTALL_DIR%\BootTracker.exe'; $s.WorkingDirectory = '%INSTALL_DIR%'; $s.Description = 'Boot Tracker - Boot Recording System'; $s.Save()"
echo   Done.

:: Create Desktop shortcut
echo [4/4] Creating Desktop shortcut...
set "DESKTOP=%PUBLIC%\Desktop\Boot Tracker.lnk"
powershell -ExecutionPolicy Bypass -Command ^
    "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%DESKTOP%'); $s.TargetPath = '%INSTALL_DIR%\BootTracker.exe'; $s.WorkingDirectory = '%INSTALL_DIR%'; $s.Description = 'Boot Tracker - Boot Recording System'; $s.Save()"
echo   Done.

:: Create uninstaller
echo @echo off > "%INSTALL_DIR%\uninstall.bat"
echo net session ^>nul 2^>^&1 ^|^| (echo Run as administrator ^& pause ^& exit /b 1) >> "%INSTALL_DIR%\uninstall.bat"
echo rmdir /s /q "%START_MENU%" >> "%INSTALL_DIR%\uninstall.bat"
echo del /q "%PUBLIC%\Desktop\Boot Tracker.lnk" >> "%INSTALL_DIR%\uninstall.bat"
echo del /q "%INSTALL_DIR%\BootTracker.exe" >> "%INSTALL_DIR%\uninstall.bat"
echo del /q "%INSTALL_DIR%\sqlite3.dll" >> "%INSTALL_DIR%\uninstall.bat"
echo del /q "%INSTALL_DIR%\uninstall.bat" >> "%INSTALL_DIR%\uninstall.bat"
echo rmdir "%INSTALL_DIR%" >> "%INSTALL_DIR%\uninstall.bat"
echo echo Boot Tracker uninstalled. >> "%INSTALL_DIR%\uninstall.bat"
echo pause >> "%INSTALL_DIR%\uninstall.bat"

echo.
echo ============================================
echo   Installation Complete!
echo.
echo   Start Menu: Boot Tracker
echo   Desktop: Boot Tracker
echo   Uninstall: %INSTALL_DIR%\uninstall.bat
echo ============================================
echo.
echo Press any key to exit...
pause >nul
