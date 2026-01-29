@echo off
setlocal

echo -------------------------------------------------------
echo Compiling NSIS Installer...
echo -------------------------------------------------------

:: 1. Define typical install paths for NSIS
set "NSIS_PATH_1=C:\Program Files (x86)\NSIS\makensis.exe"
set "NSIS_PATH_2=C:\Program Files\NSIS\makensis.exe"
set "NSIS_EXE=makensis.exe"

:: 2. Check if makensis is in PATH
where %NSIS_EXE% >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    set "COMPILER=%NSIS_EXE%"
    goto :COMPILE
)

:: 3. Check hardcoded paths
if exist "%NSIS_PATH_1%" (
    set "COMPILER=%NSIS_PATH_1%"
    goto :COMPILE
)
if exist "%NSIS_PATH_2%" (
    set "COMPILER=%NSIS_PATH_2%"
    goto :COMPILE
)

:: 4. Not found
echo [ERROR] NSIS (makensis.exe) not found.
echo Please install NSIS from https://nsis.sourceforge.io/
echo Or add it to your system PATH.
pause
exit /b 1

:COMPILE
echo Using compiler: "%COMPILER%"
"%COMPILER%" "installer_nsis.nsi"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo -------------------------------------------------------
    echo Installer Created Successfully!
    echo Check the directory for: PiNodeMonitorPro_Setup_NSIS.exe
    echo -------------------------------------------------------
) else (
    echo.
    echo [ERROR] Compilation failed.
)

pause
endlocal
