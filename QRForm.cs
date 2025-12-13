using System;
using System.Drawing;
using System.Windows.Forms;
using QRCoder;

namespace PiNodeMonitorWinForm
{
    public class QRForm : Form
    {
        private PictureBox pbQR;
        private Label lblInfo;
        private Label lblPin; // New PIN Label
        private Label lblUrl;

        public QRForm(string url)
        {
            this.Text = "Mobile Connect";
            this.Size = new Size(350, 480); // Increased Height
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblInfo = new Label();
            lblInfo.Text = "Scan with your Phone";
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 40;
            lblInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            pbQR = new PictureBox();
            pbQR.Size = new Size(250, 250);
            pbQR.SizeMode = PictureBoxSizeMode.Zoom;
            pbQR.Location = new Point((this.ClientSize.Width - 250) / 2, 50);

            // [NEW] PIN Display
            lblPin = new Label();
            lblPin.Text = $"PIN: {MobileServer.CurrentPin}";
            lblPin.TextAlign = ContentAlignment.MiddleCenter;
            lblPin.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblPin.ForeColor = Color.DarkRed;
            lblPin.Location = new Point(0, 310);
            lblPin.Size = new Size(350, 50);

            lblUrl = new Label();
            lblUrl.Text = url;
            lblUrl.TextAlign = ContentAlignment.MiddleCenter;
            lblUrl.Dock = DockStyle.Bottom;
            lblUrl.Height = 50;
            lblUrl.ForeColor = Color.Blue;
            lblUrl.Font = new Font("Consolas", 10);
            lblUrl.Cursor = Cursors.Hand;
            
            // ... Click event same ...
            lblUrl.Click += (s, e) => {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
            };

            GenerateQR(url);

            this.Controls.Add(pbQR);
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblPin); // Add PIN Label
            this.Controls.Add(lblUrl);
        }

        private void GenerateQR(string url)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                pbQR.Image = qrCodeImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("QR Generation Failed: " + ex.Message);
            }
        }
    }
}
