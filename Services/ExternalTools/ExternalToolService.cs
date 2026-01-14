using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm.Services.ExternalTools
{
    public class ExternalToolConfig
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string Arguments { get; set; } 
        public bool IsDefault { get; set; }

        public ExternalToolConfig()
        {
            Name = "";
            ExecutablePath = "";
            Arguments = "";
            IsDefault = false;
        }

        public override string ToString() => Name;
    }

    public class ExternalToolService
    {
        private const string CONFIG_FILE = "external_tools.json";
        private List<ExternalToolConfig> _tools;

        public ExternalToolService()
        {
            _tools = new List<ExternalToolConfig>();
            LoadConfigs();
        }

        public List<ExternalToolConfig> GetTools() => _tools;

        public void LoadConfigs()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    _tools = JsonConvert.DeserializeObject<List<ExternalToolConfig>>(json);
                }
            }
            catch { }

            if (_tools == null || _tools.Count == 0)
            {
                _tools = GetDefaultTools();
            }
        }

        public void SaveConfigs()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_tools, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }

        public void AddTool(ExternalToolConfig tool)
        {
            _tools.Add(tool);
            SaveConfigs();
        }

        public void RemoveTool(ExternalToolConfig tool)
        {
            _tools.Remove(tool);
            SaveConfigs();
        }

        public void UpdateTool(int index, ExternalToolConfig tool)
        {
            if (index >= 0 && index < _tools.Count)
            {
                _tools[index] = tool;
                SaveConfigs();
            }
        }

        private List<ExternalToolConfig> GetDefaultTools()
        {
            return new List<ExternalToolConfig>
            {
                new ExternalToolConfig { Name = "AnyDesk", ExecutablePath = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe", Arguments = "", IsDefault = true },
                new ExternalToolConfig { Name = "RustDesk", ExecutablePath = @"C:\Program Files\RustDesk\rustdesk.exe", Arguments = "" },
                new ExternalToolConfig { Name = "Remote Desktop", ExecutablePath = "mstsc.exe", Arguments = "/v:{ip}" }
            };
        }

        public void LaunchTool(string name, string ip, int port)
        {
            var tool = _tools.Find(t => t.Name == name);
            if (tool == null) return;

            string args = tool.Arguments.Replace("{ip}", ip).Replace("{port}", port.ToString());
            try
            {
                Process.Start(tool.ExecutablePath, args);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch " + name + ": " + ex.Message);
            }
        }
    }
}
