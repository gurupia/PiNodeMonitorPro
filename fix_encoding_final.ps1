$path = "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.cs"
$lines = Get-Content $path
# Restore precisely by index (0-indexed)
$lines[98] = '            lblBalance.Text = "Wallet: -- π";'
$lines[122] = '            btnSaveKey.Text = "💾";'
$lines[129] = '            btnSmsConfig.Text = "💬";'
$lines[162] = '            btnMobile.Text = "📱 Mobile Connect";'
$lines[178] = '            btnMulti.Text = "🖥️ Multi-View";'
$lines[210] = '            Label lblCopy = new Label { Text = "Copyright © 2025 GuruPia. All rights reserved.", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(10, 8) };'

# UTF-8 with BOM forced
$utf8WithBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllLines($path, $lines, $utf8WithBom)

# Also ensure Designer is UTF-8 with BOM
$dPath = "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs"
$dLines = Get-Content $dPath
[System.IO.File]::WriteAllLines($dPath, $dLines, $utf8WithBom)

Write-Host "Encoding fixed and characters restored in Form1.cs and Form1.Designer.cs"
