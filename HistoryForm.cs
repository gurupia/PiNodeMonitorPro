using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public class HistoryForm : Form
    {
        private DataGridView _grid;
        private string _csvPath;

        public HistoryForm(string csvPath)
        {
            _csvPath = csvPath;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Node Availability & Bonus History";
            this.Size = new System.Drawing.Size(700, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.White,
                GridColor = System.Drawing.Color.FromArgb(60, 60, 60),
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.None
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            _grid.EnableHeadersVisualStyles = false;
            _grid.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            _grid.DefaultCellStyle.ForeColor = System.Drawing.Color.AntiqueWhite;

            this.Controls.Add(_grid);
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(_csvPath))
                {
                    MessageBox.Show("No history data found yet.", "Info");
                    return;
                }

                var lines = File.ReadAllLines(_csvPath);
                if (lines.Length <= 1) return;

                // Header
                var headers = lines[0].Split(',');
                foreach (var header in headers)
                {
                    _grid.Columns.Add(header, header);
                }

                // Data Rows (Reverse to show latest first)
                for (int i = lines.Length - 1; i >= 1; i--)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length == headers.Length)
                    {
                        _grid.Rows.Add(parts);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load history: {ex.Message}");
            }
        }
    }
}
