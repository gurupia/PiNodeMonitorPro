using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json.Nodes;
using PiNodeMonitorWinForm.Services;

namespace PiNodeMonitorWinForm
{
    public partial class Form1 : Form
    {
        // Stats
        private string _statState = "Unknown";
        private int _statIn = 0;
        private int _statOut = 0;
        private string _statLocalBlock = "0";
        private int _statLedgerAge = 0;
        private int _lastIncomingCount = -1;
        private bool _wasSynced = false;
        private int _totalSeconds = 0;
        private int _totalSyncedSeconds = 0;
        
        // Timer
        private readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        private Button btnMobile; 

        // Wallet Controls
        private TextBox txtPublicKey;
        private Label lblBalance;
        private Button btnSaveKey;
        private Button btnChangeWallet;
        private WalletService _walletService;
        private NotifyIcon notifyIcon;
        private decimal _lastBalance = -1; 
        
        public Form1()
        {
            InitializeComponent();
            this.Height += 120; // Increased height for Wallet UI + Footer
            this.Text = "Pi Node Monitor Pro (Fixed v2)";
            
            try 
            { 
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("PiNodeMonitorWinForm.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            } 
            catch { }

            // Initialize Notification
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = this.Icon;
            notifyIcon.Visible = true;
            notifyIcon.Text = "Pi Node Monitor Pro";
            
            // Start Mobile Server
            Task.Run(() => MobileServer.StartServerAsync());

            // ---------------------------------------------------------
            // Wallet UI Implementation (Clean Dashboard Mode)
            // ---------------------------------------------------------
            int baseY = this.ClientSize.Height - 160; 
            
            lblBalance = new Label();
            lblBalance.Text = "Wallet: -- π";
            lblBalance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblBalance.ForeColor = Color.Gold;
            lblBalance.BackColor = Color.Transparent;
            lblBalance.AutoSize = true;
            lblBalance.Location = new Point(20, baseY);
            this.Controls.Add(lblBalance);

            txtPublicKey = new TextBox();
            txtPublicKey.PlaceholderText = "Paste Public Key (G...)";
            txtPublicKey.Size = new Size(420, 25); // Widened to 420px
            txtPublicKey.Location = new Point(20, baseY + 25);
            txtPublicKey.BackColor = Color.FromArgb(40, 40, 40);
            txtPublicKey.ForeColor = Color.White;
            txtPublicKey.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtPublicKey);

            btnSaveKey = new Button();
            btnSaveKey.Text = "💾";
            btnSaveKey.Size = new Size(30, 25);
            btnSaveKey.Location = new Point(450, baseY + 25); // Adjusted X to 450
            btnSaveKey.FlatStyle = FlatStyle.Flat;
            btnSaveKey.ForeColor = Color.White;
            this.Controls.Add(btnSaveKey);

            btnChangeWallet = new Button();
            btnChangeWallet.Text = "Change Address";
            btnChangeWallet.Size = new Size(120, 25);
            btnChangeWallet.Location = new Point(20, baseY + 25);
            btnChangeWallet.FlatStyle = FlatStyle.Flat;
            btnChangeWallet.ForeColor = Color.Gray; 
            btnChangeWallet.Cursor = Cursors.Hand;
            this.Controls.Add(btnChangeWallet);

            // Service Init
            _walletService = new WalletService();

            // Toggle Logic
            Action<bool> ToggleWalletEdit = (editing) => {
                txtPublicKey.Visible = editing;
                btnSaveKey.Visible = editing;
                btnChangeWallet.Visible = !editing;
                if (editing) txtPublicKey.Focus();
            };

            btnChangeWallet.Click += (s, e) => ToggleWalletEdit(true);

            btnSaveKey.Click += (s, e) => {
                _walletService.SaveKey(txtPublicKey.Text);
                MessageBox.Show("Wallet Key Saved!");
                UpdateWalletBalanceAsync(); 
                ToggleWalletEdit(false);
            };

            // Load saved key state
            if (!string.IsNullOrEmpty(_walletService.PublicKey)) {
                txtPublicKey.Text = _walletService.PublicKey;
                ToggleWalletEdit(false); 
            } else {
                ToggleWalletEdit(true); 
            }
            // ---------------------------------------------------------
            // Mobile Connect Button
            // ---------------------------------------------------------
            btnMobile = new Button();
            btnMobile.Text = "📱 Mobile Connect";
            btnMobile.Size = new Size(140, 40);
            // Position above Footer (35px) with padding
            btnMobile.Location = new Point(this.ClientSize.Width - 155, this.ClientSize.Height - 100); 
            btnMobile.BackColor = Color.RebeccaPurple;
            btnMobile.ForeColor = Color.White;
            btnMobile.FlatStyle = FlatStyle.Flat;
            btnMobile.FlatAppearance.BorderSize = 0;
            btnMobile.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMobile.Cursor = Cursors.Hand;
            btnMobile.Click += (s, e) => {
                try {
                    using (var qr = new QRForm($"http://{MobileServer.CurrentIpAddress}:{MobileServer.Port}")) {
                        qr.ShowDialog(this);
                    }
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            };
            this.Controls.Add(btnMobile);
            btnMobile.BringToFront();

            // ---------------------------------------------------------
            // Dark Footer Implementation
            // ---------------------------------------------------------
            Panel pnlFooter = new Panel();
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Height = 35; // Slightly taller for better spacing
            pnlFooter.BackColor = Color.FromArgb(32, 32, 32); // Deep Dark Grey

            Label lblCopy = new Label();
            lblCopy.Text = "Copyright © 2025 GuruPia. All rights reserved.";
            lblCopy.ForeColor = Color.LightGray;
            lblCopy.Font = new Font("Segoe UI", 9);
            lblCopy.AutoSize = true;
            lblCopy.Location = new Point(10, 8); // Manual positioning or Dock Left with padding
            
            LinkLabel lnkDev = new LinkLabel();
            lnkDev.Text = "Developed by gurupia.github.io";
            lnkDev.LinkColor = Color.Gold; // Gold stands out well on dark
            lnkDev.ActiveLinkColor = Color.Yellow;
            lnkDev.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lnkDev.AutoSize = true;
            lnkDev.Cursor = Cursors.Hand;
            lnkDev.LinkClicked += (s, e) => {
                try { 
                    Process.Start(new ProcessStartInfo { FileName = "https://gurupia.github.io", UseShellExecute = true }); 
                } catch {}
            };
            
            // Add to panel (Right align link manually or use Dock)
            pnlFooter.Controls.Add(lblCopy);
            pnlFooter.Controls.Add(lnkDev);
            
            // Layout logic for right alignment since Dock=Right can be tricky with AutoSize labels
            pnlFooter.Resize += (s, e) => {
                lnkDev.Location = new Point(pnlFooter.Width - lnkDev.Width - 15, 8);
            };

            this.Controls.Add(pnlFooter);
            pnlFooter.BringToFront(); // Ensure it sits on top of everything at bottom
            
            // Initial positioning trigger
            lnkDev.Location = new Point(pnlFooter.Width - lnkDev.Width - 15, 8);
            
            // Link existing Timer or create one... (continuation of existing code)
            if (timer1 != null)
            {
                timer1.Interval = 3000;
                timer1.Tick -= timer1_Tick; // Clear previous if any
                timer1.Tick += async (s, e) => await UpdateDashboardAsync();
                timer1.Start();
            }

            // Initial Load
            _ = UpdateDashboardAsync();
            
            // Apply Image 1 Style Theme
            ApplyDarkBlueTheme();
        }

        private void ApplyDarkBlueTheme()
        {
            // Deep Blue Background (Image 1 Style)
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

            // Restore/Force Specific Colors
            if (lblBalance != null) lblBalance.ForeColor = Color.Gold;
            
            // TextBoxes
            if (txtPublicKey != null) {
                txtPublicKey.BackColor = Color.FromArgb(30, 45, 70);
                txtPublicKey.ForeColor = Color.Yellow;
            }
        }

        // Designer event stub
        private void timer1_Tick(object sender, EventArgs e) { }
        private void btnToggleNode_Click(object sender, EventArgs e) { _ = ToggleNodeAsync(); }

        private async Task UpdateWalletBalanceAsync()
        {
            var bal = await _walletService.GetBalanceAsync();
            if (bal.HasValue)
            {
                decimal dBal = bal.Value;

                // Alert Logic
                if (_lastBalance != -1 && dBal > _lastBalance)
                {
                    decimal diff = dBal - _lastBalance;
                    try { System.Media.SystemSounds.Exclamation.Play(); } catch {}
                    
                    // Log Deposit
                    _walletService.LogDeposit(diff, dBal);

                    if (notifyIcon != null)
                        notifyIcon.ShowBalloonTip(7000, "💰 Deposit Detected!", $"+{diff:0.#####} π Received!\nTotal: {dBal:N2} π", ToolTipIcon.Info);
                }

                _lastBalance = dBal;
                if (lblBalance != null) lblBalance.Text = $"Wallet: {dBal:N2} π";
            }
        }

        private async Task UpdateDashboardAsync()
        {
            try 
            {
                // Also update wallet occasionally
                await UpdateWalletBalanceAsync();

                // Update Uptime
                _totalSeconds += 3;
                TimeSpan t = TimeSpan.FromSeconds(_totalSeconds);
                lblUptime.Text = $"Uptime: {t:hh\\:mm\\:ss}";

                // 1. Check if Docker is running (Robust Check)
                // Try multiple connection attempts for stability
                // Added "testnet2" based on user feedback
                string[] containers = { "pi-consensus", "stellar-dummy", "pi-node", "stellar-core", "testnet2" }; 
                bool foundRunning = false;
                string activeContainer = "";

                foreach (var name in containers)
                {
                    // Check strict 'true'
                    string inspect = await RunDockerCommandAsync($"inspect -f \"{{{{.State.Running}}}}\" {name}");
                    if (!string.IsNullOrWhiteSpace(inspect) && inspect.Trim().ToLower().Contains("true"))
                    {
                        activeContainer = name;
                        foundRunning = true;
                        break;
                    }
                }
                
                // If not found, try generic ps
                if (!foundRunning)
                {
                    string ps = await RunDockerCommandAsync("ps --format \"{{.Names}}\"");
                    if (!string.IsNullOrWhiteSpace(ps) && (ps.Contains("pi-consensus") || ps.Contains("stellar-dummy")))
                    {
                         activeContainer = ps.Contains("pi-consensus") ? "pi-consensus" : "stellar-dummy";
                         foundRunning = true;
                    }
                }

                NodeUtility.CurrentContainerName = foundRunning ? activeContainer : "pi-consensus";

                // UI Update for Container
                if (foundRunning)
                {
                    btnToggleNode.Text = "Node is ON (Click to OFF)";
                    btnToggleNode.BackColor = Color.LightGreen;
                    
                    // Fetch Stats (CPU/RAM)
                    string output = await RunDockerCommandAsync($"stats {activeContainer} --no-stream --format \"{{{{.CPUPerc}}}}|{{{{.MemUsage}}}}\"");
                     if (!string.IsNullOrWhiteSpace(output) && output.Contains("|"))
                     {
                        var parts = output.Split('|');
                        if (parts.Length >= 2) {
                            lblCPU.Text = parts[0].Trim();
                            lblRAM.Text = parts[1].Trim(); // Ensure only the first part of RAM is taken if formatted like 10MB / 2GB
                        }
                     }
                    
                    lblContainerStatus.Text = $"Active: {activeContainer}";
                    lblContainerStatus.ForeColor = Color.Blue;
                }
                else
                {
                    btnToggleNode.Text = "Node is OFF (Click to ON)";
                    btnToggleNode.BackColor = Color.Salmon;
                    lblContainerStatus.Text = "Stopped";
                    lblContainerStatus.ForeColor = Color.Red;
                    lblCPU.Text = "0%";
                    lblRAM.Text = "0MB";
                }

                // 2. Port Checks ... (No changes needed here, but included for context flow)

                // 2. Port Checks
                bool p1 = await CheckPortOpenAsync(31401);
                bool p2 = await CheckPortOpenAsync(31402);
                bool p3 = await CheckPortOpenAsync(31403);
                
                lblPort01.Text = p1 ? "Open" : "Closed";
                lblPort01.ForeColor = p1 ? Color.Green : Color.Red;
                lblPort02.Text = p2 ? "Open" : "Closed";
                lblPort02.ForeColor = p2 ? Color.Green : Color.Red;
                lblPort03.Text = p3 ? "Open" : "Closed";
                lblPort03.ForeColor = p3 ? Color.Green : Color.Red;

                // 3. Node Info & Metrics
                string state = "Unknown";
                int outgoing = 0;
                int incoming = 0;
                bool metricsSuccess = false;

                try 
                {
                    using var ctsMetric = new System.Threading.CancellationTokenSource(1500);
                    string metrics = await client.GetStringAsync("http://localhost:31403/metrics", ctsMetric.Token);
                    
                    int mOut = ParseMetricValue(metrics, "stellar_node_peers_connected_outbound");
                    int mIn = ParseMetricValue(metrics, "stellar_node_peers_connected_inbound");
                    int mAuth = ParseMetricValue(metrics, "stellar_node_peers_authenticated_count"); // Total Authenticated
                    int mAge = ParseMetricValue(metrics, "stellar_node_ledger_age_seconds");
                    int mLedger = ParseMetricValue(metrics, "stellar_node_ledger_ledger");
                    
                    if (mOut > 0 || mIn > 0 || mLedger > 0)
                    {
                        if (mAuth > 0)
                        {
                            // [Algorithm v3] Authenticated-based Logic (Most Accurate to Pi App)
                            // Pi App only counts Authenticated peers.
                            // Outgoing is hard-capped at 8 for Authenticated.
                            // Incoming is the rest.
                            outgoing = (mOut > 8) ? 8 : mOut;
                            incoming = mAuth - outgoing;
                            if (incoming < 0) incoming = 0;
                        }
                        else
                        {
                            // Fallback: Raw Connections with Clamp
                            outgoing = mOut;
                            incoming = mIn;
                            
                            if (outgoing > 8)
                            {
                                incoming += (outgoing - 8);
                                outgoing = 8;
                            }
                        }

                        _statLedgerAge = mAge;
                        _statLocalBlock = mLedger.ToString();
                        
                        if (mAge < 10) state = "Synced!";
                        else if (mAge < 60) state = "Catching up";
                        else state = "Not Synced";
                        
                        metricsSuccess = true;
                    }
                }
                catch { }

                // Fallback to Info API ... (Logic continues)

                // Fallback to Info API / Docker Exec if Metrics failed
                if (!metricsSuccess)
                {
                    if (foundRunning)
                     {
                         // Try one last attempt via Docker Exec Curl (Port 11626 is internal)
                         string infoJson = await RunDockerCommandAsync($"exec {activeContainer} curl -s --max-time 2 http://localhost:11626/info");
                         
                         if (!string.IsNullOrWhiteSpace(infoJson) && infoJson.Contains("ledger"))
                         {
                             state = "Synced (Docker)";
                             
                             try 
                             {
                                 // Robust Regex Parsing (No dependency on JSON structure)
                                 var regNum = new System.Text.RegularExpressions.Regex("\"num\"\\s*:\\s*(\\d+)");
                                 var matchNum = regNum.Match(infoJson);
                                 if (matchNum.Success) 
                                 {
                                     _statLocalBlock = matchNum.Groups[1].Value;
                                 }

                                 var regAuth = new System.Text.RegularExpressions.Regex("\"authenticated_count\"\\s*:\\s*(\\d+)");
                                 var matchAuth = regAuth.Match(infoJson);
                                 if (matchAuth.Success && int.TryParse(matchAuth.Groups[1].Value, out int totalAuth))
                                 {
                                     outgoing = (totalAuth > 8) ? 8 : totalAuth;
                                     incoming = totalAuth - outgoing;
                                     if (incoming < 0) incoming = 0;
                                 }
                             }
                             catch {}
                         }
                         else
                         {
                             state = "Running (No Metrics)";
                         }
                     }
                     else 
                     {
                         state = "Stopped";
                     }
                }

                _statState = state;
                
                // Update Status Labels
                if (state == "Synced!")
                {
                    lblMainStatus.Text = "Your computer is running the blockchain";
                    lblMainStatus.ForeColor = Color.Green;
                    _totalSyncedSeconds += 3;
                }
                else
                {
                    lblMainStatus.Text = state;
                    lblMainStatus.ForeColor = Color.Orange;
                }
                
                lblLocalBlockNum.Text = _statLocalBlock;
                
                // Note: Designer names might differ slightly, checking Designer file
                // Designer: lblLatestBlock (not lblLatestBlockNum?), lblLedgerAge
                // Let's use names from Designer file.
                if (lblLatestBlock != null) lblLatestBlock.Text = _statLocalBlock; // Reusing local block for latest
                if (lblLedgerAge != null) lblLedgerAge.Text = $"{_statLedgerAge} sec"; 
                if (lblProtocolVersion != null) lblProtocolVersion.Text = "Latest"; 
                if (lblStellarBuild != null) lblStellarBuild.Text = "stellar-core";

                // =========================================================
                // [FINAL CHECK] Enforce Outgoing Cap (Universal Rule)
                // =========================================================
                if (outgoing > 8)
                {
                    incoming += (outgoing - 8);
                    outgoing = 8;
                }

                lblIncoming.Text = incoming.ToString();
                lblOutgoing.Text = outgoing.ToString();
                
                string debugSource = metricsSuccess ? "Metrics" : "Info/Docker";
                if (incoming > 0)
                {
                    lblSupporting.Text = $"Yes ({debugSource})";
                    lblSupporting.ForeColor = Color.Green;
                }
                else
                {
                    lblSupporting.Text = $"No ({debugSource})";
                    lblSupporting.ForeColor = Color.Black;
                }

                _statIn = incoming;
                _statOut = outgoing;
                
                // Update Session Avail
                double avail = _totalSeconds > 0 ? (double)_totalSyncedSeconds / _totalSeconds * 100.0 : 0;
                lblAvailability.Text = $"Availability: {avail:F2}%";

                // Sync to Mobile
                MobileServer.CurrentStatus = new NodeStatusData
                {
                    State = _statState,
                    Incoming = _statIn,
                    Outgoing = _statOut,
                    LocalBlock = _statLocalBlock,
                    ProtocolVersion = lblProtocolVersion?.Text ?? "Unknown",
                    LedgerAge = _statLedgerAge,
                    Uptime = lblUptime.Text.Replace("Uptime: ", "")
                };
                
                // StatusStrip update
                 if (statusLabel != null)
                    statusLabel.Text = $"Synced | Container: {NodeUtility.CurrentContainerName} | Last Update: {DateTime.Now:HH:mm:ss}";

            }
            catch (Exception ex)
            {
                 // Ignore UI update errors
                 System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private async Task ToggleNodeAsync()
        {
             // Simple toggle used in Designer event
             // Implementation:
             if (btnToggleNode.Text.Contains("OFF")) // Currently ON
             {
                 await RunDockerCommandAsync("stop pi-consensus");
             }
             else
             {
                 await RunDockerCommandAsync("start pi-consensus");
             }
             await UpdateDashboardAsync();
        }
        
        private void CheckWarnings(string state, int incoming) { }

        private async Task<bool> CheckPortOpenAsync(int port)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    var result = client.BeginConnect("127.0.0.1", port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    if (!success) return false;
                    client.EndConnect(result);
                    return true;
                }
                catch { return false; }
            });
        }

        private async Task<string?> RunDockerCommandAsync(string arguments)
        {
            return await Task.Run(() =>
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
                    using var process = Process.Start(psi);
                    if (process == null) return null;
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(3000);
                    return output;
                }
                catch { return null; }
            });
        }

        private int ParseMetricValue(string data, string key)
        {
            try
            {
                var lines = data.Split('\n');
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                    if (trimmed.StartsWith(key))
                    {
                        bool isExactMatch = false;
                        if (trimmed.Length == key.Length) isExactMatch = true; 
                        else 
                        {
                            char nextChar = trimmed[key.Length];
                            if (nextChar == ' ' || nextChar == '{') isExactMatch = true;
                        }

                        if (isExactMatch)
                        {
                            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2 && int.TryParse(parts.Last(), out int val))
                            {
                                return val;
                            }
                        }
                    }
                }
            }
            catch { }
            return 0;
        }
    }
}
