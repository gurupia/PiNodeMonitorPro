# Pi Node Monitor Pro - SMS 알림 구현 기술 명세서

## 1. 개요 (Overview)
본 문서는 Pi Node가 비정상 종료되거나 "Stopped" 상태로 전환되었을 때, 관리자의 휴대폰으로 즉시 SMS 경고 메시지를 발송하는 기능을 구현하기 위한 기술적 요구사항을 정의합니다.

---

## 2. 필수 기술 스택 (Technology Stack)

### 2.1. SMS 중계 서비스 (SMS Gateway API)
C# 프로그램에서 직접 통신사 망에 접속할 수 없으므로, **HTTP REST API**를 제공하는 SMS 중계 서비스를 이용해야 합니다.

| 서비스명 | 특징 | 장점 | 단점 | 추천 대상 |
| :--- | :--- | :--- | :--- | :--- |
| **Twilio** | 글로벌 1위, 강력한 API, C# SDK 제공 | 구현이 매우 쉽고 문서가 방대함 | 한국 발송 비용이 다소 비쌈, 해외 발신으로 뜨거나 스팸 필터링 될 수 있음 | **개발 편의성 최우선** |
| **Naver Cloud SENS** | 네이버 클라우드 플랫폼 | 한국 통신망 최적화, 전송 속도 빠름, 010 번호 발신 가능(사전 등록 필요) | API Signature 생성이 다소 복잡함 (HMAC-SHA256 암호화 필요) | **안정적인 한국 서비스** |
| **CoolSMS (Nurigo)** | 국산 개발자 친화 서비스 | 카카오알림톡 지원, 심플한 JSON API | 대규모 발송 시 비용 이슈 | **가성비 및 알림톡 병행** |

### 2.2. 필요한 라이브러리 (NuGet Packages)
*   **Twilio 사용 시**: `Twilio` (공식 SDK)
*   **REST API 직접 호출 시**: `System.Net.Http.HttpClient` (기본 내장)
*   **JSON 처리**: `System.Text.Json` 또는 `Newtonsoft.Json`

---

## 3. 구현 로직 설계 (Core Logic)

### 3.1. 상태 감지 (State Detection)
`Form1.cs`의 `UpdateDashboardAsync` 메서드 내에서 Node 상태가 변경되는 시점을 포착해야 합니다.
- **Trigger**: `_statState == "Stopped"` 또는 `foundRunning == false`

### 3.2. 디바운싱 및 쿨다운 (Debouncing & Cooldown)
노드가 잠시 재부팅되거나 3초마다 상태를 체크할 때마다 문자를 보내면 **요금 폭탄**을 맞게 됩니다. 반드시 **발송 제한 로직**이 필요합니다.

```csharp
private DateTime _lastSmsSentTime = DateTime.MinValue;
private TimeSpan _smsCooldown = TimeSpan.FromHours(1); // 1시간에 최대 1통만 발송

if (currentState == "Stopped" && (DateTime.Now - _lastSmsSentTime) > _smsCooldown)
{
    SendSmsAlert();
    _lastSmsSentTime = DateTime.Now;
}
```

### 3.3. 비동기 발송 (Async Sending)
SMS 발송은 네트워크 요청이므로 반드시 `async/await`를 사용하여 UI가 멈추지 않도록 처리해야 합니다.

---

## 4. 구현 예시 (Twilio 기준 C# 코드)

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public async Task SendSmsAsync(string messageBody)
{
    string accountSid = "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"; // 설정 파일에서 로드 권장
    string authToken = "your_auth_token";
    
    TwilioClient.Init(accountSid, authToken);

    var message = await MessageResource.CreateAsync(
        body: $"[Pi Node Alert] 노드가 중지되었습니다! 확인해주세요.",
        from: new Twilio.Types.PhoneNumber("+15017122661"), // Twilio 발신 번호
        to: new Twilio.Types.PhoneNumber("+821012345678")   // 관리자 번호
    );
    
    Console.WriteLine($"SMS Sent: {message.Sid}");
}
```

---

## 5. UI 및 설정 요구사항 (User Settings)

사용자가 자신의 API 키와 전화번호를 입력할 수 있는 **설정 화면(Settings Form)**이 필요합니다.
*   **API Key / Secret Input**: (암호화하여 저장 필요)
*   **수신 전화번호 입력**: (+82 10-XXXX-XXXX 형식)
*   **테스트 발송 버튼**: 설정이 올바른지 확인하는 기능
*   **ON/OFF 토글**: SMS 기능을 끄고 켤 수 있는 스위치

---

## 6. 보안 고려사항 (Security)
*   **API Key 노출 금지**: 소스코드에 키를 하드코딩하면 해킹 시 요금 피해를 입을 수 있습니다.
*   **저장 방식**: `Properties.Settings.Default`에 저장하되, 가능하다면 `DataProtectionScope` 등을 이용해 로컬 암호화 저장을 권장합니다.

## 7. 결론 및 추천
초기 개발 단계에서는 **Twilio**를 사용하여 빠르게 기능을 검증하고, 실제 한국 내 운영 시에는 **Naver Cloud SENS**나 **CoolSMS**로 전환하여 비용과 도달률을 최적화하는 것을 권장합니다.
