$path = "f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.cs"
$lines = Get-Content $path -Raw -Encoding UTF8
# Since -Raw gives a single string, let's split it carefully or use regex on the whole content
# But -Raw might lose line ending info or handle them differently.
# Let's use [System.IO.File]::ReadAllLines instead for strictly matching what view_file sees.

$lines = [System.IO.File]::ReadAllLines($path)

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'lblBalance\.Text = "Wallet: --') { $lines[$i] = '            lblBalance.Text = "Wallet: -- π";' }
    if ($lines[$i] -match 'btnSaveKey\.Text =') { $lines[$i] = '            btnSaveKey.Text = "💾";' }
    if ($lines[$i] -match 'btnSmsConfig\.Text =') { $lines[$i] = '            btnSmsConfig.Text = "💬";' }
    if ($lines[$i] -match 'btnMobile\.Text =') { $lines[$i] = '            btnMobile.Text = "📱 Mobile Connect";' }
    if ($lines[$i] -match 'btnMulti\.Text =') { $lines[$i] = '            btnMulti.Text = "🖥️ Multi-View";' }
    if ($lines[$i] -match 'Label lblCopy = new Label \{ Text = "Copyright') { $lines[$i] = '            Label lblCopy = new Label { Text = "Copyright © 2025 GuruPia. All rights reserved.", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(10, 8) };' }
}

$utf8WithBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllLines($path, $lines, $utf8WithBom)

Write-Host "Restored characters using prefix matching."
