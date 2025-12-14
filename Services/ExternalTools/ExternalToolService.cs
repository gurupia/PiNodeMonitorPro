using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm.Services.ExternalTools
{
    public class ExternalToolConfig
    {
        public string Name { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string Arguments { get; set; } = ""; // Placeholders: {ip}, {port}
        public bool IsDefault { get; set; } = false;
    }

    public class ExternalToolService
    {
        private const string CONFIG_FILE = "external_tools.json";
        private List<ExternalToolConfig> _tools = new List<ExternalToolConfig>();

        public ExternalToolService()
        {
            LoadTools();
        }

        public List<ExternalToolConfig> GetTools()
        {
            return _tools;
        }

        public void AddTool(ExternalToolConfig tool)
        {
            _tools.Add(tool);
            SaveTools();
        }

        public void RemoveTool(ExternalToolConfig tool)
        {
            _tools.Remove(tool);
            SaveTools();
        }

        public void UpdateTool(int index, ExternalToolConfig tool)
        {
            if (index >= 0 && index < _tools.Count)
            {
                _tools[index] = tool;
                SaveTools();
            }
        }

        public void LoadTools()
        {
            if (File.Exists(CONFIG_FILE))
            {
                try
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    _tools = JsonSerializer.Deserialize<List<ExternalToolConfig>>(json) ?? new List<ExternalToolConfig>();
                }
                catch { _tools = new List<ExternalToolConfig>(); }
            }

            // Defaults if empty
            if (_tools.Count == 0)
            {
                _tools.Add(new ExternalToolConfig 
                { 
                    Name = "Windows Remote Desktop (MSTSC)", 
                    ExecutablePath = "mstsc.exe", 
                    Arguments = "/v:{ip}", 
                    IsDefault = true 
                });
                
                _tools.Add(new ExternalToolConfig 
                { 
                    Name = "RustDesk", 
                    ExecutablePath = @"C:\Program Files\RustDesk\rustdesk.exe", 
                    Arguments = "--connect {ip}" 
                });

                _tools.Add(new ExternalToolConfig 
                { 
                    Name = "HopToDesk", 
                    ExecutablePath = @"C:\Program Files\HopToDesk\HopToDesk.exe", 
                    Arguments = "--connect {ip}" 
                });

                _tools.Add(new ExternalToolConfig 
                { 
                    Name = "AnyDesk", 
                    ExecutablePath = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe", 
                    Arguments = "{ip}" 
                });

                _tools.Add(new ExternalToolConfig 
                { 
                    Name = "TeamViewer", 
                    ExecutablePath = @"C:\Program Files\TeamViewer\TeamViewer.exe", 
                    Arguments = "-i {ip}" 
                });

                SaveTools();
            }
        }

        private void SaveTools()
        {
            try
            {
                string json = JsonSerializer.Serialize(_tools, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }

        public void LaunchTool(ExternalToolConfig tool, string ip, string port = "")
        {
            try
            {
                string ipOnly = ip;
                string portOnly = "3389"; // Default RDP

                // Handle IP:Port format
                if (ip.Contains(":"))
                {
                    var parts = ip.Split(':');
                    ipOnly = parts[0];
                    if (parts.Length > 1) portOnly = parts[1];
                }

                // If port arg is provided separately, use it
                if (!string.IsNullOrEmpty(port)) portOnly = port;

                // Replace placeholders
                string args = tool.Arguments
                    .Replace("{ip}", ipOnly)
                    .Replace("{port}", portOnly);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tool.ExecutablePath,
                    Arguments = args,
                    UseShellExecute = true 
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch tool: {ex.Message}\n\nPlease check if the executable path is correct.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
