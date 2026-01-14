# 모바일 원격 제어 서버 구현 가이드

## 개요

이 문서는 WinForms 애플리케이션에서 `HttpListener`를 사용하여 모바일 원격 제어 서버를 구현하는 방법을 설명합니다.

### 주요 기능
- HTTP REST API 기반 원격 제어
- QR 코드를 통한 간편 연결
- PIN 인증 보안
- **로그 로테이션**: 일자별 파일 분리, 자동 크기 제한(10MB), 7일 보관
- **헬스체크 API**: `/api/health` 엔드포인트 (PIN 불필요, 멀티노드 모니터링용)

---

## 1. 아키텍처

```
┌─────────────────┐     HTTP/REST      ┌─────────────────┐
│  모바일 브라우저  │ ◄──────────────────► │  MobileServer   │
│  (Chrome 등)     │                      │  (HttpListener) │
└─────────────────┘                      └─────────────────┘
                                                  │
                                                  ▼
                                         ┌─────────────────┐
                                         │  WinForms App   │
                                         │  (화면 캡처 등)  │
                                         └─────────────────┘
```

---

## 2. 핵심 구성 요소

### 2.1 MobileServer.cs - 서버 클래스

```csharp
public class MobileServer
{
    private static HttpListener _listener;
    private static CancellationTokenSource _cts;
    
    public static string CurrentIpAddress { get; private set; } = "127.0.0.1";
    public static string PublicIpAddress { get; private set; } = "Unknown";
    public const int Port = 5000;
    public static string CurrentPin { get; private set; } = "0000";
    
    // 이벤트: UI에서 로깅 표시용
    public static event Action<string> RequestLogged;
    public static event Action<string> PublicIpDetected;
}
```

### 2.2 서버 시작 로직

```csharp
public static async Task StartServerAsync()
{
    if (_listener != null) return; // 중복 시작 방지
    
    try 
    {
        // 1. IP 주소 감지
        CurrentIpAddress = GetLocalIpAddress();
        _ = Task.Run(() => DetectPublicIpAsync()); // 비동기로 공인 IP 감지
        
        // 2. PIN 생성 (4자리 보안 코드)
        CurrentPin = new Random().Next(1000, 9999).ToString();
        
        // 3. HttpListener 설정
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        _listener.Prefixes.Add($"http://localhost:{Port}/");
        
        // 4. LAN IP 바인딩 (외부 접속용)
        if (CurrentIpAddress != "127.0.0.1")
        {
            try 
            { 
                _listener.Prefixes.Add($"http://{CurrentIpAddress}:{Port}/"); 
            } 
            catch (Exception ex) 
            { 
                Log($"Warning: Could not bind to {CurrentIpAddress}: {ex.Message}"); 
            }
        }
        
        // 5. 서버 시작
        _listener.Start();
        
        // 6. 요청 처리 루프 시작 (백그라운드)
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => HandleRequests(_cts.Token));
    }
    catch (Exception ex)
    {
        Log($"FATAL: Server failed to start: {ex.Message}");
    }
}
```

---

## 3. IP 주소 감지

### 3.1 로컬 IP 감지 (LAN용)

```csharp
public static List<string> GetAllLocalIpAddresses()
{
    var list = new List<string>();
    
    // NetworkInterface API 사용 (정확한 감지)
    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
    foreach (var ni in interfaces)
    {
        // 조건 1: 활성 상태
        if (ni.OperationalStatus != OperationalStatus.Up) continue;
        
        // 조건 2: 루프백 제외
        if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
        
        // 조건 3: 게이트웨이 있음 (실제 네트워크 연결)
        var props = ni.GetIPProperties();
        if (props.GatewayAddresses.Count == 0) continue;
        
        // IPv4 주소 수집
        foreach (var ip in props.UnicastAddresses)
        {
            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                string s = ip.Address.ToString();
                if (!s.StartsWith("169.254")) // APIPA 제외
                    list.Add(s);
            }
        }
    }
    
    // 우선순위 정렬: 공인 IP > 192.168 > 10.x > 172.x
    list.Sort((a, b) => GetIpScore(b).CompareTo(GetIpScore(a)));
    
    return list;
}

private static int GetIpScore(string ip)
{
    // 사설 IP 범위 체크
    if (ip.StartsWith("192.168.")) return 100;
    if (ip.StartsWith("10.")) return 90;
    if (ip.StartsWith("172."))
    {
        var parts = ip.Split('.');
        if (int.TryParse(parts[1], out int second))
            if (second >= 16 && second <= 31) return 10; // Docker/VM
    }
    return 200; // 공인 IP (가장 높은 우선순위)
}
```

### 3.2 공인 IP 감지 (외부 접속용)

```csharp
private static async Task DetectPublicIpAsync()
{
    try
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            PublicIpAddress = await client.GetStringAsync("https://api.ipify.org");
            PublicIpDetected?.Invoke(PublicIpAddress);
        }
    }
    catch { PublicIpAddress = "Failed"; }
}
```

---

## 4. 요청 처리

### 4.1 요청 라우팅

```csharp
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
    
    // PIN 인증
    string pin = req.QueryString["pin"];
    if (pin != CurrentPin && path != "/") 
    {
        res.StatusCode = 401;
        res.Close();
        return;
    }
    
    // 라우팅
    switch (path)
    {
        case "/":
        case "/index.html":
            SendResponse(res, GetIndexHtml(), "text/html");
            break;
        case "/api/status":
            SendResponse(res, GetStatusJson(), "application/json");
            break;
        case "/api/screen":
            SendScreenshot(res);
            break;
        case "/api/click":
            HandleClick(req, res);
            break;
        default:
            res.StatusCode = 404;
            res.Close();
            break;
    }
}
```

---

## 5. QRForm - 연결 UI

### 5.1 IP 선택 콤보박스

```csharp
public class QRForm : Form
{
    private ComboBox cboIps;
    
    public QRForm()
    {
        // 콤보박스 생성
        cboIps = new ComboBox();
        cboIps.DropDownStyle = ComboBoxStyle.DropDownList;
        
        // IP 목록 채우기
        string publicIp = MobileServer.PublicIpAddress;
        if (!string.IsNullOrEmpty(publicIp) && publicIp != "Unknown")
            cboIps.Items.Add($"[공인] {publicIp}");
        
        var localIps = MobileServer.GetAllLocalIpAddresses();
        foreach(var ip in localIps)
            if (ip != "127.0.0.1") cboIps.Items.Add(ip);
        
        if (cboIps.Items.Count > 0) cboIps.SelectedIndex = 0;
        
        // 공인 IP 감지 이벤트 구독
        MobileServer.PublicIpDetected += OnPublicIpDetected;
    }
    
    private void OnPublicIpDetected(string publicIp)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnPublicIpDetected(publicIp)));
            return;
        }
        
        string item = $"[공인] {publicIp}";
        if (!cboIps.Items.Contains(item) && publicIp != "Failed")
        {
            cboIps.Items.Insert(0, item);
            cboIps.SelectedIndex = 0;
        }
    }
}
```

### 5.2 QR 코드 생성

```csharp
private void UpdateQR()
{
    string ip = GetSelectedIp();
    string url = $"http://{ip}:{MobileServer.Port}/?pin={MobileServer.CurrentPin}";
    
    // QRCoder 라이브러리 사용
    QRCodeGenerator qrGenerator = new QRCodeGenerator();
    QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    QRCode qrCode = new QRCode(qrCodeData);
    pbQR.Image = qrCode.GetGraphic(20);
}
```

---

## 6. 네트워크 요구 사항

### 6.1 방화벽 설정

```powershell
# PowerShell (관리자 권한)
New-NetFirewallRule -DisplayName "PiNodeMobile" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### 6.2 HttpListener ACL (관리자 권한 없이 특정 IP 바인딩)

```cmd
# CMD (관리자 권한)
netsh http add urlacl url=http://192.168.1.100:5000/ user=Everyone
```

---

## 7. 트러블슈팅

| 문제 | 원인 | 해결책 |
|------|------|--------|
| 서버 시작 실패 | 포트 충돌 | `netstat -an | findstr 5000` 확인 |
| LAN에서 접속 불가 | 방화벽 차단 | 방화벽 규칙 추가 |
| 공인 IP로 접속 불가 | 공유기 NAT | 포트 포워딩 설정 필요 |
| PIN 인증 실패 | PIN 불일치 | QR 코드 다시 스캔 |

---

## 8. 성능 참고

- **HttpListener**: Windows `http.sys` 기반, 커널 모드 처리
- **처리량**: 초당 수천 요청 가능
- **지연 시간**: LAN 환경 1~5ms
- **권장 FPS**: 10~15 (화면 캡처 스트리밍 시)

---

## 참고 자료

- [HttpListener Class (Microsoft Docs)](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener)
- [NetworkInterface Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface)
- [QRCoder NuGet Package](https://www.nuget.org/packages/QRCoder)
