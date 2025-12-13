@echo off
setlocal

set "PROJECT_PATH=PiNodeMonitorWinForm.csproj"
set "OUTPUT_DIR=Publish\win-x64-compressed"

echo -------------------------------------------------------
echo Starting Safe & Compressed Deployment...
echo -------------------------------------------------------

:: Clean previous output
if exist "%OUTPUT_DIR%" (
    rmdir /s /q "%OUTPUT_DIR%"
)

:: Adjusted Build Command
:: -p:PublishTrimmed=false           : DISABLED Trimming (Fixes build errors in WinForms)
:: -p:PublishReadyToRun=false        : Disable R2R to reduce size
:: -p:EnableCompressionInSingleFile=true : Compress the single .exe file
dotnet publish "%PROJECT_PATH%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUTPUT_DIR%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo -------------------------------------------------------
    echo Build Success!
    echo Output Directory: %OUTPUT_DIR%
    echo -------------------------------------------------------
    start explorer "%OUTPUT_DIR%"
) else (
    echo.
    echo -------------------------------------------------------
    echo Build Failed.
    echo -------------------------------------------------------
)

pause
endlocal
