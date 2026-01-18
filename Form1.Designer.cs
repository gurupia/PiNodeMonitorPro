namespace PiNodeMonitorWinForm
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.topGrid = new System.Windows.Forms.TableLayoutPanel();
            
            // Containers
            this.groupBoxControl = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBoxMaintenance = new System.Windows.Forms.GroupBox();
            this.midFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.pnlWallet = new System.Windows.Forms.Panel();
            this.groupBoxWallet = new System.Windows.Forms.GroupBox();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.groupBoxQuickActions = new System.Windows.Forms.GroupBox();

            // Controls - Node Control
            this.lblMainStatus = new System.Windows.Forms.Label();
            this.lblLocalBlockLabel = new System.Windows.Forms.Label();
            this.lblLocalBlockNum = new System.Windows.Forms.Label();
            this.lblRemoteBlockLabel = new System.Windows.Forms.Label();
            this.lblRemoteBlockNum = new System.Windows.Forms.Label();
            this.btnToggleNode = new System.Windows.Forms.Button();

            // Controls - General Info
            this.label1 = new System.Windows.Forms.Label();
            this.lblProtocolVersion = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblStellarBuild = new System.Windows.Forms.Label();
            this.labelLocalCpu = new System.Windows.Forms.Label();
            this.lblLocalCpuCount = new System.Windows.Forms.Label();
            this.labelServerCpu = new System.Windows.Forms.Label();
            this.lblServerCpuCount = new System.Windows.Forms.Label();

            // Controls - Consensus
            this.label3 = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblLatestBlock = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblLedgerAge = new System.Windows.Forms.Label();

            // Controls - Network Stats
            this.label4 = new System.Windows.Forms.Label();
            this.lblIncoming = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblOutgoing = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblSupporting = new System.Windows.Forms.Label();

            // Controls - Container Resources
            this.label11 = new System.Windows.Forms.Label();
            this.lblContainerStatus = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblCPU = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblRAM = new System.Windows.Forms.Label();

            // Controls - Maintenance
            this.chkEnableCpuOpt = new System.Windows.Forms.CheckBox();
            this.lblGhostSpace = new System.Windows.Forms.Label();
            this.btnCompactDisk = new System.Windows.Forms.Button();

            // Controls - Session Stats
            this.lblUptime = new System.Windows.Forms.Label();
            this.lblAvailability = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnShowHistory = new System.Windows.Forms.Button();
            // Bonus UI Removed

            // Controls - Port Status
            this.label12 = new System.Windows.Forms.Label();
            this.lblPort01 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblPort02 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblPort03 = new System.Windows.Forms.Label();

            // Controls - Wallet
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.txtPublicKey = new System.Windows.Forms.TextBox();
            this.btnSaveKey = new System.Windows.Forms.Button();
            this.btnChangeWallet = new System.Windows.Forms.Button();

            // Controls - Quick Actions
            this.btnSecureTunnel = new System.Windows.Forms.Button();
            this.btnMulti = new System.Windows.Forms.Button();
            this.btnMobile = new System.Windows.Forms.Button();
            this.lblTunnelLink = new System.Windows.Forms.Label();
            // btnSmsConfig removed/hidden as per previous code but declared in fields. Let's declare it to avoid errors but not add it if not needed.
            this.btnSmsConfig = new System.Windows.Forms.Button();

            // Status Strip
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblCopyrightStatus = new System.Windows.Forms.ToolStripStatusLabel();
            
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);

            this.mainFlow.SuspendLayout();
            this.topGrid.SuspendLayout();
            this.midFlow.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBoxMaintenance.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.pnlWallet.SuspendLayout();
            this.groupBoxWallet.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.groupBoxQuickActions.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // 
            // mainFlow
            // 
            this.mainFlow.AutoScroll = true;
            this.mainFlow.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.mainFlow.Controls.Add(this.topGrid);
            this.mainFlow.Controls.Add(this.midFlow);
            this.mainFlow.Controls.Add(this.pnlWallet);
            this.mainFlow.Controls.Add(this.pnlActions);
            this.mainFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainFlow.Location = new System.Drawing.Point(0, 0);
            this.mainFlow.Name = "mainFlow";
            this.mainFlow.Padding = new System.Windows.Forms.Padding(10);
            this.mainFlow.Size = new System.Drawing.Size(550, 920);
            this.mainFlow.WrapContents = false;

            // 
            // topGrid
            // 
            this.topGrid.AutoSize = true;
            this.topGrid.ColumnCount = 2;
            this.topGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.topGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.topGrid.Controls.Add(this.groupBoxControl, 0, 0);
            this.topGrid.Controls.Add(this.groupBox2, 1, 0);
            this.topGrid.Controls.Add(this.groupBox1, 0, 1);
            this.topGrid.Controls.Add(this.groupBox4, 1, 1);
            this.topGrid.Controls.Add(this.groupBoxMaintenance, 0, 2);
            this.topGrid.Controls.Add(this.groupBox3, 1, 2);
            this.topGrid.Location = new System.Drawing.Point(10, 10);
            this.topGrid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.topGrid.Name = "topGrid";
            this.topGrid.RowCount = 3;
            // No RowStyle needed for AutoSize rows usually, but good to have explicit
            this.topGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.topGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.topGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.topGrid.Size = new System.Drawing.Size(510, 420); 

            // Common GroupBox settings
            void ConfigGB(System.Windows.Forms.GroupBox gb, string text, int height) {
                gb.Dock = System.Windows.Forms.DockStyle.Fill;
                gb.ForeColor = System.Drawing.Color.White;
                gb.Text = text;
                gb.Size = new System.Drawing.Size(250, height); 
            }

            ConfigGB(this.groupBoxControl, "Node Control", 134);
            ConfigGB(this.groupBox2, "Consensus", 134);
            ConfigGB(this.groupBox1, "General Info", 134);
            ConfigGB(this.groupBox4, "Container Resources", 134);
            ConfigGB(this.groupBoxMaintenance, "Performance Maintenance", 134);
            ConfigGB(this.groupBox3, "Network Stats", 134);

            // Node Control Internal
            this.groupBoxControl.Controls.Add(this.lblMainStatus);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockNum);
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockNum);
            this.groupBoxControl.Controls.Add(this.btnToggleNode);

            this.lblMainStatus.Location = new System.Drawing.Point(10, 20);
            this.lblMainStatus.Size = new System.Drawing.Size(230, 25);
            this.lblMainStatus.Text = "Your computer is running the blockchain";
            this.lblMainStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMainStatus.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblMainStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblLocalBlockLabel.Location = new System.Drawing.Point(10, 48);
            this.lblLocalBlockLabel.Size = new System.Drawing.Size(120, 15);
            this.lblLocalBlockLabel.Text = "Local block number:";

            this.lblLocalBlockNum.Location = new System.Drawing.Point(135, 48);
            this.lblLocalBlockNum.AutoSize = true;
            this.lblLocalBlockNum.Text = "...";
            this.lblLocalBlockNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblRemoteBlockLabel.Location = new System.Drawing.Point(10, 68);
            this.lblRemoteBlockLabel.Size = new System.Drawing.Size(120, 15);
            this.lblRemoteBlockLabel.Text = "Latest block number:";

            this.lblRemoteBlockNum.Location = new System.Drawing.Point(135, 68);
            this.lblRemoteBlockNum.AutoSize = true;
            this.lblRemoteBlockNum.Text = "...";
            this.lblRemoteBlockNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.btnToggleNode.Location = new System.Drawing.Point(10, 88);
            this.btnToggleNode.Size = new System.Drawing.Size(230, 35);
            this.btnToggleNode.Text = "Node is ON (Click to OFF)";
            this.btnToggleNode.BackColor = System.Drawing.Color.White;
            this.btnToggleNode.ForeColor = System.Drawing.Color.LimeGreen;
            this.btnToggleNode.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);

            // General Info Internal
            this.groupBox1.Controls.Add(this.label1); this.groupBox1.Controls.Add(this.lblProtocolVersion);
            this.groupBox1.Controls.Add(this.label7); this.groupBox1.Controls.Add(this.lblStellarBuild);
            this.groupBox1.Controls.Add(this.labelLocalCpu); this.groupBox1.Controls.Add(this.lblLocalCpuCount);
            this.groupBox1.Controls.Add(this.labelServerCpu); this.groupBox1.Controls.Add(this.lblServerCpuCount);

            this.label1.Location = new System.Drawing.Point(10, 22); this.label1.AutoSize = true; this.label1.Text = "Protocol Ver:";
            this.lblProtocolVersion.Location = new System.Drawing.Point(110, 22); this.lblProtocolVersion.AutoSize = true; this.lblProtocolVersion.Text = "..."; this.lblProtocolVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label7.Location = new System.Drawing.Point(10, 45); this.label7.AutoSize = true; this.label7.Text = "Core Build:";
            this.lblStellarBuild.Location = new System.Drawing.Point(110, 45); this.lblStellarBuild.AutoSize = true; this.lblStellarBuild.Text = "..."; this.lblStellarBuild.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.labelLocalCpu.Location = new System.Drawing.Point(10, 70); this.labelLocalCpu.AutoSize = true; this.labelLocalCpu.Text = "Local CPU:";
            this.lblLocalCpuCount.Location = new System.Drawing.Point(110, 70); this.lblLocalCpuCount.AutoSize = true; this.lblLocalCpuCount.Text = "Checking..."; this.lblLocalCpuCount.ForeColor = System.Drawing.Color.SkyBlue; this.lblLocalCpuCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.labelServerCpu.Location = new System.Drawing.Point(10, 95); this.labelServerCpu.AutoSize = true; this.labelServerCpu.Text = "Server CPU:";
            this.lblServerCpuCount.Location = new System.Drawing.Point(110, 95); this.lblServerCpuCount.AutoSize = true; this.lblServerCpuCount.Text = "N/A"; this.lblServerCpuCount.ForeColor = System.Drawing.Color.SkyBlue; this.lblServerCpuCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Consensus Internal
            this.groupBox2.Controls.Add(this.label3); this.groupBox2.Controls.Add(this.lblState);
            this.groupBox2.Controls.Add(this.label2); this.groupBox2.Controls.Add(this.lblLatestBlock);
            this.groupBox2.Controls.Add(this.label9); this.groupBox2.Controls.Add(this.lblLedgerAge);

            this.label3.Location = new System.Drawing.Point(10, 22); this.label3.AutoSize = true; this.label3.Text = "State:";
            this.lblState.Location = new System.Drawing.Point(110, 22); this.lblState.AutoSize = true; this.lblState.Text = "..."; this.lblState.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label2.Location = new System.Drawing.Point(10, 45); this.label2.AutoSize = true; this.label2.Text = "Block:";
            this.lblLatestBlock.Location = new System.Drawing.Point(110, 45); this.lblLatestBlock.AutoSize = true; this.lblLatestBlock.Text = "..."; this.lblLatestBlock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label9.Location = new System.Drawing.Point(10, 70); this.label9.AutoSize = true; this.label9.Text = "Age:";
            this.lblLedgerAge.Location = new System.Drawing.Point(110, 70); this.lblLedgerAge.AutoSize = true; this.lblLedgerAge.Text = "0s"; this.lblLedgerAge.ForeColor = System.Drawing.Color.SkyBlue; this.lblLedgerAge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Container Resources Internal
            this.groupBox4.Controls.Add(this.label11); this.groupBox4.Controls.Add(this.lblContainerStatus);
            this.groupBox4.Controls.Add(this.label8); this.groupBox4.Controls.Add(this.lblCPU);
            this.groupBox4.Controls.Add(this.label10); this.groupBox4.Controls.Add(this.lblRAM);

            this.label11.Location = new System.Drawing.Point(10, 22); this.label11.AutoSize = true; this.label11.Text = "Name:";
            this.lblContainerStatus.Location = new System.Drawing.Point(70, 22); this.lblContainerStatus.AutoSize = true; this.lblContainerStatus.Text = "Checking..."; this.lblContainerStatus.ForeColor = System.Drawing.Color.Yellow; this.lblContainerStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label8.Location = new System.Drawing.Point(10, 45); this.label8.AutoSize = true; this.label8.Text = "CPU:";
            this.lblCPU.Location = new System.Drawing.Point(70, 45); this.lblCPU.AutoSize = true; this.lblCPU.Text = "..."; this.lblCPU.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label10.Location = new System.Drawing.Point(10, 70); this.label10.AutoSize = true; this.label10.Text = "RAM:";
            this.lblRAM.Location = new System.Drawing.Point(70, 70); this.lblRAM.AutoSize = true; this.lblRAM.Text = "..."; this.lblRAM.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);


            // Maintenance Internal
            this.groupBoxMaintenance.Controls.Add(this.chkEnableCpuOpt);
            this.groupBoxMaintenance.Controls.Add(this.lblGhostSpace);
            this.groupBoxMaintenance.Controls.Add(this.btnCompactDisk);

            this.chkEnableCpuOpt.Location = new System.Drawing.Point(15, 22);
            this.chkEnableCpuOpt.Size = new System.Drawing.Size(220, 20);
            this.chkEnableCpuOpt.Text = "Smart Core Switching (P/E)";
            this.chkEnableCpuOpt.Checked = true;

            this.lblGhostSpace.Location = new System.Drawing.Point(15, 50);
            this.lblGhostSpace.AutoSize = true;
            this.lblGhostSpace.Text = "Ghost Space: 0.00 GB";

            this.btnCompactDisk.Location = new System.Drawing.Point(10, 80);
            this.btnCompactDisk.Size = new System.Drawing.Size(235, 35);
            this.btnCompactDisk.Text = "Compact WSL2 Disk (Optimize)";
            this.btnCompactDisk.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.btnCompactDisk.UseVisualStyleBackColor = false;

            // Network Stats Internal
            this.groupBox3.Controls.Add(this.label4); this.groupBox3.Controls.Add(this.lblIncoming);
            this.groupBox3.Controls.Add(this.label5); this.groupBox3.Controls.Add(this.lblOutgoing);
            this.groupBox3.Controls.Add(this.label6); this.groupBox3.Controls.Add(this.lblSupporting);

            this.label4.Location = new System.Drawing.Point(10, 22); this.label4.AutoSize = true; this.label4.Text = "Incoming:";
            this.lblIncoming.Location = new System.Drawing.Point(110, 22); this.lblIncoming.AutoSize = true; this.lblIncoming.Text = "0"; this.lblIncoming.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label5.Location = new System.Drawing.Point(10, 45); this.label5.AutoSize = true; this.label5.Text = "Outgoing:";
            this.lblOutgoing.Location = new System.Drawing.Point(110, 45); this.lblOutgoing.AutoSize = true; this.lblOutgoing.Text = "0"; this.lblOutgoing.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.label6.Location = new System.Drawing.Point(10, 70); this.label6.AutoSize = true; this.label6.Text = "Supporting:";
            this.lblSupporting.Location = new System.Drawing.Point(110, 70); this.lblSupporting.AutoSize = true; this.lblSupporting.Text = "No"; this.lblSupporting.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // 
            // midFlow
            // 
            this.midFlow.AutoSize = true;
            this.midFlow.Controls.Add(this.groupBox6);
            this.midFlow.Controls.Add(this.groupBox5);
            this.midFlow.Location = new System.Drawing.Point(10, 440);
            this.midFlow.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.midFlow.Name = "midFlow";
            this.midFlow.Size = new System.Drawing.Size(510, 140);
            
            // Session Stats
            this.groupBox6.Controls.Add(this.lblUptime);
            this.groupBox6.Controls.Add(this.lblAvailability);
            this.groupBox6.Controls.Add(this.btnShowHistory);
            this.groupBox6.Controls.Add(this.btnHelp);
            this.groupBox6.ForeColor = System.Drawing.Color.White;
            this.groupBox6.Text = "Session Stats";
            this.groupBox6.Size = new System.Drawing.Size(320, 134); // Fixed width logic
            this.groupBox6.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);

            this.lblUptime.Location = new System.Drawing.Point(10, 25);
            this.lblUptime.AutoSize = true;
            this.lblUptime.Text = "Uptime: 00:00:00";

            this.lblAvailability.Location = new System.Drawing.Point(10, 50);
            this.lblAvailability.AutoSize = true;
            this.lblAvailability.Text = "Availability: Checking...";

            this.btnHelp.Location = new System.Drawing.Point(10, 90);
            this.btnHelp.Size = new System.Drawing.Size(60, 30);
            this.btnHelp.Text = "Help";
            this.btnHelp.BackColor = System.Drawing.Color.SandyBrown;
            this.btnHelp.ForeColor = System.Drawing.Color.Black;

            this.btnShowHistory.Location = new System.Drawing.Point(80, 90);
            this.btnShowHistory.Size = new System.Drawing.Size(80, 30);
            this.btnShowHistory.Text = "History";
            this.btnShowHistory.BackColor = System.Drawing.Color.SandyBrown;
            this.btnShowHistory.ForeColor = System.Drawing.Color.Black;

            // Port Status
            this.groupBox5.Controls.Add(this.label12); this.groupBox5.Controls.Add(this.lblPort01);
            this.groupBox5.Controls.Add(this.label13); this.groupBox5.Controls.Add(this.lblPort02);
            this.groupBox5.Controls.Add(this.label14); this.groupBox5.Controls.Add(this.lblPort03);
            this.groupBox5.ForeColor = System.Drawing.Color.White;
            this.groupBox5.Text = "Port Status";
            this.groupBox5.Size = new System.Drawing.Size(185, 134);

            this.label12.Location = new System.Drawing.Point(15, 25);
            this.label12.AutoSize = true;
            this.label12.Text = "31401:";
            this.lblPort01.Location = new System.Drawing.Point(80, 25);
            this.lblPort01.AutoSize = true;
            this.lblPort01.Text = "Checking...";
            this.lblPort01.ForeColor = System.Drawing.Color.Yellow;

            this.label13.Location = new System.Drawing.Point(15, 55);
            this.label13.AutoSize = true;
            this.label13.Text = "31402:";
            this.lblPort02.Location = new System.Drawing.Point(80, 55);
            this.lblPort02.AutoSize = true;
            this.lblPort02.Text = "Checking...";
            this.lblPort02.ForeColor = System.Drawing.Color.Yellow;

            this.label14.Location = new System.Drawing.Point(15, 85);
            this.label14.AutoSize = true;
            this.label14.Text = "31403:";
            this.lblPort03.Location = new System.Drawing.Point(80, 85);
            this.lblPort03.AutoSize = true;
            this.lblPort03.Text = "Checking...";
            this.lblPort03.ForeColor = System.Drawing.Color.Yellow;


            // pnlWallet
            // this.pnlWallet.AutoSize = true; // REMOVED to prevent collapse
            this.pnlWallet.Controls.Add(this.groupBoxWallet);
            this.pnlWallet.Location = new System.Drawing.Point(10, 595);
            this.pnlWallet.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.pnlWallet.Name = "pnlWallet";
            this.pnlWallet.Size = new System.Drawing.Size(510, 115);

            this.groupBoxWallet.Controls.Add(this.lblBalance);
            this.groupBoxWallet.Controls.Add(this.lblPrice);
            this.groupBoxWallet.Controls.Add(this.lblTotalValue);
            this.groupBoxWallet.Controls.Add(this.txtPublicKey);
            this.groupBoxWallet.Controls.Add(this.btnSaveKey);
            this.groupBoxWallet.Controls.Add(this.btnChangeWallet);
            this.groupBoxWallet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxWallet.ForeColor = System.Drawing.Color.White;
            this.groupBoxWallet.Text = "Wallet Management Monitoring";

            this.lblBalance.Location = new System.Drawing.Point(20, 25); this.lblBalance.AutoSize = true;
            this.lblBalance.Text = "Wallet: -- ?\u03C0";
            this.lblBalance.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblBalance.ForeColor = System.Drawing.Color.Gold;

            this.lblPrice.Location = new System.Drawing.Point(20, 55); this.lblPrice.AutoSize = true;
            this.lblPrice.Text = "Price: $0.00";
            this.lblPrice.ForeColor = System.Drawing.Color.SandyBrown;

            this.lblTotalValue.Location = new System.Drawing.Point(140, 55); this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Text = "Value: $0.00";
            this.lblTotalValue.ForeColor = System.Drawing.Color.SandyBrown;

            this.txtPublicKey.Location = new System.Drawing.Point(20, 80);
            this.txtPublicKey.Size = new System.Drawing.Size(380, 21);
            this.txtPublicKey.BackColor = System.Drawing.Color.FromArgb(25, 35, 50);
            this.txtPublicKey.ForeColor = System.Drawing.Color.White;

            this.btnSaveKey.Location = new System.Drawing.Point(405, 78);
            this.btnSaveKey.Size = new System.Drawing.Size(30, 25);
            this.btnSaveKey.Text = "\uD83D\uDCBE";
            this.btnSaveKey.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.btnSaveKey.UseVisualStyleBackColor = false;

            this.btnChangeWallet.Location = new System.Drawing.Point(445, 78);
            this.btnChangeWallet.Size = new System.Drawing.Size(30, 25);
            this.btnChangeWallet.Text = "\uD83D\uDCAC";
            this.btnChangeWallet.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.btnChangeWallet.UseVisualStyleBackColor = false;

            // pnlActions
            // this.pnlActions.AutoSize = true; // REMOVED to prevent collapse
            this.pnlActions.Controls.Add(this.groupBoxQuickActions);
            this.pnlActions.Location = new System.Drawing.Point(10, 720);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(510, 115);

            this.groupBoxQuickActions.Controls.Add(this.btnSecureTunnel);
            this.groupBoxQuickActions.Controls.Add(this.btnMulti);
            this.groupBoxQuickActions.Controls.Add(this.btnMobile);
            this.groupBoxQuickActions.Controls.Add(this.lblTunnelLink);
            this.groupBoxQuickActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxQuickActions.ForeColor = System.Drawing.Color.White;
            this.groupBoxQuickActions.Text = "Remote Multi-Management";

            this.btnSecureTunnel.Location = new System.Drawing.Point(15, 25);
            this.btnSecureTunnel.Size = new System.Drawing.Size(155, 45);
            this.btnSecureTunnel.Text = " \u25CF Secure Link";
            this.btnSecureTunnel.BackColor = System.Drawing.Color.SlateGray;
            this.btnSecureTunnel.ForeColor = System.Drawing.Color.White;
            this.btnSecureTunnel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSecureTunnel.UseVisualStyleBackColor = false;

            this.btnMulti.Location = new System.Drawing.Point(180, 25);
            this.btnMulti.Size = new System.Drawing.Size(155, 45);
            this.btnMulti.Text = " \uD83D\uDDA5 Multi-View";
            this.btnMulti.BackColor = System.Drawing.Color.Teal;
            this.btnMulti.ForeColor = System.Drawing.Color.White;
            this.btnMulti.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnMulti.UseVisualStyleBackColor = false;

            this.btnMobile.Location = new System.Drawing.Point(345, 25);
            this.btnMobile.Size = new System.Drawing.Size(155, 45);
            this.btnMobile.Text = " \uD83D\uDCF1 Mobile Connect";
            this.btnMobile.BackColor = System.Drawing.Color.BlueViolet;
            this.btnMobile.ForeColor = System.Drawing.Color.White;
            this.btnMobile.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnMobile.UseVisualStyleBackColor = false;

            this.lblTunnelLink.Location = new System.Drawing.Point(20, 80);
            this.lblTunnelLink.AutoSize = true;
            this.lblTunnelLink.Text = "Secure URL: Not Active";
            this.lblTunnelLink.ForeColor = System.Drawing.Color.LightGray;

            // StatusStrip
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.lblCopyrightStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 898);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(550, 22);
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(45, 45, 48); // Dark Gray Background

            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusLabel.Text = "Ready";
            this.statusLabel.ForeColor = System.Drawing.Color.White; // White Text

            this.lblCopyrightStatus.Name = "lblCopyrightStatus";
            this.lblCopyrightStatus.Text = "Developed by gurupia.github.io  |  Copyright \u00A9 2025 GuruPia.";
            this.lblCopyrightStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCopyrightStatus.ForeColor = System.Drawing.Color.White; // White Text

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
            this.ClientSize = new System.Drawing.Size(550, 920);
            this.Controls.Add(this.mainFlow);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pi Node Monitor Pro v1.8.26 (Hybrid Layout)";

            this.mainFlow.ResumeLayout(false);
            this.mainFlow.PerformLayout();
            this.topGrid.ResumeLayout(false);
            this.topGrid.PerformLayout();
            this.midFlow.ResumeLayout(false);
            this.midFlow.PerformLayout();
            this.groupBoxControl.ResumeLayout(false); this.groupBoxControl.PerformLayout();
            this.groupBox1.ResumeLayout(false); this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false); this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false); this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false); this.groupBox4.PerformLayout();
            this.groupBoxMaintenance.ResumeLayout(false); this.groupBoxMaintenance.PerformLayout();
            this.groupBox6.ResumeLayout(false); this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false); this.groupBox5.PerformLayout();
            this.pnlWallet.ResumeLayout(false); this.pnlWallet.PerformLayout();
            this.groupBoxWallet.ResumeLayout(false); this.groupBoxWallet.PerformLayout();
            this.pnlActions.ResumeLayout(false); this.pnlActions.PerformLayout();
            this.groupBoxQuickActions.ResumeLayout(false); this.groupBoxQuickActions.PerformLayout();
            this.statusStrip1.ResumeLayout(false); this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel mainFlow;
        private System.Windows.Forms.TableLayoutPanel topGrid;
        // GroupBoxes
        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBoxMaintenance;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBoxWallet;
        private System.Windows.Forms.GroupBox groupBoxQuickActions;
        private System.Windows.Forms.Panel pnlWallet;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.FlowLayoutPanel midFlow;

        // Node Control
        private System.Windows.Forms.Label lblMainStatus;
        private System.Windows.Forms.Label lblLocalBlockLabel;
        private System.Windows.Forms.Label lblLocalBlockNum;
        private System.Windows.Forms.Label lblRemoteBlockLabel;
        private System.Windows.Forms.Label lblRemoteBlockNum;
        private System.Windows.Forms.Button btnToggleNode;

        // General Info
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblProtocolVersion;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblStellarBuild;
        private System.Windows.Forms.Label labelLocalCpu;
        private System.Windows.Forms.Label lblLocalCpuCount;
        private System.Windows.Forms.Label labelServerCpu;
        private System.Windows.Forms.Label lblServerCpuCount;

        // Consensus
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblLatestBlock;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblLedgerAge;

        // Container Resources
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblContainerStatus;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblCPU;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblRAM;

        // Network Stats
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblIncoming;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblOutgoing;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblSupporting;

        // Performance Maintenance
        private System.Windows.Forms.CheckBox chkEnableCpuOpt;
        private System.Windows.Forms.Label lblGhostSpace;
        private System.Windows.Forms.Button btnCompactDisk;

        // Session Stats
        private System.Windows.Forms.Label lblUptime;
        private System.Windows.Forms.Label lblAvailability;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnShowHistory;
        // private System.Windows.Forms.TextBox txtManualBonus; // Removed
        // private System.Windows.Forms.Button btnSetBonus;     // Removed

        // Port Status
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblPort01;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblPort02;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblPort03;

        // Wallet
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.TextBox txtPublicKey;
        private System.Windows.Forms.Button btnSaveKey;
        private System.Windows.Forms.Button btnChangeWallet;

        // Remote
        private System.Windows.Forms.Button btnSecureTunnel;
        private System.Windows.Forms.Button btnMulti;
        private System.Windows.Forms.Button btnMobile;
        private System.Windows.Forms.Label lblTunnelLink;
        private System.Windows.Forms.Button btnSmsConfig;

        // System
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel lblCopyrightStatus;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
