using System;
using System.Collections.Generic; // Added!
using System.Drawing;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Services.ExternalTools;

namespace PiNodeMonitorWinForm
{
    public class ExternalToolsSettingsForm : Form
    {
        private ExternalToolService _service;
        private ListBox lstTools;
        private TextBox txtName, txtPath, txtArgs;
        private Button btnBrowse;
        private ExternalToolConfig _selectedTool;

        public ExternalToolsSettingsForm()
        {
            _service = new ExternalToolService(); // Load current configs

            this.Text = "External Remote Tools Settings";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            LoadList();
        }

        private void InitializeUI()
        {
            // Layout: ListBox on Left (150px), Panel on Right
            lstTools = new ListBox();
            lstTools.Location = new Point(10, 10);
            lstTools.Size = new Size(150, 250);
            lstTools.SelectedIndexChanged += LstTools_SelectedIndexChanged;
            this.Controls.Add(lstTools);

            // Right Panel Controls
            int x = 170;
            int y = 10;
            
            this.Controls.Add(new Label { Text = "Name:", Location = new Point(x, y), AutoSize = true });
            txtName = new TextBox { Location = new Point(x, y + 20), Width = 200 };
            this.Controls.Add(txtName);

            y += 50;
            this.Controls.Add(new Label { Text = "Executable Path:", Location = new Point(x, y), AutoSize = true });
            txtPath = new TextBox { Location = new Point(x, y + 20), Width = 170 };
            this.Controls.Add(txtPath);
            
            btnBrowse = new Button { Text = "...", Location = new Point(x + 175, y + 19), Width = 30, Height = 23 };
            btnBrowse.Click += (s, e) => 
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Executables (*.exe)|*.exe";
                    if (ofd.ShowDialog() == DialogResult.OK) txtPath.Text = ofd.FileName;
                }
            };
            this.Controls.Add(btnBrowse);

            y += 50;
            this.Controls.Add(new Label { Text = "Arguments ({ip}, {port}):", Location = new Point(x, y), AutoSize = true });
            txtArgs = new TextBox { Location = new Point(x, y + 20), Width = 200 };
            this.Controls.Add(txtArgs);
            this.Controls.Add(new Label { Text = "Ex: /v:{ip} or --connect {ip}", Location = new Point(x, y + 45), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) });

            // Buttons
            Button btnAdd = new Button { Text = "New", Location = new Point(10, 270), Width = 70 };
            btnAdd.Click += (s, e) => {
                var newTool = new ExternalToolConfig { Name = "New Tool", Arguments = "{ip}" };
                _service.AddTool(newTool);
                LoadList();
                lstTools.SelectedItem = newTool;
            };
            this.Controls.Add(btnAdd);

            Button btnDelete = new Button { Text = "Delete", Location = new Point(90, 270), Width = 70 };
            btnDelete.Click += (s, e) => {
                if (_selectedTool != null) {
                    _service.RemoveTool(_selectedTool);
                    LoadList();
                }
            };
            this.Controls.Add(btnDelete);

            Button btnSave = new Button { Text = "Save Changes", Location = new Point(x, 270), Width = 100, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void LoadList()
        {
            lstTools.Items.Clear();
            foreach (var tool in _service.GetTools())
            {
                lstTools.Items.Add(tool);
            }
            lstTools.DisplayMember = "Name";
        }

        private void LstTools_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTools.SelectedItem is ExternalToolConfig tool)
            {
                _selectedTool = tool;
                txtName.Text = tool.Name;
                txtPath.Text = tool.ExecutablePath;
                txtArgs.Text = tool.Arguments;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_selectedTool == null) return;
            
            _selectedTool.Name = txtName.Text;
            _selectedTool.ExecutablePath = txtPath.Text;
            _selectedTool.Arguments = txtArgs.Text;

            // Trigger internal save
            _service.UpdateTool(lstTools.SelectedIndex, _selectedTool);
            
            LoadList(); // Refresh display name
            MessageBox.Show("Saved!");
        }
    }
}
