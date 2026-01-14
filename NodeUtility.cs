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
        public static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public class NodeConfig { public string CustomDockerPath { get; set; } public string CustomPiAppPath { get; set; } }
        public static NodeConfig Config { get; private set; }

        static NodeUtility() { LoadConfig(); }
        public static void LoadConfig() {
            try { if (File.Exists(ConfigPath)) Config = Newtonsoft.Json.JsonConvert.DeserializeObject<NodeConfig>(File.ReadAllText(ConfigPath)); } catch { }
            Config = Config ?? new NodeConfig();
        }
        public static void SaveConfig() { try { File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented)); } catch { } }

        // [활성화] 외부 파워쉘 스크립트 파일 실행 (사용자 요청 반영)
        public static bool ActivateProcess(string name) {
            try {
                // 1. 스크립트 경로 확인
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "Activate-Process-Window.ps1");
                if (!File.Exists(scriptPath)) return false;

                // 2. 64비트 파워쉘(SysNative) 경로 확보
                string ps = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "SysNative", "WindowsPowerShell", "v1.0", "powershell.exe");
                if (!File.Exists(ps)) ps = "powershell";

                // 3. 파일 직접 실행
                var psi = new ProcessStartInfo(ps, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" -ProcessName \"{name}\"") {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                
                using (var p = Process.Start(psi)) {
                    p.WaitForExit();
                }
                return true;
            } catch { return false; }
        }

        // [체크/동작] 파워쉘 통합 실행 헬퍼
        private static async Task<string> RunPSAsync(string cmd) {
            try {
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -Command \"{cmd}\"") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using (var p = Process.Start(psi)) return await p.StandardOutput.ReadToEndAsync();
            } catch { return ""; }
        }

        public static async Task<bool> IsDockerRunningAsync() => Process.GetProcessesByName("Docker Desktop").Length > 0;
        public static async Task<bool> IsWslInstalledAsync() => File.Exists(Environment.SystemDirectory + "\\wsl.exe") || (await RunPSAsync("wsl --status")).Contains("T");
        public static async Task<bool> IsWindowsFeatureEnabledAsync(string feature) => (await RunPSAsync($"if((Get-WindowsOptionalFeature -Online -FeatureName {feature} -ErrorAction SilentlyContinue).State -eq 'Enabled'){{'T'}}")).Contains("T") || await IsDockerRunningAsync();
        
        public static async Task EnableWindowsFeaturesAsync() => await RunCommandAsync("powershell", "-NoProfile -Command \"Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart; Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart\"", true);
        public static async Task UpdateWslAsync() => await RunCommandAsync("wsl", "--update", true);
        public static async Task<bool> IsFirewallRulePresentAsync() => (await RunPSAsync("if(netstat -an | Select-String ':31401|:31402|:31403'){{'T'}}")).Contains("T");
        public static async Task<bool> IsImagePresentAsync(string n) => (await RunPSAsync($"if(docker images | Select-String '{n}'){{'T'}}")).Contains("T");
        public static async Task<bool> IsContainerExistAsync(string n) => (await RunPSAsync($"if(docker ps -a --format '{{{{.Names}}}}' | Select-String '{n}'){{'T'}}")).Contains("T");
        
        public static async Task DetectContainerNameAsync() {
            var o = await RunPSAsync("docker ps --format '{{.Names}}'");
            if (o.Contains("pi") || o.Contains("consensus")) CurrentContainerName = o.Contains("consensus") ? "pi-consensus" : "testnet2";
        }

        public static async Task RunCommandAsync(string file, string args, bool admin = false) {
            var psi = new ProcessStartInfo(file, args) { UseShellExecute = true, CreateNoWindow = true };
            if (admin) psi.Verb = "runas";
            try { using (var p = Process.Start(psi)) if (p != null) await Task.Run(() => p.WaitForExit()); } catch { }
        }

        public static void RebootSystem() => Process.Start("shutdown", "/r /t 5");
        public static void MinimizeProcess(string n) { /* Skip PS */ }
        public static async Task<string> GetNodeUuidAsync() {
            try {
                string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pi Network", "user-preferences.json");
                if (File.Exists(p)) return System.Text.RegularExpressions.Regex.Match(File.ReadAllText(p), @"""uuid""\s*:\s*""([^""]+)""").Groups[1].Value;
            } catch { } return "";
        }
    }
}
