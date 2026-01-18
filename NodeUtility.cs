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

        // Cache Management
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (object Value, DateTime Expiry)> _cache 
            = new System.Collections.Concurrent.ConcurrentDictionary<string, (object Value, DateTime Expiry)>();

        private static T GetOrUpdateCache<T>(string key, int ttlSeconds, Func<T> updateFunc)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.Expiry > DateTime.Now) return (T)entry.Value;
            T newValue = updateFunc();
            _cache[key] = (newValue, DateTime.Now.AddSeconds(ttlSeconds));
            return newValue;
        }

        private static async Task<T> GetOrUpdateCacheAsync<T>(string key, int ttlSeconds, Func<Task<T>> updateFunc)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.Expiry > DateTime.Now) return (T)entry.Value;
            T newValue = await updateFunc();
            _cache[key] = (newValue, DateTime.Now.AddSeconds(ttlSeconds));
            return newValue;
        }

        static NodeUtility() { LoadConfig(); }
        public static void LoadConfig() {
            try { if (File.Exists(ConfigPath)) Config = Newtonsoft.Json.JsonConvert.DeserializeObject<NodeConfig>(File.ReadAllText(ConfigPath)); } catch { }
            Config = Config ?? new NodeConfig();
        }
        public static void SaveConfig() { try { File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented)); } catch { } }

        // [Optimization] Native P/Invoke for Window Management (Removes PowerShell Overhead)
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public static bool ActivateProcess(string name) 
        {
            try 
            {
                var process = System.Linq.Enumerable.FirstOrDefault(
                    Process.GetProcessesByName(name.Replace(".exe", "")), 
                    p => p.MainWindowHandle != IntPtr.Zero
                );

                if (process != null) 
                {
                    ShowWindow(process.MainWindowHandle, 9); // SW_RESTORE = 9
                    SetForegroundWindow(process.MainWindowHandle);
                    return true;
                }
            } 
            catch { }
            return false;
        }

        public static void KillProcessByName(string name)
        {
            try
            {
                var targetName = name.Replace(".exe", "");
                foreach (var process in Process.GetProcessesByName(targetName))
                {
                    try { process.Kill(); process.WaitForExit(1000); } catch { }
                }
            }
            catch { }
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
                        
                        // Use a shorter timeout for UI responsiveness
                        if (!process.WaitForExit(3000)) 
                        {
                            try { process.Kill(); } catch { }
                            return null;
                        }

                        return await process.StandardOutput.ReadToEndAsync();
                    }
                }
                catch { return ""; }
            });
        }

        public static async Task<bool> IsDockerRunningAsync() => Process.GetProcessesByName("Docker Desktop").Length > 0 || Process.GetProcessesByName("docker").Length > 0;
        public static async Task<bool> IsWslInstalledAsync() => File.Exists(Environment.SystemDirectory + "\\wsl.exe");
        
        public static async Task<bool> IsWindowsFeatureEnabledAsync(string feature) => (await RunWslCommandAsync("--status") ?? "").Contains("T") || await IsDockerRunningAsync(); // Simplified for performance, check if WSL is active as proxy for features
        
        public static async Task EnableWindowsFeaturesAsync() => await RunCommandAsync("powershell", "-NoProfile -Command \"Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart; Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart\"", true);
        public static async Task UpdateWslAsync() => await RunCommandAsync("wsl.exe", "--update", true);
        
        public static async Task<bool> IsFirewallRulePresentAsync() 
        {
            return await GetOrUpdateCacheAsync<bool>("FirewallRule", 60, async () => {
                // Direct WSL netstat is faster than PS netstat
                string output = await RunWslCommandAsync("-d docker-desktop-data -- netstat -an") ?? "";
                return output.Contains(":31401") || output.Contains(":31402") || output.Contains(":31403");
            });
        }

        public static async Task<bool> IsImagePresentAsync(string n) => (await RunDockerCommandAsync($"images -q {n}") ?? "").Length > 0;
        public static async Task<bool> IsContainerExistAsync(string n) => (await RunDockerCommandAsync($"ps -a --filter name={n} --format \"{{{{.Names}}}}\"") ?? "").Contains(n);
        
        public static async Task DetectContainerNameAsync() {
            var o = await RunDockerCommandAsync("ps --format \"{{.Names}}\"") ?? "";
            if (!string.IsNullOrEmpty(o))
            {
                if (o.Contains("testnet2")) CurrentContainerName = "testnet2";
                else if (o.Contains("pi-consensus")) CurrentContainerName = "pi-consensus";
                else if (o.Contains("pi-node")) CurrentContainerName = "pi-node";
                else CurrentContainerName = o.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            }
        }

        public static async Task RunCommandAsync(string file, string args, bool admin = false) {
            var psi = new ProcessStartInfo(file, args) { UseShellExecute = true, CreateNoWindow = true };
            if (admin) psi.Verb = "runas";
            try { using (var p = Process.Start(psi)) if (p != null) await Task.Run(() => p.WaitForExit()); } catch { }
        }

        public static void RebootSystem() => Process.Start("shutdown", "/r /t 5");
        public static void MinimizeProcess(string name) 
        { 
            try 
            {
                 var process = System.Linq.Enumerable.FirstOrDefault(
                    Process.GetProcessesByName(name.Replace(".exe", "")), 
                    p => p.MainWindowHandle != IntPtr.Zero
                );

                if (process != null)
                {
                    ShowWindowAsync(process.MainWindowHandle, 6); // SW_MINIMIZE = 6
                }
            } 
            catch { }
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
            return await GetOrUpdateCacheAsync<double>("GhostSpace", 300, async () => {
                string vhdxPath = GetWslVhdxPath();
                if (string.IsNullOrEmpty(vhdxPath)) return 0.0;

                try {
                    long physicalSize = new FileInfo(vhdxPath).Length;
                    // WSL 내부 실제 점유량 확인 (Direct exec is faster)
                    string output = await RunWslCommandAsync("-d docker-desktop-data -- df -B1 / --output=used");
                    var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length >= 2 && long.TryParse(lines[1].Trim(), out long usedSize)) {
                        double diff = (double)(physicalSize - usedSize) / (1024 * 1024 * 1024);
                        return diff > 0 ? diff : 0;
                    }
                } catch { } return 0.0;
            });
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
