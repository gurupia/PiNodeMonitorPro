using System;
using System.Drawing;
using System.Windows.Forms;
using QRCoder;

namespace PiNodeMonitorWinForm
{
    public class QRForm : Form
    {
        private ComboBox cboIps;
        private CheckBox chkRemote;
        private PictureBox pbQR;
        private Label lblPin; 
        private Label lblUrl;

        public QRForm(string _) 
        {
            this.Text = "Mobile Connect";
            this.Size = new Size(360, 500); 
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 1. Initialize all UI controls first to avoid NullReferenceException in event handlers
            cboIps = new ComboBox { 
                Location = new Point(35, 5), 
                Size = new Size(165, 25), 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            chkRemote = new CheckBox { Text = "Use Public IP", Location = new Point(210, 5), AutoSize = true };
            pbQR = new PictureBox { 
                Size = new Size(250, 250), 
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((360 - 250) / 2, 90)
            };
            lblPin = new Label { 
                Text = $"PIN: {MobileServer.CurrentPin}",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(0, 350),
                Size = new Size(360, 50)
            };
            lblUrl = new Label { 
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 40,
                ForeColor = Color.Blue,
                Font = new Font("Consolas", 9),
                Cursor = Cursors.Hand
            };

            // 2. Set up layout
            Label lblTitle = new Label {
                Text = "Scan to Connect",
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);

            Panel pnlCtrl = new Panel {  Location = new Point(10, 40), Size = new Size(340, 40) };
            Label lblIp = new Label { Text = "IP:", Location = new Point(5, 8), AutoSize = true };
            pnlCtrl.Controls.Add(lblIp);

            // Populate IPs
            string tunnelUrl = MobileServer.TunnelUrl;
            if (!string.IsNullOrEmpty(tunnelUrl)) cboIps.Items.Add($"[보안터널] {tunnelUrl}");

            string publicIp = MobileServer.PublicIpAddress;
            if (!string.IsNullOrEmpty(publicIp) && publicIp != "Unknown") cboIps.Items.Add($"[공인] {publicIp}");
            
            var localIps = MobileServer.GetAllLocalIpAddresses();
            foreach(var ip in localIps) if (ip != "127.0.0.1") cboIps.Items.Add(ip);
            
            cboIps.SelectedIndexChanged += (s, e) => UpdateQR();
            chkRemote.CheckedChanged += (s, e) => { cboIps.Enabled = !chkRemote.Checked; UpdateQR(); };
            lblUrl.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = lblUrl.Text, UseShellExecute = true }); } catch {} };

            pnlCtrl.Controls.Add(cboIps);
            pnlCtrl.Controls.Add(chkRemote);
            this.Controls.Add(pnlCtrl);
            this.Controls.Add(pbQR);
            this.Controls.Add(lblPin);
            this.Controls.Add(lblUrl);

            // 3. Selection and Initialization
            if (cboIps.Items.Count > 0) cboIps.SelectedIndex = 0;

            // 4. Subscribe to Events
            MobileServer.PublicIpDetected += OnPublicIpDetected;
            MobileServer.TunnelUrlGenerated += OnTunnelUrlGenerated;
            
            this.FormClosed += (s, e) => {
                MobileServer.PublicIpDetected -= OnPublicIpDetected;
                MobileServer.TunnelUrlGenerated -= OnTunnelUrlGenerated;
            };

            UpdateQR();
        }

        private void OnTunnelUrlGenerated(string url)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnTunnelUrlGenerated(url)));
                return;
            }

            string item = $"[보안터널] {url}";
            if (!cboIps.Items.Contains(item))
            {
                cboIps.Items.Insert(0, item);
                cboIps.SelectedIndex = 0;
            }
        }

        private void OnPublicIpDetected(string publicIp)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnPublicIpDetected(publicIp)));
                return;
            }

            // Check if public IP is already in the list
            string publicItem = $"[공인] {publicIp}";
            if (!cboIps.Items.Contains(publicItem) && publicIp != "Failed")
            {
                cboIps.Items.Insert(0, publicItem);
                cboIps.SelectedIndex = 0;
            }
        }

        private void UpdateQR()
        {
            try
            {
                string selectedItem = cboIps.SelectedItem?.ToString() ?? "";
                string url = "";
                
                if (selectedItem.StartsWith("[보안터널]"))
                {
                    string tunnelBase = selectedItem.Replace("[보안터널] ", "").Trim();
                    url = $"{tunnelBase}/?pin={MobileServer.CurrentPin}";
                }
                else
                {
                    string ip;
                    if (chkRemote.Checked)
                    {
                        ip = MobileServer.PublicIpAddress;
                    }
                    else if (selectedItem.StartsWith("[공인]"))
                    {
                        ip = selectedItem.Replace("[공인] ", "");
                    }
                    else
                    {
                        ip = selectedItem;
                    }
                    
                    if (string.IsNullOrEmpty(ip) || ip == "Unknown") ip = "127.0.0.1";
                    url = $"http://{ip}:{MobileServer.Port}/?pin={MobileServer.CurrentPin}";
                }
                
                lblUrl.Text = url;
                
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                pbQR.Image = qrCode.GetGraphic(20);
            }
            catch (Exception ex)
            {
                // Silent catch for UI stability
            }
        }
    }
}
