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

            // --- Missing Properties Initialization ---
            // GroupBox 4 Items
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(42, 15);
            this.label11.Text = "Name:";
            
            this.lblContainerStatus.AutoSize = true;
            this.lblContainerStatus.Location = new System.Drawing.Point(70, 25);
            this.lblContainerStatus.Name = "lblContainerStatus";
            this.lblContainerStatus.Size = new System.Drawing.Size(59, 15);
            this.lblContainerStatus.Text = "Checking...";

            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(33, 15);
            this.label8.Text = "CPU:";

            this.lblCPU.AutoSize = true;
            this.lblCPU.Location = new System.Drawing.Point(70, 50);
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(16, 15);
            this.lblCPU.Text = "...";

            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 75);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(36, 15);
            this.label10.Text = "RAM:";

            this.lblRAM.AutoSize = true;
            this.lblRAM.Location = new System.Drawing.Point(70, 75);
            this.lblRAM.Name = "lblRAM";
            this.lblRAM.Size = new System.Drawing.Size(16, 15);
            this.lblRAM.Text = "...";

            // GroupBox 5 Items
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(20, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 15);
            this.label12.Text = "31401:";

            this.lblPort01.AutoSize = true;
            this.lblPort01.Location = new System.Drawing.Point(80, 25);
            this.lblPort01.Name = "lblPort01";
            this.lblPort01.Size = new System.Drawing.Size(54, 15);
            this.lblPort01.Text = "Checking";

            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(170, 25);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 15);
            this.label13.Text = "31402:";

            this.lblPort02.AutoSize = true;
            this.lblPort02.Location = new System.Drawing.Point(230, 25);
            this.lblPort02.Name = "lblPort02";
            this.lblPort02.Size = new System.Drawing.Size(54, 15);
            this.lblPort02.Text = "Checking";

            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(320, 25);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(41, 15);
            this.label14.Text = "31403:";

            this.lblPort03.AutoSize = true;
            this.lblPort03.Location = new System.Drawing.Point(380, 25);
            this.lblPort03.Name = "lblPort03";
            this.lblPort03.Size = new System.Drawing.Size(54, 15);
            this.lblPort03.Text = "Checking";

            // GroupBox 6 Items
            this.lblUptime.AutoSize = true;
            this.lblUptime.Location = new System.Drawing.Point(20, 25);
            this.lblUptime.Name = "lblUptime";
            this.lblUptime.Size = new System.Drawing.Size(117, 15);
            this.lblUptime.Text = "Uptime: 00:00:00";

            this.lblAvailability.AutoSize = true;
            this.lblAvailability.Location = new System.Drawing.Point(240, 25);
            this.lblAvailability.Name = "lblAvailability";
            this.lblAvailability.Size = new System.Drawing.Size(107, 15);
            this.lblAvailability.Text = "Availability: 0.0%";
            
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // groupBoxControl (Node Control & Status)
            // 
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockNum);
            this.groupBoxControl.Controls.Add(this.lblRemoteBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockNum);
            this.groupBoxControl.Controls.Add(this.lblLocalBlockLabel);
            this.groupBoxControl.Controls.Add(this.lblMainStatus);
            this.groupBoxControl.Controls.Add(this.btnToggleNode);
            this.groupBoxControl.Location = new System.Drawing.Point(12, 12);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Size = new System.Drawing.Size(220, 140);
            this.groupBoxControl.TabIndex = 6;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "Node Control";
            // 
            // lblMainStatus
            // 
            this.lblMainStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMainStatus.ForeColor = System.Drawing.Color.Green;
            this.lblMainStatus.Location = new System.Drawing.Point(10, 20);
            this.lblMainStatus.Name = "lblMainStatus";
            this.lblMainStatus.Size = new System.Drawing.Size(200, 30);
            this.lblMainStatus.TabIndex = 1;
            this.lblMainStatus.Text = "Checking status...";
            this.lblMainStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblLocalBlockLabel
            // 
            this.lblLocalBlockLabel.AutoSize = true;
            this.lblLocalBlockLabel.Location = new System.Drawing.Point(10, 50);
            this.lblLocalBlockLabel.Name = "lblLocalBlockLabel";
            this.lblLocalBlockLabel.Size = new System.Drawing.Size(120, 15);
            this.lblLocalBlockLabel.TabIndex = 2;
            this.lblLocalBlockLabel.Text = "Local block number:";
            // 
            // lblLocalBlockNum
            // 
            this.lblLocalBlockNum.AutoSize = true;
            this.lblLocalBlockNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLocalBlockNum.Location = new System.Drawing.Point(140, 50);
            this.lblLocalBlockNum.Name = "lblLocalBlockNum";
            this.lblLocalBlockNum.Size = new System.Drawing.Size(14, 15);
            this.lblLocalBlockNum.TabIndex = 3;
            this.lblLocalBlockNum.Text = "0";
            // 
            // lblRemoteBlockLabel
            // 
            this.lblRemoteBlockLabel.AutoSize = true;
            this.lblRemoteBlockLabel.Location = new System.Drawing.Point(10, 70);
            this.lblRemoteBlockLabel.Name = "lblRemoteBlockLabel";
            this.lblRemoteBlockLabel.Size = new System.Drawing.Size(124, 15);
            this.lblRemoteBlockLabel.TabIndex = 4;
            this.lblRemoteBlockLabel.Text = "Latest block number:";
            // 
            // lblRemoteBlockNum
            // 
            this.lblRemoteBlockNum.AutoSize = true;
            this.lblRemoteBlockNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblRemoteBlockNum.Location = new System.Drawing.Point(140, 70);
            this.lblRemoteBlockNum.Name = "lblRemoteBlockNum";
            this.lblRemoteBlockNum.Size = new System.Drawing.Size(14, 15);
            this.lblRemoteBlockNum.TabIndex = 5;
            this.lblRemoteBlockNum.Text = "0";
            // 
            // btnToggleNode
            // 
            this.btnToggleNode.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnToggleNode.Location = new System.Drawing.Point(10, 95);
            this.btnToggleNode.Name = "btnToggleNode";
            this.btnToggleNode.Size = new System.Drawing.Size(200, 35);
            this.btnToggleNode.TabIndex = 0;
            this.btnToggleNode.Text = "Checking...";
            this.btnToggleNode.UseVisualStyleBackColor = true;
            this.btnToggleNode.Click += new System.EventHandler(this.btnToggleNode_Click);

            // 
            // groupBox1 (General Info)
            // 
            this.groupBox1.Controls.Add(this.lblStellarBuild);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lblProtocolVersion);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 160);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General Info";
            // 
            // lblStellarBuild
            // 
            this.lblStellarBuild.AutoSize = true;
            this.lblStellarBuild.Location = new System.Drawing.Point(100, 50);
            this.lblStellarBuild.Name = "lblStellarBuild";
            this.lblStellarBuild.Size = new System.Drawing.Size(16, 15);
            this.lblStellarBuild.TabIndex = 3;
            this.lblStellarBuild.Text = "...";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 15);
            this.label7.TabIndex = 2;
            this.label7.Text = "Core Build:";
            // 
            // lblProtocolVersion
            // 
            this.lblProtocolVersion.AutoSize = true;
            this.lblProtocolVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblProtocolVersion.Location = new System.Drawing.Point(100, 25);
            this.lblProtocolVersion.Name = "lblProtocolVersion";
            this.lblProtocolVersion.Size = new System.Drawing.Size(16, 15);
            this.lblProtocolVersion.TabIndex = 1;
            this.lblProtocolVersion.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Protocol Ver:";
            // 
            // groupBox2 (Consensus)
            // 
            this.groupBox2.Controls.Add(this.lblLedgerAge);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.lblState);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lblLatestBlock);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(240, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(232, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Consensus";
            // 
            // lblLedgerAge
            // 
            this.lblLedgerAge.AutoSize = true;
            this.lblLedgerAge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLedgerAge.Location = new System.Drawing.Point(90, 75);
            this.lblLedgerAge.Name = "lblLedgerAge";
            this.lblLedgerAge.Size = new System.Drawing.Size(16, 15);
            this.lblLedgerAge.TabIndex = 5;
            this.lblLedgerAge.Text = "...";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(10, 75);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 15);
            this.label9.TabIndex = 4;
            this.label9.Text = "Ledger Age:";
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblState.Location = new System.Drawing.Point(90, 25);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(16, 15);
            this.lblState.TabIndex = 1;
            this.lblState.Text = "...";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "State:";
            // 
            // lblLatestBlock
            // 
            this.lblLatestBlock.AutoSize = true;
            this.lblLatestBlock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLatestBlock.Location = new System.Drawing.Point(90, 50);
            this.lblLatestBlock.Name = "lblLatestBlock";
            this.lblLatestBlock.Size = new System.Drawing.Size(16, 15);
            this.lblLatestBlock.TabIndex = 3;
            this.lblLatestBlock.Text = "...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Latest Block:";
            // 
            // groupBox3 (Network)
            // 
            this.groupBox3.Controls.Add(this.lblSupporting);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.lblOutgoing);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.lblIncoming);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 250);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(220, 120);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Network Stats";
            // 
            // lblSupporting
            // 
            this.lblSupporting.AutoSize = true;
            this.lblSupporting.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSupporting.Location = new System.Drawing.Point(100, 80);
            this.lblSupporting.Name = "lblSupporting";
            this.lblSupporting.Size = new System.Drawing.Size(23, 15);
            this.lblSupporting.TabIndex = 7;
            this.lblSupporting.Text = "No";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 15);
            this.label6.TabIndex = 6;
            this.label6.Text = "Supporting:";
            // 
            // lblOutgoing
            // 
            this.lblOutgoing.AutoSize = true;
            this.lblOutgoing.Location = new System.Drawing.Point(100, 55);
            this.lblOutgoing.Name = "lblOutgoing";
            this.lblOutgoing.Size = new System.Drawing.Size(13, 15);
            this.lblOutgoing.TabIndex = 5;
            this.lblOutgoing.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "Outgoing:";
            // 
            // lblIncoming
            // 
            this.lblIncoming.AutoSize = true;
            this.lblIncoming.Location = new System.Drawing.Point(100, 30);
            this.lblIncoming.Name = "lblIncoming";
            this.lblIncoming.Size = new System.Drawing.Size(13, 15);
            this.lblIncoming.TabIndex = 3;
            this.lblIncoming.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Incoming:";
            // 
            // lblLastUpdated
            // 
            this.lblLastUpdated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLastUpdated.AutoSize = true;
            this.lblLastUpdated.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblLastUpdated.Location = new System.Drawing.Point(340, 520);
            this.lblLastUpdated.Name = "lblLastUpdated";
            this.lblLastUpdated.Size = new System.Drawing.Size(120, 15);
            this.lblLastUpdated.TabIndex = 99;
            this.lblLastUpdated.Text = "Last Updated: Never";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox4 (Container Resources)
            // 
            this.groupBox4.Controls.Add(this.lblRAM);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.lblCPU);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.lblContainerStatus);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Location = new System.Drawing.Point(240, 120);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(232, 120);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Container Resources";
            // ... (Resources content unchanged) ...

            // 
            // groupBox5 (Local Ports)
            // 
            this.groupBox5.Controls.Add(this.lblPort03);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.lblPort02);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.lblPort01);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Location = new System.Drawing.Point(12, 380);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(460, 60);
            this.groupBox5.TabIndex = 4;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Local Port Check (Listening)";
            
            // ... (Ports content unchanged) ...

            // 
            // groupBox6 (Statistics)
            // 
            this.groupBox6.Controls.Add(this.lblAvailability);
            this.groupBox6.Controls.Add(this.lblUptime);
            this.groupBox6.Location = new System.Drawing.Point(12, 450);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(460, 60);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Session Statistics";

            // ... (Stats content unchanged) ...

            // 
            // notifyIcon1
            // 
             this.notifyIcon1.Text = "Pi Node Monitor";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipTitle = "Pi Node Alert";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.statusLabel });
            this.statusStrip1.Location = new System.Drawing.Point(0, 528);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(484, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(100, 17);
            this.statusLabel.Text = "Ready";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 572);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBoxControl);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lblLastUpdated);
            this.Controls.Add(this.groupBox1);
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
            this.groupBoxControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
