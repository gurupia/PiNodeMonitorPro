using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Services.Sms;

namespace PiNodeMonitorWinForm
{
    public class SmsSettingsForm : Form
    {
        // Controls
        private TabControl tabControl;
        private TabPage tabSms;
        private TabPage tabTele;

        // SMS Tab Controls
        private ComboBox cmbProvider;
        private Label lblKey1;
        private TextBox txtKey1; 
        private Label lblKey2;
        private TextBox txtKey2; 
        private TextBox txtSender;
        private TextBox txtTarget;
        private TextBox txtTemplate;
        private CheckBox chkNodeAlert;
        private LinkLabel lnkGetKey;

        // Telegram Tab Controls
        private CheckBox chkEnableTelegram;
        private TextBox txtBotToken;
        private TextBox txtChatId;

        // Common
        private Button btnSave;
        private Button btnTest;

        private SmsConfigModel _config;
        private bool _isLoading = false;
        private string _activeProvider = "SOLAPI";

        public SmsSettingsForm()
        {
            this.Text = "Notification Settings";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 65, 95);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Load Config
            var svc = new SmsService();
            _config = svc.GetConfig() ?? new SmsConfigModel();

            // Tabs
            tabControl = new TabControl();
            tabControl.Location = new Point(10, 10);
            tabControl.Size = new Size(445, 410);
            
            tabSms = new TabPage("SMS (Paid)");
            tabSms.BackColor = Color.FromArgb(50, 70, 100);
            
            tabTele = new TabPage("Telegram (Free)");
            tabTele.BackColor = Color.FromArgb(50, 70, 100);

            tabControl.TabPages.Add(tabSms);
            tabControl.TabPages.Add(tabTele);
            this.Controls.Add(tabControl);

            // --- SMS Tab Setup ---
            InitSmsTab();

            // --- Telegram Tab Setup ---
            InitTelegramTab();

            // --- Bottom Buttons ---
            btnSave = new Button();
            btnSave.Text = "💾 Save All";
            btnSave.Location = new Point(130, 435);
            btnSave.Size = new Size(100, 35);
            btnSave.BackColor = Color.ForestGreen;
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnTest = new Button();
            btnTest.Text = "📨 Test";
            btnTest.Location = new Point(240, 435);
            btnTest.Size = new Size(80, 35);
            btnTest.BackColor = Color.SteelBlue;
            btnTest.ForeColor = Color.White;
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            LoadDataToUI();
        }

        private void InitSmsTab()
        {
            int y = 20;
            int txtW = 250;

            AddLabel(tabSms, "Provider:", 20, y);
            cmbProvider = new ComboBox();
            cmbProvider.Items.AddRange(new object[] { "SOLAPI", "TWILIO" });
            cmbProvider.Size = new Size(180, 25);
            cmbProvider.Location = new Point(120, y);
            cmbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProvider.SelectedIndexChanged += CmbProvider_SelectedIndexChanged;
            tabSms.Controls.Add(cmbProvider);

            lnkGetKey = new LinkLabel();
            lnkGetKey.Text = "🔑 Get Key";
            lnkGetKey.Location = new Point(310, y + 5);
            lnkGetKey.AutoSize = true;
            lnkGetKey.LinkColor = Color.LightSkyBlue;
            lnkGetKey.Cursor = Cursors.Hand;
            lnkGetKey.LinkClicked += (s, e) => {
                string url = cmbProvider.Text == "SOLAPI" ? "https://console.solapi.com/credentials" : "https://console.twilio.com";
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true }); } catch { }
            };
            tabSms.Controls.Add(lnkGetKey);
            y += 40;

            lblKey1 = AddLabel(tabSms, "API Key:", 20, y);
            txtKey1 = AddTextBox(tabSms, 120, y, txtW);
            y += 40;

            lblKey2 = AddLabel(tabSms, "Secret:", 20, y);
            txtKey2 = AddTextBox(tabSms, 120, y, txtW);
            txtKey2.UseSystemPasswordChar = true;
            y += 40;

            AddLabel(tabSms, "Sender Num:", 20, y);
            txtSender = AddTextBox(tabSms, 120, y, txtW);
            y += 40;

            AddLabel(tabSms, "Target Num:", 20, y);
            txtTarget = AddTextBox(tabSms, 120, y, txtW);
            y += 40;

            AddLabel(tabSms, "Template:", 20, y);
            txtTemplate = AddTextBox(tabSms, 120, y, txtW);
            
            Label lblHelp = new Label();
            lblHelp.Text = "Use {amount} and {total}";
            lblHelp.ForeColor = Color.LightGray;
            lblHelp.Font = new Font("Segoe UI", 8);
            lblHelp.Location = new Point(120, y + 26);
            lblHelp.AutoSize = true;
            tabSms.Controls.Add(lblHelp);
            y += 45;

            chkNodeAlert = new CheckBox();
            chkNodeAlert.Text = "🚨 Alert if Node Stalls (SMS & Tele)";
            chkNodeAlert.Location = new Point(120, y);
            chkNodeAlert.Size = new Size(250, 25);
            chkNodeAlert.ForeColor = Color.Salmon;
            tabSms.Controls.Add(chkNodeAlert);
        }

        private void InitTelegramTab()
        {
            int y = 30;
            int txtW = 250;

            chkEnableTelegram = new CheckBox();
            chkEnableTelegram.Text = "Enable Telegram Notifications";
            chkEnableTelegram.Location = new Point(20, y);
            chkEnableTelegram.Size = new Size(300, 25);
            chkEnableTelegram.ForeColor = Color.LightGreen;
            chkEnableTelegram.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            tabTele.Controls.Add(chkEnableTelegram);
            y += 50;

            AddLabel(tabTele, "Bot Token:", 20, y);
            txtBotToken = AddTextBox(tabTele, 120, y, txtW);
            y += 40;

            AddLabel(tabTele, "Chat ID:", 20, y);
            txtChatId = AddTextBox(tabTele, 120, y, txtW);
            y += 50;

            Label lblGuide = new Label();
            lblGuide.Text = "How to use:\n1. Search '@BotFather' in Telegram.\n2. Create new bot -> Get Token.\n3. Search '@userinfobot' -> Get Chat ID.\n4. Start chat with your bot.";
            lblGuide.ForeColor = Color.LightGray;
            lblGuide.AutoSize = true;
            lblGuide.Location = new Point(20, y);
            tabTele.Controls.Add(lblGuide);
        }

        private Label AddLabel(TabPage page, string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y + 3);
            lbl.ForeColor = Color.White;
            lbl.AutoSize = true;
            page.Controls.Add(lbl);
            return lbl;
        }

        private TextBox AddTextBox(TabPage page, int x, int y, int w)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(x, y);
            txt.Size = new Size(w, 25);
            page.Controls.Add(txt);
            return txt;
        }

        private void LoadDataToUI()
        {
            _isLoading = true;
            try
            {
                // SMS
                if (!string.IsNullOrEmpty(_config.SelectedProvider))
                     cmbProvider.SelectedItem = _config.SelectedProvider;
                else
                     cmbProvider.SelectedIndex = 0;
                
                UpdateUIForProvider();

                txtTarget.Text = _config.TargetPhone;
                txtTemplate.Text = _config.Template;
                chkNodeAlert.Checked = _config.IsNodeAlertEnabled;

                // Telegram
                chkEnableTelegram.Checked = _config.EnableTelegram;
                txtBotToken.Text = _config.TelegramBotToken;
                txtChatId.Text = _config.TelegramChatId;
            }
            finally { _isLoading = false; }
        }

        private void UpdateUIForProvider()
        {
            _activeProvider = _config.SelectedProvider;
            cmbProvider.Text = _activeProvider;

            if (_activeProvider == "SOLAPI")
            {
                lblKey1.Text = "API Key:";
                lblKey2.Text = "API Secret:";
                txtKey1.Text = _config.Solapi.Key1;
                txtKey2.Text = _config.Solapi.Key2;
                txtSender.Text = _config.Solapi.SenderPhone;
            }
            else
            {
                lblKey1.Text = "Account SID:";
                lblKey2.Text = "Auth Token:";
                txtKey1.Text = _config.Twilio.Key1;
                txtKey2.Text = _config.Twilio.Key2;
                txtSender.Text = _config.Twilio.SenderPhone;
            }
        }

        private void CmbProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            SaveCurrentUIToModel();
            _config.SelectedProvider = cmbProvider.Text;
            UpdateUIForProvider();
        }

        private void SaveCurrentUIToModel()
        {
            // SMS Common
            _config.TargetPhone = txtTarget.Text;
            _config.Template = txtTemplate.Text;
            _config.IsNodeAlertEnabled = chkNodeAlert.Checked;

            // SMS Provider Specific
            if (_activeProvider == "SOLAPI")
            {
                _config.Solapi.Key1 = txtKey1.Text;
                _config.Solapi.Key2 = txtKey2.Text;
                _config.Solapi.SenderPhone = txtSender.Text;
            }
            else
            {
                _config.Twilio.Key1 = txtKey1.Text;
                _config.Twilio.Key2 = txtKey2.Text;
                _config.Twilio.SenderPhone = txtSender.Text;
            }

            // Telegram
            _config.EnableTelegram = chkEnableTelegram.Checked;
            _config.TelegramBotToken = txtBotToken.Text;
            _config.TelegramChatId = txtChatId.Text;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveCurrentUIToModel();
            _config.SelectedProvider = cmbProvider.Text;
            SmsService.SaveSettings(_config);
            MessageBox.Show("Settings Saved!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            try
            {
                SaveCurrentUIToModel(); // Update model with latest inputs
                
                string msgTemplate = txtTemplate.Text;
                if (string.IsNullOrWhiteSpace(msgTemplate)) msgTemplate = "[PiNode] Test Message!";
                string testMsg = msgTemplate.Replace("{amount}", "3.14").Replace("{total}", "1234.56");

                var svc = new SmsService();
                // Inject updated config temporarily? 
                // SmsService loads from file. We just saved to model but maybe not to file yet if we clicked Test before Save.
                // To test properly without saving to disk, we need to pass config to Service, or Save Temp.
                // Creating a one-off logic here is easiest.

                bool smsSent = false;
                // Test SMS (Only if on SMS Tab or always?) -> Let's test based on visual context or just try both if enabled.
                
                // 1. Test SMS (if valid)
                if (!string.IsNullOrEmpty(txtKey1.Text))
                {
                    ISmsProvider provider = null;
                    if (cmbProvider.Text == "SOLAPI") 
                        provider = new SolapiSmsProvider(txtKey1.Text, txtKey2.Text, txtSender.Text);
                    else if (cmbProvider.Text == "TWILIO")
                        provider = new TwilioProvider(txtKey1.Text, txtKey2.Text, txtSender.Text);
                    
                    if (provider != null)
                    {
                        try {
                            smsSent = await provider.SendSmsAsync(txtTarget.Text, testMsg);
                        } catch {}
                    }
                }

                // 2. Test Telegram
                bool telegramSent = false;
                if (chkEnableTelegram.Checked && !string.IsNullOrEmpty(txtBotToken.Text) && !string.IsNullOrEmpty(txtChatId.Text))
                {
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        string url = $"https://api.telegram.org/bot{txtBotToken.Text}/sendMessage?chat_id={txtChatId.Text}&text={System.Uri.EscapeDataString(testMsg + " (Telegram Test)")}";
                        var response = await client.GetAsync(url);
                        telegramSent = response.IsSuccessStatusCode;
                    }
                    catch { }
                }

                MessageBox.Show($"Test Signal Sent!\n(SMS Result: {smsSent})\n(Telegram Result: {telegramSent})", "Test Complete");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally { btnTest.Enabled = true; }
        }
    }
}
