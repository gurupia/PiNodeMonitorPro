using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            pnlTop.Controls.AddRange(new Control[] { lbl1, txtAlias, lbl2, txtIp, lbl3, txtPin, btnAdd });
            this.Controls.Add(pnlTop);

            // --- Grid ---
            gridNodes = new DataGridView();
            gridNodes.Dock = DockStyle.Fill;
            gridNodes.BackgroundColor = Color.FromArgb(45, 45, 48);
            gridNodes.ForeColor = Color.Black; // Cell text color
            gridNodes.AllowUserToAddRows = false;
            gridNodes.RowHeadersVisible = false;
            gridNodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridNodes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            gridNodes.Columns.Add("Alias", "Name");
            gridNodes.Columns.Add("IP", "Address");
            gridNodes.Columns.Add("Status", "State");
            gridNodes.Columns.Add("Block", "Block Height");
            gridNodes.Columns.Add("Out", "Out");
            gridNodes.Columns.Add("In", "In");
            gridNodes.Columns.Add("LastUpdate", "Updated");
            
            // Remove Button Column
            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn();
            btnDel.HeaderText = "Action";
            btnDel.Text = "Remove";
            btnDel.UseColumnTextForButtonValue = true;
            gridNodes.Columns.Add(btnDel);

            gridNodes.CellClick += GridNodes_CellClick;

            this.Controls.Add(gridNodes);

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
            // Handle Remove Button
            if (e.RowIndex >= 0 && e.ColumnIndex == 7) // Remove Column
            {
                _nodes.RemoveAt(e.RowIndex);
                gridNodes.Rows.RemoveAt(e.RowIndex);
                SaveConfigs();
            }
        }

        private void AddGridRow(RemoteNodeConfig node)
        {
            int idx = gridNodes.Rows.Add(node.Alias, node.IpAddress, "Waiting...", "-", "-", "-", "-");
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

        private async Task CheckNodeStatus(RemoteNodeConfig node, DataGridViewRow row)
        {
            try
            {
                string url = $"http://{node.IpAddress}/api/status?pin={node.Pin}";
                // Ensure http:// prefix if missing
                if (!node.IpAddress.StartsWith("http")) url = $"http://{node.IpAddress}/api/status?pin={node.Pin}";

                var response = await _httpClient.GetStringAsync(url);
                var status = JsonSerializer.Deserialize<NodeStatusData>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                        
                        // Check Stall? (Optional logic here)
                    }
                });
            }
            catch
            {
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
                    _nodes = JsonSerializer.Deserialize<List<RemoteNodeConfig>>(json) ?? new List<RemoteNodeConfig>();
                    foreach (var n in _nodes) AddGridRow(n);
                }
                catch { }
            }
        }

        private void SaveConfigs()
        {
            try
            {
                string json = JsonSerializer.Serialize(_nodes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }
    }

    public class RemoteNodeConfig
    {
        public string Alias { get; set; }
        public string IpAddress { get; set; }
        public string Pin { get; set; }
    }
}
