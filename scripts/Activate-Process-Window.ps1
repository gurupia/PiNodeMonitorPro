param (
    [string]$ProcessName
)

$ErrorActionPreference = 'SilentlyContinue'

# 프로세스 찾기 (메인 윈도우 핸들이 있는 것 우선)
$p = Get-Process $ProcessName | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1

if ($p) {
    # Win32 API 선언
    $Signature = @"
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
"@
    
    $Win32Api = Add-Type -MemberDefinition $Signature -Name "Win32Util" -Namespace "Gurupia" -PassThru
    
    # 창 복원 및 활성화 (SW_RESTORE = 9)
    $Win32Api::ShowWindow($p.MainWindowHandle, 9)
    $Win32Api::SetForegroundWindow($p.MainWindowHandle)
    
    Write-Output "TRUE"
}
else {
    Write-Output "FALSE"
}
