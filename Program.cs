using System;
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

            // --- Smart Pre-flight Check ---
            // 0. Check Windows Environment (WSL2, Platform, etc.)
            bool vmp = NodeUtility.IsWindowsFeatureEnabledAsync("VirtualMachinePlatform").GetAwaiter().GetResult();
            bool wsl = NodeUtility.IsWindowsFeatureEnabledAsync("Microsoft-Windows-Subsystem-Linux").GetAwaiter().GetResult();
            bool wslInstalled = NodeUtility.IsWslInstalledAsync().GetAwaiter().GetResult();
            bool checkEnvironment = vmp && wsl && wslInstalled;

            // 1. Check Docker
            bool checkDocker = NodeUtility.IsDockerRunningAsync().GetAwaiter().GetResult();
            
            // 2. Check Container (Confirm Node is Set Up)
            bool checkContainer = false;
            if (checkDocker)
            {
                // We look for 'pi-consensus' OR 'testnet2' container
                if (NodeUtility.IsContainerExistAsync("pi-consensus").GetAwaiter().GetResult())
                {
                    checkContainer = true;
                    NodeUtility.CurrentContainerName = "pi-consensus";
                }
                else if (NodeUtility.IsContainerExistAsync("testnet2").GetAwaiter().GetResult())
                {
                    checkContainer = true;
                    NodeUtility.CurrentContainerName = "testnet2";
                }
            }

            // 3. Check Firewall (Simplified)
            bool checkFirewall = NodeUtility.IsFirewallRulePresentAsync().GetAwaiter().GetResult();

            // Logic: If anything is missing, Show Wizard
            if (!checkEnvironment || !checkDocker || !checkContainer || !checkFirewall)
            {
                using (var wizard = new SetupWizardForm())
                {
                    var result = wizard.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        // User cancelled the wizard, exit app
                        return;
                    }
                }
            }
            // ------------------------------

            Application.Run(new Form1());
        }
    }
}