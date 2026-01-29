using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Services.ExternalTools; // Added

namespace PiNodeMonitorWinForm
{
    public class MultiMonitorForm : Form
    {
        // UI Controls
        private DataGridView gridNodes;
        private TextBox txtAlias, txtIp, txtPin;
        private Button btnAdd;
        private System.Windows.Forms.Timer pollTimer;
        private Label lblStatus;

        // Data
        private List<RemoteNodeConfig> _nodes = new List<RemoteNodeConfig>();
        private const string CONFIG_FILE = "nodes_config.json";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        public MultiMonitorForm()
        {
            this.Text = "Multi-Node Control Center";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            InitializeUI();
            LoadConfigs();
            
            pollTimer = new System.Windows.Forms.Timer();
            pollTimer.Interval = 5000; // 5 seconds
            pollTimer.Tick += async (s, e) => await PollAllNodes();
            pollTimer.Start();
        }

        private void InitializeUI()
        {
            // --- Top Panel (Add Node) ---
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(30, 30, 30) };
            
            Label lbl1 = new Label { Text = "Alias:", Location = new Point(10, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtAlias = new TextBox { Location = new Point(55, 18), Width = 100 };
            
            Label lbl2 = new Label { Text = "IP:Port:", Location = new Point(170, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtIp = new TextBox { Location = new Point(225, 18), Width = 150 }; // e.g., 192.168.1.5:5000
            
            Label lbl3 = new Label { Text = "PIN:", Location = new Point(390, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtPin = new TextBox { Location = new Point(425, 18), Width = 60 };

            btnAdd = new Button { Text = "Add Node", Location = new Point(510, 16), Width = 100, Height = 28 };
            btnAdd.BackColor = Color.SteelBlue;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Click += BtnAdd_Click;

            // Config Button
            Button btnConfig = new Button { Text = "⚙️ Tools Config", Location = new Point(620, 16), Width = 120, Height = 28 };
            btnConfig.BackColor = Color.FromArgb(64, 64, 64);
            btnConfig.FlatStyle = FlatStyle.Flat;
            btnConfig.Click += (s, e) => new ExternalToolsSettingsForm().ShowDialog();

            pnlTop.Controls.AddRange(new Control[] { lbl1, txtAlias, lbl2, txtIp, lbl3, txtPin, btnAdd, btnConfig });
            this.Controls.Add(pnlTop);

            // --- Grid Container (Fix overlap issue) ---
            Panel pnlGridContainer = new Panel();
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Padding = new Padding(0, 5, 0, 0); // Slight gap
            
            gridNodes = new DataGridView();
            gridNodes.Dock = DockStyle.Fill;
            gridNodes.BackgroundColor = Color.FromArgb(45, 45, 48);
            gridNodes.ForeColor = Color.Black; // Cell text color
            gridNodes.AllowUserToAddRows = false;
            gridNodes.RowHeadersVisible = false;
            gridNodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridNodes.MultiSelect = false;
            gridNodes.MouseDown += GridNodes_MouseDown; // Context Menu trigger
            gridNodes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            gridNodes.Columns.Add("Alias", "Name");
            gridNodes.Columns.Add("IP", "Address");
            gridNodes.Columns.Add("Status", "State");
            gridNodes.Columns.Add("Block", "Block Height");
            gridNodes.Columns.Add("Out", "Out");
            gridNodes.Columns.Add("In", "In");
            gridNodes.Columns.Add("LastUpdate", "Updated");
            
            // Remote View Button Column
            DataGridViewButtonColumn btnRemote = new DataGridViewButtonColumn();
            btnRemote.HeaderText = "Remote";
            btnRemote.Text = "📺 View";
            btnRemote.UseColumnTextForButtonValue = true;
            gridNodes.Columns.Add(btnRemote);

            // Remove Button Column
            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn();
            btnDel.HeaderText = "Action";
            btnDel.Text = "Remove";
            btnDel.UseColumnTextForButtonValue = true;
            gridNodes.Columns.Add(btnDel);

            gridNodes.CellClick += GridNodes_CellClick;

            pnlGridContainer.Controls.Add(gridNodes); // Grid inside Container
            this.Controls.Add(pnlGridContainer);      // Container inside Form
            
            // Correct Docking Order: 
            // Control at the bottom of Z-order (SendToBack) is docked FIRST.
            // We want Top panel to reserve space first, so send it to back.
            pnlTop.SendToBack(); 
            pnlGridContainer.BringToFront();
            
            // --- Status Bar ---
            StatusStrip statusStrip = new StatusStrip();
            lblStatus = new Label { Text = "Ready", AutoSize = true, ForeColor = Color.Black }; 
            // StatusStrip items are specific, let's just use a Panel or simple label if not using ToolStripItem
            // Simpler: Just a label added to bottom or use the form's status bar logic later. 
            // For now, no complex status bar effectively needed.
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAlias.Text) || string.IsNullOrWhiteSpace(txtIp.Text) || string.IsNullOrWhiteSpace(txtPin.Text))
            {
                MessageBox.Show("Please fill all fields (Alias, IP:Port, PIN).");
                return;
            }

            var node = new RemoteNodeConfig 
            { 
                Alias = txtAlias.Text, 
                IpAddress = txtIp.Text, 
                Pin = txtPin.Text 
            };

            _nodes.Add(node);
            AddGridRow(node);
            SaveConfigs();
            
            txtAlias.Clear();
            txtIp.Clear();
            txtPin.Clear();
        }

        private void GridNodes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Handle Remote View Button (Index 7)
            if (e.ColumnIndex == 7) 
            {
                if (gridNodes.Rows[e.RowIndex].Tag is RemoteNodeConfig node)
                {
                    new RemoteViewForm(node.IpAddress, node.Pin, node.Alias).Show();
                }
            }
            // Handle Remove Button (Index 8)
            else if (e.ColumnIndex == 8) 
            {
                _nodes.RemoveAt(e.RowIndex);
                gridNodes.Rows.RemoveAt(e.RowIndex);
                SaveConfigs();
            }
        }

        private void AddGridRow(RemoteNodeConfig node)
        {
            // Add row with placeholder values. Buttons (Remote, Remove) are auto-rendered.
            int idx = gridNodes.Rows.Add(node.Alias, node.IpAddress, "Waiting...", "-", "-", "-", "-", null, null);
            gridNodes.Rows[idx].Tag = node; // Link row to object
        }

        private async Task PollAllNodes()
        {
            // Parallel polling
            var tasks = new List<Task>();
            foreach (DataGridViewRow row in gridNodes.Rows)
            {
                if (row.Tag is RemoteNodeConfig node)
                {
                    tasks.Add(CheckNodeStatus(node, row));
                }
            }
            await Task.WhenAll(tasks);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            pollTimer.Stop();
            base.OnFormClosing(e);
        }

        private async Task CheckNodeStatus(RemoteNodeConfig node, DataGridViewRow row)
        {
            try
            {
                string url = node.IpAddress.StartsWith("http") ? 
                    $"{node.IpAddress}/api/status?pin={node.Pin}" : 
                    $"http://{node.IpAddress}/api/status?pin={node.Pin}";

                var response = await _httpClient.GetStringAsync(url);
                var status = JsonConvert.DeserializeObject<NodeStatusData>(response);

                if (this.IsDisposed || !this.IsHandleCreated) return;

                this.Invoke((MethodInvoker)delegate 
                {
                    if (status != null)
                    {
                        row.Cells[2].Value = status.State;
                        row.Cells[3].Value = status.LocalBlock;
                        row.Cells[4].Value = status.Outgoing;
                        row.Cells[5].Value = status.Incoming;
                        row.Cells[6].Value = DateTime.Now.ToString("HH:mm:ss");

                        // Coloring
                        if (status.State.Contains("Synced") || status.State.Contains("Running"))
                        {
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.LightPink;
                        }
                    }
                });
            }
            catch
            {
                if (this.IsDisposed || !this.IsHandleCreated) return;
                
                this.Invoke((MethodInvoker)delegate 
                {
                    row.Cells[2].Value = "Offline";
                    row.Cells[6].Value = DateTime.Now.ToString("HH:mm:ss");
                    row.DefaultCellStyle.BackColor = Color.Gray;
                });
            }
        }

        private void LoadConfigs()
        {
            if (File.Exists(CONFIG_FILE))
            {
                try
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    _nodes = JsonConvert.DeserializeObject<List<RemoteNodeConfig>>(json) ?? new List<RemoteNodeConfig>();
                    foreach (var n in _nodes) AddGridRow(n);
                }
                catch { }
            }
        }

        private void SaveConfigs()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_nodes, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }

        private void GridNodes_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = gridNodes.HitTest(e.X, e.Y);
                if (hti.RowIndex >= 0)
                {
                    // Select the row
                    gridNodes.ClearSelection();
                    gridNodes.Rows[hti.RowIndex].Selected = true;

                    var node = gridNodes.Rows[hti.RowIndex].Tag as RemoteNodeConfig;
                    if (node == null) return;

                    // Build Context Menu
                    ContextMenuStrip mnu = new ContextMenuStrip();
                    mnu.Items.Add(new ToolStripMenuItem("Remote Control Options") { Enabled = false, BackColor = Color.LightGray });
                    mnu.Items.Add(new ToolStripSeparator());

                    // 1. Internal Viewer
                    var itemInternal = new ToolStripMenuItem("📺 View Screen (Internal)");
                    itemInternal.Click += (s, ev) => new RemoteViewForm(node.IpAddress, node.Pin, node.Alias).Show();
                    mnu.Items.Add(itemInternal);

                    // 2. External Tools
                    var svc = new ExternalToolService();
                    var tools = svc.GetTools();
                    if (tools.Count > 0) mnu.Items.Add(new ToolStripSeparator());

                    foreach (var tool in tools)
                    {
                        var item = new ToolStripMenuItem($"🚀 Open with {tool.Name}");
                        item.Click += (s, ev) => {
                            string[] parts = node.IpAddress.Split(':');
                            string ip = parts[0];
                            int port = parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 5000;
                            svc.LaunchTool(tool.Name, ip, port);
                        };
                        mnu.Items.Add(item);
                    }

                    mnu.Show(gridNodes, new Point(e.X, e.Y));
                }
            }
        }
    }

    public class RemoteNodeConfig
    {
        public string Alias { get; set; }
        public string IpAddress { get; set; }
        public string Pin { get; set; }
    }
}
