using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Services.Sms;

namespace PiNodeMonitorWinForm
{
    public class SmsSettingsForm : Form
    {
        private ComboBox cmbProvider;
        private TextBox txtKey1; // API Key or SID
        private TextBox txtKey2; // Secret or Token
        private TextBox txtSender;
        private TextBox txtTarget;
        private Button btnSave;
        private Button btnTest;

        public SmsSettingsForm()
        {
            this.Text = "SMS Notification Settings";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 65, 95); // Deep Blue Theme
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int lblW = 100;
            int txtW = 250;

            // Provider
            AddLabel("Provider:", 20, y);
            cmbProvider = new ComboBox();
            cmbProvider.Items.AddRange(new object[] { "COOL", "TWILIO" });
            cmbProvider.SelectedIndex = 0;
            cmbProvider.Location = new Point(120, y);
            cmbProvider.Size = new Size(txtW, 25);
            cmbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbProvider);
            y += 40;

            // Key1
            AddLabel("API Key / SID:", 20, y);
            txtKey1 = AddTextBox(120, y, txtW);
            y += 40;

            // Key2
            AddLabel("Secret / Token:", 20, y);
            txtKey2 = AddTextBox(120, y, txtW);
            txtKey2.UseSystemPasswordChar = true;
            y += 40;

            // Sender
            AddLabel("Sender Number:", 20, y);
            txtSender = AddTextBox(120, y, txtW);
            y += 40;

            // Target
            AddLabel("Target Number:", 20, y);
            txtTarget = AddTextBox(120, y, txtW);
            y += 50;

            // Buttons
            btnSave = new Button();
            btnSave.Text = "💾 Save";
            btnSave.Location = new Point(120, y);
            btnSave.Size = new Size(80, 35);
            btnSave.BackColor = Color.ForestGreen;
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnTest = new Button();
            btnTest.Text = "📨 Test";
            btnTest.Location = new Point(210, y);
            btnTest.Size = new Size(80, 35);
            btnTest.BackColor = Color.SteelBlue;
            btnTest.ForeColor = Color.White;
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            LoadSettings();
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y + 3);
            lbl.ForeColor = Color.White;
            lbl.AutoSize = true;
            this.Controls.Add(lbl);
        }

        private TextBox AddTextBox(int x, int y, int w)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(x, y);
            txt.Size = new Size(w, 25);
            this.Controls.Add(txt);
            return txt;
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists("sms_config.txt"))
                {
                    var line = File.ReadAllText("sms_config.txt").Trim();
                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        cmbProvider.Text = parts[0];
                        txtKey1.Text = parts[1];
                        txtKey2.Text = parts[2];
                        txtSender.Text = parts[3];
                        txtTarget.Text = parts[4];
                    }
                }
            }
            catch { }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string data = $"{cmbProvider.Text}|{txtKey1.Text.Trim()}|{txtKey2.Text.Trim()}|{txtSender.Text.Trim()}|{txtTarget.Text.Trim()}";
                File.WriteAllText("sms_config.txt", data);
                MessageBox.Show("Settings Saved!\nRestart or Reload might be required.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message);
            }
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            try
            {
                // Create temporary service to test
                SaveTempConfig(); // Temporarily save to file so Service can pick it up, or manually instantiate (better)
                
                ISmsProvider provider = null;
                string type = cmbProvider.Text;
                if (type == "COOL") provider = new CoolSmsProvider(txtKey1.Text, txtKey2.Text, txtSender.Text);
                else provider = new TwilioProvider(txtKey1.Text, txtKey2.Text, txtSender.Text);

                bool result = await provider.SendSmsAsync(txtTarget.Text, "[PiNode] Test Message Success!");
                
                if (result) MessageBox.Show("Test SMS Sent Successfully!", "Success");
                else MessageBox.Show("Failed to send test SMS.", "Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnTest.Enabled = true;
            }
        }

        private void SaveTempConfig()
        {
            string data = $"{cmbProvider.Text}|{txtKey1.Text.Trim()}|{txtKey2.Text.Trim()}|{txtSender.Text.Trim()}|{txtTarget.Text.Trim()}";
            File.WriteAllText("sms_config.txt", data);
        }
    }
}
