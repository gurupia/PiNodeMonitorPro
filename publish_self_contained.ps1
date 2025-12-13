$projectPath = "PiNodeMonitorWinForm.csproj"
$outputDir = "Publish\win-x64-compressed"

Write-Host "Starting Safe & Compressed Deployment..." -ForegroundColor Cyan

# Clean previous output
if (Test-Path $outputDir) {
    Remove-Item -Path $outputDir -Recurse -Force
}

# Adjusted Build Command for Stability and Size
# -p:PublishTrimmed=false  : DISABLED Trimming to prevent WinForms build errors
# -p:PublishReadyToRun=false : Disable R2R to reduce size
# -p:EnableCompressionInSingleFile=true : Enable internal compression (Size reduces significantly)
dotnet publish $projectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $outputDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild Success!" -ForegroundColor Green
    Write-Host "Output Directory: $(Resolve-Path $outputDir)" -ForegroundColor Gray
    Invoke-Item $outputDir
}
else {
    Write-Host "`nBuild Failed." -ForegroundColor Red
    Read-Host "Press Enter to exit..."
}
