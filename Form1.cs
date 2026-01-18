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

        // Legacy/Direct UI Controls (Keep for Designer compatibility)
        private NotifyIcon notifyIcon;

        public Form1()
        {
            // Performance Optimization
            PerfUtility.SetCpuAffinity(PerfUtility.CpuGroup.PCoresOnly);

            InitializeComponent();
            ApplyDarkBlueTheme();

            // MVP Initialization
            _presenter = new MainPresenter(this);

            // Tray Icon Setup
            this.notifyIcon = this.notifyIcon1;
            this.notifyIcon1.Icon = SystemIcons.Application;
            this.notifyIcon1.Text = "Pi Node Monitor Pro";
            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("열기 (Open)", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; notifyIcon1.Visible = false; });
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("종료 (Exit)", null, (s, e) => { Application.Exit(); });
            this.notifyIcon1.ContextMenuStrip = trayMenu;
            this.notifyIcon1.MouseDoubleClick += (s, e) => {
                 this.Show();
                 this.WindowState = FormWindowState.Normal;
                 notifyIcon1.Visible = false;
            };

            // Event Bindings
            this.Resize += (s,e) => {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.Hide();
                    notifyIcon1.Visible = true;
                }
            };
            
            this.Load += (s, e) => ViewLoaded?.Invoke(this, EventArgs.Empty);

            if (btnToggleNode != null) btnToggleNode.Click += (s, e) => ToggleNodeClicked?.Invoke(this, EventArgs.Empty);
            if (btnShowHistory != null) btnShowHistory.Click += (s, e) => ShowHistoryClicked?.Invoke(this, EventArgs.Empty);
            if (btnSecureTunnel != null) btnSecureTunnel.Click += (s, e) => SecureLinkClicked?.Invoke(this, EventArgs.Empty);
            if (btnMulti != null) btnMulti.Click += (s, e) => MultiViewClicked?.Invoke(this, EventArgs.Empty);
            if (btnMobile != null) btnMobile.Click += (s, e) => MobileConnectClicked?.Invoke(this, EventArgs.Empty);
            
            // Temporary: Map legacy logic for buttons not yet fully moved to Presenter
            if (btnHelp != null) btnHelp.Click += (s, e) => { using (var help = new HelpForm()) help.ShowDialog(this); };
        }

        // IMainView Implementation
        public void InvokeUI(Action action)
        {
            if (this.InvokeRequired) this.Invoke(action);
            else action();
        }

        public void UpdateWallet(WalletData wallet)
        {
            if (lblBalance != null) lblBalance.Text = $"Wallet: {wallet.Balance:N2} π";
            if (lblPrice != null) lblPrice.Text = $"Price: ${wallet.PriceUsd:F2} / {wallet.PriceKrw:N0}₩";
        }
        
        public void UpdateNodeMetrics(NodeMetrics metrics)
        {
            if (lblUptime != null) lblUptime.Text = $"Uptime: {metrics.Uptime:hh\\:mm\\:ss}";
            if (lblState != null) {
                lblState.Text = metrics.ConsensusState;
                lblState.ForeColor = metrics.ConsensusState.Contains("Synced") ? Color.Green : Color.Orange;
            }
            // Update Docker Status UI
            if (metrics.IsDockerRunning) {
                if(btnToggleNode != null) {
                    btnToggleNode.Text = "Node is ON";
                    btnToggleNode.BackColor = Color.LightGreen;
                }
                if(lblContainerStatus != null) lblContainerStatus.Text = $"Active: {metrics.ActiveContainerName}";
            } else {
                if(btnToggleNode != null) {
                    btnToggleNode.Text = "Node is OFF";
                    btnToggleNode.BackColor = Color.Salmon;
                }
                if(lblContainerStatus != null) lblContainerStatus.Text = "Stopped";
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
            // Non-blocking error display (StatusBar or Toast)
            if (statusLabel != null) {
                statusLabel.Text = $"Error: {message}";
                statusLabel.ForeColor = Color.Red;
            }
        }

        // Styling
        private void ApplyDarkBlueTheme()
        {
            this.BackColor = Color.FromArgb(40, 60, 90);
             void RecursivelyStyle(Control c)
            {
                foreach (Control child in c.Controls)
                {
                    if (child is Label || child is GroupBox || child is CheckBox || child is RadioButton)
                    {
                        child.ForeColor = Color.White;
                    }
                    if (child.HasChildren) RecursivelyStyle(child);
                }
            }
            RecursivelyStyle(this);

            if (lblBalance != null) lblBalance.ForeColor = Color.Gold;
            if (txtPublicKey != null) {
                txtPublicKey.BackColor = Color.FromArgb(30, 45, 70);
                txtPublicKey.ForeColor = Color.Yellow;
            }
        }
    }
}
