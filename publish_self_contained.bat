@echo off
setlocal enabledelayedexpansion

:: =====================================================================
:: Pi Node Monitor Pro - Single File Deployment Build Script
:: Designed by: Master System Architect
:: =====================================================================

set "PROJECT_NAME=PiNodeMonitorWinForm"
set "PROJECT_FILE=%PROJECT_NAME%.csproj"
set "OUTPUT_ROOT=Publish"
set "TARGET_DIR=%OUTPUT_ROOT%\win-x64-compressed"
set "BUILD_CONFIG=Release"
set "RUNTIME_ID=win-x64"

title %PROJECT_NAME% Deployment Builder

echo =======================================================
echo    PI NODE MONITOR PRO - BUILD SYSTEM
echo =======================================================
echo.
echo [1/3] Environment Verification...

:: Check for .NET SDK
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK not found. Please install .NET 8 SDK.
    pause
    exit /b 1
)

echo [OK] .NET SDK Detected.
echo [INFO] Project: %PROJECT_FILE%
echo [INFO] Config:  %BUILD_CONFIG%
echo [INFO] Target:  %RUNTIME_ID%
echo.

echo [2/3] Cleaning previous build artifacts...
if exist "%TARGET_DIR%" (
    echo [INFO] Removing old publish folder...
    rd /s /q "%TARGET_DIR%"
)
echo [OK] Cleaned.
echo.

echo [3/3] Executing Publish Command (Single-File, Self-Contained)...
echo [WAIT] This may take a minute depending on resources...

:: Publish Command breakdown:
:: -r win-x64 : Targeted for Windows 64-bit
:: --self-contained true : Includes .NET runtime (No install needed on target machine)
:: -p:PublishSingleFile=true : Bundle into one executable
:: -p:PublishReadyToRun=false : Disable AOT for better compatibility and smaller size
:: -p:EnableCompressionInSingleFile=true : Maximize compression
:: -p:IncludeNativeLibrariesForSelfExtract=true : Handle native DLLs automatically
:: -p:PublishTrimmed=false : Avoid trimming to prevent WinForms reflection issues

dotnet publish "%PROJECT_FILE%" ^
    -c %BUILD_CONFIG% ^
    -r %RUNTIME_ID% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=false ^
    -p:EnableCompressionInSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishTrimmed=false ^
    -o "%TARGET_DIR%"

if %ERRORLEVEL% equ 0 (
    echo.
    echo =======================================================
    echo    BUILD SUCCESSFUL!
    echo =======================================================
    echo [EXEC] Result saved to: %TARGET_DIR%
    echo [INFO] Executable: %PROJECT_NAME%.exe
    echo.
    echo Opening output directory...
    start explorer "%TARGET_DIR%"
) else (
    echo.
    echo =======================================================
    echo    BUILD FAILED! (Error Level: %ERRORLEVEL%)
    echo =======================================================
    echo [ERROR] check the logs above for dotnet publish errors.
)

echo.
echo Press any key to exit.
pause >nul
endlocal
