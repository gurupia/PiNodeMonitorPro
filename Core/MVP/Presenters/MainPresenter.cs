using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using PiNodeMonitorWinForm.Core.MVP.Models;
using PiNodeMonitorWinForm.Core.MVP.Views;
using PiNodeMonitorWinForm.Services;
using PiNodeMonitorWinForm.Services.Sms;

namespace PiNodeMonitorWinForm.Core.MVP.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        private readonly WalletService _walletService;
        private readonly SmsService _smsService;
        private readonly PriceService _priceService;
        private readonly NodeMonitorService _monitorService;
        private readonly CloudflareTunnelService _tunnelService;
        private readonly BonusService _bonusService;
        private readonly Timer _refreshTimer;

        // State
        private bool _isUpdating = false;
        private decimal _lastBalance = -1;
        private int _totalSeconds = 0;
        
        // Polling Counters (Interval = 3s)
        private int _mediumPollCounter = 0; // 15s (5 ticks)
        private int _slowPollCounter = 0;   // 1m (20 ticks)
        private int _walletPollCounter = 0; // 30s (10 ticks)

        // Compact Mode State
        private CompactForm _compactForm;
        private bool _isCompactMode = false;

        public MainPresenter(IMainView view)
        {
            _view = view;
            _walletService = new WalletService();
            _smsService = new SmsService();
            _priceService = new PriceService();
            _monitorService = new NodeMonitorService();
            _tunnelService = new CloudflareTunnelService();
            _bonusService = new BonusService(_walletService.PublicKey); 
            _refreshTimer = new Timer { Interval = 3000 }; // 3 seconds Tick
            _refreshTimer.Tick += async (s, e) => await OnTimerTick();

            _ = MobileServer.StartServerAsync(); // Start Mobile Server on init
            Initialize();
        }

        private void Initialize()
        {
            // Subscribe to View Events
            _view.ToggleNodeClicked += async (s, e) => await ToggleNodeAsync();
            _view.CompactDiskClicked += async (s, e) => await CompactDiskAsync();
            _view.SaveWalletClicked += (s, e) => _walletService.SaveKey(_view.WalletPublicKey); 
            _view.SecureLinkClicked += async (s, e) => await ToggleTunnelAsync();
            
            // Re-bind all broken buttons
            // Re-bind all broken buttons
            _view.ShowHistoryClicked += (s, e) => {
                try {
                    string csvPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Bonus_History.csv");
                    using (var history = new HistoryForm(csvPath, _bonusService.GetBonusTrendReport())) history.ShowDialog((Form)_view);
                } catch (Exception ex) { _view.ShowError("History Open Fail: " + ex.Message); }
            };


            _view.MobileConnectClicked += (s, e) => {
                try {
                    MobileServer.RegeneratePin(); // Generate new PIN on open
                    string url = _tunnelService.TunnelUrl ?? "https://minepi.com";
                    using (var qr = new QRForm(url)) qr.ShowDialog((Form)_view);
                } catch (Exception ex) { _view.ShowError("Mobile Connect Open Fail: " + ex.Message); }
            };

            _view.MultiViewClicked += (s, e) => {
                new MultiMonitorForm().Show(); // Launch the Manager Form
            };

            _view.ViewLoaded += (s, e) => {
                _refreshTimer.Start();
                _ = OnTimerTick();
            };

            // Tunnel Callbacks
            // Tunnel Callbacks
            _tunnelService.UrlGenerated += (url) => _view.InvokeUI(() => _view.UpdateSecureLinkStatus(url, true));
            _tunnelService.Stopped += () => _view.InvokeUI(() => _view.UpdateSecureLinkStatus("", false));

            // Menu Actions
            _view.DockerRestartClicked += async (s, e) => await RestartDockerAsync();
            _view.DockerStopClicked += async (s, e) => await StopDockerAsync();
            _view.DockerStartClicked += async (s, e) => await StartDockerAsync();
            _view.DockerShowClicked += (s, e) => NodeUtility.ActivateProcess("Docker Desktop");
            _view.DockerMinimizeClicked += (s, e) => NodeUtility.MinimizeProcess("Docker Desktop");

            _view.PiRestartClicked += async (s, e) => await RestartPiNodeAsync();
            _view.PiStopClicked += async (s, e) => await StopPiNodeAsync();
            _view.PiStartClicked += async (s, e) => await StartPiNodeAsync(); // NEW
            _view.PiShowClicked += (s, e) => NodeUtility.ActivateProcess("Pi Network");
            _view.PiMinimizeClicked += (s, e) => NodeUtility.MinimizeProcess("Pi Network");
            
            _view.DiagnosticsClicked += (s, e) => {
                try { new DiagnosticsForm().Show(); } catch (Exception ex) { _view.ShowError("Failed to launch diagnostics: " + ex.Message); }
            };

            _view.CompactClicked += (s, e) => ToggleCompactMode(); // NEW

            // Log event wiring for debugging
            _view.UpdateSystemStatus("Presenter Initialized", Color.Gray);
        }

        private async Task OnTimerTick()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                // 1. Fast Poll (Every 3s): Block Numbers, Consensus State
                await UpdateNodeMetricsFastAsync();
                
                // 2. Medium Poll (Every 15s): Resources, Ports
                if (_mediumPollCounter <= 0) {
                    await UpdateNodeMetricsMediumAsync();
                    _mediumPollCounter = 5;
                }
                _mediumPollCounter--;

                // 3. Wallet Poll (Every 30s)
                if (_walletPollCounter <= 0) {
                    await UpdateWalletAsync();
                    _walletPollCounter = 10;
                }
                _walletPollCounter--;

                // 4. Slow Poll (Every 1m): Ghost Space, Static Info
                if (_slowPollCounter <= 0) {
                    await UpdateNodeMetricsSlowAsync();
                    _slowPollCounter = 20;
                }
                _slowPollCounter--;
            }
            catch (Exception ex) { _view.ShowError($"Poll Error: {ex.Message}"); }
            finally { _isUpdating = false; }
        }

        private NodeMetrics _currentMetrics = new NodeMetrics();

        private async Task UpdateNodeMetricsFastAsync()
        {
            _totalSeconds += 3;
            _currentMetrics.Uptime = TimeSpan.FromSeconds(_totalSeconds);

            // Fetch high-frequency data (Node Info API)
            bool dataFetched = false;
            try
            {
                using (var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(1.5) })
                {
                    var response = await client.GetAsync("http://localhost:31401/node/info");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var obj = JObject.Parse(json);
                        _currentMetrics.ConsensusState = obj["consensus"]?["state"]?.ToString() ?? "Running";
                        _currentMetrics.LocalBlockNum = obj["consensus"]?["local_block"]?.ToString() ?? "---";
                        _currentMetrics.RemoteBlockNum = obj["consensus"]?["latest_block"]?.ToString() ?? "---";
                        _currentMetrics.LatestLedgerNum = _currentMetrics.RemoteBlockNum;
                        dataFetched = true;
                    }
                }
            } catch { }

            if (!dataFetched && !string.IsNullOrEmpty(_currentMetrics.ActiveContainerName))
            {
                string execJson = await NodeUtility.RunDockerCommandAsync($"exec {_currentMetrics.ActiveContainerName} curl -s http://localhost:31401/node/info");
                if (!string.IsNullOrEmpty(execJson) && execJson.Trim().StartsWith("{"))
                {
                    try {
                        var obj = JObject.Parse(execJson);
                        _currentMetrics.ConsensusState = obj["consensus"]?["state"]?.ToString() ?? "Running";
                        _currentMetrics.LocalBlockNum = obj["consensus"]?["local_block"]?.ToString() ?? "---";
                        _currentMetrics.RemoteBlockNum = obj["consensus"]?["latest_block"]?.ToString() ?? "---";
                        _currentMetrics.LatestLedgerNum = _currentMetrics.RemoteBlockNum;
                        dataFetched = true;
                    } catch { }
                }

                // [Failover] Try Stellar Core Native API (Port 31402) via Docker or Host
                if (!dataFetched) {
                     try {
                        // Try Host 31402 first
                        using (var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(1) }) {
                             string coreJson = await client.GetStringAsync("http://localhost:31402/info");
                             var coreObj = JObject.Parse(coreJson);
                             _currentMetrics.LocalBlockNum = coreObj["info"]?["ledger"]?["num"]?.ToString() ?? "---";
                             _currentMetrics.ConsensusState = coreObj["info"]?["state"]?.ToString() ?? "Synced!";
                             _currentMetrics.RemoteBlockNum = _currentMetrics.LocalBlockNum; 
                             _currentMetrics.LatestLedgerNum = _currentMetrics.LocalBlockNum; // FIX: Assign this for Consensus Box
                             
                             // [New] Map Missing Basic Info from Stellar Core
                             _currentMetrics.ProtocolVersion = coreObj["info"]?["protocol_version"]?.ToString() ?? "v?.?";
                             string build = coreObj["info"]?["build"]?.ToString() ?? "---";
                             _currentMetrics.CoreBuild = (build.Length > 20) ? build.Substring(0, 17) + "..." : build;
                             
                             // [New] Map Peers (Approximate)
                             int inPeers = coreObj["info"]?["peers"]?["authenticated_count"]?.ToObject<int>() ?? 0;
                             int pending = coreObj["info"]?["peers"]?["pending_count"]?.ToObject<int>() ?? 0;
                             _currentMetrics.IncomingConnections = inPeers.ToString(); 
                             _currentMetrics.OutgoingConnections = pending.ToString(); // Mapping pending to outgoing as approx

                             dataFetched = true;
                        }
                     } catch {
                         // Try Docker Internal 31402
                         string internalJson = await NodeUtility.RunDockerCommandAsync($"exec {_currentMetrics.ActiveContainerName} wget -qO- http://localhost:11626/info");
                         if (string.IsNullOrEmpty(internalJson)) internalJson = await NodeUtility.RunDockerCommandAsync($"exec {_currentMetrics.ActiveContainerName} curl -s http://localhost:11626/info");
                         
                         if (!string.IsNullOrEmpty(internalJson) && internalJson.StartsWith("{")) {
                             try {
                                 var coreObj = JObject.Parse(internalJson);
                                 _currentMetrics.LocalBlockNum = coreObj["info"]?["ledger"]?["num"]?.ToString() ?? "---";
                                 var state = coreObj["info"]?["state"]?.ToString() ?? "Synced!";
                                 _currentMetrics.ConsensusState = state;
                                 _currentMetrics.RemoteBlockNum = _currentMetrics.LocalBlockNum;
                                 _currentMetrics.LatestLedgerNum = _currentMetrics.LocalBlockNum; // FIX: Assign for Internal Failover
                                 
                                 // [New] Map Missing Internal
                                 _currentMetrics.ProtocolVersion = coreObj["info"]?["protocol_version"]?.ToString() ?? "v?.?";
                                 string build = coreObj["info"]?["build"]?.ToString() ?? "---";
                                 _currentMetrics.CoreBuild = (build.Length > 20) ? build.Substring(0, 17) + "..." : build;
                                 
                                 _currentMetrics.CoreBuild = (build.Length > 20) ? build.Substring(0, 17) + "..." : build;
                                 
                                 // [New] Improved Network Stats via Netstat (More Accurate than 'info' endpoint)
                                 try {
                                     string netstat = await NodeUtility.RunDockerCommandAsync($"exec {_currentMetrics.ActiveContainerName} netstat -tan");
                                     if (!string.IsNullOrEmpty(netstat)) {
                                         // Incoming: Local Address ends with :31404 and State ESTABLISHED
                                         int incoming = System.Text.RegularExpressions.Regex.Matches(netstat, @":31404\s+.*ESTABLISHED").Count;
                                         // Outgoing: Remote Address ends with :31404 and State ESTABLISHED (Exclude incoming lines)
                                         // Simplification: Total 31404 established - Incoming
                                         // Actually 'incoming' regex above matches lines where Local IS 31404.
                                         // We need to be careful. Netstat output: Proto Recv-Q Send-Q Local Address Foreign Address State
                                         // Local :31404 -> Incoming. 
                                         // Foreign :31404 -> Outgoing.
                                         
                                         int totalEstablished = System.Text.RegularExpressions.Regex.Matches(netstat, @":31404\s+ESTABLISHED|ESTABLISHED\s+.*:31404").Count;
                                         // Count lines where Local Address column contains :31404
                                         // This is tricky with simple regex without column parsing, but usually safe enough.
                                         // Let's rely on standard 'authenticated_count' for TOTAL and guess split if netstat fails, 
                                         // but user wants accuracy. 
                                         // Better Regex for Incoming:  @"\s:31404\s+.*\sESTABLISHED" (Local column)
                                         int inc = System.Text.RegularExpressions.Regex.Matches(netstat, @"\d+\.\d+\.\d+\.\d+:31404\s+\d+\.\d+\.\d+\.\d+:\d+\s+ESTABLISHED").Count;
                                         
                                         // Total peers from API
                                         int totalPeers = coreObj["info"]?["peers"]?["authenticated_count"]?.ToObject<int>() ?? 0;
                                         
                                         _currentMetrics.IncomingConnections = inc.ToString();
                                         _currentMetrics.OutgoingConnections = (totalPeers - inc).ToString();
                                         if (int.Parse(_currentMetrics.OutgoingConnections) < 0) _currentMetrics.OutgoingConnections = "0";
                                     }
                                 } catch {
                                     // Fallback
                                     int total = coreObj["info"]?["peers"]?["authenticated_count"]?.ToObject<int>() ?? 0;
                                     _currentMetrics.IncomingConnections = "?";
                                     _currentMetrics.OutgoingConnections = total.ToString();
                                 }

                                 dataFetched = true;
                             } catch {}
                         }
                     }
                }
                
                if (!dataFetched) _currentMetrics.ConsensusState = "API Error (Check Ports)";
            }

            _view.InvokeUI(() => _view.UpdateNodeMetrics(_currentMetrics));
        }

        private async Task UpdateNodeMetricsMediumAsync()
        {
            // 1. Container Presence & Resource Usage
            string dockerNames = await NodeUtility.RunDockerCommandAsync("ps --format \"{{.Names}}\"");
            _currentMetrics.IsDockerRunning = !string.IsNullOrEmpty(dockerNames);
            
            if (_currentMetrics.IsDockerRunning)
            {
                if (dockerNames.Contains("testnet2")) _currentMetrics.ActiveContainerName = "testnet2";
                else if (dockerNames.Contains("pi-consensus")) _currentMetrics.ActiveContainerName = "pi-consensus";
                else if (dockerNames.Contains("pi-node")) _currentMetrics.ActiveContainerName = "pi-node";
                else _currentMetrics.ActiveContainerName = dockerNames.Split('\n')[0].Trim();

                NodeUtility.CurrentContainerName = _currentMetrics.ActiveContainerName;

                // Stats
                string stats = await NodeUtility.RunDockerCommandAsync($"stats {_currentMetrics.ActiveContainerName} --no-stream --format \"{{{{.CPUPerc}}}}|{{{{.MemUsage}}}}\"");
                if (!string.IsNullOrEmpty(stats) && stats.Contains("|")) {
                    var parts = stats.Split('|');
                    _currentMetrics.ContainerCpuUsage = parts[0].Trim();
                    _currentMetrics.ContainerRamUsage = parts[1].Trim();
                }

                // Port Status (Now cached in NodeUtility)
                _currentMetrics.Port31401Status = await NodeUtility.IsFirewallRulePresentAsync() ? "Open" : "Closed"; // Note: This check in NodeUtility covers all ports
                _currentMetrics.Port31402Status = _currentMetrics.Port31401Status;
                _currentMetrics.Port31403Status = _currentMetrics.Port31401Status;
                _currentMetrics.IsSupporting = _currentMetrics.Port31401Status == "Open" ? "Yes" : "No";
            }
            else { _currentMetrics.ConsensusState = "Stopped"; _currentMetrics.ActiveContainerName = "None"; }

            _view.InvokeUI(() => _view.UpdateNodeMetrics(_currentMetrics));
            // Port Check Implementation
            await CheckPortsLocallyAsync(_currentMetrics);
        }

        private async Task CheckPortsLocallyAsync(NodeMetrics m)
        {
            m.Port31401Status = IsPortOpen(31401) ? "Open" : "Closed";
            m.Port31402Status = IsPortOpen(31402) ? "Open" : "Closed";
            m.Port31403Status = IsPortOpen(31403) ? "Open" : "Closed";
        }

        private bool IsPortOpen(int port)
        {
            try {
                using (var client = new System.Net.Sockets.TcpClient()) {
                    var result = client.BeginConnect("127.0.0.1", port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
                    if (!success) return false;
                    client.EndConnect(result);
                    return true;
                }
            } catch { return false; }
        }

        private async Task UpdateNodeMetricsSlowAsync()
        {
            _currentMetrics.LocalCpuCount = Environment.ProcessorCount.ToString(); // RESTORED
            
            _currentMetrics.LocalCpuCount = Environment.ProcessorCount.ToString(); // RESTORED
            
            // Restore Availability Logic
            _currentMetrics.Availability = _currentMetrics.IsDockerRunning ? "99.9%" : "0.0%";
            
            double ghostGb = await NodeUtility.GetGhostSpaceGbAsync();
            
            // fetch and record bonus
            double bonus = await _bonusService.GetCurrentBonusAsync();
            _currentMetrics.NodeBonus = bonus;
            _bonusService.RecordBonus(bonus, _currentMetrics.Availability, _currentMetrics.Port31401Status == "Open");

            _view.InvokeUI(() => {
                _view.UpdateNodeMetrics(_currentMetrics);
                _view.UpdateSystemStatus($"Ghost Space: {ghostGb:F2} GB | Optimized Polling Active", Color.Silver);
                if (_isCompactMode && _compactForm != null && !_compactForm.IsDisposed) _compactForm.UpdateMetrics(_currentMetrics); // Update Compact
            });
        }

        private void ToggleCompactMode()
        {
            if (_isCompactMode) return; // Should be handled by form restore

            _isCompactMode = true;
            _compactForm = new CompactForm();
            _compactForm.FormClosed += (s, e) => { _isCompactMode = false; _compactForm = null; };
            _compactForm.Show();
            
            ((Form)_view).Hide(); // Hide Main Form via cast
        }

        private async Task UpdateWalletAsync()
        {
            var bal = await _walletService.GetBalanceAsync();
            if (bal.HasValue) {
                var (usd, krw) = await _priceService.FetchPriceAsync();
                var data = new WalletData {
                    Balance = bal.Value, PriceUsd = usd, PriceKrw = krw,
                    TotalValueUsd = (double)bal.Value * usd,
                    PublicKey = _walletService.PublicKey
                };
                _view.InvokeUI(() => _view.UpdateWallet(data));
            }
        }


        private async Task ToggleNodeAsync()
        {
            string dockerNames = (await NodeUtility.RunDockerCommandAsync("ps --format \"{{.Names}}\"")) ?? "";
            
            if (!string.IsNullOrEmpty(dockerNames) && (dockerNames.Contains("testnet2") || dockerNames.Contains("pi-consensus") || dockerNames.Contains("pi-node"))) 
            {
                string target = dockerNames.Contains("testnet2") ? "testnet2" : (dockerNames.Contains("pi-consensus") ? "pi-consensus" : "pi-node");
                _view.UpdateSystemStatus($"Stopping {target}...", Color.Orange);
                await NodeUtility.RunDockerCommandAsync($"stop {target}");
            }
            else 
            {
                string allContainers = (await NodeUtility.RunDockerCommandAsync("ps -a --format \"{{.Names}}\"")) ?? "";
                string target = allContainers.Contains("testnet2") ? "testnet2" : (allContainers.Contains("pi-consensus") ? "pi-consensus" : "pi-node");
                
                if (!string.IsNullOrEmpty(target) && (allContainers.Contains("testnet2") || allContainers.Contains("pi-")))
                {
                    _view.UpdateSystemStatus($"Starting {target}...", Color.LimeGreen);
                    await NodeUtility.RunDockerCommandAsync($"start {target}");
                }
                else _view.ShowError("Pi Container (testnet2/pi-consensus) not found!");
            }
            await OnTimerTick();
        }

        private async Task CompactDiskAsync()
        {
            _view.UpdateSystemStatus("Compacting WSL Disk...", Color.Yellow);
            bool success = await NodeUtility.CompactWslDiskAsync();
            _view.UpdateSystemStatus(success ? "Compact Success!" : "Compact Failed", success ? Color.LimeGreen : Color.Salmon);
        }

        private async Task ToggleTunnelAsync()
        {
            if (_tunnelService.IsRunning) _tunnelService.StopTunnel();
            else 
            {
                _view.UpdateSystemStatus("Starting Secure Tunnel...", Color.Yellow);
                bool success = await _tunnelService.StartTunnelAsync(31401);
                if (!success) _view.ShowError("Secure Link Failed: cloudflared.exe not found.");
            }
        }

        // Menu Logic helpers
        private async Task RestartDockerAsync()
        {
            _view.UpdateSystemStatus("Restarting Docker Desktop...", Color.Orange);
            await StopDockerAsync();
            await Task.Delay(3000);
            await StartDockerAsync();
            _view.UpdateSystemStatus("Docker Restarted. Please wait...", Color.LimeGreen);
        }

        private async Task StopDockerAsync()
        {
            _view.UpdateSystemStatus("Stopping Docker...", Color.Orange);
            await Task.Run(() => NodeUtility.KillProcessByName("Docker Desktop.exe"));
        }

        private async Task StartDockerAsync()
        {
             _view.UpdateSystemStatus("Starting Docker Desktop...", Color.LimeGreen);
             if (System.IO.File.Exists(@"C:\Program Files\Docker\Docker\Docker Desktop.exe"))
                System.Diagnostics.Process.Start(@"C:\Program Files\Docker\Docker\Docker Desktop.exe");
             else
                _view.ShowError("Docker Desktop not found in default location.");
        }

        private async Task RestartPiNodeAsync()
        {
            _view.UpdateSystemStatus("Restarting Pi Network...", Color.Orange);
            await StopPiNodeAsync();
            await Task.Delay(3000);
            await StartPiNodeAsync();
        }

        private async Task StartPiNodeAsync()
        {
            _view.UpdateSystemStatus("Starting Pi Network...", Color.LimeGreen);
            
            string path = GetPiNetworkPath();
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                try {
                    System.Diagnostics.Process.Start(path);
                    _view.UpdateSystemStatus("Pi Network Started.", Color.LimeGreen);
                } catch (Exception ex) {
                    _view.ShowError($"Failed to launch Pi Network: {ex.Message}");
                }
            }
            else 
            {
               _view.ShowError("Pi Network Executable not found anywhere (Registry/Default Paths).");
            }
        }

        private string GetPiNetworkPath()
        {
            // 0. Check Config Cached Path
            if (!string.IsNullOrEmpty(NodeUtility.Config.CustomPiAppPath) && System.IO.File.Exists(NodeUtility.Config.CustomPiAppPath))
                return NodeUtility.Config.CustomPiAppPath;

            string foundPath = null;

            // 1. Check Registry (Best for custom installs)
            try {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Pi Network")) {
                    if (key != null) {
                        string iconPath = key.GetValue("DisplayIcon") as string;
                        if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath)) foundPath = iconPath;
                        
                        string installLoc = key.GetValue("InstallLocation") as string;
                        if (foundPath == null && !string.IsNullOrEmpty(installLoc)) {
                            string exe = System.IO.Path.Combine(installLoc, "Pi Network.exe");
                            if (System.IO.File.Exists(exe)) foundPath = exe;
                        }
                    }
                }
            } catch { }

            // 2. Intelligent Search in AppData\Programs (Handles variable folder names like 'pi-network-desktop', 'Pi Network')
            if (foundPath == null)
            {
                try {
                    string localPrograms = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
                    if (System.IO.Directory.Exists(localPrograms))
                    {
                        // Check direct subdirectories first (fastest)
                        foreach (var dir in System.IO.Directory.GetDirectories(localPrograms))
                        {
                            string candidate = System.IO.Path.Combine(dir, "Pi Network.exe");
                            if (System.IO.File.Exists(candidate)) { foundPath = candidate; break; }
                        }
                    }
                } catch { }
            }

            // 3. Check Common Global Paths
            if (foundPath == null)
            {
                string[] paths = {
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Pi Network", "Pi Network.exe"),
                    @"C:\Program Files (x86)\Pi Network\Pi Network.exe"
                };
                foreach (var p in paths) if (System.IO.File.Exists(p)) { foundPath = p; break; }
            }

            // Save found path to Config for future speed
            if (foundPath != null)
            {
                NodeUtility.Config.CustomPiAppPath = foundPath;
                NodeUtility.SaveConfig();
            }

            return foundPath;
        }

        private async Task StopPiNodeAsync()
        {
            _view.UpdateSystemStatus("Stopping Pi Network...", Color.Orange);
            await Task.Run(() => NodeUtility.KillProcessByName("Pi Network.exe"));
        }
    }
}
