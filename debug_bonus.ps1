$appData = [System.IO.Path]::Combine($env:APPDATA, "Pi Network")
$prefPath = [System.IO.Path]::Combine($appData, "user-preferences.json")
$logPath = [System.IO.Path]::Combine($appData, "logs", "main.log")
$uuid = $null

Write-Host "--- Pi Node UUID Diagnostic ---" -ForegroundColor Cyan

# 1. Check user-preferences.json
if (Test-Path $prefPath) {
    Write-Host "[CHECK] user-preferences.json found."
    $json = Get-Content $prefPath -Raw
    if ($json -match '"uuid"\s*:\s*"([^"]+)"') {
        $uuid = $Matches[1]
        Write-Host "[SUCCESS] UUID found in user-preferences.json: $uuid" -ForegroundColor Green
    }
}
else {
    Write-Host "[WARN] user-preferences.json NOT found." -ForegroundColor Yellow
}

# 2. Check main.log if not found
if (-not $uuid -and (Test-Path $logPath)) {
    Write-Host "[CHECK] main.log found. Searching for UUID..."
    $log = Get-Content $logPath -Tail 500
    foreach ($line in $log) {
        if ($line -match 'uuid[=:]"?([a-f0-9\-]{36})"?') {
            $uuid = $Matches[1]
        }
    }
    if ($uuid) {
        Write-Host "[SUCCESS] UUID found in main.log: $uuid" -ForegroundColor Green
    }
}

# 3. Test API if UUID found
if ($uuid) {
    $url = "https://node-automation.minepi.com/api/node-info?uuid=$uuid"
    Write-Host "[CHECK] Calling API: $url"
    try {
        $resp = Invoke-RestMethod -Uri $url -Method Get
        Write-Host "[SUCCESS] API Response Received:" -ForegroundColor Green
        $resp | ConvertTo-Json | Write-Host
        
        if ($resp.node_bonus -or $resp.bonus) {
            Write-Host "[INFO] Bonus value detected in response." -ForegroundColor Cyan
        }
        else {
            Write-Host "[WARN] No bonus field found in raw response. Parsing might fail." -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "[ERROR] API Call failed: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "[CRITICAL] Could not detect UUID. Please ensure you are logged into the Pi Desktop app." -ForegroundColor Red
}

Write-Host "-------------------------------"
