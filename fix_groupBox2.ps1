$path = 'f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs'
$lines = Get-Content $path
$start = -1
$end = -1

for ($i = 46; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'this\.label3\.Text =') {
        $start = $i
    }
    if ($lines[$i] -match 'this\.label9\.Text =') {
        $end = $i
        break
    }
}

if ($start -ge 0 -and $end -ge 0) {
    $newLines = @(
        '            // Consensus Labels restored',
        '            this.label3.AutoSize = true;',
        '            this.label3.Location = new System.Drawing.Point(10, 25);',
        '            this.label3.Text = "State:";',
        '            this.lblState.AutoSize = true;',
        '            this.lblState.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblState.Location = new System.Drawing.Point(110, 25);',
        '            this.lblState.Text = "...";',
        '',
        '            this.label2.AutoSize = true;',
        '            this.label2.Location = new System.Drawing.Point(10, 50);',
        '            this.label2.Text = "Block:";',
        '            this.lblLatestBlock.AutoSize = true;',
        '            this.lblLatestBlock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblLatestBlock.Location = new System.Drawing.Point(110, 50);',
        '            this.lblLatestBlock.Text = "...";',
        '',
        '            this.label9.AutoSize = true;',
        '            this.label9.Location = new System.Drawing.Point(10, 75);',
        '            this.label9.Text = "Age:";',
        '            this.lblLedgerAge.AutoSize = true;',
        '            this.lblLedgerAge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblLedgerAge.Location = new System.Drawing.Point(110, 75);',
        '            this.lblLedgerAge.Text = "...";'
    )
    
    $finalContent = $lines[0..($start - 1)] + $newLines + $lines[($end + 1)..($lines.Count - 1)]
    [System.IO.File]::WriteAllLines($path, $finalContent, [System.Text.Encoding]::UTF8)
    Write-Host "Successfully restored labels in groupBox2"
}
else {
    Write-Error "Markers not found: start=$start, end=$end"
}
