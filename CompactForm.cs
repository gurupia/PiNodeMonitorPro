using System;
using System.Drawing;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Core.MVP.Models;

namespace PiNodeMonitorWinForm
{
    public class CompactForm : Form
    {
        private Label lblBlock;
        private Label lblStatus;
        private Label lblIncoming;
        private Button btnExpand;
        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        public CompactForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(300, 100);
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            
            // Drag Support
            this.MouseDown += (s, e) => { isDragging = true; dragCursorPoint = Cursor.Position; dragFormPoint = this.Location; };
            this.MouseMove += (s, e) => { if (isDragging) { Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint)); this.Location = Point.Add(dragFormPoint, new Size(dif)); } };
            this.MouseUp += (s, e) => isDragging = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            var lblTitle = new Label { Text = "Pi Node Monitor (Mini)", Location = new Point(10, 5), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };
            lblTitle.MouseDown += (s, e) => OnMouseDown(e); // Pass drag
            this.Controls.Add(lblTitle);

            lblStatus = new Label { Text = "State: ...", Location = new Point(10, 25), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.LimeGreen };
            this.Controls.Add(lblStatus);

            lblBlock = new Label { Text = "Block: ...", Location = new Point(10, 48), AutoSize = true, Font = new Font("Segoe UI", 9) };
            this.Controls.Add(lblBlock);

            lblIncoming = new Label { Text = "In: 0", Location = new Point(150, 48), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.Yellow };
            this.Controls.Add(lblIncoming);

            btnExpand = new Button { Text = "⬜", Location = new Point(270, 5), Size = new Size(25, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnExpand.FlatAppearance.BorderSize = 0;
            btnExpand.Click += (s, e) => { this.Hide(); Application.OpenForms["Form1"].Show(); Application.OpenForms["Form1"].WindowState = FormWindowState.Normal; };
            this.Controls.Add(btnExpand);
        }

        public void UpdateMetrics(NodeMetrics metrics)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => UpdateMetrics(metrics))); return; }
            
            lblStatus.Text = $"State: {metrics.ConsensusState}";
            lblStatus.ForeColor = metrics.ConsensusState == "Synced!" ? Color.LimeGreen : (metrics.ConsensusState == "Joining SCP" ? Color.Yellow : Color.Red);
            
            lblBlock.Text = $"Block: {metrics.LocalBlockNum}";
            lblIncoming.Text = $"In: {metrics.IncomingConnections}";
        }
    }
}
