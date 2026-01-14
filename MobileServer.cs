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

        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static string PublicIpAddress { get; private set; } = "Unknown";
        public const int Port = 5000;
        public static string CurrentPin { get; private set; } = "0000";
        public static string LastCaptureMode { get; private set; } = "Ready";

        public static event Action<string> RequestLogged;

        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mobile_access.log");

        public static void Log(string message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            RequestLogged?.Invoke(message); // Still notify UI
            try 
            {
                File.AppendAllText(LogPath, logLine + Environment.NewLine);
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
                Log("Starting Mobile Server...");
                try { NativeCapture.Engine_Initialize(); } catch { }

                CurrentIpAddress = GetLocalIpAddress();
                _ = Task.Run(() => DetectPublicIpAsync()); 
                _ = Task.Run(() => NodeUtility.DetectContainerNameAsync());
                CurrentPin = new Random().Next(1000, 9999).ToString(); 

                _listener = new HttpListener();
                
                // 1. Loopback (No admin required)
                _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                
                // 2. Local IP (Requires admin or netsh acl)
                try {
                    _listener.Prefixes.Add($"http://{CurrentIpAddress}:{Port}/");
                } catch { }

                _listener.Start();

                _cts = new CancellationTokenSource();
                _ = Task.Run(() => HandleRequests(_cts.Token));
                Log($"Server Started. PIN: {CurrentPin}");
                Log($"Local: http://127.0.0.1:{Port}/?pin={CurrentPin}");
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
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string bestMatch = "127.0.0.1";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string s = ip.ToString();
                    // Prefer 192.168 (Home) or 10. (Private)
                    if (s.StartsWith("192.168.") || s.StartsWith("10.")) return s;
                    // Fallback to any valid IP (excluding localhost loopback which we cover anyway)
                    bestMatch = s;
                }
            }
            return bestMatch;
        }

        public static System.Collections.Generic.List<string> GetAllLocalIpAddresses()
        {
            var list = new System.Collections.Generic.List<string>();
            try 
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string s = ip.ToString();
                        if (s != "127.0.0.1") list.Add(s);
                    }
                }
            } catch { }
            if (list.Count == 0) list.Add("127.0.0.1");
            return list;
        }

        private static async Task DetectPublicIpAsync()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    PublicIpAddress = await client.GetStringAsync("https://api.ipify.org");
                }
            }
            catch { PublicIpAddress = "Failed"; }
        }

        private static string GetIndexHtml()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <title>Pi Node Mobile Control</title>
    <style>
        body { margin: 0; background: #0d1117; color: #c9d1d9; font-family: -apple-system, system-ui, sans-serif; overflow-x: hidden; }
        #header { height: 60px; background: #161b22; display: flex; align-items: center; padding: 0 20px; border-bottom: 1px solid #30363d; position: sticky; top: 0; z-index: 100; }
        .logo { color: #f2a900; font-weight: bold; font-size: 1.2rem; display: flex; align-items: center; }
        .logo span { background: #f2a900; color: #000; padding: 2px 6px; border-radius: 4px; margin-right: 8px; font-size: 0.9rem; }
        
        .container { padding: 20px; max-width: 600px; margin: 0 auto; padding-bottom: 100px; }
        .card { background: #161b22; border: 1px solid #30363d; border-radius: 12px; padding: 20px; margin-bottom: 20px; }
        .card-title { font-size: 0.9rem; color: #8b949e; margin-bottom: 15px; font-weight: 500; text-transform: uppercase; letter-spacing: 0.5px; }
        
        .status-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
        .status-item { padding: 15px; background: #0d1117; border-radius: 10px; border: 1px solid #30363d; }
        .status-label { font-size: 0.75rem; color: #8b949e; margin-bottom: 5px; }
        .status-value { font-size: 1.1rem; font-weight: 600; color: #58a6ff; }
        .status-value.success { color: #3fb950; }
        
        .action-btn { width: 100%; padding: 16px; margin-bottom: 12px; border-radius: 10px; border: none; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: transform 0.1s; -webkit-tap-highlight-color: transparent; }
        .action-btn:active { transform: scale(0.97); }
        .action-btn.primary { background: #238636; color: #fff; }
        .action-btn.secondary { background: #21262d; color: #c9d1d9; border: 1px solid #30363d; }
        .action-btn.danger { background: #da3633; color: #fff; }
        .action-btn i { margin-right: 10px; font-style: normal; }

        #remote-view { width: 100%; border-radius: 8px; background: #000; cursor: pointer; border: 2px solid #30363d; margin-top: 10px; }
        
        #pin-overlay { position: fixed; inset: 0; background: #0d1117; z-index: 2000; display: flex; flex-direction: column; align-items: center; justify-content: center; }
        #pin-input { background: #161b22; border: 1px solid #30363d; color: #fff; padding: 20px; font-size: 2rem; width: 160px; text-align: center; border-radius: 12px; letter-spacing: 8px; margin-bottom: 30px; }
        
        #tabs { position: fixed; bottom: 0; width: 100%; height: 70px; background: #161b22; border-top: 1px solid #30363d; display: flex; z-index: 1000; }
        .tab { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; font-size: 0.75rem; color: #8b949e; }
        .tab.active { color: #f2a900; }
        .tab-icon { font-size: 1.4rem; margin-bottom: 4px; }
        
        #remote-full-ui { position: fixed; inset: 0; background: #000; z-index: 1500; display: none; flex-direction: column; }
        #remote-loading { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; background: rgba(0,0,0,0.5); z-index: 200; color: #fff; font-size: 1.2rem; }
        #debug-console { position: absolute; top: 60px; left: 10px; right: 10px; background: rgba(0,0,0,0.6); color: #0f0; font-family: monospace; font-size: 10px; padding: 5px; pointer-events: none; z-index: 300; border: 1px solid #0f0; border-radius: 4px; display: block; }
    </style>
</head>
<body>
    <div id='pin-overlay' style='display:none;'>
        <div class='logo' style='margin-bottom:40px; font-size:1.5rem;'><span>PI</span> Sentinel</div>
        <input type='password' id='pin-input' maxlength='4' placeholder='0000' inputmode='numeric'>
        <button class='action-btn primary' style='width:160px;' onclick='verifyPin()'>UNLOCK</button>
    </div>

    <div id='dashboard-ui'>
        <div id='header'>
            <div class='logo'><span>PI</span> Sentinel</div>
            <div style='margin-left:auto; display:flex; gap:10px; align-items:center;'>
                <div style='font-size:0.8rem; background:#30363d; padding:4px 10px; border-radius:20px;' id='pin-display'>PIN: --</div>
                <button onclick='logout()' style='background:#da3633; border:none; color:#fff; padding:4px 8px; border-radius:4px; font-size:0.7rem;'>RESET</button>
            </div>
        </div>

        <div class='container' id='home-tab'>
            <div class='card'>
                <div class='card-title'>NODE STATUS</div>
                <div class='status-grid'>
                    <div class='status-item'>
                        <div class='status-label'>State</div>
                        <div class='status-value' id='val-state'>---</div>
                    </div>
                    <div class='status-item'>
                        <div class='status-label'>Age</div>
                        <div class='status-value' id='val-age'>---</div>
                    </div>
                    <div class='status-item'>
                        <div class='status-label'>Incoming</div>
                        <div class='status-value' id='val-in'>---</div>
                    </div>
                    <div class='status-item'>
                        <div class='status-label'>Outgoing</div>
                        <div class='status-value' id='val-out'>---</div>
                    </div>
                </div>
            </div>

            <div class='card'>
                <div class='card-title'>QUICK ACTIONS</div>
                <button class='action-btn primary' onclick='runAction(""restart_container"")'><i>🔄</i> Restart Pi Container</button>
                <button class='action-btn secondary' onclick='runAction(""restart_docker"")'><i>🐋</i> Restart Docker Desktop</button>
                <button class='action-btn secondary' onclick='runAction(""restart_pi"")'><i>🥧</i> Restart Pi App</button>
            </div>

            <div class='card'>
                <div class='card-title'>LIVE MONITOR (Tap to Remote)</div>
                <img id='remote-thumb' src='' onclick='switchTab(""remote"")' style='width:100%; border-radius:8px;'>
            </div>
        </div>

        <div id='remote-full-ui'>
            <div style='height:50px; background:#161b22; display:flex; align-items:center; padding:0 15px;'>
                <button onclick='switchTab(""home"")' style='background:none; border:1px solid #30363d; color:#fff; padding:5px 12px; border-radius:6px;'>&larr; Back</button>
                <span style='margin-left:15px; font-size:0.9rem; font-weight:bold;'>Remote Control</span>
            </div>
            <div style='flex:1; position:relative; overflow:hidden; display:flex; align-items:center; justify-content:center;'>
                <div id='debug-console'>Debug Console Ready...</div>
                <div id='remote-loading' style='display:none;'>Loading Stream...</div>
                <img id='screen-img' draggable='false' style='width:100%; height:100%; object-fit:contain; background:#111;'>
                <div id='shield' style='position:absolute; inset:0; z-index:100;'></div>
            </div>
            <div id='status-bar' style='height:30px; background:#161b22; font-size:0.7rem; display:flex; align-items:center; padding:0 15px;'>---</div>
        </div>

        <div id='tabs'>
            <div class='tab active' id='tab-home' onclick='switchTab(""home"")'>
                <div class='tab-icon'>🏠</div>
                <div>Dashboard</div>
            </div>
            <div class='tab' id='tab-remote' onclick='switchTab(""remote"")'>
                <div class='tab-icon'>📺</div>
                <div>Remote</div>
            </div>
        </div>
    </div>

    <script>
        const LOG_LIMIT = 5;
        let lastLogs = [];
        function remoteLog(msg) {
            console.log(msg);
            lastLogs.push('[' + new Date().toLocaleTimeString() + '] ' + msg);
            if(lastLogs.length > LOG_LIMIT) lastLogs.shift();
            const debugPanel = document.getElementById('debug-console');
            if(debugPanel) debugPanel.innerHTML = lastLogs.join('<br>');
            fetch('/api/debug', { method: 'POST', body: msg }).catch(() => {});
        }

        let userPin = new URLSearchParams(window.location.search).get('pin') || localStorage.getItem('sentinel_pin') || '';
        let isRemoteActive = false;
        let isLoopRunning = false;
        let svrW = 1920, svrH = 1080;

        function logout() {
            remoteLog('Session RESET requested.');
            userPin = '';
            isRemoteActive = false;
            localStorage.removeItem('sentinel_pin');
            location.href = window.location.pathname; 
        }

        if (!userPin) {
            document.getElementById('pin-overlay').style.display = 'flex';
            document.getElementById('dashboard-ui').style.display = 'none';
        } else {
            startSession();
        }

        function verifyPin() {
            const input = document.getElementById('pin-input').value;
            if (input.length === 4) {
                userPin = input;
                localStorage.setItem('sentinel_pin', input);
                location.reload(); 
            }
        }

        function switchTab(t) {
            const isRemote = t === 'remote';
            document.getElementById('home-tab').style.display = isRemote ? 'none' : 'block';
            document.getElementById('remote-full-ui').style.display = isRemote ? 'flex' : 'none';
            document.getElementById('tab-home').className = isRemote ? 'tab' : 'tab active';
            document.getElementById('tab-remote').className = isRemote ? 'tab active' : 'tab';
            
            isRemoteActive = isRemote;
            if (isRemote) {
                remoteLog('Switched to REMOTE. Starting BLOB loop.');
                loadNextImage();
            } else {
                remoteLog('Switched to HOME.');
            }
        }

        function runAction(type) {
            if(!confirm('Action: ' + type + '\n실행하시겠습니까?')) return;
            fetch('/api/control/action?pin=' + userPin + '&type=' + type)
                .then(r => {
                    if(r.status === 401) { logout(); return; }
                    return r.text();
                })
                .then(t => { if(t) alert('Success: ' + t); })
                .catch(e => alert('Error: ' + e.message));
        }

        function startSession() {
            remoteLog('Script initialized. PIN: ' + userPin);
            document.getElementById('pin-display').innerText = 'PIN: ' + userPin;
            updateStatus();
            loadNextThumb();
            setInterval(updateStatus, 5000);
            setInterval(loadNextThumb, 3000);
            remoteLog('Session started. PIN in use.');
        }

        function updateStatus() {
            if(!userPin) return;
            fetch('/api/status?pin=' + userPin)
                .then(r => {
                    if (r.status === 401) { logout(); throw new Error('401'); }
                    return r.json();
                })
                .then(d => {
                    svrW = d.screenWidth; svrH = d.screenHeight;
                    document.getElementById('val-state').innerText = d.State;
                    document.getElementById('val-age').innerText = d.LedgerAge + 's';
                    document.getElementById('val-in').innerText = d.Incoming;
                    document.getElementById('val-out').innerText = d.Outgoing;
                    document.getElementById('status-bar').innerText = 'D:' + d.State + ' | S:' + svrW + 'x' + svrH + ' | P:' + userPin;
                    
                    const stateEl = document.getElementById('val-state');
                    stateEl.className = d.State === 'Synced' ? 'status-value success' : 'status-value';
                }).catch(()=>{});
        }

        function loadNextThumb() {
            if (isRemoteActive || !userPin) return;
            fetch('/api/screen?pin=' + userPin + '&t=' + Date.now())
                .then(r => r.blob())
                .then(blob => {
                    const url = URL.createObjectURL(blob);
                    document.getElementById('remote-thumb').src = url;
                }).catch(()=>{});
        }

        function loadNextImage() {
            if (!isRemoteActive || !userPin || isLoopRunning) return;
            isLoopRunning = true;
            
            const screenImg = document.getElementById('screen-img');
            const loading = document.getElementById('remote-loading');
            
            const innerLoop = () => {
                if (!isRemoteActive || !userPin) { isLoopRunning = false; return; }
                
                fetch('/api/screen?pin=' + userPin + '&t=' + Date.now())
                    .then(r => {
                        if(r.status === 401) { logout(); throw new Error('401'); }
                        return r.blob();
                    })
                    .then(blob => {
                        const url = URL.createObjectURL(blob);
                        const oldUrl = screenImg.src;
                        screenImg.src = url;
                        if(oldUrl && oldUrl.startsWith('blob:')) URL.revokeObjectURL(oldUrl);
                        loading.style.display = 'none';
                        setTimeout(innerLoop, 50);
                    })
                    .catch(e => {
                        remoteLog('Stream Fetch Error: ' + e.message);
                        loading.style.display = 'flex';
                        setTimeout(innerLoop, 1000);
                    });
            };
            innerLoop();
        }

        const shield = document.getElementById('shield');
        function handleControl(e, act) {
            if (!isRemoteActive || !userPin) return;
            e.preventDefault();
            const rect = shield.getBoundingClientRect();
            let clientX, clientY;
            if (e.touches && e.touches.length > 0) {
                clientX = e.touches[0].clientX; clientY = e.touches[0].clientY;
            } else {
                clientX = e.clientX; clientY = e.clientY;
            }
            const x = Math.round(((clientX - rect.left) / rect.width) * svrW);
            const y = Math.round(((clientY - rect.top) / rect.height) * svrH);
            fetch('/api/mouse/' + act + '?pin=' + userPin + '&x=' + x + '&y=' + y).catch(()=>{});
        }

        shield.addEventListener('mousedown', e => handleControl(e, 'down'));
        shield.addEventListener('mouseup', e => handleControl(e, 'up'));
        shield.addEventListener('mousemove', e => { if(e.buttons > 0) handleControl(e, 'move'); });
        shield.addEventListener('touchstart', e => handleControl(e, 'down'));
        shield.addEventListener('touchend', e => handleControl(e, 'up'));
        shield.addEventListener('touchmove', e => handleControl(e, 'move'));
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
        public double NodeBonus { get; set; }
    }
}
