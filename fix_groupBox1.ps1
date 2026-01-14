$path = 'f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs'
$lines = Get-Content $path
$start = -1
$end = -1

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '// label1 restored') {
        $start = $i
    }
    if ($lines[$i] -match 'this\.labelServerCpu\.Text =') {
        $end = $i
        break
    }
}

if ($start -ge 0 -and $end -ge 0) {
    $newLines = @(
        '            // label1 & lblProtocolVersion restored',
        '            this.label1.AutoSize = true;',
        '            this.label1.Location = new System.Drawing.Point(10, 25);',
        '            this.label1.Text = "Protocol Ver:";',
        '            this.lblProtocolVersion.AutoSize = true;',
        '            this.lblProtocolVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblProtocolVersion.Location = new System.Drawing.Point(110, 25);',
        '            this.lblProtocolVersion.Text = "...";',
        '',
        '            // label7 & lblStellarBuild restored',
        '            this.label7.AutoSize = true;',
        '            this.label7.Location = new System.Drawing.Point(10, 50);',
        '            this.label7.Text = "Core Build:";',
        '            this.lblStellarBuild.AutoSize = true;',
        '            this.lblStellarBuild.Location = new System.Drawing.Point(110, 50);',
        '            this.lblStellarBuild.Text = "...";',
        '',
        '            // labelLocalCpu & lblLocalCpuCount restored',
        '            this.labelLocalCpu.AutoSize = true;',
        '            this.labelLocalCpu.Location = new System.Drawing.Point(10, 75);',
        '            this.labelLocalCpu.Text = "Local CPU:";',
        '            this.lblLocalCpuCount.AutoSize = true;',
        '            this.lblLocalCpuCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblLocalCpuCount.Location = new System.Drawing.Point(110, 75);',
        '            this.lblLocalCpuCount.Text = "0 Cores";',
        '',
        '            // labelServerCpu & lblServerCpuCount restored',
        '            this.labelServerCpu.AutoSize = true;',
        '            this.labelServerCpu.Location = new System.Drawing.Point(10, 100);',
        '            this.labelServerCpu.Text = "Server CPU:";',
        '            this.lblServerCpuCount.AutoSize = true;',
        '            this.lblServerCpuCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);',
        '            this.lblServerCpuCount.Location = new System.Drawing.Point(110, 100);',
        '            this.lblServerCpuCount.Text = "N/A";'
    )
    
    $finalContent = $lines[0..($start - 1)] + $newLines + $lines[($end + 1)..($lines.Count - 1)]
    [System.IO.File]::WriteAllLines($path, $finalContent, [System.Text.Encoding]::UTF8)
    Write-Host "Successfully restored labels in groupBox1"
}
else {
    Write-Error "Markers not found: start=$start, end=$end"
}
