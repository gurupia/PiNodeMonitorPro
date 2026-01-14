using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;

namespace PiNodeMonitorWinForm
{
    public class MobileServer
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private static HttpListener _listener;
        private static CancellationTokenSource _cts;
        private static int _screenReqCount = 0;
        private static DateTime _serverStartTime;

        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static string PublicIpAddress { get; private set; } = "Unknown";
        public static int Port { get; private set; } = 5000;
        public static string CurrentPin { get; private set; } = "0000";
        public static string LastCaptureMode { get; private set; } = "Ready";

        static MobileServer()
        {
            // Load port from config
            var portStr = System.Configuration.ConfigurationManager.AppSettings["MobileServer.Port"];
            if (int.TryParse(portStr, out int port)) Port = port;
        }

        public static event Action<string> RequestLogged;
        public static event Action<string> PublicIpDetected;

        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static string LogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static string _currentLogPath;
        private static readonly object _logLock = new object();
        private const int MaxLogSizeMB = 10;
        private const int LogRetentionDays = 7;

        public static void Log(string message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            RequestLogged?.Invoke(message);
            
            try 
            {
                lock (_logLock)
                {
                    EnsureLogDirectory();
                    string logPath = GetCurrentLogPath();
                    File.AppendAllText(logPath, logLine + Environment.NewLine);
                }
            }
            catch { }
        }

        private static void EnsureLogDirectory()
        {
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
                CleanOldLogs();
            }
        }

        private static string GetCurrentLogPath()
        {
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string basePath = Path.Combine(LogDir, $"mobile_{dateStr}.log");
            
            // Check file size, rotate if needed
            if (File.Exists(basePath))
            {
                var info = new FileInfo(basePath);
                if (info.Length > MaxLogSizeMB * 1024 * 1024)
                {
                    string rotatedPath = Path.Combine(LogDir, $"mobile_{dateStr}_{DateTime.Now:HHmmss}.log");
                    File.Move(basePath, rotatedPath);
                }
            }
            return basePath;
        }

        private static void CleanOldLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-LogRetentionDays);
                foreach (var file in Directory.GetFiles(LogDir, "mobile_*.log"))
                {
                    if (File.GetCreationTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch { }
        }


        public static void RegeneratePin()
        {
            CurrentPin = new Random().Next(1000, 9999).ToString();
            Log($"PIN Regenerated: {CurrentPin}");
        }

        public static async Task StartServerAsync()
        {
            if (_listener != null) return;
            
            try 
            {
                _serverStartTime = DateTime.Now;
                Log("Starting Mobile Server...");
                try { NativeCapture.Engine_Initialize(); } catch { }

                CurrentIpAddress = GetLocalIpAddress();
                _ = Task.Run(() => DetectPublicIpAsync()); 
                _ = Task.Run(() => NodeUtility.DetectContainerNameAsync());
                CurrentPin = new Random().Next(1000, 9999).ToString(); 

                _listener = new HttpListener();
                
                // 1. Loopback (Always works without admin)
                _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                _listener.Prefixes.Add($"http://localhost:{Port}/");
                
                // 2. Try to bind to the detected local IP (for LAN access)
                if (CurrentIpAddress != "127.0.0.1")
                {
                    try 
                    { 
                        _listener.Prefixes.Add($"http://{CurrentIpAddress}:{Port}/"); 
                        Log($"Bound to LAN IP: {CurrentIpAddress}");
                    } 
                    catch (Exception ex) 
                    { 
                        Log($"Warning: Could not bind to {CurrentIpAddress}: {ex.Message}"); 
                    }
                }

                _listener.Start();

                _cts = new CancellationTokenSource();
                _ = Task.Run(() => HandleRequests(_cts.Token));
                Log($"Server Started. PIN: {CurrentPin}");
                Log($"Local: http://127.0.0.1:{Port}/?pin={CurrentPin}");
                if (CurrentIpAddress != "127.0.0.1")
                    Log($"LAN: http://{CurrentIpAddress}:{Port}/?pin={CurrentPin}");
            }
            catch (Exception ex)
            {
                Log($"FATAL: Server failed to start: {ex.Message}");
            }
        }

        private static async Task HandleRequests(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context));
                }
                catch { if (token.IsCancellationRequested) break; }
            }
        }

        private static async Task ProcessRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;
            string path = req.Url.AbsolutePath.ToLower();
            string clientIp = req.RemoteEndPoint.ToString();

            try
            {
                // API: Remote Debug Logging (For troubleshooting)
                if (path == "/api/debug")
                {
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        string body = reader.ReadToEnd();
                        Log($"[MOBILE_LOG] {body}");
                    }
                    SendResponse(res, "ok", "text/plain");
                    return;
                }
                
                // API: Health Check (No PIN required - for monitoring)
                if (path == "/api/health")
                {
                    var uptime = DateTime.Now - _serverStartTime;
                    var health = new {
                        status = "ok",
                        server = "MobileServer",
                        version = "1.0",
                        uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m",
                        uptimeSeconds = (int)uptime.TotalSeconds,
                        port = Port,
                        ip = CurrentIpAddress,
                        timestamp = DateTime.Now.ToString("o")
                    };
                    SendResponse(res, JsonConvert.SerializeObject(health), "application/json");
                    return;
                }
                // Log every request for debugging
                if (path == "/api/screen")
                {
                    _screenReqCount++;
                    if (_screenReqCount % 50 == 0) Log($"[{clientIp}] INCOMING: /api/screen (50 frames passed)");
                }
                else
                {
                    Log($"[{clientIp}] INCOMING: {path}");
                }
                // UI: Index
                if (path == "/" || path == "/index.html")
                {
                    SendResponse(res, GetIndexHtml(), "text/html");
                    Log($"[{clientIp}] served index.html");
                }
                // API: Status
                else if (path == "/api/status")
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { 
                        Log($"[{clientIp}] Unauthorized Status Request (PIN: {pin})");
                        res.StatusCode = 401; res.Close(); return; 
                    }

                    var status = CurrentStatus;
                    var json = JsonConvert.SerializeObject(new {
                        status.State,
                        status.Incoming,
                        status.Outgoing,
                        status.LocalBlock,
                        status.ProtocolVersion,
                        status.LedgerAge,
                        status.Uptime,
                        status.NodeBonus,
                        captureMode = LastCaptureMode,
                        screenWidth = Screen.PrimaryScreen.Bounds.Width,
                        screenHeight = Screen.PrimaryScreen.Bounds.Height
                    });
                    SendResponse(res, json, "application/json");
                }
                // API: Screen
                else if (path == "/api/screen")
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { 
                        RequestLogged?.Invoke($"[{clientIp}] Unauthorized Screen Request (PIN: {pin})");
                        res.StatusCode = 401; res.Close(); return; 
                    }

                    byte[] imgData = null;
                    try
                    {
                        ImageBuffer buf = new ImageBuffer();
                        CaptureOptions opts = new CaptureOptions { includeCursor = true, delayMs = 0, engineType = 1 };
                        if (NativeCapture.Capture_FullScreen(ref buf, ref opts) == NativeCapture.GC_OK)
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var bmp = new Bitmap(buf.width, buf.height, buf.stride, PixelFormat.Format32bppArgb, buf.data))
                                {
                                    bmp.Save(ms, ImageFormat.Jpeg);
                                    imgData = ms.ToArray();
                                }
                            }
                            NativeCapture.Image_Free(ref buf);
                            LastCaptureMode = "DXGI";
                        }
                    }
                    catch (Exception ex) { Log($"[{clientIp}] DXGI Error: {ex.Message}"); }

                    if (imgData == null) // GDI Fallback
                    {
                        try 
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                                {
                                    using (var g = Graphics.FromImage(bmp))
                                    {
                                        g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                                    }
                                    bmp.Save(ms, ImageFormat.Jpeg);
                                    imgData = ms.ToArray();
                                }
                            }
                            LastCaptureMode = "GDI";
                        }
                        catch (Exception ex) { Log($"[{clientIp}] GDI Error: {ex.Message}"); }
                    }

                    if (imgData != null)
                    {
                        res.AddHeader("Access-Control-Allow-Origin", "*");
                        res.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                        res.ContentType = "image/jpeg";
                        res.ContentLength64 = imgData.Length;
                        res.OutputStream.Write(imgData, 0, imgData.Length);
                        res.Close();
                    }
                    else
                    {
                        Log($"[{clientIp}] Screen Capture COMPLETELY failed.");
                        res.StatusCode = 503;
                        res.Close();
                    }
                }
                // API: Mouse Control
                else if (path.StartsWith("/api/mouse/"))
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { res.StatusCode = 401; res.Close(); return; }

                    int x = int.Parse(req.QueryString["x"]);
                    int y = int.Parse(req.QueryString["y"]);

                    if (path == "/api/mouse/down")
                    {
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    }
                    else if (path == "/api/mouse/up")
                    {
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                    else if (path == "/api/mouse/move")
                    {
                        SetCursorPos(x, y);
                    }
                    SendResponse(res, "ok", "text/plain");
                }
                // API: Maintenance Actions
                else if (path == "/api/control/action")
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { res.StatusCode = 401; res.Close(); return; }

                    string act = req.QueryString["type"];
                    Log($"[ACTION] Requested: {act}");

                    try 
                    {
                        if (act == "restart_container")
                        {
                            _ = NodeUtility.RunCommandAsync("docker", "restart " + NodeUtility.CurrentContainerName);
                        }
                        else if (act == "restart_docker")
                        {
                            _ = Task.Run(async () => {
                                Log("Terminating Docker Desktop...");
                                await NodeUtility.RunCommandAsync("taskkill", "/F /IM \"Docker Desktop.exe\"");
                                await Task.Delay(2000);
                                Log("Starting Docker Desktop...");
                                Process.Start(@"C:\Program Files\Docker\Docker\Docker Desktop.exe");
                            });
                        }
                        else if (act == "restart_pi")
                        {
                            _ = Task.Run(async () => {
                                Log("Terminating Pi Network...");
                                await NodeUtility.RunCommandAsync("taskkill", "/F /IM \"Pi Network.exe\"");
                                await Task.Delay(2000);
                                Log("Starting Pi Network...");
                                string piPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Pi Network\Pi Network.exe");
                                if (File.Exists(piPath)) Process.Start(piPath);
                                else Log("Pi Network Executable not found at " + piPath);
                            });
                        }
                        SendResponse(res, "success", "text/plain");
                    }
                    catch (Exception ex)
                    {
                        Log($"[ACTION] Failed: {ex.Message}");
                        SendResponse(res, "error: " + ex.Message, "text/plain");
                    }
                    return;
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                Log($"[{clientIp}] Error processing {path}: {ex.Message}");
                try { res.StatusCode = 500; res.Close(); } catch { }
            }
        }

        private static void SendResponse(HttpListenerResponse res, string content, string contentType)
        {
            try 
            {
                byte[] buf = Encoding.UTF8.GetBytes(content);
                res.AddHeader("Access-Control-Allow-Origin", "*");
                res.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                res.ContentType = contentType;
                res.ContentLength64 = buf.Length;
                res.OutputStream.Write(buf, 0, buf.Length);
                res.Close();
            }
            catch { }
        }

        private static string GetLocalIpAddress()
        {
            var ips = GetAllLocalIpAddresses();
            if (ips.Count > 0) return ips[0];
            return "127.0.0.1";
        }

        public static System.Collections.Generic.List<string> GetAllLocalIpAddresses()
        {
            var list = new System.Collections.Generic.List<string>();
            try 
            {
                // Strict validation: Only Operational Interfaces with Gateways
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    // 1. Must be Up
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    
                    // 2. Must not be Loopback
                    if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;

                    // 3. Must have a Gateway (excludes purely internal virtual switches)
                    var props = ni.GetIPProperties();
                    if (props.GatewayAddresses.Count == 0) continue;

                    // 4. Get Unicast Addresses match
                    foreach (var ip in props.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string s = ip.Address.ToString();
                            if (!s.StartsWith("169.254")) // Exclude APIPA (Link-Local)
                            {
                                list.Add(s);
                            }
                        }
                    }
                }

                // Sort: 192.168 -> 10 -> Others
                list.Sort((a, b) => {
                    int scoreA = GetIpScore(a);
                    int scoreB = GetIpScore(b);
                    return scoreB.CompareTo(scoreA);
                });
            } catch { }
            
            // Fallback if strict check fails (e.g. some VPN layouts)
            if (list.Count == 0)
            {
               try {
                  var host = Dns.GetHostEntry(Dns.GetHostName());
                  foreach (var ip in host.AddressList)
                      if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) list.Add(ip.ToString());
               } catch {}
            }

            if (list.Count == 0) list.Add("127.0.0.1");
            return list;
        }

        private static int GetIpScore(string ip)
        {
            // 1. Check for Private Ranges
            bool isPrivate = false;
            if (ip.StartsWith("192.168.")) isPrivate = true;
            else if (ip.StartsWith("10.")) isPrivate = true;
            else if (ip.StartsWith("172.")) 
            {
                // 172.16.0.0 - 172.31.255.255 is private
                var parts = ip.Split('.');
                if (int.TryParse(parts[1], out int second))
                {
                    if (second >= 16 && second <= 31) isPrivate = true;
                }
            }

            // 2. Scoring Logic
            if (!isPrivate) return 200;       // Public IP (Modem Direct) - Highest Priority
            if (ip.StartsWith("192.168.")) return 100; // Standard Home LAN
            if (ip.StartsWith("10.")) return 90;       // Private Class A
            return 10;                         // 172.x / Others (Likely Docker/VM)
        }

        private static async Task DetectPublicIpAsync()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    PublicIpAddress = await client.GetStringAsync("https://api.ipify.org");
                    PublicIpDetected?.Invoke(PublicIpAddress);
                }
            }
            catch { PublicIpAddress = "Failed"; }
        }

        private static string _cachedIndexHtml;
        private static readonly string IndexHtmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mobile_index.html");

        private static string GetIndexHtml()
        {
            // Return cached if available
            if (!string.IsNullOrEmpty(_cachedIndexHtml))
                return _cachedIndexHtml;

            try
            {
                if (File.Exists(IndexHtmlPath))
                {
                    _cachedIndexHtml = File.ReadAllText(IndexHtmlPath, Encoding.UTF8);
                    return _cachedIndexHtml;
                }
            }
            catch (Exception ex)
            {
                Log($"Error loading mobile_index.html: {ex.Message}");
            }

            // Fallback minimal HTML
            return @"<!DOCTYPE html><html><body style='background:#0d1117;color:#fff;font-family:sans-serif;text-align:center;padding:50px;'>
<h1>Pi Node Mobile Control</h1>
<p style='color:#da3633;'>Error: mobile_index.html not found.</p>
<p>Please ensure Resources/mobile_index.html exists.</p>
</body></html>";
        }
    }

    public class NodeStatusData
    {
        public string State { get; set; } = "Unknown";
        public int Incoming { get; set; }
        public int Outgoing { get; set; }
        public string LocalBlock { get; set; } = "0";
        public string ProtocolVersion { get; set; } = "-";
        public int LedgerAge { get; set; }
        public string Uptime { get; set; } = "-"; 
        public double NodeBonus { get; set; }
    }
}
