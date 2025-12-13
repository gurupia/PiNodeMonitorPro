using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public partial class SetupWizardForm : Form
    {
        private int _currentStep = 1;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblInstruction;
        private Button btnAction;
        private Button btnNext;
        private Button btnManual;
        private ProgressBar progressBar;

        public SetupWizardForm()
        {
            InitializeComponent();
            LoadStep(1); // Start at Step 1
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblStatus = new Label();
            this.lblInstruction = new Label();
            this.btnAction = new Button();
            this.btnNext = new Button();
            this.progressBar = new ProgressBar();

            this.SuspendLayout();

            // Form
            this.ClientSize = new Size(500, 350);
            this.Text = "Pi Node Setup Wizard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(460, 40);
            lblTitle.Text = "Step 1: System Environment";
            
            // Progress
            progressBar.Location = new Point(20, 70);
            progressBar.Size = new Size(460, 10);
            progressBar.Maximum = 4;
            progressBar.Value = 1;

            // Status
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStatus.Location = new Point(20, 100);
            lblStatus.Size = new Size(460, 30);
            lblStatus.Text = "Checking...";
            lblStatus.ForeColor = Color.Gray;

            // Instruction
            lblInstruction.Font = new Font("Segoe UI", 10F);
            lblInstruction.Location = new Point(20, 140);
            lblInstruction.Size = new Size(460, 100);
            lblInstruction.Text = "Please wait while we inspect your system...";

            // Action Button (Install/Fix)
            btnAction.Location = new Point(20, 260);
            btnAction.Size = new Size(150, 40);
            btnAction.Text = "Install / Fix";
            btnAction.Visible = false;
            btnAction.Click += BtnAction_Click;

            // Next/Check Button
            btnNext.Location = new Point(330, 260);
            btnNext.Size = new Size(150, 40);
            btnNext.Text = "Check Again";
            btnNext.Click += BtnNext_Click;

            // Manual Button
            this.btnManual = new Button();
            this.btnManual.Location = new Point(180, 260);
            this.btnManual.Size = new Size(140, 40);
            this.btnManual.Text = "Set Name Manually";
            this.btnManual.Visible = false;
            this.btnManual.Click += BtnManual_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblInstruction);
            this.Controls.Add(btnAction);
            this.Controls.Add(btnManual);
            this.Controls.Add(btnNext);
            
            this.ResumeLayout(false);
        }

        private async void LoadStep(int step)
        {
            _currentStep = step;
            progressBar.Value = step;
            btnNext.Enabled = false;
            progressBar.Value = step;
            btnNext.Enabled = false;
            btnAction.Visible = false;
            btnManual.Visible = false;
            lblStatus.ForeColor = Color.Gray;

            switch (step)
            {
                case 1:
                    lblTitle.Text = "Step 1: Check Docker";
                    lblStatus.Text = "Checking Docker...";
                    lblInstruction.Text = "Checking if Docker Desktop is installed and running.\nThis is required to run the Pi Node.";
                    bool dockerOk = await NodeUtility.IsDockerRunningAsync();
                    UpdateStepUI(dockerOk, "Docker is Running!", "Docker is NOT running or not installed.", "Download Docker");
                    break;

                case 2:
                    lblTitle.Text = "Step 2: Check Node Container";
                    lblStatus.Text = "Checking Container...";
                    lblInstruction.Text = "Checking if 'pi-consensus' or 'testnet2' container exists.\nThis is created automatically when you turn on the Node switch in the Pi App.";
                    
                    bool containerOk = false;
                    if (await NodeUtility.IsContainerExistAsync("pi-consensus")) { containerOk = true; NodeUtility.CurrentContainerName = "pi-consensus"; }
                    else if (await NodeUtility.IsContainerExistAsync("testnet2")) { containerOk = true; NodeUtility.CurrentContainerName = "testnet2"; }

                    if (!containerOk) btnManual.Visible = true;

                    UpdateStepUI(containerOk, "Node Container Found!", "Container missing.", "Open Guide/App");
                    break;

                case 3:
                    lblTitle.Text = "Step 3: Check Firewall";
                    lblStatus.Text = "Checking Firewall...";
                    lblInstruction.Text = "Checking if TCP ports 31401-31403 are open.\nThese allow other nodes to connect to you.";
                    bool firewallOk = await NodeUtility.IsFirewallRulePresentAsync();
                    UpdateStepUI(firewallOk, "Firewall Configured!", "Ports may be blocked.", "Open Firewall Settings");
                    
                    // Allow Skip on Step 3
                    if (!firewallOk)
                    {
                        btnNext.Text = "Skip & Continue";
                        lblInstruction.Text += "\n\nYou can skip this step and configure the firewall manually later.";
                    }
                    break;

                case 4:
                    // All Done
                    this.DialogResult = DialogResult.OK; // Launch Dashboard
                    this.Close();
                    break;
            }
        }

        private void UpdateStepUI(bool isOk, string successMsg, string failMsg, string actionLabel)
        {
            btnNext.Enabled = true;
            if (isOk)
            {
                lblStatus.Text = successMsg;
                lblStatus.ForeColor = Color.Green;
                lblInstruction.Text = "Everything looks good. Click 'Next' to proceed.";
                btnNext.Text = "Next >";
                btnAction.Visible = false;
            }
            else
            {
                lblStatus.Text = failMsg;
                lblStatus.ForeColor = Color.Red;
                lblInstruction.Text = "Issue detected. Please click the button below to fix it, or fix it manually and click 'Check Again'.";
                btnNext.Text = "Check Again";
                btnAction.Text = actionLabel;
                btnAction.Visible = true;
            }
        }

        private async void BtnNext_Click(object sender, EventArgs e)
        {
            // Re-verify before moving
            btnNext.Enabled = false;
            bool ok = false;

            if (_currentStep == 1) ok = await NodeUtility.IsDockerRunningAsync();
            else if (_currentStep == 2) 
            {
                ok = await NodeUtility.IsContainerExistAsync("pi-consensus");
                if (!ok) ok = await NodeUtility.IsContainerExistAsync("testnet2");
            }
            else if (_currentStep == 3)
            {
                // Allow skip
                if (btnNext.Text.Contains("Skip"))
                {
                    ok = true; // Force pass
                }
                else
                {
                    ok = await NodeUtility.IsFirewallRulePresentAsync();
                }
            }

            if (ok)
            {
                LoadStep(_currentStep + 1);
            }
            else
            {
                MessageBox.Show("The condition is not met yet. Please try the Fix button or check manually.", "Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadStep(_currentStep); // Reload to reset UI
            }
        }

        private async void BtnAction_Click(object sender, EventArgs e)
        {
            btnAction.Enabled = false;
            try 
            {
                if (_currentStep == 1)
                {
                    // Open Docker Download Page
                    Process.Start(new ProcessStartInfo { FileName = "https://www.docker.com/products/docker-desktop", UseShellExecute = true });
                    lblInstruction.Text = "Opened Docker website. Please install Docker Desktop and START it.\nThen click 'Check Again'.";
                }
                else if (_currentStep == 2)
                {
                    // Open Guide
                    Process.Start(new ProcessStartInfo { FileName = "https://minepi.com/node-info", UseShellExecute = true });
                    lblInstruction.Text = "Please run the Pi Node App and turn on the switch.\nThis will create the Node container.\nThen click 'Check Again'.";
                }
                else if (_currentStep == 3)
                {
                    // Open Windows Firewall Settings
                    Process.Start(new ProcessStartInfo { FileName = "control", Arguments = "firewall.cpl", UseShellExecute = true });
                    lblInstruction.Text = "Opened Windows Firewall settings.\nPlease add inbound rules for TCP ports 31401-31403.\nThen click 'Check Again' or 'Skip & Continue'.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during action: " + ex.Message);
            }
            finally
            {
                btnAction.Enabled = true;
            }
        }

        private async void BtnManual_Click(object sender, EventArgs e)
        {
            string input = ShowInputDialog("Enter Container Name", "If you named your container differently, enter it here:\n(Default: pi-consensus)");
            if (!string.IsNullOrWhiteSpace(input))
            {
                lblInstruction.Text = $"Checking custom container '{input}'...";
                bool ok = await NodeUtility.IsContainerExistAsync(input);
                if (ok)
                {
                    NodeUtility.CurrentContainerName = input;
                    MessageBox.Show($"Found container '{input}'!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStep(3); // Success, go to next step directly
                }
                else
                {
                     MessageBox.Show($"Could not find container '{input}'.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Helper for Input Dialog (Pure C# Code)
        private string ShowInputDialog(string title, string prompt)
        {
            Form promptForm = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 60, Width = 340 };
            Button confirmation = new Button() { Text = "Check", Left = 250, Width = 100, Top = 100, DialogResult = DialogResult.OK };
            
            promptForm.Controls.Add(textLabel);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }
    }
}
