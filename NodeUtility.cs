using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm
{
    public static class NodeUtility
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        public static string CurrentContainerName { get; set; }
        public static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public class NodeConfig
        {
            public string CustomDockerPath { get; set; }
            public string CustomPiAppPath { get; set; }
        }

        public static NodeConfig Config { get; private set; }

        static NodeUtility()
        {
            CurrentContainerName = "testnet2"; 
            LoadConfig();
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    Config = Newtonsoft.Json.JsonConvert.DeserializeObject<NodeConfig>(json);
                }
            }
            catch { }
            if (Config == null) Config = new NodeConfig();
        }

        public static void SaveConfig()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        public static async Task DetectContainerNameAsync()
        {
            try
            {
                var psi = new ProcessStartInfo("docker", "ps --format \"{{.Names}}\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        string output = p.StandardOutput.ReadToEnd();
                        await Task.Run(() => p.WaitForExit());
                        string[] names = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var name in names)
                        {
                            if (name.Contains("pi") || name.Contains("consensus"))
                            {
                                CurrentContainerName = name;
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static async Task<bool> IsDockerRunningAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info --format \"{{.ServerVersion}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return false;
                    string output = process.StandardOutput.ReadToEnd();
                    await Task.Run(() => process.WaitForExit());
                    return !string.IsNullOrWhiteSpace(output) && process.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        public static async Task<bool> IsImagePresentAsync(string imageNamePart)
        {
            string output = "";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "images",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        output = process.StandardOutput.ReadToEnd();
                        await Task.Run(() => process.WaitForExit());
                    }
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/c docker images",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (var process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            output = process.StandardOutput.ReadToEnd();
                            await Task.Run(() => process.WaitForExit());
                        }
                    }
                }
                catch { }
            }
            return !string.IsNullOrWhiteSpace(output) && output.Contains(imageNamePart);
        }

        public static async Task<bool> IsContainerExistAsync(string containerName)
        {
            string output = "";
            try 
            {
                var psi = new ProcessStartInfo("docker", "ps -a") 
                { 
                    RedirectStandardOutput = true, 
                    UseShellExecute = false, 
                    CreateNoWindow = true 
                };
                using (var p = Process.Start(psi)) 
                { 
                    if (p != null)
                    {
                        output = p.StandardOutput.ReadToEnd(); 
                        await Task.Run(() => p.WaitForExit()); 
                    }
                }
            }
            catch {}

            if (string.IsNullOrWhiteSpace(output))
            {
                try 
                {
                    var psi = new ProcessStartInfo("cmd", "/c docker ps -a") 
                    { 
                        RedirectStandardOutput = true, 
                        UseShellExecute = false, 
                        CreateNoWindow = true 
                    };
                    using (var p = Process.Start(psi)) 
                    { 
                        if (p != null)
                        {
                            output = p.StandardOutput.ReadToEnd(); 
                            await Task.Run(() => p.WaitForExit()); 
                        }
                    }
                }
                catch {}
            }
            return !string.IsNullOrWhiteSpace(output) && output.Contains(containerName);
        }

        public static async Task<string> GetNodeUuidAsync()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            try
            {
                string prefPath = Path.Combine(appData, "Pi Network", "user-preferences.json");
                if (File.Exists(prefPath))
                {
                    string json = File.ReadAllText(prefPath);
                    var match = Regex.Match(json, @"""uuid""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);
                    if (match.Success) return match.Groups[1].Value;
                    
                    match = Regex.Match(json, @"uuid\s*[=:']\s*['""]?([a-f0-9\-]{36})['""]?", RegexOptions.IgnoreCase);
                    if (match.Success) return match.Groups[1].Value;
                }
            }
            catch { }

            try
            {
                string[] logPaths = {
                    Path.Combine(appData, "Pi Network", "logs", "main.log"),
                    Path.Combine(appData, "Pi Network", "main.log")
                };

                foreach (var logPath in logPaths)
                {
                    if (File.Exists(logPath))
                    {
                        using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sr = new StreamReader(fs))
                        {
                            string content = sr.ReadToEnd();
                            var matches = Regex.Matches(content, @"uuid\s*[=:']\s*['""]?([a-f0-9\-]{36})['""]?", RegexOptions.IgnoreCase);
                            if (matches.Count > 0) return matches[matches.Count - 1].Groups[1].Value;
                        }
                    }
                }
            }
            catch { }
            return "";
        }

        public static async Task<bool> IsFirewallRulePresentAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-an",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return false;
                    string output = process.StandardOutput.ReadToEnd();
                    await Task.Run(() => process.WaitForExit());
                    bool has31401 = output.Contains(":31401") && output.Contains("LISTENING");
                    bool has31402 = output.Contains(":31402") && output.Contains("LISTENING");
                    bool has31403 = output.Contains(":31403") && output.Contains("LISTENING");
                    return has31401 || has31402 || has31403;
                }
            }
            catch { return false; }
        }

        public static async Task RunCommandAsync(string fileName, string args, bool asAdmin = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = asAdmin ? false : true 
            };

            if (asAdmin) psi.Verb = "runas";

            try
            {
                using (var p = Process.Start(psi))
                {
                    if (p != null) await Task.Run(() => p.WaitForExit());
                }
            }
            catch { }
        }

        public static async Task<bool> IsWindowsFeatureEnabledAsync(string featureName)
        {
            try
            {
                // 1차 시도: PowerShell (Internal Enum Check)
                // -NoProfile을 사용하여 부팅 속도 향상 및 간섭 최소화
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -Command \"if((Get-WindowsOptionalFeature -Online -FeatureName {featureName}).State -eq 'Enabled') {{ write-host 'TRUE' }}\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        string output = await p.StandardOutput.ReadToEndAsync();
                        await Task.Run(() => p.WaitForExit());
                        if (output.Contains("TRUE")) return true;
                    }
                }

                // 2차 시도: DISM (Fallback - localization 고려)
                var dismPsi = new ProcessStartInfo("dism", $"/online /get-featureinfo /featurename:{featureName}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(dismPsi))
                {
                    if (p != null)
                    {
                        string output = await p.StandardOutput.ReadToEndAsync();
                        await Task.Run(() => p.WaitForExit());
                        // 영어(Enabled) 및 한국어(사용) 모두 체크
                        return output.IndexOf("Enabled", StringComparison.OrdinalIgnoreCase) >= 0 || 
                               output.IndexOf("사용", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
            catch { }
            return false;
        }

        public static async Task EnableWindowsFeaturesAsync()
        {
            // Enable VirtualMachinePlatform and Microsoft-Windows-Subsystem-Linux
            string script = "Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart; " +
                            "Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart";
            await RunCommandAsync("powershell", $"-Command \"{script}\"", true);
        }

        public static async Task<bool> IsWslInstalledAsync()
        {
            try
            {
                var psi = new ProcessStartInfo("wsl", "--status")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        await Task.Run(() => p.WaitForExit());
                        return p.ExitCode == 0;
                    }
                }
            }
            catch { }
            return false;
        }

        public static async Task UpdateWslAsync()
        {
            await RunCommandAsync("wsl", "--update", true);
        }

        public static void RebootSystem()
        {
            Process.Start("shutdown", "/r /t 5");
        }

        public static void ActivateProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    IntPtr hWnd = processes[0].MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                    }
                }
            }
            catch { }
        }

        public static void MinimizeProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    IntPtr hWnd = processes[0].MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_MINIMIZE);
                    }
                }
            }
            catch { }
        }
    }
}
