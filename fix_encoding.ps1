$files = @(
    "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.cs",
    "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs"
)

foreach ($path in $files) {
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        
        # Manually fix specific corrupted strings in memory before saving
        # Note: PowerShell handles strings as UTF-16 internally, so this should work
        
        # Fix Form1.cs corruptions
        if ($path -match "Form1.cs") {
            $content = $content -replace "Wallet: -- \?", "Wallet: -- π"
            $content = $content -replace "\?벑 Mobile Connect", "📱 Mobile Connect"
            $content = $content -replace "\?뼢截\?Multi-View", "🖥️ Multi-View"
            $content = $content -replace "Copyright 짤 2025", "Copyright © 2025"
            $content = $content -replace 'btnSaveKey.Text = "\?뚥";', 'btnSaveKey.Text = "💾";'
            $content = $content -replace 'btnSmsConfig.Text = "\?뵠";', 'btnSmsConfig.Text = "💬";'
            # Generic recovery for any other ? replacements might be risky, but let's target specific ones
        }

        # Save with UTF8 + BOM (Required for Visual Studio/C# to recognize special chars)
        $utf8WithBom = New-Object System.Text.UTF8Encoding($true)
        [System.IO.File]::WriteAllText($path, $content, $utf8WithBom)
        
        Write-Host "Fixed encoding and restored characters for: $path"
    }
}
