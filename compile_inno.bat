@echo off
setlocal

echo -------------------------------------------------------
echo Compiling Inno Setup Installer...
echo -------------------------------------------------------

:: 1. Define typical install paths for Inno Setup (Check version 6 first, then 5)
set "INNO_PATH_1=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
set "INNO_PATH_2=C:\Program Files\Inno Setup 6\ISCC.exe"
set "INNO_PATH_3=C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
set "INNO_EXE=ISCC.exe"

:: 2. Check if ISCC is in PATH
where %INNO_EXE% >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    set "COMPILER=%INNO_EXE%"
    goto :COMPILE
)

:: 3. Check hardcoded paths
if exist "%INNO_PATH_1%" (
    set "COMPILER=%INNO_PATH_1%"
    goto :COMPILE
)
if exist "%INNO_PATH_2%" (
    set "COMPILER=%INNO_PATH_2%"
    goto :COMPILE
)
if exist "%INNO_PATH_3%" (
    set "COMPILER=%INNO_PATH_3%"
    goto :COMPILE
)

:: 4. Not found
echo [ERROR] Inno Setup (ISCC.exe) not found.
echo Please install Inno Setup from https://jrsoftware.org/isdl.php
echo Or add it to your system PATH.
pause
exit /b 1

:COMPILE
echo Using compiler: "%COMPILER%"
"%COMPILER%" "installer_inno.iss"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo -------------------------------------------------------
    echo Installer Created Successfully!
    echo Check the directory for: PiNodeMonitorPro_Setup_Inno.exe
    echo -------------------------------------------------------
) else (
    echo.
    echo [ERROR] Compilation failed.
)

pause
endlocal
