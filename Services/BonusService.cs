using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PiNodeMonitorWinForm.Services
{
    public class BonusService
    {
        private string _uuid;
        private readonly string _csvPath;
        private readonly HttpClient _httpClient;
        private readonly string _debugLogPath;

        public BonusService(string uuid)
        {
            _uuid = uuid;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            
            _csvPath = Path.Combine(dataDir, "Bonus_History.csv");
            _debugLogPath = Path.Combine(dataDir, "Debug.log");

            if (!File.Exists(_csvPath))
            {
                File.WriteAllText(_csvPath, "Timestamp,NodeBonus,Availability,PortsCheck\n");
            }
            
            LogDebug($"BonusService Initialized with UUID: {_uuid}");
        }

        public void LogDebug(string message)
        {
            try
            {
                string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                File.AppendAllText(_debugLogPath, logLine);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BonusService] LogDebug write error: {ex.Message}"); }
        }

        public async Task<NodeInfoResult> GetNodeInfoAsync()
        {
            var result = new NodeInfoResult();
            if (string.IsNullOrEmpty(_uuid)) 
            {
                LogDebug("GetNodeInfoAsync failed: UUID is null or empty.");
                return result;
            }

            // Ensure TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string[] urls = {
                string.Format("https://node-automation.minepi.com/api/node-info?uuid={0}", _uuid),
                string.Format("https://node-stats.pinet.com/api/v1/stats?uuid={0}", _uuid),
                string.Format("https://minepi.com/api/node-info?uuid={0}", _uuid),
                string.Format("https://socialchain.app/api/node_info?uuid={0}", _uuid)
            };

            foreach (var url in urls)
            {
                try
                {
                    LogDebug($"Attempting fetch from: {url}");
                    var response = await _httpClient.GetStringAsync(url);
                    LogDebug($"Response received (Length: {response.Length})");
                    
                    var json = JToken.Parse(response);
                    if (json != null)
                    {
                        SearchJson(json, result);
                        
                        if (result.Bonus > 0) 
                        {
                            LogDebug("Bonus data successfully parsed.");
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDebug($"Fetch error for {url}: {ex.Message}");
                }
            }

            LogDebug($"GetNodeInfoAsync finished. Bonus found: {result.Bonus}");
            return result;
        }

        private void SearchJson(JToken token, NodeInfoResult result)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    string key = property.Name.ToLower();
                    if ((key == "node_bonus" || key == "bonus" || key == "nodebonus") && result.Bonus == 0)
                    {
                        result.Bonus = property.Value.Value<double>();
                        if (result.Bonus > 0) LogDebug($"Found Bonus: {result.Bonus} (Key: {property.Name})");
                    }
                    else if ((key == "cpu_count" || key == "cpus" || key == "cores" || key == "cpu_cores") && result.CpuCount == 0)
                    {
                        result.CpuCount = property.Value.Value<int>();
                    }
                    else if ((key == "total_ram" || key == "total_memory" || key == "memory" || key == "ram") && result.TotalRam == 0)
                    {
                        result.TotalRam = property.Value.Value<long>();
                    }
                    
                    SearchJson(property.Value, result);
                }
            }
            else if (token is JArray arr)
            {
                foreach (var item in arr) SearchJson(item, result);
            }
        }

        public void SetUuid(string uuid)
        {
            _uuid = uuid;
            LogDebug($"UUID Updated: {_uuid}");
        }

        public async Task<double> GetCurrentBonusAsync()
        {
            var res = await GetNodeInfoAsync();
            return res.Bonus;
        }

        public void RecordBonus(double bonus, string availability, bool portsOk)
        {
            try
            {
                if (bonus < 0) return;
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{bonus:F4},{availability},{portsOk}\n";
                File.AppendAllText(_csvPath, line);
            }
            catch (Exception ex) { LogDebug($"RecordBonus error: {ex.Message}"); }
        }
    }

    public class NodeInfoResult
    {
        public double Bonus { get; set; } = 0;
        public int CpuCount { get; set; } = 0;
        public long TotalRam { get; set; } = 0;
    }
}
