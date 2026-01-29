using System;
using System.Drawing;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Core.MVP.Models;
using PiNodeMonitorWinForm.Core.MVP.Presenters;
using PiNodeMonitorWinForm.Core.MVP.Views;
using PiNodeMonitorWinForm.Services;

namespace PiNodeMonitorWinForm
{
    public partial class Form1 : Form, IMainView
    {
        private MainPresenter _presenter;

        // IMainView Events
        public event EventHandler ViewLoaded;
        public event EventHandler ToggleNodeClicked;
        public event EventHandler ShowHistoryClicked;
        public event EventHandler SecureLinkClicked;
        public event EventHandler MultiViewClicked;
        public event EventHandler MobileConnectClicked;
        public event EventHandler CompactDiskClicked;
        public event EventHandler SaveWalletClicked;
        public event EventHandler ChangeWalletClicked;
        
        // Menu Events
        public event EventHandler DockerRestartClicked;
        public event EventHandler DockerStopClicked;
        public event EventHandler DockerStartClicked;
        public event EventHandler DockerShowClicked;
        public event EventHandler DockerMinimizeClicked;

        public event EventHandler PiRestartClicked;
        public event EventHandler PiStopClicked;
        public event EventHandler PiStartClicked; // NEW
        public event EventHandler PiShowClicked;
        public event EventHandler PiMinimizeClicked;
        public event EventHandler DiagnosticsClicked;
        public event Action CompactClicked; // Changed to Action to match IMainView
        public event Action ThemeToggleClicked; // NEW
        public event Action<double> ManualBonusSaved;

        // Tray Icon
        private NotifyIcon notifyIcon;

        public string WalletPublicKey => txtPublicKey?.Text ?? "";

        public Form1()
        {
            PerfUtility.SetCpuAffinity(PerfUtility.CpuGroup.PCoresOnly);
            InitializeComponent();
            ApplyDarkBlueTheme();

            _presenter = new MainPresenter(this);

            // Tray Setup
            this.notifyIcon = this.notifyIcon1;
            this.notifyIcon1.Icon = SystemIcons.Application;
            this.notifyIcon1.Text = "Pi Node Monitor Pro";
            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; notifyIcon1.Visible = false; });
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (s, e) => { Application.Exit(); });
            this.notifyIcon1.ContextMenuStrip = trayMenu;
            this.notifyIcon1.MouseDoubleClick += (s, e) => {
                 this.Show();
                 this.WindowState = FormWindowState.Normal;
                 notifyIcon1.Visible = false;
            };

            // System Events
            this.Resize += (s,e) => {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.Hide();
                    notifyIcon1.Visible = true;
                }
            };
            this.Load += (s, e) => ViewLoaded?.Invoke(this, EventArgs.Empty);

            // Button Bindings
            WireButtons();
        }

        private void WireButtons()
        {
            if (btnToggleNode != null) btnToggleNode.Click += (s, e) => {
                PerfUtility.Log("Button Click: ToggleNode");
                ToggleNodeClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnShowHistory != null) btnShowHistory.Click += (s, e) => {
                PerfUtility.Log("Button Click: ShowHistory");
                ShowHistoryClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnSecureTunnel != null) btnSecureTunnel.Click += (s, e) => {
                PerfUtility.Log("Button Click: SecureTunnel");
                SecureLinkClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnMulti != null) btnMulti.Click += (s, e) => {
                PerfUtility.Log("Button Click: MultiView");
                MultiViewClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnMobile != null) btnMobile.Click += (s, e) => {
                PerfUtility.Log("Button Click: MobileConnect");
                MobileConnectClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnCompactDisk != null) btnCompactDisk.Click += (s, e) => {
                PerfUtility.Log("Button Click: CompactDisk");
                CompactDiskClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnSaveKey != null) btnSaveKey.Click += (s, e) => {
                PerfUtility.Log("Button Click: SaveWallet");
                SaveWalletClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnChangeWallet != null) btnChangeWallet.Click += (s, e) => {
                PerfUtility.Log("Button Click: ChangeWallet");
                ChangeWalletClicked?.Invoke(this, EventArgs.Empty);
            };
            if (btnSaveManualBonus != null) btnSaveManualBonus.Click += (s, e) => {
                if (double.TryParse(txtManualBonus.Text, out double bonus)) {
                    ManualBonusSaved?.Invoke(bonus);
                    MessageBox.Show("Node Bonus Saved!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else MessageBox.Show("Invalid Number format.");
            };
            if (btnHelp != null) btnHelp.Click += (s, e) => { 
                PerfUtility.Log("Button Click: Help");
                using (var help = new HelpForm()) help.ShowDialog(this); 
            };

            // Menu Bindings
            if (menuDockerRestart != null) menuDockerRestart.Click += (s, e) => DockerRestartClicked?.Invoke(this, EventArgs.Empty);
            if (menuDockerStop != null) menuDockerStop.Click += (s, e) => DockerStopClicked?.Invoke(this, EventArgs.Empty);
            if (menuDockerStart != null) menuDockerStart.Click += (s, e) => DockerStartClicked?.Invoke(this, EventArgs.Empty);
            if (menuDockerShow != null) menuDockerShow.Click += (s, e) => DockerShowClicked?.Invoke(this, EventArgs.Empty);
            if (menuDockerMinimize != null) menuDockerMinimize.Click += (s, e) => DockerMinimizeClicked?.Invoke(this, EventArgs.Empty);
            
            if (menuPiRestart != null) menuPiRestart.Click += (s, e) => PiRestartClicked?.Invoke(this, EventArgs.Empty);
            if (menuPiStop != null) menuPiStop.Click += (s, e) => PiStopClicked?.Invoke(this, EventArgs.Empty);
            if (menuPiStart != null) menuPiStart.Click += (s, e) => PiStartClicked?.Invoke(this, EventArgs.Empty);
            if (menuPiShow != null) menuPiShow.Click += (s, e) => PiShowClicked?.Invoke(this, EventArgs.Empty);
            if (menuPiMinimize != null) menuPiMinimize.Click += (s, e) => PiMinimizeClicked?.Invoke(this, EventArgs.Empty);
            if (menuSetupWizard != null) menuSetupWizard.Click += (s, e) => {
                using (var wizard = new SetupWizardForm()) wizard.ShowDialog(this);
            };
            if (menuDiagnostics != null) menuDiagnostics.Click += (s, e) => DiagnosticsClicked?.Invoke(this, EventArgs.Empty);
            if (menuCompact != null) menuCompact.Click += (s, e) => CompactClicked?.Invoke();
            if (menuTheme != null) menuTheme.Click += (s, e) => ThemeToggleClicked?.Invoke();
        }

        public void InvokeUI(Action action)
        {
            if (this.InvokeRequired) this.Invoke(action);
            else action();
        }

        public void UpdateWallet(WalletData wallet)
        {
            if (lblBalance != null) lblBalance.Text = $"Wallet: {wallet.Balance:N2} π";
            if (lblPrice != null) lblPrice.Text = $"Price: ${wallet.PriceUsd:F2}";
            if (lblTotalValue != null) lblTotalValue.Text = $"Value: ${wallet.TotalValueUsd:N2}";
            if (txtPublicKey != null && string.IsNullOrEmpty(txtPublicKey.Text)) txtPublicKey.Text = wallet.PublicKey;
        }
        
        public void UpdateNodeMetrics(NodeMetrics m)
        {
            // Node Control
            if (lblLocalBlockNum != null) lblLocalBlockNum.Text = m.LocalBlockNum;
            if (lblRemoteBlockNum != null) lblRemoteBlockNum.Text = m.RemoteBlockNum;
            
            // Consensus
            if (lblState != null) {
                lblState.Text = m.ConsensusState;
                lblState.ForeColor = m.ConsensusState.Contains("Synced") ? Color.LimeGreen : Color.Orange;
            }
            if (lblLatestBlock != null) lblLatestBlock.Text = m.LatestLedgerNum;
            if (lblLedgerAge != null) lblLedgerAge.Text = m.LedgerAge;

            // General Info
            if (lblProtocolVersion != null) lblProtocolVersion.Text = m.ProtocolVersion;
            if (lblStellarBuild != null) lblStellarBuild.Text = m.CoreBuild;
            if (lblStellarBuild != null) lblStellarBuild.Text = m.CoreBuild;
            if (lblLocalCpuCount != null) lblLocalCpuCount.Text = m.LocalCpuCount; // CPU is incorrectly empty in Presenter, will accept value here

            // Container Resources
            if (lblContainerStatus != null) {
                lblContainerStatus.Text = m.ActiveContainerName;
                lblContainerStatus.ForeColor = m.IsDockerRunning ? Color.Yellow : Color.Salmon;
            }
            if (lblCPU != null) lblCPU.Text = m.ContainerCpuUsage;
            if (lblRAM != null) lblRAM.Text = m.ContainerRamUsage;

            // Network Stats
            if (lblIncoming != null) lblIncoming.Text = m.IncomingConnections;
            if (lblOutgoing != null) lblOutgoing.Text = m.OutgoingConnections;
            if (lblSupporting != null) lblSupporting.Text = m.IsSupporting;

            // Session Stats
            if (lblUptime != null) lblUptime.Text = $"Uptime: {m.Uptime:hh\\:mm\\:ss}";
            if (lblAvailability != null) lblAvailability.Text = $"Availability: {m.Availability}";

            // Port Status
            // Port Status
            if (lblPort01 != null) { lblPort01.Text = m.Port31401Status; lblPort01.ForeColor = m.Port31401Status == "Open" ? Color.LimeGreen : Color.Salmon; }
            if (lblPort02 != null) { lblPort02.Text = m.Port31402Status; lblPort02.ForeColor = m.Port31402Status == "Open" ? Color.LimeGreen : Color.Salmon; }
            if (lblPort03 != null) { lblPort03.Text = m.Port31403Status; lblPort03.ForeColor = m.Port31403Status == "Open" ? Color.LimeGreen : Color.Salmon; }

            // Node Button State
            if(btnToggleNode != null) {
                btnToggleNode.Text = m.IsDockerRunning ? "Node is ON (Click to OFF)" : "Node is OFF (Click to ON)";
                btnToggleNode.ForeColor = m.IsDockerRunning ? Color.LimeGreen : Color.Salmon;
            }
        }

        public void UpdateSystemStatus(string status, Color color)
        {
            if (statusLabel != null) {
                statusLabel.Text = status;
                statusLabel.ForeColor = color;
            }
        }

        public void ShowError(string message)
        {
            if (statusLabel != null) {
                statusLabel.Text = $"Error: {message}";
                statusLabel.ForeColor = Color.Red;
            }
        }

        public void UpdateSecureLinkStatus(string url, bool active)
        {
            if (lblTunnelLink != null) {
                lblTunnelLink.Text = active ? $"Secure URL: {url}" : "Secure URL: Not Active";
                lblTunnelLink.ForeColor = active ? Color.LimeGreen : Color.LightGray;
            }
        }

        public void SetManualBonus(double bonus)
        {
            InvokeUI(() => {
                txtManualBonus.Text = bonus.ToString("F4");
            });
        }

        public void ToggleTheme(bool isDark)
        {
            Color backColor = isDark ? Color.FromArgb(25, 25, 25) : Color.WhiteSmoke;
            Color foreColor = isDark ? Color.White : Color.Black;
            Color secondaryBack = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240);
            
            this.BackColor = backColor;
            this.ForeColor = foreColor;

            if (menuStrip1 != null) { menuStrip1.BackColor = secondaryBack; menuStrip1.ForeColor = foreColor; }
            if (statusStrip1 != null) { statusStrip1.BackColor = secondaryBack; statusStrip1.ForeColor = foreColor; }
            if (menuTools != null) menuTools.ForeColor = foreColor;

            void RecursivelyStyle(Control c)
            {
                foreach (Control child in c.Controls)
                {
                    if (child is GroupBox gb) gb.ForeColor = foreColor;
                    if (child is CheckBox chk) chk.ForeColor = foreColor;
                    if (child is RadioButton rb) rb.ForeColor = foreColor;
                    if (child is Label lbl) {
                        // Don't overwrite colored status labels if possible, but hard to distinguish by name dynamically.
                        // We will rely on Presenter to refresh data/colors immediately after toggle.
                        // For static labels (Title), set color.
                        lbl.ForeColor = foreColor; 
                    }
                    
                    if (child.HasChildren) RecursivelyStyle(child);
                }
            }
            RecursivelyStyle(this);

            // Special overrides
            if (lblBalance != null) lblBalance.ForeColor = isDark ? Color.Gold : Color.DarkGoldenrod;
            if (txtPublicKey != null) {
                txtPublicKey.BackColor = isDark ? Color.FromArgb(25, 35, 50) : Color.White;
                txtPublicKey.ForeColor = foreColor;
            }
            
            // Re-apply special static logic
            if (lblTunnelLink != null && lblTunnelLink.Text.Contains("Not Active")) lblTunnelLink.ForeColor = Color.LightGray;
        }

        private void ApplyDarkBlueTheme() { ToggleTheme(true); } // Legacy wrapper if needed
    }
}
