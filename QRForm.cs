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

            Label lblTitle = new Label();
            lblTitle.Text = "Scan to Connect";
            lblTitle.TextAlign = ContentAlignment.TopCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 30;
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // Controls Panel
            Panel pnlCtrl = new Panel {  Location = new Point(10, 40), Size = new Size(340, 40) };
            
            Label lblIp = new Label { Text = "IP:", Location = new Point(5, 8), AutoSize = true };
            pnlCtrl.Controls.Add(lblIp);

            // Populate IPs: Tunnel URL first, then Public IP, then local IPs
            string tunnelUrl = MobileServer.TunnelUrl;
            if (!string.IsNullOrEmpty(tunnelUrl))
            {
                cboIps.Items.Add($"[보안터널] {tunnelUrl}");
            }

            string publicIp = MobileServer.PublicIpAddress;
            if (!string.IsNullOrEmpty(publicIp) && publicIp != "Unknown")
            {
                cboIps.Items.Add($"[공인] {publicIp}");
            }
            
            var localIps = MobileServer.GetAllLocalIpAddresses();
            foreach(var ip in localIps) 
            {
                if (ip != "127.0.0.1") cboIps.Items.Add(ip);
            }
            
            if (cboIps.Items.Count > 0) cboIps.SelectedIndex = 0;
            
            cboIps.SelectedIndexChanged += (s, e) => UpdateQR();
            pnlCtrl.Controls.Add(cboIps);

            chkRemote = new CheckBox { Text = "Use Public IP", Location = new Point(210, 5), AutoSize = true };
            chkRemote.CheckedChanged += (s, e) => { cboIps.Enabled = !chkRemote.Checked; UpdateQR(); };
            pnlCtrl.Controls.Add(chkRemote);

            this.Controls.Add(pnlCtrl);

            pbQR = new PictureBox();
            pbQR.Size = new Size(250, 250);
            pbQR.SizeMode = PictureBoxSizeMode.Zoom;
            pbQR.Location = new Point((this.ClientSize.Width - 250) / 2, 90);
            this.Controls.Add(pbQR);

            lblPin = new Label();
            lblPin.Text = $"PIN: {MobileServer.CurrentPin}";
            lblPin.TextAlign = ContentAlignment.MiddleCenter;
            lblPin.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblPin.ForeColor = Color.DarkRed;
            lblPin.Location = new Point(0, 350);
            lblPin.Size = new Size(360, 50);
            this.Controls.Add(lblPin);

            lblUrl = new Label();
            lblUrl.TextAlign = ContentAlignment.MiddleCenter;
            lblUrl.Dock = DockStyle.Bottom;
            lblUrl.Height = 40;
            lblUrl.ForeColor = Color.Blue;
            lblUrl.Font = new Font("Consolas", 9);
            lblUrl.Cursor = Cursors.Hand;
            lblUrl.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = lblUrl.Text, UseShellExecute = true }); } catch {} };
            this.Controls.Add(lblUrl);

            // Subscribe to Detection events
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
