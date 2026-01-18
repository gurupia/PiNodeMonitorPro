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
        public event EventHandler CompactClicked; // NEW

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
            if (menuDiagnostics != null) menuDiagnostics.Click += (s, e) => DiagnosticsClicked?.Invoke(this, EventArgs.Empty);
            if (menuCompact != null) menuCompact.Click += (s, e) => CompactClicked?.Invoke(this, EventArgs.Empty);
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

        private void ApplyDarkBlueTheme()
        {
            this.BackColor = Color.FromArgb(25, 25, 25);
            void RecursivelyStyle(Control c)
            {
                foreach (Control child in c.Controls)
                {
                    if (child is Label || child is GroupBox || child is CheckBox || child is RadioButton)
                        child.ForeColor = Color.White;
                    if (child.HasChildren) RecursivelyStyle(child);
                }
            }
            RecursivelyStyle(this);
            if (lblBalance != null) lblBalance.ForeColor = Color.Gold;
            if (txtPublicKey != null) {
                txtPublicKey.BackColor = Color.FromArgb(25, 35, 50);
                txtPublicKey.ForeColor = Color.White;
            }
        }
    }
}
