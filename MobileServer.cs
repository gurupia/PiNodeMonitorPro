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

        public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
        public static string PublicIpAddress { get; private set; } = "Unknown";
        public static int Port { get; private set; } = 5000;
        public static string CurrentPin { get; private set; } = "0000";
        public static string LastCaptureMode { get; private set; } = "Ready";

        public static NodeStatusData CurrentStatus { get; set; } = new NodeStatusData();

        public static async Task StartServerAsync()
        {
            if (_listener != null) return;
            
            try { NativeCapture.Engine_Initialize(); } catch { }

            CurrentIpAddress = GetLocalIpAddress();
            _ = Task.Run(() => DetectPublicIpAsync()); 
            CurrentPin = new Random().Next(1000, 9999).ToString(); 

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{Port}/");
            _listener.Start();

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => HandleRequests(_cts.Token));
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

            try
            {
                // API: Status
                if (path == "/api/status")
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { res.StatusCode = 401; res.Close(); return; }

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
                    await SendResponse(res, json, "application/json");
                }
                // API: Screen
                else if (path == "/api/screen")
                {
                    string pin = req.QueryString["pin"];
                    if (pin != CurrentPin) { res.StatusCode = 401; res.Close(); return; }

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
                    catch { }

                    if (imgData == null) // GDI Fallback
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

                    res.ContentType = "image/jpeg";
                    res.ContentLength64 = imgData.Length;
                    await res.OutputStream.WriteAsync(imgData, 0, imgData.Length);
                    res.Close();
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
                        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                    }
                    else if (path == "/api/mouse/up")
                    {
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                    }
                    else if (path == "/api/mouse/move")
                    {
                        SetCursorPos(x, y);
                    }
                    await SendResponse(res, "ok", "text/plain");
                }
                // UI: Index
                else if (path == "/" || path == "/index.html")
                {
                    await SendResponse(res, GetIndexHtml(), "text/html");
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                try { res.StatusCode = 500; res.Close(); } catch { }
            }
        }

        private static async Task SendResponse(HttpListenerResponse res, string content, string contentType)
        {
            byte[] buf = Encoding.UTF8.GetBytes(content);
            res.ContentType = contentType;
            res.ContentLength64 = buf.Length;
            await res.OutputStream.WriteAsync(buf, 0, buf.Length);
            res.Close();
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
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
    <title>Pi Node Remote Pro</title>
    <style>
        body { margin: 0; background: #000; color: #fff; font-family: -apple-system, sans-serif; overflow: hidden; touch-action: none; }
        #header { height: 50px; background: #1a1a1a; display: flex; align-items: center; padding: 0 15px; justify-content: space-between; border-bottom: 2px solid #333; }
        #stream-container { position: relative; width: 100vw; height: calc(100vh - 50px); overflow: hidden; background: #000; display:flex; align-items:center; justify-content:center; }
        #screen-img { max-width: 100%; max-height: 100%; object-fit: contain; }
        .badge { background: #333; padding: 4px 10px; border-radius: 6px; font-size: 13px; font-weight: bold; color: #ffeb3b; }
        #status-bar { position: absolute; bottom: 0; width: 100%; height: 35px; background: rgba(0,0,0,0.7); display: flex; align-items: center; padding: 0 15px; font-size: 12px; color: #ccc; box-sizing: border-box; }
        .live-indicator { width: 8px; height: 8px; background: #f44336; border-radius: 50%; margin-right: 8px; animation: blink 1s infinite; }
        @keyframes blink { 0% { opacity: 1; } 50% { opacity: 0.3; } 100% { opacity: 1; } }
    </style>
</head>
<body>
    <div id='header'>
        <div style='display:flex; align-items:center;'><div class='live-indicator'></div><strong>Pi Node Monitor</strong></div>
        <span id='pin-display' class='badge'>PIN: --</span>
    </div>
    <div id='stream-container'>
        <img id='screen-img' draggable='false' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII='>
        <div id='shield' style='position:absolute;top:0;left:0;width:100%;height:100%;z-index:100;'></div>
        <div id='status-bar'>Connecting to Node...</div>
    </div>

    <script>
        const userPin = new URLSearchParams(window.location.search).get('pin') || '';
        document.getElementById('pin-display').innerText = 'PIN: ' + userPin;

        const img = document.getElementById('screen-img');
        const shield = document.getElementById('shield');
        const status = document.getElementById('status-bar');
        
        let isLive = true;
        let svrW = 1920, svrH = 1080;
        let lastLoadTime = Date.now();

        // --- Adaptive Performance Loading ---
        function loadNextImage() {
            if (!isLive) return;
            const nextImg = new Image();
            nextImg.onload = () => {
                img.src = nextImg.src;
                const delay = Math.max(50, 200 - (Date.now() - lastLoadTime)); // Target 5fps but adapt to latency
                lastLoadTime = Date.now();
                setTimeout(loadNextImage, delay);
            };
            nextImg.onerror = () => setTimeout(loadNextImage, 500);
            nextImg.src = '/api/screen?pin=' + userPin + '&t=' + Date.now();
        }
        loadNextImage();

        function updateStatus() {
            fetch('/api/status?pin=' + userPin)
                .then(r => r.json())
                .then(d => {
                    svrW = d.screenWidth; svrH = d.screenHeight;
                    status.innerText = d.State + ' | In: ' + d.Incoming + ' | Out: ' + d.Outgoing + ' | Age: ' + d.LedgerAge + 's';
                }).catch(()=>{ status.innerText = 'Connection Lost'; });
        }
        setInterval(updateStatus, 3000);

        // --- Multi-Platform Control Logic (Touch/Mouse) ---
        function handleControl(e, act) {
            e.preventDefault();
            const rect = shield.getBoundingClientRect();
            let clientX, clientY;

            if (e.touches && e.touches.length > 0) {
                clientX = e.touches[0].clientX;
                clientY = e.touches[0].clientY;
            } else {
                clientX = e.clientX;
                clientY = e.clientY;
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
