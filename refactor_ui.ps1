$path = 'f:\repos\CSharp\PiDesktop_054\pi-network-desktop\resources\app.asar-repack\PiNodeMonitorWinForm\Form1.Designer.cs'
$content = Get-Content $path -Raw

# Replace Initialization Block with COMPLETE restored logic
$startPattern = 'private void InitializeComponent\(\)'
$endPattern = 'this\.groupBoxControl\.ResumeLayout\(false\);'

$newCode = @"
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblStellarBuild = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblProtocolVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblLedgerAge = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLatestBlock = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblSupporting = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblOutgoing = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblIncoming = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblLastUpdated = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblRAM = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblCPU = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblContainerStatus = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblPort03 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblPort02 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblPort01 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.lblAvailability = new System.Windows.Forms.Label();
            this.lblUptime = new System.Windows.Forms.Label();
            this.lblBonus = new System.Windows.Forms.Label();
            this.lblServerCpuCount = new System.Windows.Forms.Label();
            this.labelServerCpu = new System.Windows.Forms.Label();
            this.lblLocalCpuCount = new System.Windows.Forms.Label();
            this.labelLocalCpu = new System.Windows.Forms.Label();
            this.btnShowHistory = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBoxControl = new System.Windows.Forms.GroupBox();
            this.btnToggleNode = new System.Windows.Forms.Button();
            this.lblMainStatus = new System.Windows.Forms.Label();
            this.lblLocalBlockLabel = new System.Windows.Forms.Label();
            this.lblLocalBlockNum = new System.Windows.Forms.Label();
            this.lblRemoteBlockLabel = new System.Windows.Forms.Label();
            this.lblRemoteBlockNum = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.topRowFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.leftFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.rightFlow = new System.Windows.Forms.FlowLayoutPanel();

            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.SuspendLayout();

            // Config Flows
            this.mainFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainFlow.AutoScroll = true;
            this.mainFlow.Padding = new System.Windows.Forms.Padding(10);
            this.mainFlow.WrapContents = false;
            this.mainFlow.BackColor = System.Drawing.Color.FromArgb(41, 41, 41);

            this.topRowFlow.AutoSize = true;
            this.topRowFlow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.topRowFlow.Margin = new System.Windows.Forms.Padding(0);
            this.topRowFlow.WrapContents = false;

            this.leftFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.leftFlow.AutoSize = true;
            this.leftFlow.Margin = new System.Windows.Forms.Padding(0);

            this.rightFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.rightFlow.AutoSize = true;
            this.rightFlow.Margin = new System.Windows.Forms.Padding(0);

            // GroupBox 1 (General Info)
            this.groupBox1.Controls.Add(this.lblLocalCpuCount);
            this.groupBox1.Controls.Add(this.labelLocalCpu);
            this.groupBox1.Controls.Add(this.lblServerCpuCount);
            this.groupBox1.Controls.Add(this.labelServerCpu);
            this.groupBox1.Controls.Add(this.lblStellarBuild);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lblProtocolVersion);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupBox1.MinimumSize = new System.Drawing.Size(230, 130);
            this.groupBox1.Text = "General Info";
            this.groupBox1.ForeColor = System.Drawing.Color.White;

            // GroupBox 2 (Consensus)
            this.groupBox2.Controls.Add(this.lblLedgerAge);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.lblState);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lblLatestBlock);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(10, 0, 0, 10);
            this.groupBox2.Size = new System.Drawing.Size(230, 100);
            this.groupBox2.Text = "Consensus";
            this.groupBox2.ForeColor = System.Drawing.Color.White;

            // GroupBox 3 (Network)
            this.groupBox3.Controls.Add(this.lblSupporting);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.lblOutgoing);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.lblIncoming);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupBox3.Size = new System.Drawing.Size(230, 120);
            this.groupBox3.Text = "Network Stats";
            this.groupBox3.ForeColor = System.Drawing.Color.White;

            // GroupBox 4 (Resources)
            this.groupBox4.Controls.Add(this.lblRAM);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.lblCPU);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.lblContainerStatus);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(10, 0, 0, 10);
            this.groupBox4.Size = new System.Drawing.Size(230, 120);
            this.groupBox4.Text = "Container Resources";
            this.groupBox4.ForeColor = System.Drawing.Color.White;

            // GroupBox 5 (Ports)
            this.groupBox5.Controls.Add(this.lblPort03);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.lblPort02);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.lblPort01);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupBox5.Size = new System.Drawing.Size(470, 60);
            this.groupBox5.Text = "Local Port Check (Listening)";
            this.groupBox5.ForeColor = System.Drawing.Color.White;

            // GroupBox 6 (Stats)
            this.groupBox6.Controls.Add(this.btnHelp);
            this.groupBox6.Controls.Add(this.btnShowHistory);
            this.groupBox6.Controls.Add(this.lblBonus);
            this.groupBox6.Controls.Add(this.lblAvailability);
            this.groupBox6.Controls.Add(this.lblUptime);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupBox6.Size = new System.Drawing.Size(470, 70);
            this.groupBox6.Text = "Session Statistics";
            this.groupBox6.ForeColor = System.Drawing.Color.White;

            // GroupBox Control
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockNum);
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockNum);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblMainStatus);
            this.groupBoxControl.Controls.Add(this.btnToggleNode);
            this.groupBoxControl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupBoxControl.Size = new System.Drawing.Size(230, 140);
            this.groupBoxControl.Text = "Node Control";
            this.groupBoxControl.ForeColor = System.Drawing.Color.White;

            // Labels setup (simplified context)
            this.label1.Text = "Protocol Ver:"; this.label1.Location = new System.Drawing.Point(10, 25); this.lblProtocolVersion.Location = new System.Drawing.Point(110, 25);
            this.label7.Text = "Core Build:"; this.label7.Location = new System.Drawing.Point(10, 50); this.lblStellarBuild.Location = new System.Drawing.Point(110, 50);
            this.labelLocalCpu.Text = "Local CPU:"; this.labelLocalCpu.Location = new System.Drawing.Point(10, 75); this.lblLocalCpuCount.Location = new System.Drawing.Point(110, 75);
            this.labelServerCpu.Text = "Server CPU:"; this.labelServerCpu.Location = new System.Drawing.Point(10, 100); this.lblServerCpuCount.Location = new System.Drawing.Point(110, 100);
            
            this.label3.Text = "State:"; this.label3.Location = new System.Drawing.Point(10, 25); this.lblState.Location = new System.Drawing.Point(110, 25);
            this.label2.Text = "Block:"; this.label2.Location = new System.Drawing.Point(10, 50); this.lblLatestBlock.Location = new System.Drawing.Point(110, 50);
            this.label9.Text = "Age:"; this.label9.Location = new System.Drawing.Point(10, 75); this.lblLedgerAge.Location = new System.Drawing.Point(110, 75);

            // Assembly
            this.leftFlow.Controls.Add(this.groupBoxControl);
            this.leftFlow.Controls.Add(this.groupBox1);
            this.leftFlow.Controls.Add(this.groupBox3);
            this.rightFlow.Controls.Add(this.groupBox2);
            this.rightFlow.Controls.Add(this.groupBox4);
            this.topRowFlow.Controls.Add(this.leftFlow);
            this.topRowFlow.Controls.Add(this.rightFlow);
            this.mainFlow.Controls.Add(this.topRowFlow);
            this.mainFlow.Controls.Add(this.groupBox5);
            this.mainFlow.Controls.Add(this.groupBox6);
            
            this.lblLastUpdated.ForeColor = System.Drawing.Color.Silver;
            this.mainFlow.Controls.Add(this.lblLastUpdated);

            this.ClientSize = new System.Drawing.Size(510, 720);
            this.Controls.Add(this.mainFlow);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Pi Node Monitor Pro";

            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBoxControl.ResumeLayout(false);
"@

# Regex replace with the whole block
$content = $content -replace "(?s)private void InitializeComponent\(\).*?this\.groupBoxControl\.ResumeLayout\(false\);", $newCode

[System.IO.File]::WriteAllText($path, $content)
