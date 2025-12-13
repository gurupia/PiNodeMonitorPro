using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();

        private DateTime _sessionStartTime = DateTime.Now;
        private DateTime _lastTickTime = DateTime.Now;
        private double _totalSyncedSeconds = 0;
        private bool _wasSynced = false;
        private int _lastIncomingCount = -1;

        public Form1()
        {
            InitializeComponent();
            
            // Setup Tooltips
            toolTip1.SetToolTip(lblLocalBlockNum, "Current block number synchronized by your node");
            toolTip1.SetToolTip(lblRemoteBlockNum, "Latest block number on the network");
            toolTip1.SetToolTip(lblLedgerAge, "Time since last block was created (seconds)");
            toolTip1.SetToolTip(lblState, "Current synchronization state of the node");
            toolTip1.SetToolTip(lblIncoming, "Number of inbound peer connections");
            toolTip1.SetToolTip(lblOutgoing, "Number of outbound peer connections");
            toolTip1.SetToolTip(lblSupporting, "Whether your node is supporting the network (Incoming > 0)");
            toolTip1.SetToolTip(lblCPU, "Container CPU usage");
            toolTip1.SetToolTip(lblRAM, "Container memory usage");
            toolTip1.SetToolTip(lblPort01, "Port 31401 status (Node API)");
            toolTip1.SetToolTip(lblPort02, "Port 31402 status (Peer Connection)");
            toolTip1.SetToolTip(lblPort03, "Port 31403 status (Stellar Core)");
            toolTip1.SetToolTip(lblUptime, "Time since monitoring started");
            toolTip1.SetToolTip(lblAvailability, "Percentage of time node was synced during this session");
            
            // Icon
            this.notifyIcon1.Icon = SystemIcons.Application;
            this.notifyIcon1.Text = "Pi Node Monitor Pro";
            this.notifyIcon1.MouseDoubleClick += (s, e) => 
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };

            // Start immediate update
            _ = UpdateDashboardAsync();

            // [NEW] Mobile Connect Button (Dynamic Add)
            Button btnMobile = new Button();
            btnMobile.Text = "📱 Mobile Connect";
            btnMobile.Size = new Size(120, 30);
            // Move to Bottom-Right (Safe Area)
            btnMobile.Location = new Point(this.ClientSize.Width - 135, this.ClientSize.Height - 65); 
            btnMobile.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMobile.BackColor = Color.FromArgb(138, 58, 185); // Pi Color
            btnMobile.ForeColor = Color.White;
            btnMobile.FlatStyle = FlatStyle.Flat;
            btnMobile.FlatAppearance.BorderSize = 0; // Cleaner look
            btnMobile.Cursor = Cursors.Hand;
            btnMobile.Click += (s, e) => 
            {
                string url = $"http://{MobileServer.CurrentIpAddress}:{MobileServer.Port}";
                using (var qr = new QRForm(url))
                {
                    qr.ShowDialog(this);
                }
            };
            this.Controls.Add(btnMobile);
            // Bring to front to ensure it's not hidden
            btnMobile.BringToFront();

            // [NEW] Start Web Server
            _ = MobileServer.StartServerAsync();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _ = UpdateDashboardAsync();
        }

        private DateTime _lastBlockTime = DateTime.Now;
        private int _lastBlockNum = 0;
        
        // Data Logging Fields
        private DateTime _lastLogTime = DateTime.MinValue;
        private string _statState = "Unknown";
        private string _statLocalBlock = "0";
        private string _statRemoteBlock = "0";
        private int _statIn = 0;
        private int _statOut = 0;
        private int _statLedgerAge = 0;

        private async Task UpdateDashboardAsync()
        {
             // 0. Stats Calculation & Immediate UI Update
            var now = DateTime.Now;
            var sessionDuration = now - _sessionStartTime;
            double stepSeconds = (now - _lastTickTime).TotalSeconds;
            _lastTickTime = now;
            
            // Display Uptime immediately
            lblUptime.Text = $"Uptime: {sessionDuration:hh\\:mm\\:ss}";
            lblUptime.Visible = true;
            
            // Display Availability (Current Session)
            double availPct = sessionDuration.TotalSeconds > 10 ? (_totalSyncedSeconds / sessionDuration.TotalSeconds) * 100.0 : 0;
            lblAvailability.Text = $"Session Avail: {availPct:F2}%";
            lblAvailability.Visible = true;

            bool isSynced = false;

            // 1. Fetch Horizon (31401) for Protocol & Block Info
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(2000);
                string json = await client.GetStringAsync("http://localhost:31401/", cts.Token);
                
                var node = JsonNode.Parse(json);
                int currentProto = (int)(node["current_protocol_version"] ?? 0);
                int supportedProto = (int)(node["supported_protocol_version"] ?? 0);
                int coreLedger = (int)(node["core_latest_ledger"] ?? 0);
                
                // Capture Remote Block
                _statRemoteBlock = coreLedger.ToString();
                lblRemoteBlockNum.Text = coreLedger.ToString();

                if (currentProto > 0 && currentProto == supportedProto)
                    lblProtocolVersion.Text = "Latest";
                else
                    lblProtocolVersion.Text = currentProto.ToString();

                if (coreLedger > _lastBlockNum)
                {
                    _lastBlockNum = coreLedger;
                    _lastBlockTime = DateTime.Now;
                    lblLatestBlock.Text = "Just now";
                }
                else
                {
                    var diff = DateTime.Now - _lastBlockTime;
                    if (diff.TotalSeconds < 60)
                        lblLatestBlock.Text = "a few seconds ago";
                    else
                        lblLatestBlock.Text = $"{(int)diff.TotalMinutes} mins ago";
                }
                
                lblProtocolVersion.ForeColor = Color.Black;
                lblLatestBlock.ForeColor = Color.Black;
            }
            catch
            {
                lblProtocolVersion.Text = "Connection Error";
                lblLatestBlock.Text = "Connection Error";
                lblRemoteBlockNum.Text = "Error";
                lblProtocolVersion.ForeColor = Color.Red;
            }

            // 2. Fetch Core (31403) for Consensus State & Peers
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(2000);
                string json = await client.GetStringAsync("http://localhost:31403/info", cts.Token);
                
                var node = JsonNode.Parse(json);
                var info = node["info"];
                
                string state = (string)(info["state"] ?? "Unknown");
                string build = (string)(info["build"] ?? "Unknown");
                int ledgerAge = (int)(info["ledger"]?["age"] ?? 0);
                int localLedger = (int)(info["ledger"]?["num"] ?? 0);
                
                _statState = state;
                _statLocalBlock = localLedger.ToString();
                _statLedgerAge = ledgerAge;

                if (state == "Synced!") isSynced = true;

                lblState.Text = state;
                lblState.ForeColor = isSynced ? Color.Green : Color.Orange;
                lblStellarBuild.Text = build.Length > 20 ? build.Substring(0, 20) + "..." : build;
                lblLedgerAge.Text = $"{ledgerAge} sec";
                lblLocalBlockNum.Text = localLedger.ToString();
                
                if (isSynced)
                {
                    lblMainStatus.Text = "Your computer is running the blockchain";
                    lblMainStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblMainStatus.Text = "Node is not synced / Catching up";
                    lblMainStatus.ForeColor = Color.Orange;
                }

                lblContainerStatus.Text = "Standard HTTP (31403)";
                lblContainerStatus.ForeColor = Color.Black;

                int outgoing = 0;
                int incoming = 0;

                if (info["peers"] != null)
                {
                    if (info["peers"]["authenticated_count"] != null)
                    {
                        outgoing = (int)info["peers"]["authenticated_count"];
                        incoming = (int)(info["peers"]["pending_count"] ?? 0);
                    }
                }
                
                if (outgoing == 0 && incoming == 0)
                {
                    try 
                    {
                        using var ctsMetric = new System.Threading.CancellationTokenSource(1000);
                        string metrics = await client.GetStringAsync("http://localhost:31403/metrics", ctsMetric.Token);
                        outgoing = ParseMetricValue(metrics, "stellar_node_peers_connected_outbound");
                        incoming = ParseMetricValue(metrics, "stellar_node_peers_connected_inbound");
                    }
                    catch { }
                }

                lblIncoming.Text = incoming.ToString();
                lblOutgoing.Text = outgoing.ToString();
                
                if (incoming > 0)
                {
                    lblSupporting.Text = "Yes";
                    lblSupporting.ForeColor = Color.Green;
                    // If incoming > 0, we can assume 'Open' for 31402/03 potentially, but Port Check is better.
                }
                else
                {
                    lblSupporting.Text = "No";
                    lblSupporting.ForeColor = Color.Black;
                }

                _statIn = incoming;
                _statOut = outgoing;

                CheckWarnings(state, incoming);
                _lastIncomingCount = incoming;
            }
            catch
            {
                // Fallback: Docker Exec
                string dockerResult = null;
                string successContainer = "";
                string container = NodeUtility.CurrentContainerName;
                string[] ports = { "1570", "11626" };

                // Port loop (1570 or 11626)
                foreach (var port in ports)
                {
                    // Try CURL
                    dockerResult = await RunDockerCommandAsync($"exec {container} curl -s --max-time 3 http://localhost:{port}/info");
                    if (!string.IsNullOrWhiteSpace(dockerResult) && dockerResult.Trim().StartsWith("{")) 
                    {
                        successContainer = container;
                        break;
                    }
                    // Try WGET
                    dockerResult = await RunDockerCommandAsync($"exec {container} wget -qO- -T 3 http://localhost:{port}/info");
                    if (!string.IsNullOrWhiteSpace(dockerResult) && dockerResult.Trim().StartsWith("{")) 
                    {
                        successContainer = container;
                        break;
                    }
                }

                try 
                {
                    if (string.IsNullOrWhiteSpace(dockerResult)) throw new Exception("All connection attempts failed");

                    var node = JsonNode.Parse(dockerResult);
                    var info = node["info"];
                    
                    string state = (string)(info["state"] ?? "Docker Exec OK");
                    string build = (string)(info["build"] ?? "Unknown");
                    int ledgerAge = (int)(info["ledger"]?["age"] ?? 0);
                    int localLedger = (int)(info["ledger"]?["num"] ?? 0);
                    
                    _statState = state;
                    _statLocalBlock = localLedger.ToString();
                    _statLedgerAge = ledgerAge;

                    if (state == "Synced!") isSynced = true;

                    int outgoing = 0;
                    int incoming = 0;

                    if (info["peers"] != null)
                    {
                        var peers = info["peers"];
                        outgoing = (int)(peers["authenticated_count"] ?? 0);
                        incoming = (int)(peers["pending_count"] ?? 0); 
                    }

                    lblState.Text = state;
                    lblState.ForeColor = Color.Blue;
                    lblStellarBuild.Text = build.Length > 20 ? build.Substring(0, 20) + "..." : build;
                    lblLedgerAge.Text = $"{ledgerAge} sec";
                    lblLocalBlockNum.Text = localLedger.ToString();

                    if (isSynced)
                    {
                        lblMainStatus.Text = "Your computer is running the blockchain";
                        lblMainStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblMainStatus.Text = "Node is not synced / Catching up";
                        lblMainStatus.ForeColor = Color.Orange;
                    }

                    lblContainerStatus.Text = $"Docker: {successContainer}";
                    lblContainerStatus.ForeColor = Color.Blue;

                    lblIncoming.Text = incoming.ToString();
                    lblOutgoing.Text = outgoing.ToString();
                    
                    if (incoming > 0)
                    {
                        lblSupporting.Text = "Yes";
                        lblSupporting.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblSupporting.Text = "No";
                        lblSupporting.ForeColor = Color.Black;
                    }

                    CheckWarnings(state, incoming);
                    _lastIncomingCount = incoming;
                }
                catch (Exception ex)
                {
                    lblState.Text = "Unreachable"; // Shorten error
                    lblState.ForeColor = Color.Red;
                    lblContainerStatus.Text = "Not Found/Stopped";
                }
            }

            // Update Stats Accumulation
            if (isSynced) _totalSyncedSeconds += stepSeconds;
            
            // Alert State Logic
            _wasSynced = isSynced;
            lblLastUpdated.Text = $"Last Updated: {DateTime.Now:HH:mm:ss}";
            
            // [NEW] Sync Data to Mobile Server
            MobileServer.CurrentStatus = new NodeStatusData
            {
                State = _statState,
                Incoming = _statIn,
                Outgoing = _statOut,
                LocalBlock = _statLocalBlock,
                ProtocolVersion = lblProtocolVersion.Text,
                LedgerAge = _statLedgerAge
            };

            // Update Status Bar
            string containerName = NodeUtility.CurrentContainerName;
            string syncStatus = isSynced ? "Synced" : "Syncing";
            statusLabel.Text = $"{syncStatus} | Container: {containerName} | Last Update: {DateTime.Now:HH:mm:ss}";
            
            // 2.5 Log to CSV every 1 minute
            if ((DateTime.Now - _lastLogTime).TotalMinutes >= 1)
            {
                LogToCsv();
                _lastLogTime = DateTime.Now;
            }

            // 3. Update System Stats safely
            try 
            {
                _ = UpdateSystemStatsAsync();
            }
            catch {}
        }
        
        private void LogToCsv()
        {
            try 
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "node_monitor_history.csv");
                bool exists = System.IO.File.Exists(path);
                
                using (var sw = System.IO.File.AppendText(path))
                {
                    if (!exists) sw.WriteLine("Timestamp,State,LocalBlock,RemoteBlock,Incoming,Outgoing,LedgerAgeSec");
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{_statState},{_statLocalBlock},{_statRemoteBlock},{_statIn},{_statOut},{_statLedgerAge}");
                }
            }
            catch { }
        }

        private void CheckWarnings(string currentState, int currentIncoming)
        {
            // Alert if lost sync
            if (_wasSynced && currentState != "Synced!")
            {
                notifyIcon1.ShowBalloonTip(3000, "Pi Node Alert", "Node Lost Sync!", ToolTipIcon.Warning);
            }

            // Alert if lost incoming
            if (_lastIncomingCount > 0 && currentIncoming == 0)
            {
                notifyIcon1.ShowBalloonTip(3000, "Pi Node Alert", "Incoming connections dropped to 0!", ToolTipIcon.Warning);
            }
        }

        private string _consensusContainerName = "pi-consensus"; // Detected dynamically

        private async void btnToggleNode_Click(object sender, EventArgs e)
        {
            // Safety Check: Confirm before Stopping
            if (btnToggleNode.Text.Contains("ON"))
            {
                var result = MessageBox.Show("Are you sure you want to STOP the Pi Node?\n\nStopping the node will halt blockchain synchronization.", 
                                             "Confirm Node Stop", 
                                             MessageBoxButtons.YesNo, 
                                             MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes) return;
            }

            btnToggleNode.Enabled = false;
            string originalText = btnToggleNode.Text;
            btnToggleNode.Text = "Working...";

            bool isRunning = originalText.Contains("ON"); 
            // Better logic: check color or stored state, but text is fine for quick toggle if reliable
            
            string cmd = isRunning ? $"stop {_consensusContainerName}" : $"start {_consensusContainerName}";
            
            await RunDockerCommandAsync(cmd);
            await Task.Delay(2000); // Wait for Docker
            
            _ = UpdateDashboardAsync();
            btnToggleNode.Enabled = true;
        }

        private async Task UpdateSystemStatsAsync()
        {
            try
            {
                // 1. Check Container Status for Switch
                string[] containers = { "pi-consensus", "testnet2" };
                bool foundRunning = false;
                string activeContainer = "";

                foreach (var name in containers)
                {
                    // Check if running
                    string inspect = await RunDockerCommandAsync($"inspect -f \"{{{{.State.Running}}}}\" {name}");
                    if (!string.IsNullOrWhiteSpace(inspect) && inspect.Trim().ToLower() == "true")
                    {
                        activeContainer = name;
                        foundRunning = true;
                        break;
                    }
                }
                _consensusContainerName = foundRunning ? activeContainer : "pi-consensus";

                // Update Switch UI (Ensure UI Thread)
                this.BeginInvoke((MethodInvoker)delegate 
                {
                    if (foundRunning)
                    {
                        btnToggleNode.Text = "Node is ON (Click to OFF)";
                        btnToggleNode.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        btnToggleNode.Text = "Node is OFF (Click to ON)";
                        btnToggleNode.BackColor = Color.Salmon;
                    }
                });

                // 2. Port Checks
                bool p1 = await CheckPortOpenAsync(31401);
                bool p2 = await CheckPortOpenAsync(31402);
                bool p3 = await CheckPortOpenAsync(31403);

                this.BeginInvoke((MethodInvoker)delegate 
                {
                    lblPort01.Text = p1 ? "Open" : "Closed";
                    lblPort01.ForeColor = p1 ? Color.Green : Color.Red;
                    
                    lblPort02.Text = p2 ? "Open" : "Closed";
                    lblPort02.ForeColor = p2 ? Color.Green : Color.Red;

                    lblPort03.Text = p3 ? "Open" : "Closed";
                    lblPort03.ForeColor = p3 ? Color.Green : Color.Red;
                });

                // 3. Docker Stats
                if (foundRunning)
                {
                    string output = await RunDockerCommandAsync($"stats {activeContainer} --no-stream --format \"{{{{.CPUPerc}}}}|{{{{.MemUsage}}}}\"");
                    
                    this.BeginInvoke((MethodInvoker)delegate 
                    {
                        if (!string.IsNullOrWhiteSpace(output) && output.Contains("|"))
                        {
                           var parts = output.Split('|');
                           if (parts.Length == 2)
                           {
                               lblCPU.Text = parts[0].Trim();
                               lblRAM.Text = parts[1].Trim();
                               
                               lblContainerStatus.Text = $"Active: {activeContainer}";
                               lblContainerStatus.ForeColor = Color.Blue;
                           }
                        }
                    });
                }
                else
                {
                    this.BeginInvoke((MethodInvoker)delegate 
                    {
                        lblCPU.Text = "0%";
                        lblRAM.Text = "0MB";
                        lblContainerStatus.Text = "Stopped";
                        lblContainerStatus.ForeColor = Color.Red;
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error safely
                 this.BeginInvoke((MethodInvoker)delegate 
                 {
                    lblContainerStatus.Text = "Error: " + ex.Message;
                 });
            }
        }

        private async Task<bool> CheckPortOpenAsync(int port)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                     // Connect with simple timeout logic via Task.Wait
                    var result = client.BeginConnect("127.0.0.1", port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    if (!success) return false;
                    client.EndConnect(result);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<string?> RunDockerCommandAsync(string arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process == null) return null;
                    
                    // Read output
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(3000); // 3 sec timeout
                    
                    if (string.IsNullOrWhiteSpace(output)) return null;
                    return output;
                }
                catch
                {
                    return null;
                }
            });
        }

        private int ParseMetricValue(string data, string key)
        {
            try
            {
                // Simple parser for lines like: key value
                var lines = data.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith(key))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && int.TryParse(parts.Last(), out int val))
                        {
                            return val;
                        }
                    }
                }
            }
            catch { }
            return 0;
        }
    }
}
