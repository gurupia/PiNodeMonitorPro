using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace PiNodeMonitorWinForm
{
    public class MobileServer
    {
        private static WebApplication? _app;
        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static int Port { get; private set; } = 5000;
        public static string CurrentPin { get; private set; } = "0000";

        // Shared Data (Thread-safe updated from Form1)
        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static async Task StartServerAsync()
        {
            if (_app != null) return;

            try
            {
                // 1. Find Local IP & Generate PIN
                CurrentIpAddress = GetLocalIpAddress();
                CurrentPin = new Random().Next(1000, 9999).ToString(); // 4-digit PIN

                var builder = WebApplication.CreateBuilder();
                
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
                    string pin = context.Request.Query["pin"].ToString();
                    if (pin != CurrentPin) return Results.Unauthorized();

                    return Results.Json(CurrentStatus);
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
                _app = null;
            }
        }

        private static string GetLocalIpAddress()
        {
            try 
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch {}
            return "127.0.0.1";
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
        .metric { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }
        .metric:last-child { border-bottom: none; }
        .label { color: #666; }
        .value { font-weight: bold; color: #333; }
        
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

        <div class='card'>
            <div class='label'>Node Status</div>
            <div id='stateText' class='status'>Loading...</div>
            <div id='blockInfo' style='font-size:14px; color:#555;'></div>
        </div>

        <div class='card'>
            <div class='metric'><span class='label'>Incoming (Peers)</span><span id='valIn' class='value'>-</span></div>
            <div class='metric'><span class='label'>Outgoing (Peers)</span><span id='valOut' class='value'>-</span></div>
            <div class='metric'><span class='label'>Protocol Ver</span><span id='valProto' class='value'>-</span></div>
            <div class='metric'><span class='label'>Latest Block</span><span id='valLedger' class='value'>-</span></div>
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
            // Verify by first fetch
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
                
                // Login Success
                document.getElementById('loginOverlay').classList.add('hidden');
                document.getElementById('dashboard').classList.remove('hidden');
                
                updateUI(data);
                
                // Start Loop
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
            stateEl.className = 'status ' + (data.state === 'Synced!' ? 'ok' : 'warn');
            
            document.getElementById('valIn').innerText = data.incoming;
            document.getElementById('valOut').innerText = data.outgoing;
            document.getElementById('valProto').innerText = data.protocolVersion;
            document.getElementById('valLedger').innerText = data.localBlock;
            document.getElementById('blockInfo').innerText = 'Ledger Age: ' + data.ledgerAge + 's';
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
    }
}
