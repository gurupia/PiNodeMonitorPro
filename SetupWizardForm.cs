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
        private Button btnReboot;
        private Button btnNext;
        private Button btnManual;
        private Button btnSkip;
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
            progressBar.Maximum = 5;
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

            // Skip Button
            this.btnSkip = new Button();
            this.btnSkip.Location = new Point(180, 260); // Shares position with btnManual (only one shown usually)
            this.btnSkip.Size = new Size(140, 40);
            this.btnSkip.Text = "Skip this step";
            this.btnSkip.ForeColor = Color.DarkGray;
            this.btnSkip.Visible = false;
            this.btnSkip.Click += BtnSkip_Click;

            // Reboot Button
            this.btnReboot = new Button();
            this.btnReboot.Location = new Point(180, 260);
            this.btnReboot.Size = new Size(140, 40);
            this.btnReboot.Text = "Reboot System";
            this.btnReboot.BackColor = Color.Salmon;
            this.btnReboot.Visible = false;
            this.btnReboot.Click += (s, e) => { if (MessageBox.Show("시스템을 지금 다시 시작하시겠습니까?", "Reboot", MessageBoxButtons.YesNo) == DialogResult.Yes) NodeUtility.RebootSystem(); };

            this.Controls.Add(lblTitle);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblInstruction);
            this.Controls.Add(btnAction);
            this.Controls.Add(btnReboot);
            this.Controls.Add(btnManual);
            this.Controls.Add(btnSkip);
            this.Controls.Add(btnNext);
            
            this.ResumeLayout(false);
        }

        private async void LoadStep(int step)
        {
            _currentStep = step;
            progressBar.Value = step;
            btnNext.Enabled = false;
            btnAction.Visible = false;
            btnManual.Visible = false;
            btnSkip.Visible = false;
            btnReboot.Visible = false;
            lblStatus.ForeColor = Color.Gray;

            switch (step)
            {
                case 1:
                    lblTitle.Text = "Step 1: Windows Environment";
                    lblStatus.Text = "Checking Windows Features...";
                    lblInstruction.Text = "Checking if 'Virtual Machine Platform' and 'WSL' features are enabled.\nThese are necessary for Docker and Pi Node.";
                    
                    bool vmp = await NodeUtility.IsWindowsFeatureEnabledAsync("VirtualMachinePlatform");
                    bool wsl = await NodeUtility.IsWindowsFeatureEnabledAsync("Microsoft-Windows-Subsystem-Linux");
                    bool wslInstalled = await NodeUtility.IsWslInstalledAsync();

                    if (!vmp || !wsl)
                    {
                        UpdateStepUI(false, "Features Missing", "Virtual Machine Platform or WSL is not enabled.", "Enable Features");
                    }
                    else if (!wslInstalled)
                    {
                         UpdateStepUI(false, "WSL Update Needed", "WSL is enabled but needs update/install.", "Update WSL2");
                    }
                    else
                    {
                        UpdateStepUI(true, "Environment Ready!", "Windows features are configured correctly.", "");
                        // Auto-Advance if successful
                        await Task.Delay(500);
                        LoadStep(2);
                    }
                    break;

                case 2:
                    lblTitle.Text = "Step 2: Check Docker";
                    lblStatus.Text = "Checking Docker...";
                    lblInstruction.Text = "Checking if Docker Desktop is installed and running.\nThis is required to run the Pi Node.";
                    bool dockerOk = await NodeUtility.IsDockerRunningAsync();
                    
                    if (!dockerOk) {
                        btnManual.Visible = true;
                        btnManual.Text = "Set Manual Path";
                    }

                    UpdateStepUI(dockerOk, "Docker is Running!", "Docker is NOT running or not installed.", "Download Docker");
                    if (dockerOk) {
                        await Task.Delay(500);
                        LoadStep(3);
                    }
                    break;

                case 3:
                    lblTitle.Text = "Step 3: Check Node Container";
                    lblStatus.Text = "Checking Container...";
                    lblInstruction.Text = "Checking if 'pi-consensus' or 'testnet2' container exists.\nThis is created automatically when you turn on the Node switch in the Pi App.";
                    
                    bool containerOk = false;
                    if (await NodeUtility.IsContainerExistAsync("pi-consensus")) { containerOk = true; NodeUtility.CurrentContainerName = "pi-consensus"; }
                    else if (await NodeUtility.IsContainerExistAsync("testnet2")) { containerOk = true; NodeUtility.CurrentContainerName = "testnet2"; }

                    btnManual.Visible = true;
                    btnManual.Text = containerOk ? "Custom Container Name" : "Set Manual App Path";

                    UpdateStepUI(containerOk, "Node Container Found!", "Container missing.", "Open Guide/App");
                    if (containerOk) {
                        await Task.Delay(500);
                        LoadStep(4);
                    }
                    break;

                case 4:
                    lblTitle.Text = "Step 4: Check Firewall";
                    lblStatus.Text = "Checking Firewall...";
                    lblInstruction.Text = "Checking if TCP ports 31401-31403 are open.\nThese allow other nodes to connect to you.";
                    bool firewallOk = await NodeUtility.IsFirewallRulePresentAsync();
                    UpdateStepUI(firewallOk, "Firewall Configured!", "Ports may be blocked.", "Open Firewall Settings");
                    
                    // Allow Skip on Step 4
                    if (!firewallOk)
                    {
                        btnNext.Text = "Skip & Continue";
                        lblInstruction.Text += "\n\nYou can skip this step and configure the firewall manually later.";
                    }
                    break;

                case 5:
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
                btnSkip.Visible = true; // Show Skip button when a check fails
            }
        }

        private async void BtnNext_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            
            // 실물 체크: 도커가 실행 중이면 이미 모든 환경이 갖춰진 것이므로 무조건 다음 단계로 진행
            if (await NodeUtility.IsDockerRunningAsync())
            {
                LoadStep(_currentStep + 1);
                return;
            }

            bool ok = false;
            if (_currentStep == 1)
            {
                bool vmp = await NodeUtility.IsWindowsFeatureEnabledAsync("VirtualMachinePlatform");
                bool wsl = await NodeUtility.IsWindowsFeatureEnabledAsync("Microsoft-Windows-Subsystem-Linux");
                ok = vmp && wsl && await NodeUtility.IsWslInstalledAsync();
            }
            else if (_currentStep == 2) ok = await NodeUtility.IsDockerRunningAsync();
            else if (_currentStep == 3) 
            {
                ok = (await NodeUtility.IsContainerExistAsync("pi-consensus")) || (await NodeUtility.IsContainerExistAsync("testnet2"));
            }
            else if (_currentStep == 4)
            {
                if (btnNext.Text.Contains("Skip")) ok = true;
                else ok = await NodeUtility.IsFirewallRulePresentAsync();
            }

            if (ok) LoadStep(_currentStep + 1);
            else LoadStep(_currentStep); // Reload current step status
        }

        private void BtnSkip_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to skip this step?\nThe application might not work correctly if requirements are not met.", 
                                         "Skip Step", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                LoadStep(_currentStep + 1);
            }
        }

        private async void BtnAction_Click(object sender, EventArgs e)
        {
            btnAction.Enabled = false;
            try 
            {
                if (_currentStep == 1)
                {
                    if (btnAction.Text.Contains("Features"))
                    {
                        lblInstruction.Text = "Enabling Windows Features. This may take a minute...\nPlease wait.";
                        await NodeUtility.EnableWindowsFeaturesAsync();
                        lblInstruction.Text = "Features enabled! YOU MUST REBOOT YOUR COMPUTER.\nPlease click 'Reboot System' below.";
                        btnAction.Visible = false;
                        btnReboot.Visible = true;
                    }
                    else if (btnAction.Text.Contains("Update"))
                    {
                        lblInstruction.Text = "Updating WSL2. This may pop up a console window...\nPlease wait.";
                        await NodeUtility.UpdateWslAsync();
                        lblInstruction.Text = "WSL2 Update completed. Click 'Check Again' to proceed.";
                    }
                }
                else if (_currentStep == 2)
                {
                    // Open Docker Download Page
                    Process.Start(new ProcessStartInfo { FileName = "https://www.docker.com/products/docker-desktop", UseShellExecute = true });
                    lblInstruction.Text = "Opened Docker website. Please install Docker Desktop and START it.\nThen click 'Check Again'.";
                }
                else if (_currentStep == 3)
                {
                    // Open Guide
                    Process.Start(new ProcessStartInfo { FileName = "https://minepi.com/node-info", UseShellExecute = true });
                    lblInstruction.Text = "Please run the Pi Node App and turn on the switch.\nThis will create the Node container.\nThen click 'Check Again'.";
                }
                else if (_currentStep == 4)
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
            if (_currentStep == 2)
            {
                if (PickApplicationPath("Docker Desktop 실행 파일 선택 (Docker Desktop.exe)", path => NodeUtility.Config.CustomDockerPath = path))
                {
                    NodeUtility.SaveConfig();
                    LoadStep(2); // Re-check
                }
            }
            else if (_currentStep == 3)
            {
                // If container is missing, we prioritize setting the app path to help the user start the app
                bool containerOk = (await NodeUtility.IsContainerExistAsync("pi-consensus")) || (await NodeUtility.IsContainerExistAsync("testnet2"));
                
                if (!containerOk)
                {
                    if (PickApplicationPath("Pi Network 실행 파일 선택 (Pi Network.exe)", path => NodeUtility.Config.CustomPiAppPath = path))
                    {
                        NodeUtility.SaveConfig();
                        LoadStep(3); // Re-check
                    }
                }
                else
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
                            LoadStep(4); 
                        }
                        else
                        {
                            MessageBox.Show($"Could not find container '{input}'.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private bool PickApplicationPath(string title, Action<string> saveAction)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Executable Files|*.exe", Title = title })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    saveAction(ofd.FileName);
                    return true;
                }
            }
            return false;
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
