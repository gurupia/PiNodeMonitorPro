#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PiNodeMonitorWinForm
{
    public class MobileServer
    {
        // Win32 API for Mouse Control
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private static WebApplication? _app;
        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static string PublicIpAddress { get; private set; } = "Unknown";
        public static int Port { get; private set; } = 5000;
        public static string CurrentPin { get; private set; } = "0000";

        // Shared Data (Thread-safe updated from Form1)
        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static async Task StartServerAsync()
        {
            if (_app != null) return;

            try
            {
                // 1. Find IP Addresses & Generate PIN
                CurrentIpAddress = GetLocalIpAddress();
                _ = DetectPublicIpAsync(); // Run in background
                CurrentPin = new Random().Next(1000, 9999).ToString(); 

                var builder = WebApplication.CreateBuilder();
                
                // Configure Kestrel to listen on all interfaces
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(Port);
                });

                builder.Services.AddCors(); 

                _app = builder.Build();

                // 2. Define Routes
                
                // API: Get Status JSON (Protected by PIN)
                _app.MapGet("/api/status", (HttpContext context) => 
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();

                    return Results.Json(CurrentStatus);
                });

                // API: Screen Capture
                _app.MapGet("/api/screen", (HttpContext context) =>
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();

                    try
                    {
                        var bounds = Screen.PrimaryScreen.Bounds;
                        using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
                        {
                             using (Graphics g = Graphics.FromImage(bmp))
                             {
                                 g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                             }
                             using (var ms = new MemoryStream())
                             {
                                 bmp.Save(ms, ImageFormat.Jpeg);
                                 return Results.File(ms.ToArray(), "image/jpeg");
                             }
                        }
                    }
                    catch { return Results.Problem("Capture Failed"); }
                });

                // API: Mouse Click
                _app.MapGet("/api/click", (HttpContext context) => 
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();
                    
                    if (int.TryParse(context.Request.Query["x"], out int x) && 
                        int.TryParse(context.Request.Query["y"], out int y))
                    {
                        // Scaling might be needed if resolution differs, but assuming 1:1 for now
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                        return Results.Ok("Clicked");
                    }
                    return Results.BadRequest();
                });

                // Page: Main Dashboard (HTML with Login)
                _app.MapGet("/", async (HttpContext context) =>
                {
                    string html = GetDashboardHtml();
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html);
                });

                await _app.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Server Error: {ex.Message}");
            }
        }

        public static async Task StopServerAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
                _app = null;
            }
        }

        private static string GetLocalIpAddress()
        {
            try 
            {
                // Robust way to find the interface connected to the internet
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
            }
            catch 
            {
                // Fallback
                return "127.0.0.1";
            }
        }

        private static async Task DetectPublicIpAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    PublicIpAddress = await client.GetStringAsync("https://api.ipify.org");
                }
            }
            catch 
            {
                PublicIpAddress = "Error";
            }
        }

        // Simple Mobile Dashboard HTML
        private static string GetDashboardHtml()
        {
            return @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Pi Node Monitor</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f0f2f5; margin: 0; padding: 20px; text-align: center; }
        .card { background: white; border-radius: 15px; padding: 20px; margin-bottom: 20px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        h1 { color: #8a3ab9; margin-bottom: 5px; }
        .status { font-size: 24px; font-weight: bold; margin: 10px 0; }
        .status.ok { color: #28a745; }
        .status.warn { color: #ffc107; }
        .status.err { color: #dc3545; }
        .metric { display: flex; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid #eee; }
        .metric:last-child { border-bottom: none; }
        .label { color: #666; font-size: 14px; }
        .value { font-weight: bold; color: #333; font-size: 15px; }
        .section-title { text-align: left; font-size: 12px; font-weight: bold; color: #888; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 1px; }
        
        /* Login Modal */
        #loginOverlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: #f0f2f5; z-index: 100; display: flex; flex-direction: column; justify-content: center; align-items: center; }
        .pin-input { font-size: 24px; padding: 10px; text-align: center; letter-spacing: 5px; width: 150px; border: 2px solid #ddd; border-radius: 10px; margin-bottom: 20px; }
        .btn-go { background: #8a3ab9; color: white; padding: 12px 40px; border-radius: 25px; border: none; font-size: 18px; font-weight: bold; cursor: pointer; }
        .hidden { display: none !important; }
    </style>
</head>
<body>
    <!-- Login Screen -->
    <div id='loginOverlay'>
        <h2 style='color:#333;'>Security Check</h2>
        <p>Enter 4-digit PIN found on PC App</p>
        <input type='tel' id='pinInput' class='pin-input' maxlength='4' placeholder='****'>
        <button class='btn-go' onclick='tryLogin()'>Access</button>
        <p id='loginMsg' style='color:red; margin-top:10px;'></p>
    </div>

    <!-- Dashboard -->
    <div id='dashboard' class='hidden'>
        <h1>Pi Node Monitor</h1>
        <p style='color:#666; font-size:12px;'>Real-time Dashboard</p>

        <!-- Main Status Card -->
        <div class='card'>
            <div class='label'>Current Status</div>
            <div id='stateText' class='status'>Loading...</div>
            <div id='blockInfo' style='font-size:14px; color:#555;'></div>
        </div>

        <!-- Network & Block Info -->
        <div class='card'>
            <div class='section-title'>Network</div>
            <div class='metric'>
                <span class='label'>Outgoing (Target: 8)</span>
                <span id='valOut' class='value' style='color:#007bff'>-</span>
            </div>
            <div class='metric'>
                <span class='label'>Incoming (Peers)</span>
                <span id='valIn' class='value'>-</span>
            </div>
            <div class='metric'>
                <span class='label'>Local Block</span>
                <span id='valLedger' class='value'>-</span>
            </div>
            <div class='metric'>
                <span class='label'>Ledger Age</span>
                <span id='valAge' class='value'>-</span>
            </div>
        </div>

        <!-- System & Consensus Info -->
        <div class='card'>
            <div class='section-title'>System & Consensus</div>
            <div class='metric'>
                <span class='label'>Protocol Ver</span>
                <span id='valProto' class='value'>-</span>
            </div>
            <div class='metric'>
                <span class='label'>Container Uptime</span>
                <span id='valUptime' class='value'>-</span>
            </div>
             <div class='metric'>
                <span class='label'>Latest Consensus</span>
                <span class='value' style='color:#28a745'>Synced</span>
            </div>
        </div>
        
        <div style='color:#aaa; font-size:11px; margin-top:30px;'>
            Auto-refreshes every 3 seconds<br>
            Powered by Gurupia
        </div>
    </div>

    <script>
        let userPin = '';

        function tryLogin() {
            const input = document.getElementById('pinInput').value;
            if (input.length !== 4) {
                document.getElementById('loginMsg').innerText = 'Enter 4 digits';
                return;
            }
            userPin = input;
            checkStatus();
        }

        async function checkStatus() {
            try {
                const response = await fetch('/api/status?pin=' + userPin);
                if (response.status === 401) {
                    document.getElementById('loginMsg').innerText = 'Incorrect PIN';
                    return;
                }
                if (!response.ok) throw new Error('Error');
                
                const data = await response.json();
                
                document.getElementById('loginOverlay').classList.add('hidden');
                document.getElementById('dashboard').classList.remove('hidden');
                
                updateUI(data);
                
                if (!window.loopStarted) {
                    window.loopStarted = true;
                    setInterval(checkStatus, 3000);
                }

            } catch (err) {
                 if(document.getElementById('dashboard').classList.contains('hidden')) {
                     document.getElementById('loginMsg').innerText = 'Connection Failed';
                 } else {
                     document.getElementById('stateText').innerText = 'Disconnected';
                     document.getElementById('stateText').className = 'status err';
                 }
            }
        }
        
        function updateUI(data) {
            const stateEl = document.getElementById('stateText');
            stateEl.innerText = data.state;
            
            // Outgoing >= 8 is GOOD
            const isGood = (data.state === 'Synced!' && data.outgoing >= 8);
            const isWarn = (!isGood && (data.state === 'Synced!' || data.outgoing > 0));
            
            if (isGood) stateEl.className = 'status ok';
            else if (isWarn) stateEl.className = 'status warn';
            else stateEl.className = 'status err';
            
            document.getElementById('valIn').innerText = data.incoming;
            document.getElementById('valOut').innerText = data.outgoing;
            document.getElementById('valOut').style.color = (data.outgoing >= 8) ? '#28a745' : '#dc3545';
            
            document.getElementById('valProto').innerText = data.protocolVersion;
            document.getElementById('valLedger').innerText = data.localBlock;
            document.getElementById('valAge').innerText = data.ledgerAge + 's';
            
            document.getElementById('valUptime').innerText = data.uptime || '-';
            document.getElementById('blockInfo').innerText = 'Block: ' + data.localBlock;
        }
    </script>
</body>
</html>";
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
    }
}
