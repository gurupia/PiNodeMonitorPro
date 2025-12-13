using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm
{
    public static class NodeUtility
    {
        // Global setting for the container name we found or selected
        public static string CurrentContainerName { get; set; } = "pi-consensus";

        // 1. Check if Docker is installed and running
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

                using var process = Process.Start(psi);
                if (process == null) return false;

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // If version is returned, Docker is running
                return !string.IsNullOrWhiteSpace(output) && process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        // 2. Check if specific Docker image exists (Broad Search with Fallback)
        public static async Task<bool> IsImagePresentAsync(string imageNamePart)
        {
            string output = "";
            
            // Way 1: Direct execution
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

                using var process = Process.Start(psi);
                if (process != null)
                {
                    output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                }
            }
            catch { }

            // Way 2: Fallback to CMD if output is empty (sometimes PATH issues)
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
                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                    }
                }
                catch { }
            }

            // Check entire block of text
            return !string.IsNullOrWhiteSpace(output) && output.Contains(imageNamePart);
        }

        // 2.5 Check if specific Docker CONTAINER exists (Running or Stopped)
        // This is better than image check because it confirms the node is set up.
        public static async Task<bool> IsContainerExistAsync(string containerName)
        {
            string output = "";
            try // Direct
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
                        output = await p.StandardOutput.ReadToEndAsync(); 
                        await p.WaitForExitAsync(); 
                    }
                }
            }
            catch {}

            if (string.IsNullOrWhiteSpace(output))
            {
                try // CMD Fallback
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
                            output = await p.StandardOutput.ReadToEndAsync(); 
                            await p.WaitForExitAsync(); 
                        }
                    }
                }
                catch {}
            }

            // Check if output contains the container name
            return !string.IsNullOrWhiteSpace(output) && output.Contains(containerName);
        }

        // 3. Check if Pi Node ports are actually LISTENING (more reliable than firewall rules)
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

                using var process = Process.Start(psi);
                if (process == null) return false;

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Check if ports 31401, 31402, 31403 are in LISTENING state
                bool has31401 = output.Contains(":31401") && output.Contains("LISTENING");
                bool has31402 = output.Contains(":31402") && output.Contains("LISTENING");
                bool has31403 = output.Contains(":31403") && output.Contains("LISTENING");

                // If at least one port is listening, consider it OK (node is running)
                return has31401 || has31402 || has31403;
            }
            catch
            {
                return false;
            }
        }

        // Helper to run arbitrary commands (for fixes)
        public static async Task RunCommandAsync(string fileName, string args, bool asAdmin = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = true, // Needed for Verb generic
                CreateNoWindow = asAdmin ? false : true 
            };

            if (asAdmin)
            {
                psi.Verb = "runas"; // Request Admin
            }

            try
            {
                using var p = Process.Start(psi);
                if (p != null) await p.WaitForExitAsync();
            }
            catch { /* User might have cancelled UAC */ }
        }
    }
}
