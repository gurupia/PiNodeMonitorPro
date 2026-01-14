using System;
using System.Drawing;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            lblLocalCpuCount = new System.Windows.Forms.Label();
            labelLocalCpu = new System.Windows.Forms.Label();
            lblServerCpuCount = new System.Windows.Forms.Label();
            labelServerCpu = new System.Windows.Forms.Label();
            lblStellarBuild = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            lblProtocolVersion = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            lblLedgerAge = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            lblState = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lblLatestBlock = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox3 = new System.Windows.Forms.GroupBox();
            lblSupporting = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            lblOutgoing = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            lblIncoming = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            lblLastUpdated = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            groupBox4 = new System.Windows.Forms.GroupBox();
            lblRAM = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            lblCPU = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            lblContainerStatus = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            groupBox5 = new System.Windows.Forms.GroupBox();
            lblPort03 = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            lblPort02 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            lblPort01 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            groupBox6 = new System.Windows.Forms.GroupBox();
            btnHelp = new System.Windows.Forms.Button();
            btnShowHistory = new System.Windows.Forms.Button();
            lblBonus = new System.Windows.Forms.Label();
            lblAvailability = new System.Windows.Forms.Label();
            lblUptime = new System.Windows.Forms.Label();
            notifyIcon1 = new NotifyIcon(components);
            groupBoxControl = new System.Windows.Forms.GroupBox();
            lblRemoteBlockNum = new System.Windows.Forms.Label();
            lblRemoteBlockLabel = new System.Windows.Forms.Label();
            lblLocalBlockNum = new System.Windows.Forms.Label();
            lblLocalBlockLabel = new System.Windows.Forms.Label();
            lblMainStatus = new System.Windows.Forms.Label();
            btnToggleNode = new System.Windows.Forms.Button();
            toolTip1 = new ToolTip(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            mainFlow = new System.Windows.Forms.FlowLayoutPanel();
            topRowFlow = new System.Windows.Forms.FlowLayoutPanel();
            leftFlow = new System.Windows.Forms.FlowLayoutPanel();
            rightFlow = new System.Windows.Forms.FlowLayoutPanel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBoxControl.SuspendLayout();
            mainFlow.SuspendLayout();
            topRowFlow.SuspendLayout();
            leftFlow.SuspendLayout();
            rightFlow.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(lblLocalCpuCount);
            groupBox1.Controls.Add(labelLocalCpu);
            groupBox1.Controls.Add(lblServerCpuCount);
            groupBox1.Controls.Add(labelServerCpu);
            groupBox1.Controls.Add(lblStellarBuild);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(lblProtocolVersion);
            groupBox1.Controls.Add(label1);
            groupBox1.ForeColor = Color.White;
            groupBox1.Location = new Point(0, 150);
            groupBox1.Margin = new Padding(0, 0, 0, 10);
            groupBox1.MinimumSize = new Size(230, 130);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(230, 134);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "General Info";
            // 
            // lblLocalCpuCount
            // 
            lblLocalCpuCount.AutoSize = true;
            lblLocalCpuCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLocalCpuCount.Location = new Point(110, 75);
            lblLocalCpuCount.Name = "lblLocalCpuCount";
            lblLocalCpuCount.Size = new Size(48, 15);
            lblLocalCpuCount.TabIndex = 0;
            lblLocalCpuCount.Text = "0 Cores";
            // 
            // labelLocalCpu
            // 
            labelLocalCpu.AutoSize = true;
            labelLocalCpu.Location = new Point(10, 75);
            labelLocalCpu.Name = "labelLocalCpu";
            labelLocalCpu.Size = new Size(65, 15);
            labelLocalCpu.TabIndex = 1;
            labelLocalCpu.Text = "Local CPU:";
            // 
            // lblServerCpuCount
            // 
            lblServerCpuCount.AutoSize = true;
            lblServerCpuCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblServerCpuCount.Location = new Point(110, 100);
            lblServerCpuCount.Name = "lblServerCpuCount";
            lblServerCpuCount.Size = new Size(29, 15);
            lblServerCpuCount.TabIndex = 2;
            lblServerCpuCount.Text = "N/A";
            // 
            // labelServerCpu
            // 
            labelServerCpu.AutoSize = true;
            labelServerCpu.Location = new Point(10, 100);
            labelServerCpu.Name = "labelServerCpu";
            labelServerCpu.Size = new Size(70, 15);
            labelServerCpu.TabIndex = 3;
            labelServerCpu.Text = "Server CPU:";
            // 
            // lblStellarBuild
            // 
            lblStellarBuild.AutoSize = true;
            lblStellarBuild.Location = new Point(110, 50);
            lblStellarBuild.Name = "lblStellarBuild";
            lblStellarBuild.Size = new Size(16, 15);
            lblStellarBuild.TabIndex = 4;
            lblStellarBuild.Text = "...";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(10, 50);
            label7.Name = "label7";
            label7.Size = new Size(66, 15);
            label7.TabIndex = 5;
            label7.Text = "Core Build:";
            // 
            // lblProtocolVersion
            // 
            lblProtocolVersion.AutoSize = true;
            lblProtocolVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProtocolVersion.Location = new Point(110, 25);
            lblProtocolVersion.Name = "lblProtocolVersion";
            lblProtocolVersion.Size = new Size(16, 15);
            lblProtocolVersion.TabIndex = 6;
            lblProtocolVersion.Text = "...";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 25);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 7;
            label1.Text = "Protocol Ver:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblLedgerAge);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(lblState);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(lblLatestBlock);
            groupBox2.Controls.Add(label2);
            groupBox2.ForeColor = Color.White;
            groupBox2.Location = new Point(10, 0);
            groupBox2.Margin = new Padding(10, 0, 0, 10);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(230, 100);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Consensus";
            // 
            // lblLedgerAge
            // 
            lblLedgerAge.AutoSize = true;
            lblLedgerAge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLedgerAge.Location = new Point(110, 75);
            lblLedgerAge.Name = "lblLedgerAge";
            lblLedgerAge.Size = new Size(16, 15);
            lblLedgerAge.TabIndex = 0;
            lblLedgerAge.Text = "...";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(10, 75);
            label9.Name = "label9";
            label9.Size = new Size(31, 15);
            label9.TabIndex = 1;
            label9.Text = "Age:";
            // 
            // lblState
            // 
            lblState.AutoSize = true;
            lblState.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblState.Location = new Point(110, 25);
            lblState.Name = "lblState";
            lblState.Size = new Size(16, 15);
            lblState.TabIndex = 2;
            lblState.Text = "...";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 25);
            label3.Name = "label3";
            label3.Size = new Size(37, 15);
            label3.TabIndex = 3;
            label3.Text = "State:";
            // 
            // lblLatestBlock
            // 
            lblLatestBlock.AutoSize = true;
            lblLatestBlock.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLatestBlock.Location = new Point(110, 50);
            lblLatestBlock.Name = "lblLatestBlock";
            lblLatestBlock.Size = new Size(16, 15);
            lblLatestBlock.TabIndex = 4;
            lblLatestBlock.Text = "...";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 50);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 5;
            label2.Text = "Block:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lblSupporting);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(lblOutgoing);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(lblIncoming);
            groupBox3.Controls.Add(label4);
            groupBox3.ForeColor = Color.White;
            groupBox3.Location = new Point(0, 294);
            groupBox3.Margin = new Padding(0, 0, 0, 10);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(230, 120);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Network Stats";
            // 
            // lblSupporting
            // 
            lblSupporting.AutoSize = true;
            lblSupporting.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSupporting.Location = new Point(100, 80);
            lblSupporting.Name = "lblSupporting";
            lblSupporting.Size = new Size(23, 15);
            lblSupporting.TabIndex = 0;
            lblSupporting.Text = "No";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(10, 80);
            label6.Name = "label6";
            label6.Size = new Size(70, 15);
            label6.TabIndex = 1;
            label6.Text = "Supporting:";
            // 
            // lblOutgoing
            // 
            lblOutgoing.AutoSize = true;
            lblOutgoing.Location = new Point(100, 55);
            lblOutgoing.Name = "lblOutgoing";
            lblOutgoing.Size = new Size(14, 15);
            lblOutgoing.TabIndex = 2;
            lblOutgoing.Text = "0";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 55);
            label5.Name = "label5";
            label5.Size = new Size(61, 15);
            label5.TabIndex = 3;
            label5.Text = "Outgoing:";
            // 
            // lblIncoming
            // 
            lblIncoming.AutoSize = true;
            lblIncoming.Location = new Point(100, 30);
            lblIncoming.Name = "lblIncoming";
            lblIncoming.Size = new Size(14, 15);
            lblIncoming.TabIndex = 4;
            lblIncoming.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 30);
            label4.Name = "label4";
            label4.Size = new Size(61, 15);
            label4.TabIndex = 5;
            label4.Text = "Incoming:";
            // 
            // lblLastUpdated
            // 
            lblLastUpdated.ForeColor = Color.Silver;
            lblLastUpdated.Location = new Point(13, 584);
            lblLastUpdated.Name = "lblLastUpdated";
            lblLastUpdated.Size = new Size(100, 23);
            lblLastUpdated.TabIndex = 3;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(lblRAM);
            groupBox4.Controls.Add(label10);
            groupBox4.Controls.Add(lblCPU);
            groupBox4.Controls.Add(label8);
            groupBox4.Controls.Add(lblContainerStatus);
            groupBox4.Controls.Add(label11);
            groupBox4.ForeColor = Color.White;
            groupBox4.Location = new Point(10, 110);
            groupBox4.Margin = new Padding(10, 0, 0, 10);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(230, 120);
            groupBox4.TabIndex = 1;
            groupBox4.TabStop = false;
            groupBox4.Text = "Container Resources";
            // 
            // lblRAM
            // 
            lblRAM.AutoSize = true;
            lblRAM.Location = new Point(70, 75);
            lblRAM.Name = "lblRAM";
            lblRAM.Size = new Size(16, 15);
            lblRAM.TabIndex = 0;
            lblRAM.Text = "...";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(10, 75);
            label10.Name = "label10";
            label10.Size = new Size(36, 15);
            label10.TabIndex = 1;
            label10.Text = "RAM:";
            // 
            // lblCPU
            // 
            lblCPU.AutoSize = true;
            lblCPU.Location = new Point(70, 50);
            lblCPU.Name = "lblCPU";
            lblCPU.Size = new Size(16, 15);
            lblCPU.TabIndex = 2;
            lblCPU.Text = "...";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(10, 50);
            label8.Name = "label8";
            label8.Size = new Size(33, 15);
            label8.TabIndex = 3;
            label8.Text = "CPU:";
            // 
            // lblContainerStatus
            // 
            lblContainerStatus.AutoSize = true;
            lblContainerStatus.Location = new Point(70, 25);
            lblContainerStatus.Name = "lblContainerStatus";
            lblContainerStatus.Size = new Size(66, 15);
            lblContainerStatus.TabIndex = 4;
            lblContainerStatus.Text = "Checking...";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(10, 25);
            label11.Name = "label11";
            label11.Size = new Size(42, 15);
            label11.TabIndex = 5;
            label11.Text = "Name:";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(lblPort03);
            groupBox5.Controls.Add(label14);
            groupBox5.Controls.Add(lblPort02);
            groupBox5.Controls.Add(label13);
            groupBox5.Controls.Add(lblPort01);
            groupBox5.Controls.Add(label12);
            groupBox5.ForeColor = Color.White;
            groupBox5.Location = new Point(10, 434);
            groupBox5.Margin = new Padding(0, 0, 0, 10);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(470, 60);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            groupBox5.Text = "Local Port Check (Listening)";
            // 
            // lblPort03
            // 
            lblPort03.AutoSize = true;
            lblPort03.Location = new Point(380, 25);
            lblPort03.Name = "lblPort03";
            lblPort03.Size = new Size(57, 15);
            lblPort03.TabIndex = 0;
            lblPort03.Text = "Checking";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(320, 25);
            label14.Name = "label14";
            label14.Size = new Size(45, 15);
            label14.TabIndex = 1;
            label14.Text = "31403:";
            // 
            // lblPort02
            // 
            lblPort02.AutoSize = true;
            lblPort02.Location = new Point(230, 25);
            lblPort02.Name = "lblPort02";
            lblPort02.Size = new Size(57, 15);
            lblPort02.TabIndex = 2;
            lblPort02.Text = "Checking";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(170, 25);
            label13.Name = "label13";
            label13.Size = new Size(45, 15);
            label13.TabIndex = 3;
            label13.Text = "31402:";
            // 
            // lblPort01
            // 
            lblPort01.AutoSize = true;
            lblPort01.Location = new Point(80, 25);
            lblPort01.Name = "lblPort01";
            lblPort01.Size = new Size(57, 15);
            lblPort01.TabIndex = 4;
            lblPort01.Text = "Checking";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(20, 25);
            label12.Name = "label12";
            label12.Size = new Size(45, 15);
            label12.TabIndex = 5;
            label12.Text = "31401:";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(btnHelp);
            groupBox6.Controls.Add(btnShowHistory);
            groupBox6.Controls.Add(lblBonus);
            groupBox6.Controls.Add(lblAvailability);
            groupBox6.Controls.Add(lblUptime);
            groupBox6.ForeColor = Color.White;
            groupBox6.Location = new Point(10, 504);
            groupBox6.Margin = new Padding(0, 0, 0, 10);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(470, 70);
            groupBox6.TabIndex = 2;
            groupBox6.TabStop = false;
            groupBox6.Text = "Session Statistics";
            // 
            // btnHelp
            // 
            btnHelp.BackColor = Color.FromArgb(255, 192, 128);
            btnHelp.ForeColor = Color.Black;
            btnHelp.Location = new Point(340, 20);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(52, 26);
            btnHelp.TabIndex = 0;
            btnHelp.Text = "Help";
            btnHelp.UseVisualStyleBackColor = false;
            btnHelp.Click += btnHelp_Click;
            // 
            // btnShowHistory
            // 
            btnShowHistory.BackColor = Color.FromArgb(255, 192, 128);
            btnShowHistory.ForeColor = Color.Black;
            btnShowHistory.Location = new Point(395, 20);
            btnShowHistory.Name = "btnShowHistory";
            btnShowHistory.Size = new Size(58, 26);
            btnShowHistory.TabIndex = 1;
            btnShowHistory.Text = "History";
            btnShowHistory.UseVisualStyleBackColor = false;
            btnShowHistory.Click += btnShowHistory_Click;
            // 
            // lblBonus
            // 
            lblBonus.AutoSize = true;
            lblBonus.BackColor = Color.FromArgb(41, 41, 41);
            lblBonus.BorderStyle = BorderStyle.FixedSingle;
            lblBonus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBonus.ForeColor = Color.Peru;
            lblBonus.Location = new Point(240, 25);
            lblBonus.Name = "lblBonus";
            lblBonus.Size = new Size(87, 17);
            lblBonus.TabIndex = 2;
            lblBonus.Text = "Bonus: 0.0000";
            // 
            // lblAvailability
            // 
            lblAvailability.AutoSize = true;
            lblAvailability.Location = new Point(125, 25);
            lblAvailability.Name = "lblAvailability";
            lblAvailability.Size = new Size(106, 15);
            lblAvailability.TabIndex = 3;
            lblAvailability.Text = "Availability: 0.00%";
            // 
            // lblUptime
            // 
            lblUptime.AutoSize = true;
            lblUptime.Location = new Point(15, 25);
            lblUptime.Name = "lblUptime";
            lblUptime.Size = new Size(101, 15);
            lblUptime.TabIndex = 4;
            lblUptime.Text = "Uptime: 00:00:00";
            // 
            // groupBoxControl
            // 
            groupBoxControl.Controls.Add(lblRemoteBlockNum);
            groupBoxControl.Controls.Add(lblRemoteBlockLabel);
            groupBoxControl.Controls.Add(lblLocalBlockNum);
            groupBoxControl.Controls.Add(lblLocalBlockLabel);
            groupBoxControl.Controls.Add(lblMainStatus);
            groupBoxControl.Controls.Add(btnToggleNode);
            groupBoxControl.ForeColor = Color.White;
            groupBoxControl.Location = new Point(0, 0);
            groupBoxControl.Margin = new Padding(0, 0, 0, 10);
            groupBoxControl.Name = "groupBoxControl";
            groupBoxControl.Size = new Size(230, 140);
            groupBoxControl.TabIndex = 0;
            groupBoxControl.TabStop = false;
            groupBoxControl.Text = "Node Control";
            // 
            // lblRemoteBlockNum
            // 
            lblRemoteBlockNum.AutoSize = true;
            lblRemoteBlockNum.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRemoteBlockNum.Location = new Point(140, 70);
            lblRemoteBlockNum.Name = "lblRemoteBlockNum";
            lblRemoteBlockNum.Size = new Size(14, 15);
            lblRemoteBlockNum.TabIndex = 0;
            lblRemoteBlockNum.Text = "0";
            // 
            // lblRemoteBlockLabel
            // 
            lblRemoteBlockLabel.AutoSize = true;
            lblRemoteBlockLabel.Location = new Point(10, 70);
            lblRemoteBlockLabel.Name = "lblRemoteBlockLabel";
            lblRemoteBlockLabel.Size = new Size(120, 15);
            lblRemoteBlockLabel.TabIndex = 1;
            lblRemoteBlockLabel.Text = "Latest block number:";
            // 
            // lblLocalBlockNum
            // 
            lblLocalBlockNum.AutoSize = true;
            lblLocalBlockNum.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLocalBlockNum.Location = new Point(140, 50);
            lblLocalBlockNum.Name = "lblLocalBlockNum";
            lblLocalBlockNum.Size = new Size(14, 15);
            lblLocalBlockNum.TabIndex = 2;
            lblLocalBlockNum.Text = "0";
            // 
            // lblLocalBlockLabel
            // 
            lblLocalBlockLabel.AutoSize = true;
            lblLocalBlockLabel.Location = new Point(10, 50);
            lblLocalBlockLabel.Name = "lblLocalBlockLabel";
            lblLocalBlockLabel.Size = new Size(117, 15);
            lblLocalBlockLabel.TabIndex = 3;
            lblLocalBlockLabel.Text = "Local block number:";
            // 
            // lblMainStatus
            // 
            lblMainStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMainStatus.ForeColor = Color.LightGreen;
            lblMainStatus.Location = new Point(10, 20);
            lblMainStatus.Name = "lblMainStatus";
            lblMainStatus.Size = new Size(200, 30);
            lblMainStatus.TabIndex = 4;
            lblMainStatus.Text = "Checking status...";
            lblMainStatus.TextAlign = ContentAlignment.TopCenter;
            // 
            // btnToggleNode
            // 
            btnToggleNode.BackColor = Color.Snow;
            btnToggleNode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnToggleNode.ForeColor = Color.Salmon;
            btnToggleNode.Location = new Point(10, 95);
            btnToggleNode.Name = "btnToggleNode";
            btnToggleNode.Size = new Size(200, 35);
            btnToggleNode.TabIndex = 5;
            btnToggleNode.Text = "Checking...";
            btnToggleNode.UseVisualStyleBackColor = false;
            btnToggleNode.Click += btnToggleNode_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 698);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(510, 22);
            statusStrip1.TabIndex = 1;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(23, 23);
            // 
            // mainFlow
            // 
            mainFlow.AutoScroll = true;
            mainFlow.BackColor = Color.FromArgb(41, 41, 41);
            mainFlow.Controls.Add(topRowFlow);
            mainFlow.Controls.Add(groupBox5);
            mainFlow.Controls.Add(groupBox6);
            mainFlow.Controls.Add(lblLastUpdated);
            mainFlow.Dock = DockStyle.Fill;
            mainFlow.FlowDirection = FlowDirection.TopDown;
            mainFlow.Location = new Point(0, 0);
            mainFlow.Name = "mainFlow";
            mainFlow.Padding = new Padding(10);
            mainFlow.Size = new Size(510, 698);
            mainFlow.TabIndex = 0;
            mainFlow.WrapContents = false;
            // 
            // topRowFlow
            // 
            topRowFlow.AutoSize = true;
            topRowFlow.Controls.Add(leftFlow);
            topRowFlow.Controls.Add(rightFlow);
            topRowFlow.Location = new Point(10, 10);
            topRowFlow.Margin = new Padding(0);
            topRowFlow.Name = "topRowFlow";
            topRowFlow.Size = new Size(470, 424);
            topRowFlow.TabIndex = 0;
            topRowFlow.WrapContents = false;
            // 
            // leftFlow
            // 
            leftFlow.AutoSize = true;
            leftFlow.Controls.Add(groupBoxControl);
            leftFlow.Controls.Add(groupBox1);
            leftFlow.Controls.Add(groupBox3);
            leftFlow.FlowDirection = FlowDirection.TopDown;
            leftFlow.Location = new Point(0, 0);
            leftFlow.Margin = new Padding(0);
            leftFlow.Name = "leftFlow";
            leftFlow.Size = new Size(230, 424);
            leftFlow.TabIndex = 0;
            // 
            // rightFlow
            // 
            rightFlow.AutoSize = true;
            rightFlow.Controls.Add(groupBox2);
            rightFlow.Controls.Add(groupBox4);
            rightFlow.FlowDirection = FlowDirection.TopDown;
            rightFlow.Location = new Point(230, 0);
            rightFlow.Margin = new Padding(0);
            rightFlow.Name = "rightFlow";
            rightFlow.Size = new Size(240, 240);
            rightFlow.TabIndex = 1;
            // 
            // Form1
            // 
            ClientSize = new Size(510, 720);
            Controls.Add(mainFlow);
            Controls.Add(statusStrip1);
            Name = "Form1";
            Text = "Pi Node Monitor Pro";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBoxControl.ResumeLayout(false);
            groupBoxControl.PerformLayout();
            mainFlow.ResumeLayout(false);
            mainFlow.PerformLayout();
            topRowFlow.ResumeLayout(false);
            topRowFlow.PerformLayout();
            leftFlow.ResumeLayout(false);
            leftFlow.PerformLayout();
            rightFlow.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblProtocolVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblLatestBlock;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblSupporting;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblOutgoing;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblIncoming;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblLastUpdated;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblStellarBuild;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblLedgerAge;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblContainerStatus;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblPort03;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblPort02;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblPort01;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblRAM;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblCPU;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label lblAvailability;
        private System.Windows.Forms.Label lblUptime;
        private System.Windows.Forms.Label lblBonus;
        private System.Windows.Forms.Label lblServerCpuCount;
        private System.Windows.Forms.Label labelServerCpu;
        private System.Windows.Forms.Label lblLocalCpuCount;
        private System.Windows.Forms.Label labelLocalCpu;
        private System.Windows.Forms.Button btnShowHistory;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.Button btnToggleNode;
        private System.Windows.Forms.Label lblMainStatus;
        private System.Windows.Forms.Label lblLocalBlockLabel;
        private System.Windows.Forms.Label lblLocalBlockNum;
        private System.Windows.Forms.Label lblRemoteBlockLabel;
        private System.Windows.Forms.Label lblRemoteBlockNum;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.FlowLayoutPanel mainFlow;
        private System.Windows.Forms.FlowLayoutPanel topRowFlow;
        private System.Windows.Forms.FlowLayoutPanel leftFlow;
        private System.Windows.Forms.FlowLayoutPanel rightFlow;
    }
}
