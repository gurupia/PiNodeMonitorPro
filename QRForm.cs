using System;
using System.Drawing;
using System.Windows.Forms;
using QRCoder;

namespace PiNodeMonitorWinForm
{
    public class QRForm : Form
    {
        private PictureBox pbQR;
        private Label lblPin; 
        private Label lblUrl;
        private Button btnLocal;
        private Button btnRemote;

        public QRForm(string _) // url parameter ignored now
        {
            this.Text = "Mobile Connect";
            this.Size = new Size(350, 520); // Taller for buttons
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header Label
            Label lblTitle = new Label();
            lblTitle.Text = "Scan to Connect";
            lblTitle.TextAlign = ContentAlignment.TopCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 30;
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // Toggle Buttons
            btnLocal = new Button { Text = "WiFi (Local)",  Location = new Point(25, 35), Size = new Size(140, 30) };
            btnRemote = new Button { Text = "Remote (LTE)", Location = new Point(185, 35), Size = new Size(140, 30) };
            
            btnLocal.Click += (s, e) => ShowQR(false);
            btnRemote.Click += (s, e) => ShowQR(true);
            
            this.Controls.Add(btnLocal);
            this.Controls.Add(btnRemote);

            pbQR = new PictureBox();
            pbQR.Size = new Size(250, 250);
            pbQR.SizeMode = PictureBoxSizeMode.Zoom;
            pbQR.Location = new Point((this.ClientSize.Width - 250) / 2, 80);
            this.Controls.Add(pbQR);

            // PIN Display
            lblPin = new Label();
            lblPin.Text = $"PIN: {MobileServer.CurrentPin}";
            lblPin.TextAlign = ContentAlignment.MiddleCenter;
            lblPin.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblPin.ForeColor = Color.DarkRed;
            lblPin.Location = new Point(0, 340);
            lblPin.Size = new Size(350, 50);
            this.Controls.Add(lblPin);

            lblUrl = new Label();
            lblUrl.TextAlign = ContentAlignment.MiddleCenter;
            lblUrl.Dock = DockStyle.Bottom;
            lblUrl.Height = 40;
            lblUrl.ForeColor = Color.Blue;
            lblUrl.Font = new Font("Consolas", 10);
            lblUrl.Cursor = Cursors.Hand;
            this.Controls.Add(lblUrl);

            // Default
            ShowQR(false); 
        }

        private void ShowQR(bool isRemote)
        {
            try
            {
                // Highlight active button
                btnLocal.BackColor = isRemote ? SystemColors.Control : Color.LightBlue;
                btnRemote.BackColor = isRemote ? Color.LightBlue : SystemColors.Control;

                string ip = isRemote ? MobileServer.PublicIpAddress : MobileServer.CurrentIpAddress;
                string url = $"http://{ip}:{MobileServer.Port}";
                
                lblUrl.Text = url;
                lblUrl.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                pbQR.Image = qrCode.GetGraphic(20);
            }
            catch (Exception ex)
            {
                MessageBox.Show("QR Error: " + ex.Message);
            }
        }
    }
}
