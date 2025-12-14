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
        // Constants used for drag support (though we just use Down/Up logic separately for now)
        // private const int MOUSEEVENTF_MOVE = 0x0001; 

        private static WebApplication? _app;
        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static string PublicIpAddress { get; private set; } = "Unknown";
        public static int Port { get; private set; } = 5000;
        public static string CurrentPin { get; private set; } = "0000";
        public static string LastCaptureMode { get; private set; } = "Ready";

        // Shared Data (Thread-safe updated from Form1)
        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static async Task StartServerAsync()
        {
            if (_app != null) return;
            
            // Init DXGI Engine
            try { NativeCapture.InitializeDxgi(); } catch { }

            try
            {
                // 1. Find IP Addresses & Generate PIN
                CurrentIpAddress = GetLocalIpAddress();
                _ = DetectPublicIpAsync(); // Run in background
                // CurrentPin = new Random().Next(1000, 9999).ToString(); 
                CurrentPin = "0000"; // Fixed PIN for testing

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

                    var status = CurrentStatus;
                    var response = new {
                        status.State,
                        status.Incoming,
                        status.Outgoing,
                        status.LocalBlock,
                        status.ProtocolVersion,
                        status.LedgerAge,
                        status.Uptime,
                        captureMode = LastCaptureMode,
                        screenWidth = Screen.PrimaryScreen.Bounds.Width,
                        screenHeight = Screen.PrimaryScreen.Bounds.Height
                    };

                    return Results.Json(response);
                });

                // API: Screen Capture
                _app.MapGet("/api/screen", (HttpContext context) =>
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();
                    
                    // 1. Try DXGI Capture
                    try 
                    {
                        IntPtr buffer = IntPtr.Zero;
                        int size = 0;
                        
                        // Check if DLL is available and function succeeds
                        if (NativeCapture.CaptureScreenToMemory(0, out buffer, out size, 70)) 
                        {
                            try
                            {
                                LastCaptureMode = "DXGI";
                                byte[] data = new byte[size];
                                System.Runtime.InteropServices.Marshal.Copy(buffer, data, 0, size);
                                // Add Header to indicate engine
                                context.Response.Headers["X-Capture-Engine"] = "DXGI";
                                return Results.File(data, "image/jpeg");
                            }
                            finally
                            {
                                NativeCapture.FreeMemory(buffer);
                            }
                        }
                    }
                    catch { /* Ignore DLL errors and proceed to fallback */ }

                    // 2. Fallback to GDI+
                    try
                    {
                        LastCaptureMode = "GDI+";
                        context.Response.Headers["X-Capture-Engine"] = "GDI+"; // Slow engine
                        using (Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                            }
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
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
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                        return Results.Ok("Clicked");
                    }
                    return Results.BadRequest();
                });

                // API: Detailed Mouse Control (For Drag & Drop)
                _app.MapGet("/api/mouse/down", (HttpContext context) => ProcessMouse(context, MOUSEEVENTF_LEFTDOWN, true));
                _app.MapGet("/api/mouse/up", (HttpContext context) => ProcessMouse(context, MOUSEEVENTF_LEFTUP, true));
                _app.MapGet("/api/mouse/move", (HttpContext context) => ProcessMouse(context, 0, false));

                // API: Keyboard Type
                _app.MapGet("/api/type", (HttpContext context) =>
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();
                    
                    string? text = context.Request.Query["text"];
                    if (!string.IsNullOrEmpty(text))
                    {
                        try {
                            // Run on Main UI Thread for safety
                            Application.OpenForms[0]?.Invoke(new Action(() => 
                            {
                                SendKeys.SendWait(text);
                            }));
                            return Results.Ok("Typed");
                        } catch { return Results.Problem("Typing Failed"); }
                    }
                    return Results.BadRequest();
                });

                // API: MJPEG Video Stream (High Performance)
                _app.MapGet("/api/stream", async (HttpContext context) =>
                {
                    string? pin = context.Request.Query["pin"];
                    if (string.IsNullOrEmpty(pin) || pin != CurrentPin) 
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    context.Response.Headers.Append("Cache-Control", "no-cache");
                    context.Response.Headers.ContentType = "multipart/x-mixed-replace; boundary=frame";
                    
                    var boundary = System.Text.Encoding.ASCII.GetBytes("\r\n--frame\r\nContent-Type: image/jpeg\r\n\r\n");
                    
                    // Initial Init for this stream thread
                    try { NativeCapture.InitializeDxgi(); } catch {}

                    try
                    {
                        while (!context.RequestAborted.IsCancellationRequested)
                        {
                            var startTime = DateTime.Now;
                            byte[]? imageBytes = null;

                            // 1. Capture Logic (Selectable)
                            IntPtr buffer = IntPtr.Zero;
                            int size = 0;
                            bool dxgiSuccess = false;
                            
                            string? reqEngine = context.Request.Query["engine"];
                            
                            // Only try DXGI if explicitly requested
                            if (reqEngine == "dxgi")
                            {
                                try 
                                { 
                                    // First try
                                    dxgiSuccess = NativeCapture.CaptureScreenToMemory(0, out buffer, out size, 65); 
                                    if(!dxgiSuccess)
                                    {
                                         NativeCapture.InitializeDxgi(); // Retry init
                                         dxgiSuccess = NativeCapture.CaptureScreenToMemory(0, out buffer, out size, 65);
                                    }
                                } 
                                catch {}
                            }

                            if (dxgiSuccess && size > 0)
                            {
                                LastCaptureMode = "DXGI (High Perf)";
                                imageBytes = new byte[size];
                                Marshal.Copy(buffer, imageBytes, 0, size);
                                NativeCapture.FreeMemory(buffer);
                            }
                            else
                            {
                                // GDI+ Fallback (Safe Mode with Smart Resizing)
                                LastCaptureMode = "GDI+ (Fast)";
                                
                                using (var originalBmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                                {
                                    using (var g = Graphics.FromImage(originalBmp)) g.CopyFromScreen(0, 0, 0, 0, originalBmp.Size);
                                    
                                    // Smart Resizing: Downscale to HD (1280px width) for speed
                                    // This drastically reduces JPEG encoding time and network lag
                                    int targetW = 1280;
                                    int targetH = (int)(originalBmp.Height * ((float)targetW / originalBmp.Width));
                                    
                                    if (originalBmp.Width > targetW)
                                    {
                                        using (var thumb = originalBmp.GetThumbnailImage(targetW, targetH, () => false, IntPtr.Zero))
                                        using (var ms = new MemoryStream())
                                        {
                                            thumb.Save(ms, ImageFormat.Jpeg);
                                            imageBytes = ms.ToArray();
                                        }
                                    }
                                    else
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            originalBmp.Save(ms, ImageFormat.Jpeg);
                                            imageBytes = ms.ToArray();
                                        }
                                    }
                                }
                            }

                            if (imageBytes != null)
                            {
                                // Robust MJPEG Header with Content-Length
                                var header = $"\r\n--frame\r\nContent-Type: image/jpeg\r\nContent-Length: {imageBytes.Length}\r\n\r\n";
                                var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);

                                await context.Response.Body.WriteAsync(headerBytes, 0, headerBytes.Length);
                                await context.Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
                                await context.Response.Body.FlushAsync();
                            }

                            // Adaptive FPS
                            // DXGI is fast (30FPS default), GDI+ (15FPS)
                            int targetFps = LastCaptureMode.Contains("DXGI") ? 30 : 15; 
                            int targetDelay = 1000 / targetFps;

                            var duration = (DateTime.Now - startTime).TotalMilliseconds;
                            var delay = targetDelay - (int)duration;
                            if (delay > 0) await Task.Delay(delay);
                        }
                    }
                    catch { /* Client Disconnected */ }
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

        private static IResult ProcessMouse(HttpContext context, int actionFlag, bool click)
        {
            string? pin = context.Request.Query["pin"];
            if (string.IsNullOrEmpty(pin) || pin != CurrentPin) return Results.Unauthorized();
            
            if (int.TryParse(context.Request.Query["x"], out int x) && 
                int.TryParse(context.Request.Query["y"], out int y))
            {
                // Robust Mouse Control: Use mouse_event for everything (Absolute Coords)
                // This ensures Windows treats moves as actual hardware input (vital for Drag & Drop)
                
                int screenW = Screen.PrimaryScreen.Bounds.Width;
                int screenH = Screen.PrimaryScreen.Bounds.Height;
                
                // Convert pixels to 0..65535 Normalized Coords
                int absX = (int)((x * 65535) / screenW);
                int absY = (int)((y * 65535) / screenH);
                
                // Constants
                const int MOUSEEVENTF_MOVE = 0x0001;
                const int MOUSEEVENTF_ABSOLUTE = 0x8000;
                
                if (click)
                {
                    // For Down/Up: Move to pos AND click (Atomic)
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | actionFlag, absX, absY, 0, 0);
                }
                else
                {
                    // For Move: Just Move
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, absX, absY, 0, 0);
                }

                return Results.Ok();
            }
            return Results.BadRequest();
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
        
        /* Prevent Dragging Image on PC */
        img { -webkit-user-drag: none; user-select: none; -moz-user-select: none; -webkit-user-select: none; -ms-user-select: none; }
        
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
            <div class='metric'>
                <span class='label'>Streaming Info</span>
                <span id='valEngine' class='value' style='color:#8a3ab9'>-</span>
            </div>
            
            <!-- Live View Control -->
            <div style='margin-top:15px; text-align:center;'>
                 <button id='btnLive' class='btn-go' style='padding:10px 30px; font-size:16px;' onclick='toggleLive()'>🔴 Live View</button>
            </div>
            
            <!-- Live Screen Container -->
            <div id='liveContainer' class='card hidden' style='margin-top:20px; padding:10px; background:#000;'>
                <div class='section-title' style='color:#fff; display:flex; justify-content:space-between; align-items:center;'>
                    <span>Desktop Stream</span>
                    <div>
                        <button id='btnEngine' onclick='toggleEngine()' style='font-size:10px; padding:4px 8px; background:#444; color:#fff; border:1px solid #666; width:70px;'>Mode: Safe</button>
                        <span style='font-size:10px; color:#aaa; margin-left:5px;'>Tap to Click</span>
                    </div>
                </div>
                

                <!-- Video Feed Container (Relative for Overlay) -->
                <div id='videoWrapper' style='position:relative; width:100%; min-height:200px; background:#222; border-radius:5px; overflow:hidden;'>
                    
                    <!-- MJPEG Stream Image -->
                    <img id='liveStream' style='width:100%; height:100%; object-fit:contain; display:block;' draggable='false' />

                    <!-- Transparent Input Shield (Absolute Top) -->
                    <!-- Catches all mouse/touch events independently of image refresh -->
                    <div id='inputShield' style='position:absolute; top:0; left:0; width:100%; height:100%; cursor:default; touch-action:none; z-index:10;'
                         oncontextmenu='return false;'>
                    </div>
                </div>
                
                <!-- Remote Actions -->
                <div style='margin-top:15px; display:flex; gap:10px; justify-content:center;'>
                    <input type='text' id='txtType' placeholder='Type here...' style='padding:8px; border-radius:5px; border:1px solid #ccc; width:60%;'>
                    <button onclick='sendText()' style='padding:8px 15px; border-radius:5px; border:none; background:#28a745; color:white; font-weight:bold;'>Send</button>
                </div>
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
                
                // Store resolution
                if(data.screenWidth) {
                    svrW = data.screenWidth;
                    svrH = data.screenHeight;
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

            if (data.captureMode) {
                 document.getElementById('valEngine').innerText = data.captureMode;
            }
        }
        
        let isLive = false;
        let svrW = 1920;
        let svrH = 1080;
        let currentEngine = 'safe'; // 'safe' (GDI+) or 'dxgi'

        function toggleLive() {
            isLive = !isLive;
            const btn = document.getElementById('btnLive');
            const container = document.getElementById('liveContainer');
            const img = document.getElementById('liveStream');
            
            if (isLive) {
                btn.innerText = '⏹ Stop View';
                btn.style.background = '#dc3545'; // Red
                container.classList.remove('hidden');
                refreshStream();
            } else {
                btn.innerText = '🔴 Live View';
                btn.style.background = '#8a3ab9'; // Purple
                container.classList.add('hidden');
                img.src = '';
            }
        }
        
        function toggleEngine() {
            const btn = document.getElementById('btnEngine');
            if(currentEngine === 'safe') {
                currentEngine = 'dxgi';
                btn.innerText = 'Mode: DXGI';
                btn.style.background = '#007bff';
            } else {
                currentEngine = 'safe';
                btn.innerText = 'Mode: Safe';
                btn.style.background = '#444';
            }
            if(isLive) refreshStream();
        }
        
        function refreshStream() {
            const img = document.getElementById('liveStream');
            img.src = '/api/stream?pin=' + userPin + '&engine=' + currentEngine + '&t=' + new Date().getTime();
        }

        // ==========================================
        //  Robust Pointer Logic (Mouse & Touch)
        // ==========================================
        // We use the Shield, not the Image
        const shield = document.getElementById('inputShield'); 
        let isDragging = false;
        let lastMoveTime = 0;

        // PC Mouse Events (Attached to Shield)
        shield.addEventListener('mousedown', (e) => {
            if(!isLive) return;
            e.preventDefault(); 
            isDragging = true;
            processPointer(e.clientX, e.clientY, 'down');
        });

        window.addEventListener('mousemove', (e) => {
            if(!isLive || !isDragging) return;
            const now = Date.now();
            if(now - lastMoveTime < 30) return;
            lastMoveTime = now;
            processPointer(e.clientX, e.clientY, 'move');
        });

        window.addEventListener('mouseup', (e) => {
            if(!isLive || !isDragging) return;
            isDragging = false;
            processPointer(e.clientX, e.clientY, 'up');
        });

        // Mobile Touch Events
        shield.addEventListener('touchstart', (e) => {
            if(!isLive) return;
            e.preventDefault(); 
            const t = e.touches[0];
            processPointer(t.clientX, t.clientY, 'down');
        }, {passive: false});

        shield.addEventListener('touchmove', (e) => {
            if(!isLive) return;
            e.preventDefault(); 
            const t = e.touches[0];
            
            const now = Date.now();
            if(now - lastMoveTime < 30) return;
            lastMoveTime = now;
            
            processPointer(t.clientX, t.clientY, 'move');
        }, {passive: false});

        shield.addEventListener('touchend', (e) => {
            if(!isLive) return;
            const t = e.changedTouches[0]; 
            processPointer(t.clientX, t.clientY, 'up');
        });

        function processPointer(clientX, clientY, action) {
            // Calculate coords based on Shield Rect
            const rect = shield.getBoundingClientRect();
            
            // Calculate relative pos
            let xPct = (clientX - rect.left) / rect.width;
            let yPct = (clientY - rect.top) / rect.height;

            // Clamp to 0.0 - 1.0 (Fix out of bounds issues)
            if(xPct < 0) xPct = 0; if(xPct > 1) xPct = 1;
            if(yPct < 0) yPct = 0; if(yPct > 1) yPct = 1;

            const finalX = Math.round(xPct * svrW);
            const finalY = Math.round(yPct * svrH);

            // Debug Log
            // console.log(action, finalX, finalY);

            let url = '';
            if (action === 'down') url = '/api/mouse/down';
            else if (action === 'up') url = '/api/mouse/up';
            else if (action === 'move') url = '/api/mouse/move';

            if(url) {
                fetch(url + '?pin=' + userPin + '&x=' + finalX + '&y=' + finalY).catch(()=>{});
            }

            if(action === 'down') showTouchFeedback(clientX, clientY);
        }

        async function sendText() {
            const txt = document.getElementById('txtType').value;
            if(!txt) return;
            await fetch('/api/type?pin=' + userPin + '&text=' + encodeURIComponent(txt));
            document.getElementById('txtType').value = '';
        }

        function showTouchFeedback(x, y) {
            const dot = document.createElement('div');
            dot.style.position = 'fixed';
            dot.style.left = (x - 10) + 'px';
            dot.style.top = (y - 10) + 'px';
            dot.style.width = '20px';
            dot.style.height = '20px';
            dot.style.background = 'rgba(255, 255, 0, 0.5)';
            dot.style.borderRadius = '50%';
            dot.style.pointerEvents = 'none';
            document.body.appendChild(dot);
            setTimeout(() => dot.remove(), 300);
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
