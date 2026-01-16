using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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
                        // 1. Try DTO Parsing
                        var dto = JsonConvert.DeserializeObject<PiNodeInfoDTO>(response);
                        ExtractFromDto(dto, result);

                        // 2. Fallback to Recursive Search if data still missing
                        if (result.Bonus == 0)
                        {
                            SearchJson(json, result);
                        }
                        
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

        private void ExtractFromDto(PiNodeInfoDTO dto, NodeInfoResult result)
        {
            if (dto == null) return;

            if (result.Bonus == 0) result.Bonus = dto.NodeBonus ?? dto.Bonus ?? 0;
            if (result.CpuCount == 0) result.CpuCount = dto.CpuCount ?? 0;
            if (result.TotalRam == 0) result.TotalRam = dto.TotalRam ?? 0;

            // Check nested data
            if (dto.Data != null) ExtractFromDto(dto.Data, result);
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
                        try { result.Bonus = property.Value.Value<double>(); } catch { }
                        if (result.Bonus > 0) LogDebug($"Found Bonus: {result.Bonus} (Key: {property.Name})");
                    }
                    else if ((key == "cpu_count" || key == "cpus" || key == "cores" || key == "cpu_cores") && result.CpuCount == 0)
                    {
                        try { result.CpuCount = property.Value.Value<int>(); } catch { }
                    }
                    else if ((key == "total_ram" || key == "total_memory" || key == "memory" || key == "ram") && result.TotalRam == 0)
                    {
                        try { result.TotalRam = property.Value.Value<long>(); } catch { }
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

        public string GetBonusTrendReport()
        {
            try
            {
                if (!File.Exists(_csvPath)) return "No data available.";

                var lines = File.ReadAllLines(_csvPath).Skip(1).ToList();
                if (lines.Count == 0) return "No history recorded yet.";

                var now = DateTime.Now;
                var last24h = new System.Collections.Generic.List<double>();
                var last7d = new System.Collections.Generic.List<double>();

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length < 2) continue;

                    if (DateTime.TryParse(parts[0], out DateTime ts) && double.TryParse(parts[1], out double val))
                    {
                        if (ts > now.AddHours(-24)) last24h.Add(val);
                        if (ts > now.AddDays(-7)) last7d.Add(val);
                    }
                }

                double avg24h = last24h.Count > 0 ? last24h.Average() : 0;
                double avg7d = last7d.Count > 0 ? last7d.Average() : 0;
                double current = last24h.Count > 0 ? last24h.Last() : 0;

                string trend = "Steady";
                if (current > avg24h * 1.1) trend = "Rising ↑";
                else if (current < avg24h * 0.9 && avg24h > 0) trend = "Falling ↓";

                return $"[Trend] {trend}\n- Current: {current:F4}\n- 24h Avg: {avg24h:F4}\n- 7d Avg: {avg7d:F4}\n- Data Points: {last24h.Count} (24h) / {last7d.Count} (7d)";
            }
            catch (Exception ex) { return "Error analyzing trend: " + ex.Message; }
        }
    }

    public class NodeInfoResult
    {
        public double Bonus { get; set; } = 0;
        public int CpuCount { get; set; } = 0;
        public long TotalRam { get; set; } = 0;
    }

    public class PiNodeInfoDTO
    {
        [JsonProperty("node_bonus")]
        public double? NodeBonus { get; set; }

        [JsonProperty("bonus")]
        public double? Bonus { get; set; }

        [JsonProperty("cpu_count")]
        public int? CpuCount { get; set; }

        [JsonProperty("total_ram")]
        public long? TotalRam { get; set; }

        [JsonProperty("data")]
        public PiNodeInfoDTO Data { get; set; } // For nested responses
    }
}
