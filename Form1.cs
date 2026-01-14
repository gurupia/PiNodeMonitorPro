using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using PiNodeMonitorWinForm.Services;
using PiNodeMonitorWinForm.Services.Sms;

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
        private int _totalSeconds = 0;
        private int _totalSyncedSeconds = 0;

        // Timer
        private readonly HttpClient client = new HttpClient();
        private Button btnMobile; 

        // Wallet Controls
        private TextBox txtPublicKey;
        private Label lblBalance;
        private Button btnSaveKey;
        private Button btnChangeWallet;
        private WalletService _walletService;
        private SmsService _smsService;
        private NodeMonitorService _monitorService;
        private TelegramBotService _botService;
        private NotifyIcon notifyIcon;
        private decimal _lastBalance = -1; 
        private BonusService _bonusService;
        private double _currentBonus = 0;
        private int _bonusUpdateCounter = 0;

        // Cloudflare Tunnel
        private CloudflareTunnelService _tunnelService;
        private Button btnSecureTunnel;
        private Label lblTunnelLink;
        
        public Form1()
        {
            InitializeComponent();
            client.Timeout = TimeSpan.FromSeconds(3);
            this.Height += 120; // Increased height for Wallet UI + Footer
            this.Width += 60;   // Increased width for SMS Button
            this.Text = "Pi Node Monitor Pro";
            lblLocalCpuCount.Text = $"{Environment.ProcessorCount} Threads";
            
            // 1. Menu Strip Setup
            MenuStrip menuStrip = new MenuStrip();

            // --- Docker Menu ---
            ToolStripMenuItem menuDocker = new ToolStripMenuItem("Docker");
            menuDocker.DropDownItems.Add("도커 실행 (Start)", null, (s, e) => { 
                try { 
                    string path = NodeUtility.Config.CustomDockerPath;
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) path = @"C:\Program Files\Docker\Docker\Docker Desktop.exe";

                    if (File.Exists(path)) Process.Start("cmd.exe", $"/c start \"\" \"{path}\"");
                    else Process.Start("cmd.exe", "/c start docker-desktop://");
                } catch (Exception ex) { 
                    MessageBox.Show($"도커를 실행할 수 없습니다: {ex.Message}", "에러"); 
                } 
            });
            menuDocker.DropDownItems.Add("도커 대시보드 열기", null, (s, e) => { 
                try { 
                    string path = NodeUtility.Config.CustomDockerPath;
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) path = @"C:\Program Files\Docker\Docker\Docker Desktop.exe";

                    if (File.Exists(path)) Process.Start("cmd.exe", $"/c start \"\" \"{path}\"");
                    else Process.Start("cmd.exe", "/c start docker-desktop://dashboard");
                } catch { 
                    NodeUtility.ActivateProcess("Docker Desktop");
                }
            });
            menuDocker.DropDownItems.Add("도커 활성화 (Show)", null, (s, e) => { NodeUtility.ActivateProcess("Docker Desktop"); });
            menuDocker.DropDownItems.Add("도커 최소화 (Minimize)", null, (s, e) => { NodeUtility.MinimizeProcess("Docker Desktop"); });
            menuDocker.DropDownItems.Add("-"); // Separator
            menuDocker.DropDownItems.Add("도커 서비스 재시작", null, async (s, e) => { await NodeUtility.RunCommandAsync("powershell", "-Command \"Restart-Service *docker*\"", true); });
            menuDocker.DropDownItems.Add("실시간 로그 보기 (Tail)", null, (s, e) => { try { Process.Start("cmd", $"/c docker logs -f {NodeUtility.CurrentContainerName} & pause"); } catch { } });
            menuDocker.DropDownItems.Add("미사용 리소스 정리 (Prune)", null, async (s, e) => { if (MessageBox.Show("미사용 도커 리소스를 모두 정리하시겠습니까?", "Prune", MessageBoxButtons.YesNo) == DialogResult.Yes) await NodeUtility.RunCommandAsync("docker", "system prune -f"); });
            menuDocker.DropDownItems.Add("WSL2 상태 점검", null, async (s, e) => { await NodeUtility.RunCommandAsync("powershell", "-Command \"wsl --status; pause\"", false); });

            // --- Pi Node Menu ---
            ToolStripMenuItem menuPiNode = new ToolStripMenuItem("Pi Node");
            // 💡 다양한 경로 탐색 (현재 실행 경로 기준, 기본 설치 경로 등)
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoAppPath = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\..\Pi Network.exe"));
            string localProgramsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Pi Network", "Pi Network.exe");
            string localProgramsDesktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "pi-network-desktop", "Pi Network.exe");
            string localOldPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pi Network", "Pi Network.exe");
            
            menuPiNode.DropDownItems.Add("노드 앱 실행 (Start)", null, (s, e) => { 
                try { 
                    string path = NodeUtility.Config.CustomPiAppPath;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                        Process.Start("cmd.exe", $"/c start \"\" \"{path}\"");
                        return;
                    }

                    string target = null;
                    if (File.Exists(repoAppPath)) target = repoAppPath;
                    else if (File.Exists(localProgramsDesktopPath)) target = localProgramsDesktopPath;
                    else if (File.Exists(localProgramsPath)) target = localProgramsPath;
                    else if (File.Exists(localOldPath)) target = localOldPath;

                    if (target != null) Process.Start("cmd.exe", $"/c start \"\" \"{target}\"");
                    else MessageBox.Show("Pi Network 앱을 찾을 수 없습니다. [대시보드 > 앱 경로 수동 설정]을 이용해 주세요.", "알림");
                } catch (Exception ex) { 
                    MessageBox.Show($"노드 앱을 실행할 수 없습니다: {ex.Message}", "에러"); 
                } 
            });
            menuPiNode.DropDownItems.Add("노드 앱 활성화 (Show)", null, (s, e) => { NodeUtility.ActivateProcess("Pi Network"); });
            menuPiNode.DropDownItems.Add("노드 앱 최소화 (Minimize)", null, (s, e) => { NodeUtility.MinimizeProcess("Pi Network"); });
            menuPiNode.DropDownItems.Add("-");
            menuPiNode.DropDownItems.Add("컨테이너 재시작 (testnet2)", null, async (s, e) => { await NodeUtility.RunCommandAsync("docker", $"restart {NodeUtility.CurrentContainerName}"); await UpdateDashboardAsync(); });
            menuPiNode.DropDownItems.Add("외부 포트 체크 (Website)", null, (s, e) => { try { Process.Start("https://pi-blockchain.net"); } catch { } });
            menuPiNode.DropDownItems.Add("설정 폴더 열기 (Roaming)", null, (s, e) => { try { Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pi Network")); } catch { } });
            menuPiNode.DropDownItems.Add("노드 UUID 확인", null, async (s, e) => { string uuid = await NodeUtility.GetNodeUuidAsync(); MessageBox.Show($"Node UUID: {uuid}", "UUID Info"); if(!string.IsNullOrEmpty(uuid)) Clipboard.SetText(uuid); });
            menuPiNode.DropDownItems.Add("합의 쿼럼 상태 (JSON)", null, async (s, e) => { try { string quorum = await client.GetStringAsync("http://localhost:31403/quorum"); MessageBox.Show(quorum, "Quorum Details"); } catch { MessageBox.Show("Core port is not reachable."); } });
            menuPiNode.DropDownItems.Add("-");
            menuPiNode.DropDownItems.Add("데이터 초기화 (Fresh Sync)", null, async (s, e) => { 
                if (MessageBox.Show("모든 블록 데이터를 삭제하고 처음부터 다시 동기화하시겠습니까?\n이 작업은 매우 오래 걸립니다. (최대 수일 소요)", "초기화 경고 (1/2)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    if (MessageBox.Show("정말로 진행하시겠습니까?\n기존에 다운로드한 블록 데이터가 모두 삭제되며 되돌릴 수 없습니다.", "최종 확인 (2/2)", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK) {
                        await NodeUtility.RunCommandAsync("docker", $"stop {NodeUtility.CurrentContainerName}");
                        await NodeUtility.RunCommandAsync("docker", $"rm {NodeUtility.CurrentContainerName}");
                        MessageBox.Show("컨테이너와 데이터가 제거되었습니다.\n이제 파이 앱을 실행하여 'Node' 메뉴 내에서 [Continue]를 눌러 재설치를 진행하세요.", "완료");
                    }
                }
            });

            // --- Dashboard Menu ---
            ToolStripMenuItem menuDashboard = new ToolStripMenuItem("Dashboard");
            menuDashboard.DropDownItems.Add("Force Refresh Dashboard", null, async (s, e) => await UpdateDashboardAsync());
            menuDashboard.DropDownItems.Add("View Bonus History", null, (s, e) => btnShowHistory_Click(s, e));
            menuDashboard.DropDownItems.Add("View mobile_access.log", null, (s, e) => { try { Process.Start(MobileServer.LogPath); } catch { } });
            menuDashboard.DropDownItems.Add("-");
            menuDashboard.DropDownItems.Add("Reset Uptime Stats", null, (s, e) => { _totalSeconds = 0; _totalSyncedSeconds = 0; });
            menuDashboard.DropDownItems.Add("Reset Wallet Balance", null, (s, e) => { _lastBalance = -1; UpdateWalletBalanceAsync(); });
            menuDashboard.DropDownItems.Add("-");
            menuDashboard.DropDownItems.Add("환경 설정 마법사 (Setup)", null, (s, e) => { 
                using (var wizard = new SetupWizardForm()) { wizard.ShowDialog(); } 
            });
            menuDashboard.DropDownItems.Add("앱 경로 수동 설정", null, (s, e) => { ShowPathSettingsDialog(); });

            menuStrip.Items.Add(menuDocker);
            menuStrip.Items.Add(menuPiNode);
            menuStrip.Items.Add(menuDashboard);
            menuStrip.Dock = DockStyle.Top;
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            
            // Adjust mainFlow to not overlap menu
            mainFlow.Padding = new Padding(10, 5, 10, 20); // Reset padding
            mainFlow.Dock = DockStyle.Fill;
            mainFlow.BringToFront(); // Ensure Fill is on top of docked items in Z-order if needed, but Docking handles it.
            // Actually, in WinForms, the LAST control added with Dock.Fill covers others unless they were added before.
            // But we have existing controls in Designer. 
            // Better to add menuStrip to the Form's Controls and ensure it's at the top.
            
            // Initialize Services
            _bonusService = new BonusService("387f2aaa-2883-443f-b69c-fe6a77647b1f");
            _tunnelService = new CloudflareTunnelService();
            _tunnelService.UrlGenerated += (url) => {
                this.Invoke((Action)(() => {
                    lblTunnelLink.Text = "Secure URL: " + url;
                    lblTunnelLink.ForeColor = Color.LimeGreen;
                    btnSecureTunnel.Text = "🔒 Close Tunnel";
                    btnSecureTunnel.Enabled = true;
                }));
            };
            _tunnelService.Stopped += () => {
                this.Invoke((Action)(() => {
                    lblTunnelLink.Text = "Secure URL: Not Active";
                    lblTunnelLink.ForeColor = Color.Gray;
                    btnSecureTunnel.Text = "🌐 Secure Link";
                    btnSecureTunnel.Enabled = true;
                }));
            };

            try 
            { 
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("PiNodeMonitorWinForm.app.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            } 
            catch { }
 
            // Detect Real UUID and update BonusService
            Task.Run(async () => {
                string realUuid = await NodeUtility.GetNodeUuidAsync();
                if (!string.IsNullOrEmpty(realUuid))
                {
                    _bonusService.SetUuid(realUuid);
                    _bonusUpdateCounter = 0; // Trigger immediate update on next tick
                    System.Diagnostics.Debug.WriteLine($"Real UUID Detected: {realUuid}");
                }
            });

            // Initialize Notification
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = this.Icon;
            notifyIcon.Visible = true;
            notifyIcon.Text = "Pi Node Monitor Pro";
            
            // Start Mobile Server
            Task.Run(() => MobileServer.StartServerAsync());
            
            // Service Init
            _walletService = new WalletService();
            _smsService = new SmsService();
            _monitorService = new NodeMonitorService();
            _botService = new TelegramBotService();
            InitializeBot();

            // ---------------------------------------------------------
            // ---------------------------------------------------------
            // Wallet & Footer UI (FlowLayout Integration)
            // ---------------------------------------------------------
            
            // 1. Wallet Status Label
            lblBalance = new Label();
            lblBalance.Text = "Wallet: -- π";
            lblBalance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblBalance.ForeColor = Color.Gold;
            lblBalance.BackColor = Color.Transparent;
            lblBalance.AutoSize = true;
            lblBalance.Margin = new Padding(0, 20, 0, 5);
            mainFlow.Controls.Add(lblBalance);

            // 2. Wallet Input Row (Horizontal Flow)
            FlowLayoutPanel walletFlow = new FlowLayoutPanel();
            walletFlow.FlowDirection = FlowDirection.LeftToRight;
            walletFlow.AutoSize = true;
            walletFlow.Margin = new Padding(0, 0, 0, 10);
            walletFlow.WrapContents = false;

            txtPublicKey = new TextBox();
            // txtInput = new TextBox { Width = 200 }; // Not declared
            // txtPublicKey.PlaceholderText = "Paste Public Key (G...)"; // Not supported in net48
            txtPublicKey.Size = new Size(390, 25);
            txtPublicKey.BackColor = Color.FromArgb(40, 40, 40);
            txtPublicKey.ForeColor = Color.White;
            txtPublicKey.BorderStyle = BorderStyle.FixedSingle;
            walletFlow.Controls.Add(txtPublicKey);

            btnSaveKey = new Button();
            btnSaveKey.Text = "💾";
            btnSaveKey.Size = new Size(35, 25);
            btnSaveKey.FlatStyle = FlatStyle.Flat;
            btnSaveKey.ForeColor = Color.White;
            walletFlow.Controls.Add(btnSaveKey);

            var btnSmsConfig = new Button();
            btnSmsConfig.Text = "💬";
            btnSmsConfig.Size = new Size(35, 25);
            btnSmsConfig.FlatStyle = FlatStyle.Flat;
            btnSmsConfig.ForeColor = Color.LightSkyBlue;
            btnSmsConfig.Cursor = Cursors.Hand;
            btnSmsConfig.Click += (s, e) => {
                using (var frm = new SmsSettingsForm()) {
                    if (frm.ShowDialog() == DialogResult.OK) {
                        _smsService.LoadSettings();
                        InitializeBot();
                    }
                }
            };
            walletFlow.Controls.Add(btnSmsConfig);

            btnChangeWallet = new Button();
            btnChangeWallet.Text = "Change Address";
            btnChangeWallet.Size = new Size(120, 25);
            btnChangeWallet.FlatStyle = FlatStyle.Flat;
            btnChangeWallet.ForeColor = Color.Gray; 
            btnChangeWallet.Cursor = Cursors.Hand;
            walletFlow.Controls.Add(btnChangeWallet);

            mainFlow.Controls.Add(walletFlow);

            // 3. Multi-View & Mobile Connect Row (Horizontal Flow)
            FlowLayoutPanel buttonFlow = new FlowLayoutPanel();
            buttonFlow.FlowDirection = FlowDirection.LeftToRight;
            buttonFlow.Size = new Size(470, 50);
            buttonFlow.Margin = new Padding(0, 10, 0, 10);
            buttonFlow.FlowDirection = FlowDirection.RightToLeft;

            btnMobile = new Button();
            btnMobile.Text = "📱 Mobile Connect";
            btnMobile.Size = new Size(130, 40);
            btnMobile.BackColor = Color.BlueViolet;
            btnMobile.ForeColor = Color.White;
            btnMobile.FlatStyle = FlatStyle.Flat;
            btnMobile.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMobile.Click += (s, e) => {
                try {
                    using (var qr = new QRForm($"http://{MobileServer.CurrentIpAddress}:{MobileServer.Port}")) {
                        qr.ShowDialog(this);
                    }
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            };
            buttonFlow.Controls.Add(btnMobile);

            Button btnMulti = new Button();
            btnMulti.Text = "🖥️ Multi-View";
            btnMulti.Size = new Size(130, 40);
            btnMulti.BackColor = Color.Teal;
            btnMulti.ForeColor = Color.White;
            btnMulti.FlatStyle = FlatStyle.Flat;
            btnMulti.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMulti.Click += (s, e) => { new MultiMonitorForm().Show(); };
            buttonFlow.Controls.Add(btnMulti);

            btnSecureTunnel = new Button();
            btnSecureTunnel.Text = "🌐 Secure Link";
            btnSecureTunnel.Size = new Size(130, 40);
            btnSecureTunnel.BackColor = Color.SlateGray;
            btnSecureTunnel.ForeColor = Color.White;
            btnSecureTunnel.FlatStyle = FlatStyle.Flat;
            btnSecureTunnel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSecureTunnel.Click += async (s, e) => {
                if (_tunnelService.IsRunning)
                {
                    btnSecureTunnel.Enabled = false;
                    _tunnelService.StopTunnel();
                }
                else
                {
                    btnSecureTunnel.Enabled = false;
                    btnSecureTunnel.Text = "⏳ Opening...";
                    bool started = await _tunnelService.StartTunnelAsync(MobileServer.Port);
                    if (!started)
                    {
                        btnSecureTunnel.Enabled = true;
                        btnSecureTunnel.Text = "🌐 Secure Link";
                        MessageBox.Show("cloudflared.exe 파일을 찾을 수 없습니다.\n실행 파일과 같은 폴더에 파일을 복사해 주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };
            buttonFlow.Controls.Add(btnSecureTunnel);

            mainFlow.Controls.Add(buttonFlow);

            // 4. Tunnel URL Display
            lblTunnelLink = new Label();
            lblTunnelLink.Text = "Secure URL: Not Active";
            lblTunnelLink.AutoSize = true;
            lblTunnelLink.ForeColor = Color.Gray;
            lblTunnelLink.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblTunnelLink.Margin = new Padding(10, 0, 0, 10);
            lblTunnelLink.Cursor = Cursors.Hand;
            lblTunnelLink.Click += (s, e) => {
                if (_tunnelService.IsRunning && !string.IsNullOrEmpty(_tunnelService.TunnelUrl))
                {
                    try 
                    {
                        Clipboard.SetText(_tunnelService.TunnelUrl);
                        MessageBox.Show("Secure URL copied to clipboard!");
                    }
                    catch (Exception)
                    {
                        // Clipboard might be locked by another process. Ignore and proceed to open link.
                    }

                    if (_tunnelService.TunnelUrl.StartsWith("http"))
                    {
                        Process.Start(_tunnelService.TunnelUrl);
                    }
                }
            };
            mainFlow.Controls.Add(lblTunnelLink);

            // Rest of Toggle Logic
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
            if (!string.IsNullOrEmpty(_walletService.PublicKey)) {
                txtPublicKey.Text = _walletService.PublicKey;
                ToggleWalletEdit(false); 
            } else { ToggleWalletEdit(true); }

            // 4. Dark Footer (External to Flow)
            Panel pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 35, BackColor = Color.FromArgb(32, 32, 32) };
            Label lblCopy = new Label { Text = "Copyright © 2025 GuruPia. All rights reserved.", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(10, 8) };
            LinkLabel lnkDev = new LinkLabel { Text = "Developed by gurupia.github.io", LinkColor = Color.Gold, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Cursor = Cursors.Hand };
            lnkDev.LinkClicked += (s, e) => { try { Process.Start(new ProcessStartInfo { FileName = "https://gurupia.github.io", UseShellExecute = true }); } catch {} };
            pnlFooter.Controls.Add(lblCopy);
            pnlFooter.Controls.Add(lnkDev);
            this.Controls.Add(pnlFooter);
            pnlFooter.Resize += (s, e) => { lnkDev.Location = new Point(pnlFooter.Width - lnkDev.Width - 15, 8); };
            pnlFooter.BringToFront();
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

                    // SMS Alert
                    _ = _smsService.SendAlertAsync(diff, dBal);
                    
                    if (notifyIcon != null)
                        notifyIcon.ShowBalloonTip(7000, "?뮥 Deposit Detected!", $"+{diff:0.#####} ? Received!\nTotal: {dBal:N2} ?", ToolTipIcon.Info);
                }

                _lastBalance = dBal;
                if (lblBalance != null) lblBalance.Text = $"Wallet: {dBal:N2} ?";
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
                    btnToggleNode.ForeColor = Color.Green;
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
                    lblContainerStatus.ForeColor = Color.Yellow;
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
                    using (var ctsMetric = new CancellationTokenSource(1500))
                    {
                        string metrics = await client.GetStringAsync("http://localhost:31403/metrics");
                    
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
                if (state == "Synced!" || state == "Synced (Docker)")
                {
                    lblMainStatus.Text = (state == "Synced!") ? "Your computer is running the blockchain" : state;
                    lblMainStatus.ForeColor = Color.Green;
                    _totalSyncedSeconds += 3;
                }
                else
                {
                    lblMainStatus.Text = state;
                    lblMainStatus.ForeColor = Color.Orange;
                }
                
                lblLocalBlockNum.Text = _statLocalBlock;
                if (lblRemoteBlockNum != null) lblRemoteBlockNum.Text = _statLocalBlock;
                if (lblState != null) lblState.Text = _statState;
                if (lblLatestBlock != null) lblLatestBlock.Text = _statLocalBlock;
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

                // Update Bonus (Periodic Check)
                if (_bonusUpdateCounter <= 0)
                {
                    _bonusService.LogDebug("UpdateDashboardAsync: Fetching bonus data...");
                    var nodeInfo = await _bonusService.GetNodeInfoAsync();
                    _currentBonus = nodeInfo.Bonus;
                    _bonusService.LogDebug($"UpdateDashboardAsync: Fetched Bonus = {_currentBonus}");

                    if (_currentBonus >= 0)
                    {
                        lblBonus.Text = $"Bonus: {_currentBonus:F4}";
                        if (nodeInfo.CpuCount > 0)
                        {
                            lblServerCpuCount.Text = $"{nodeInfo.CpuCount} Cores";
                        }
                        else
                        {
                            lblServerCpuCount.Text = "N/A (Pending)";
                        }
                        _bonusService.LogDebug($"UI Updated: Bonus={_currentBonus}, CPU={nodeInfo.CpuCount}");
                        
                        // Record to CSV
                        bool portsOk = lblPort01.Text == "Listening" && lblPort03.Text == "Listening";
                        _bonusService.RecordBonus(_currentBonus, avail.ToString("F2"), portsOk);
                    }
                    _bonusUpdateCounter = 12; // Every 1 minute
                }
                _bonusUpdateCounter--;

                // Sync to Mobile
                MobileServer.CurrentStatus = new NodeStatusData
                {
                    State = _statState,
                    Incoming = _statIn,
                    Outgoing = _statOut,
                    LocalBlock = _statLocalBlock,
                    ProtocolVersion = lblProtocolVersion?.Text ?? "Unknown",
                    LedgerAge = _statLedgerAge,
                    Uptime = lblUptime.Text.Replace("Uptime: ", ""),
                    NodeBonus = _currentBonus
                };
                
                // StatusStrip update
                 if (statusLabel != null)
                    statusLabel.Text = $"Synced | Container: {NodeUtility.CurrentContainerName} | Engine: {MobileServer.LastCaptureMode} | Last Update: {DateTime.Now:HH:mm:ss}";
                 
                 // [New] Check Node Health for SMS Alerts
                 CheckNodeHealth();
            }
            catch (Exception ex)
            {
                 // Ignore UI update errors
                 System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void CheckNodeHealth()
        {
            if (_statLocalBlock == "0" || _statLocalBlock == "1") return;

            string alertMsg = _monitorService.CheckNodeStatus(_statLocalBlock, _statState);
            if (alertMsg != null)
            {
                // Only send if enabled in Settings
                if (_smsService.IsNodeAlertEnabled)
                     _ = _smsService.SendAlertAsync(alertMsg);
            }
        }

        private void btnShowHistory_Click(object sender, EventArgs e)
        {
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Bonus_History.csv");
            using (var historyForm = new HistoryForm(csvPath))
            {
                historyForm.ShowDialog();
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (var helpForm = new HelpForm())
            {
                helpForm.ShowDialog();
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

        private void InitializeBot()
        {
            var conf = _smsService.GetConfig();
            if (conf != null && conf.EnableTelegram && !string.IsNullOrEmpty(conf.TelegramBotToken))
            {
                _botService.OnStatusRequested -= Bot_OnStatusRequested; // Prevent Double Hook
                _botService.OnRestartRequested -= Bot_OnRestartRequested;
                
                _botService.OnStatusRequested += Bot_OnStatusRequested;
                _botService.OnRestartRequested += Bot_OnRestartRequested;
                
                _botService.Start(conf.TelegramBotToken, conf.TelegramChatId);
            }
            else
            {
                _botService.Stop();
            }
        }

        private string Bot_OnStatusRequested()
        {
            return $"?뱤 **Node Status Report**\n\n" +
                   $"?뙇 **State**: {_statState}\n" +
                   $"?벀 **Block**: {_statLocalBlock}\n" +
                   $"?뵕 **Peers**: In {_statIn} / Out {_statOut}\n" +
                   $"?뮥 **Balance**: {_lastBalance:N2} Pi\n" +
                   $"??**Uptime**: {lblUptime.Text.Replace("Uptime: ", "")}\n" +
                   $"?뱟 **Time**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        private void Bot_OnRestartRequested()
        {
            // Run in Task to avoid blocking bot thread
            Task.Run(async () => 
            {
                await RunDockerCommandAsync("restart pi-consensus");
                await UpdateDashboardAsync(); // Refresh Status
            });
        }
        
        private void CheckWarnings(string state, int incoming) { }

        private async Task<bool> CheckPortOpenAsync(int port)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    using (var client = new System.Net.Sockets.TcpClient())
                    {
                        await client.ConnectAsync("127.0.0.1", port);
                        return true;
                    }
                }
                catch { return false; }
            });
        }

        private async Task<string?> RunDockerCommandAsync(string arguments)
        {
            return await Task.Run(async () =>
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
                    using (var process = Process.Start(psi))
                    {
                        if (process == null) return null;
                        string output = await process.StandardOutput.ReadToEndAsync();
                        await Task.Run(() => process.WaitForExit(3000)); 
                        return output;
                    }
                }
                catch { return null; }
            });
        }

        private int ParseMetricValue(string data, string key)
        {
            try
            {
                var lines = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
                            var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
        private void ShowPathSettingsDialog()
        {
            Form settingsForm = new Form()
            {
                Width = 600,
                Height = 350,
                Text = "앱 경로 수동 설정 (Path Settings)",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lblDocker = new Label() { Text = "Docker Desktop 경로:", Top = 20, Left = 20, Width = 540 };
            TextBox txtDocker = new TextBox() { Top = 45, Left = 20, Width = 440, Text = NodeUtility.Config.CustomDockerPath };
            Button btnDocker = new Button() { Text = "찾기...", Top = 43, Left = 470, Width = 90 };

            Label lblPi = new Label() { Text = "Pi Network 앱 경로:", Top = 100, Left = 20, Width = 540 };
            TextBox txtPi = new TextBox() { Top = 125, Left = 20, Width = 440, Text = NodeUtility.Config.CustomPiAppPath };
            Button btnPi = new Button() { Text = "찾기...", Top = 123, Left = 470, Width = 90 };

            Button btnSave = new Button() { Text = "저장 및 적용 (Save)", Top = 200, Left = 20, Width = 540, Height = 40, Font = new Font(this.Font, FontStyle.Bold) };

            btnDocker.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Executable Files|*.exe", Title = "Docker Desktop 실행 파일 선택" }) {
                    if (ofd.ShowDialog() == DialogResult.OK) txtDocker.Text = ofd.FileName;
                }
            };

            btnPi.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Executable Files|*.exe", Title = "Pi Network 실행 파일 선택" }) {
                    if (ofd.ShowDialog() == DialogResult.OK) txtPi.Text = ofd.FileName;
                }
            };

            btnSave.Click += (s, e) => {
                NodeUtility.Config.CustomDockerPath = txtDocker.Text.Trim();
                NodeUtility.Config.CustomPiAppPath = txtPi.Text.Trim();
                NodeUtility.SaveConfig();
                MessageBox.Show("설정이 저장되었습니다.", "알림");
                settingsForm.Close();
            };

            settingsForm.Controls.AddRange(new Control[] { lblDocker, txtDocker, btnDocker, lblPi, txtPi, btnPi, btnSave });
            settingsForm.ShowDialog();
        }
    }
}
