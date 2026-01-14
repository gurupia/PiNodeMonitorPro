Add-Type @"
using System;
using System.Runtime.InteropServices;
public class WinAPI2 {
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@

$procs = [System.Diagnostics.Process]::GetProcessesByName("Docker Desktop")
Write-Host "프로세스 수: $($procs.Count)"

foreach ($p in $procs) {
    $hwnd = $p.MainWindowHandle
    Write-Host "PID: $($p.Id), Handle: $hwnd, Title: '$($p.MainWindowTitle)'"
    
    if ($hwnd -ne [IntPtr]::Zero) {
        Write-Host "  -> 창 활성화 시도..."
        $r1 = [WinAPI2]::ShowWindow($hwnd, 9)
        $r2 = [WinAPI2]::SetForegroundWindow($hwnd)
        Write-Host "  -> ShowWindow: $r1, SetForegroundWindow: $r2"
    }
}
