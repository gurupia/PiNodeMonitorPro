using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm
{
    public static class NodeUtility
    {
        public static string CurrentContainerName { get; set; } = "testnet2";
        public static string ConfigDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string ConfigPath = Path.Combine(ConfigDir, "config.json");
        public class NodeConfig { 
            public string CustomDockerPath { get; set; } 
            public string CustomPiAppPath { get; set; } 
            public string LogPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            public bool EnableCpuOptimization { get; set; } = false; // 기본값 OFF
            public bool EnableDiskWeightAlert { get; set; } = true;
            public DateTime LastDiskCompacted { get; set; } = DateTime.MinValue;
        }
        public static NodeConfig Config { get; private set; }

        static NodeUtility() { LoadConfig(); }
        public static void LoadConfig() {
            try { if (File.Exists(ConfigPath)) Config = Newtonsoft.Json.JsonConvert.DeserializeObject<NodeConfig>(File.ReadAllText(ConfigPath)); } catch { }
            Config = Config ?? new NodeConfig();
        }
        public static void SaveConfig() { try { File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented)); } catch { } }

        // [활성화] 보안 강화: 검증된 로직을 내장하여 실행 (외부 파일 변조 방지 + SysNative 64비트 호환)
        public static bool ActivateProcess(string name) {
            try {
                string script = $@"
$ErrorActionPreference = 'SilentlyContinue'
$p = Get-Process '{name}' | Where-Object {{ $_.MainWindowHandle -ne 0 }} | Select-Object -First 1
if ($p) {{
    $sig = @'
    [DllImport(""user32.dll"")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport(""user32.dll"")] public static extern bool SetForegroundWindow(IntPtr hWnd);
'@
    $w = Add-Type -MemberDefinition $sig -Name ""Win32Util"" -Namespace ""Gurupia"" -PassThru
    $w::ShowWindow($p.MainWindowHandle, 9)
    $w::SetForegroundWindow($p.MainWindowHandle)
}}";
                string b64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                
                // 3. 64비트 환경이므로 기본 powershell 호출 (자동으로 System32의 64비트 버전 사용됨)
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {b64}") {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (var p = Process.Start(psi)) {
                    p.WaitForExit();
                }
                return true;
            } catch { return false; }
        }

        // [체크/동작] 파워쉘 통합 실행 헬퍼 (구형 - 점진적 폐기)
        public static async Task<string> RunPSAsync(string cmd) {
            try {
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -Command \"{cmd}\"") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using (var p = Process.Start(psi)) return await p.StandardOutput.ReadToEndAsync();
            } catch { return ""; }
        }

        // [신규] WSL 직접 실행 헬퍼 (저부하)
        public static async Task<string> RunWslCommandAsync(string cmd)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        Arguments = cmd,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };
                    using (var process = Process.Start(psi))
                    {
                        if (process == null) return "";
                        string output = await process.StandardOutput.ReadToEndAsync();
                        process.WaitForExit(2000);
                        return output;
                    }
                }
                catch { return ""; }
            });
        }

        public static async Task<string> RunDockerCommandAsync(string arguments)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (var process = Process.Start(psi))
                    {
                        if (process == null) return null;
                        string output = await process.StandardOutput.ReadToEndAsync();
                        await Task.Run(() => process.WaitForExit(3000));
                        return output;
                    }
                }
                catch { return null; }
            });
        }

        public static async Task<bool> IsDockerRunningAsync() => Process.GetProcessesByName("Docker Desktop").Length > 0;
        public static async Task<bool> IsWslInstalledAsync() => File.Exists(Environment.SystemDirectory + "\\wsl.exe");
        
        public static async Task<bool> IsWindowsFeatureEnabledAsync(string feature) => (await RunWslCommandAsync("--status")).Contains("T") || await IsDockerRunningAsync(); // Simplified for performance, check if WSL is active as proxy for features
        
        public static async Task EnableWindowsFeaturesAsync() => await RunCommandAsync("powershell", "-NoProfile -Command \"Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart; Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart\"", true);
        public static async Task UpdateWslAsync() => await RunCommandAsync("wsl.exe", "--update", true);
        
        public static async Task<bool> IsFirewallRulePresentAsync() 
        {
            // Direct WSL netstat is faster than PS netstat
            string output = await RunWslCommandAsync("-d docker-desktop-data -- netstat -an");
            return output.Contains(":31401") || output.Contains(":31402") || output.Contains(":31403");
        }

        public static async Task<bool> IsImagePresentAsync(string n) => (await RunDockerCommandAsync($"images -q {n}")).Length > 0;
        public static async Task<bool> IsContainerExistAsync(string n) => (await RunDockerCommandAsync($"ps -a --filter name={n} --format \"{{{{.Names}}}}\"")).Contains(n);
        
        public static async Task DetectContainerNameAsync() {
            var o = await RunDockerCommandAsync("ps --format \"{{.Names}}\"");
            if (o.Contains("pi") || o.Contains("consensus")) CurrentContainerName = o.Contains("consensus") ? "pi-consensus" : "testnet2";
        }

        public static async Task RunCommandAsync(string file, string args, bool admin = false) {
            var psi = new ProcessStartInfo(file, args) { UseShellExecute = true, CreateNoWindow = true };
            if (admin) psi.Verb = "runas";
            try { using (var p = Process.Start(psi)) if (p != null) await Task.Run(() => p.WaitForExit()); } catch { }
        }

        public static void RebootSystem() => Process.Start("shutdown", "/r /t 5");
        public static void MinimizeProcess(string name) { 
            try {
                string script = $@"
$p = Get-Process '{name}' -ErrorAction SilentlyContinue | Where-Object {{ $_.MainWindowHandle -ne 0 }} | Select-Object -First 1
if ($p) {{
    $sig = '[DllImport(""user32.dll"")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);'
    $type = Add-Type -MemberDefinition $sig -Name ""Win32Min"" -Namespace ""Gurupia"" -PassThru
    $type::ShowWindowAsync($p.MainWindowHandle, 6) # SW_MINIMIZE = 6
}}";
                string b64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                Process.Start(new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {b64}") { CreateNoWindow = true, UseShellExecute = false });
            } catch { }
        }
        public static async Task<string> GetNodeUuidAsync() {
            try {
                string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pi Network", "user-preferences.json");
                if (File.Exists(p)) return System.Text.RegularExpressions.Regex.Match(File.ReadAllText(p), @"""uuid""\s*:\s*""([^""]+)""").Groups[1].Value;
            } catch { } return "";
        }

        // [신규] WSL2 VHDX 실경로 탐지 (Registry 기반)
        public static string GetWslVhdxPath() {
            try {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Lxss")) {
                    if (key == null) return null;
                    foreach (var subKeyName in key.GetSubKeyNames()) {
                        using (var subKey = key.OpenSubKey(subKeyName)) {
                            string distName = subKey?.GetValue("DistributionName") as string;
                            if (distName == "docker-desktop-data") {
                                string basePath = subKey?.GetValue("BasePath") as string;
                                if (!string.IsNullOrEmpty(basePath)) {
                                    string vhdx = Path.Combine(basePath, "ext4.vhdx");
                                    if (File.Exists(vhdx)) return vhdx;
                                }
                            }
                        }
                    }
                }
            } catch { } return null;
        }

        // [신규] Ghost Space (버려지는 용량) 계산 (GB 단위)
        public static async Task<double> GetGhostSpaceGbAsync() {
            string vhdxPath = GetWslVhdxPath();
            if (string.IsNullOrEmpty(vhdxPath)) return 0;

            try {
                long physicalSize = new FileInfo(vhdxPath).Length;
                // WSL 내부 실제 점유량 확인 (Direct exec is faster)
                string output = await RunWslCommandAsync("-d docker-desktop-data -- df -B1 / --output=used");
                var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length >= 2 && long.TryParse(lines[1].Trim(), out long usedSize)) {
                    double diff = (double)(physicalSize - usedSize) / (1024 * 1024 * 1024);
                    return diff > 0 ? diff : 0;
                }
            } catch { } return 0;
        }

        // [신규] 최적화 엔진 실행 (Selective Compact)
        public static async Task<bool> CompactWslDiskAsync() {
            string vhdxPath = GetWslVhdxPath();
            if (string.IsNullOrEmpty(vhdxPath)) return false;

            // 1순위: wsl --manage (v1.1.0+)
            var versionInfo = await RunWslCommandAsync("--version");
            if (versionInfo.Contains("1.1.0") || versionInfo.Contains("2.")) {
                await RunCommandAsync("wsl", "--manage docker-desktop-data --compact", true);
                return true;
            }

            // 2순위: Diskpart (Legacy)
            string scriptPath = Path.Combine(Path.GetTempPath(), "compact_vhdx.txt");
            string script = $"select vdisk file=\"{vhdxPath}\"\nattach vdisk readonly\ncompact vdisk\ndetach vdisk\nexit";
            File.WriteAllText(scriptPath, script);
            
            await RunCommandAsync("diskpart", $"/s \"{scriptPath}\"", true);
            try { File.Delete(scriptPath); } catch { }
            return true;
        }
    }
}
