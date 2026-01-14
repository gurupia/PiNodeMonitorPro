$path = "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.cs"
$content = [System.IO.File]::ReadAllText($path)

# Dictionary of broken strings to fix
$fixes = @{
    'lblBalance.Text = "Wallet: -- ?"'     = 'lblBalance.Text = "Wallet: -- π"'
    'btnSaveKey.Text = "?뮶"'               = 'btnSaveKey.Text = "💾"'
    'btnSmsConfig.Text = "?뮠"'             = 'btnSmsConfig.Text = "💬"'
    'btnMobile.Text = "?벑 Mobile Connect"' = 'btnMobile.Text = "📱 Mobile Connect"'
    'btnMulti.Text = "?뼢截?Multi-View"'     = 'btnMulti.Text = "🖥️ Multi-View"'
    'Text = "Copyright 짤 2025'             = 'Text = "Copyright © 2025'
}

foreach ($key in $fixes.Keys) {
    if ($content.Contains($key)) {
        $content = $content.Replace($key, $fixes[$key])
        Write-Host "Fixed: $key"
    }
    else {
        Write-Host "Could not find: $key"
    }
}

# Save with UTF-8 + BOM
$utf8WithBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($path, $content, $utf8WithBom)

# Also ensure Designer.cs is UTF-8 with BOM
$dPath = "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs"
$dContent = [System.IO.File]::ReadAllText($dPath)
[System.IO.File]::WriteAllText($dPath, $dContent, $utf8WithBom)

Write-Host "Encoding fix complete."
