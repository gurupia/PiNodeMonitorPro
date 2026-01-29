using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm
{
    public class DiagnosticsForm : Form
    {
        private ListBox lstLog;
        private Button btnStart;
        private Button btnFixWsl;
        private Button btnFixDocker;
        private Button btnFixFirewall;
        private ProgressBar progressBar;
        private Label lblStatus;

        public DiagnosticsForm()
        {
            this.Text = "System Health Diagnostics";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            ApplyTheme();

            InitializeUI();
        }

        private void ApplyTheme()
        {
            bool isDark = NodeUtility.Config.IsDarkMode;
            this.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.WhiteSmoke;
            this.ForeColor = isDark ? Color.White : Color.Black;
            if (lstLog != null)
            {
                lstLog.BackColor = isDark ? Color.FromArgb(45, 45, 48) : Color.White;
                lstLog.ForeColor = isDark ? Color.LightGray : Color.Black;
            }
        }

        private void InitializeUI()
        {
            lblStatus = new Label { Text = "Ready to scan...", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            this.Controls.Add(lblStatus);

            progressBar = new ProgressBar { Location = new Point(20, 50), Size = new Size(540, 20) };
            this.Controls.Add(progressBar);

            lstLog = new ListBox { Location = new Point(20, 90), Size = new Size(540, 250), BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.LightGray, Font = new Font("Consolas", 9) };
            this.Controls.Add(lstLog);

            btnStart = new Button { Text = "Start Diagnosis", Location = new Point(20, 360), Size = new Size(120, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnStart.Click += async (s, e) => await RunDiagnosticsAsync();
            this.Controls.Add(btnStart);

            // Dynamic Fix Buttons (Initially Hidden)
            btnFixWsl = CreateFixButton("Enable WSL Features", 150, async () => {
                await NodeUtility.EnableWindowsFeaturesAsync();
                MessageBox.Show("Enabled features. Please Reboot.", "Reboot Required");
            });
            btnFixDocker = CreateFixButton("Start Docker", 150, async () => {
                NodeUtility.ActivateProcess("Docker Desktop"); // Try activate first
                 // If needed, we could run logic to start it, but usually user installed it.
                 MessageBox.Show("Please launch Docker Desktop manually if it doesn't open.", "Info");
            });
            btnFixFirewall = CreateFixButton("Open Firewall Ports", 280, async () => {
                 await NodeUtility.RunCommandAsync("powershell", "New-NetFirewallRule -DisplayName 'Pi Network' -Direction Inbound -LocalPort 31400-31409 -Protocol TCP -Action Allow", true);
                 MessageBox.Show("Firewall rules applied.", "Success");
            });

            Button btnClose = new Button { Text = "Close", Location = new Point(460, 360), Size = new Size(100, 35), BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private Button CreateFixButton(string text, int x, Func<Task> action)
        {
            var btn = new Button { 
                Text = text, 
                Location = new Point(x, 360), 
                Size = new Size(120, 35), 
                BackColor = Color.IndianRed, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Visible = false 
            };
            btn.Click += async (s, e) => { await action(); btn.Visible = false; await RunDiagnosticsAsync(); }; // Re-run after fix
            this.Controls.Add(btn);
            return btn;
        }

        private void Log(string msg, Color? color = null)
        {
            if (lstLog.InvokeRequired) { lstLog.Invoke(new Action(() => Log(msg, color))); return; }
            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private async Task RunDiagnosticsAsync()
        {
            btnStart.Enabled = false;
            btnFixWsl.Visible = false;
            btnFixDocker.Visible = false;
            btnFixFirewall.Visible = false;
            lstLog.Items.Clear();
            progressBar.Value = 0;

            Log("Starting System Diagnostics...");
            
            // Check 1: Windows Features
            lblStatus.Text = "Checking Windows Features...";
            progressBar.Value = 20;
            bool vmp = await NodeUtility.IsWindowsFeatureEnabledAsync("VirtualMachinePlatform");
            Log($"Virtual Machine Platform: {(vmp ? "OK" : "DISABLED")}");
            if (!vmp) btnFixWsl.Visible = true;
            
            // Check 2: WSL
            progressBar.Value = 40;
            bool wsl = await NodeUtility.IsWslInstalledAsync();
            Log($"WSL2 Subsystem: {(wsl ? "Installed" : "MISSING")}");
            if (!wsl) btnFixWsl.Visible = true;

            // Check 3: Docker
            lblStatus.Text = "Checking Docker...";
            progressBar.Value = 60;
            bool docker = await NodeUtility.IsDockerRunningAsync();
            Log($"Docker Desktop: {(docker ? "Running" : "STOPPED")}");
            if (!docker) btnFixDocker.Visible = true;

            // Check 4: Container
            progressBar.Value = 80;
            string container = NodeUtility.CurrentContainerName;
            bool containerExists = await NodeUtility.IsContainerExistAsync(container);
            Log($"Consensus Container ({container}): {(containerExists ? "Found" : "MISSING")}");

            // Check 5: Ports
            lblStatus.Text = "Checking Firewall...";
            progressBar.Value = 90;
            bool fw = await NodeUtility.IsFirewallRulePresentAsync();
            Log($"Firewall Rules: {(fw ? "Active" : "MISSING")}");
            if (!fw) btnFixFirewall.Visible = true;

            progressBar.Value = 100;
            lblStatus.Text = "Diagnosis Complete.";
            btnStart.Enabled = true;

            if (vmp && wsl && docker && containerExists && fw)
                MessageBox.Show("All systems look healthy!", "Diagnostics Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Issues detected. Please review the log.", "Issues Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
