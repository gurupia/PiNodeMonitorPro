using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public class RemoteViewForm : Form
    {
        private PictureBox picScreen;
        private System.Windows.Forms.Timer refreshTimer;
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _pin;
        private bool _isRequesting = false;

        public RemoteViewForm(string ipAddress, string pin, string alias)
        {
            this.Text = $"Remote View - {alias} ({ipAddress})";
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;

            _baseUrl = ipAddress.StartsWith("http") ? ipAddress : $"http://{ipAddress}";
            _pin = pin;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

            picScreen = new PictureBox();
            picScreen.Dock = DockStyle.Fill;
            picScreen.SizeMode = PictureBoxSizeMode.Zoom; // Important to fit screen
            picScreen.Cursor = Cursors.Cross; // Target cursor
            picScreen.MouseClick += PicScreen_MouseClick;
            this.Controls.Add(picScreen);

            // --- Input Panel ---
            Panel pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(40, 40, 40) };
            
            TextBox txtInput = new TextBox { Location = new Point(10, 10), Width = 300 };
            txtInput.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter) {
                    await SendText(txtInput.Text);
                    txtInput.Text = "";
                    e.SuppressKeyPress = true; // Prevent beep
                }
            };
            
            Button btnSend = new Button { Text = "Send Text", Location = new Point(320, 8), Width = 100, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSend.Click += async (s, e) => {
                await SendText(txtInput.Text);
                txtInput.Text = "";
            };

            pnlBot.Controls.Add(txtInput);
            pnlBot.Controls.Add(btnSend);
            this.Controls.Add(pnlBot);
            picScreen.BringToFront(); // Ensure PictureBox fills the rest


            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 200; // 5 FPS (Boosted by DXGI)
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
            
            // Initial load
            RefreshScreen();
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_isRequesting) return; // Drop frame if lagging
            await RefreshScreen();
        }

        private async Task RefreshScreen()
        {
            _isRequesting = true;
            try
            {
                byte[] data = await _httpClient.GetByteArrayAsync($"{_baseUrl}/api/screen?pin={_pin}");
                using (var ms = new MemoryStream(data))
                {
                    Image old = picScreen.Image;
                    picScreen.Image = Image.FromStream(ms);
                    old?.Dispose();
                }
            }
            catch (Exception ex)
            {
                this.Text = $"Remote View - Disconnected ({ex.Message})";
            }
            finally
            {
                _isRequesting = false;
            }
        }

        private async void PicScreen_MouseClick(object sender, MouseEventArgs e)
        {
            if (picScreen.Image == null) return;

            // Coordinate Transformation (UI -> Real Image)
            // Since SizeMode is Zoom, we need to calculate the actual image position
            
            float imageRatio = (float)picScreen.Image.Width / picScreen.Image.Height;
            float containerRatio = (float)picScreen.Width / picScreen.Height;
            
            float actualWidth, actualHeight, offsetX, offsetY;

            if (containerRatio > imageRatio)
            {
                // Container is wider than image (Pillarbox)
                actualHeight = picScreen.Height;
                actualWidth = actualHeight * imageRatio;
                offsetY = 0;
                offsetX = (picScreen.Width - actualWidth) / 2;
            }
            else
            {
                // Container is taller than image (Letterbox)
                actualWidth = picScreen.Width;
                actualHeight = actualWidth / imageRatio;
                offsetX = 0;
                offsetY = (picScreen.Height - actualHeight) / 2;
            }

            // Click is outside the image area
            if (e.X < offsetX || e.X > offsetX + actualWidth || 
                e.Y < offsetY || e.Y > offsetY + actualHeight) return;

            // Calculate relative position (0.0 ~ 1.0)
            float relativeX = (e.X - offsetX) / actualWidth;
            float relativeY = (e.Y - offsetY) / actualHeight;

            // Calculate real server coordinates
            int serverX = (int)(relativeX * picScreen.Image.Width);
            int serverY = (int)(relativeY * picScreen.Image.Height);

            // Send Click Request
            try
            {
                await _httpClient.GetAsync($"{_baseUrl}/api/click?pin={_pin}&x={serverX}&y={serverY}");
            }
            catch { }
        }

        private async Task SendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                string safeText = Uri.EscapeDataString(text);
                await _httpClient.GetAsync($"{_baseUrl}/api/type?pin={_pin}&text={safeText}");
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            _httpClient.Dispose();
            base.OnFormClosing(e);
        }
    }
}
