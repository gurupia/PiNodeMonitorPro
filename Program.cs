using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run pre-flight checks asynchronously to avoid UI thread blocking
            var preflightResult = Task.Run(async () => await RunPreflightChecksAsync()).GetAwaiter().GetResult();

            if (!preflightResult.AllPassed)
            {
                using (var wizard = new SetupWizardForm())
                {
                    var result = wizard.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }
            }

            Application.Run(new Form1());
        }

        private static async Task<PreflightResult> RunPreflightChecksAsync()
        {
            var result = new PreflightResult();

            // Run checks in parallel for faster startup
            var vmpTask = NodeUtility.IsWindowsFeatureEnabledAsync("VirtualMachinePlatform");
            var wslTask = NodeUtility.IsWindowsFeatureEnabledAsync("Microsoft-Windows-Subsystem-Linux");
            var wslInstalledTask = NodeUtility.IsWslInstalledAsync();
            var dockerTask = NodeUtility.IsDockerRunningAsync();

            await Task.WhenAll(vmpTask, wslTask, wslInstalledTask, dockerTask);

            result.EnvironmentOk = vmpTask.Result && wslTask.Result && wslInstalledTask.Result;
            result.DockerOk = dockerTask.Result;

            // Container check (depends on Docker)
            if (result.DockerOk)
            {
                if (await NodeUtility.IsContainerExistAsync("pi-consensus"))
                {
                    result.ContainerOk = true;
                    NodeUtility.CurrentContainerName = "pi-consensus";
                }
                else if (await NodeUtility.IsContainerExistAsync("testnet2"))
                {
                    result.ContainerOk = true;
                    NodeUtility.CurrentContainerName = "testnet2";
                }
            }

            result.FirewallOk = await NodeUtility.IsFirewallRulePresentAsync();

            return result;
        }

        private class PreflightResult
        {
            public bool EnvironmentOk { get; set; }
            public bool DockerOk { get; set; }
            public bool ContainerOk { get; set; }
            public bool FirewallOk { get; set; }
            public bool AllPassed => EnvironmentOk && DockerOk && ContainerOk && FirewallOk;
        }
    }
}
