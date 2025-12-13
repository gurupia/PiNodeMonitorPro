# Pi Node Monitor Pro - 메신저(Telegram/Kakao) 알림 구현 가이드

## 1. 개요 (Overview)
Pi Node의 상태 변화(중단 등)를 감지했을 때, 사용자가 가장 자주 확인하는 메신저 앱으로 실시간 알림을 전송하는 기술 명세입니다.

---

## 2. 텔레그램 (Telegram) - **[강력 추천]**

텔레그램은 **완전 무료**이며, API 사용이 매우 간단하여 서버 모니터링 알림용으로 업계 표준처럼 사용됩니다.

### 2.1. 준비 사항
1.  **봇 생성**: 텔레그램 앱에서 `@BotFather` 검색 -> `/newbot` 입력 -> 봇 이름 설정 -> **API Token** 발급.
2.  **Chat ID 확인**: 생성한 봇에게 아무 메시지나 보낸 후, `https://api.telegram.org/bot<TOKEN>/getUpdates` 접속하여 `chat` 객체의 `id` 확인.

### 2.2. 기술 구현 (C#)
별도의 SDK 없이 `HttpClient`만으로 구현 가능합니다.

```csharp
using System.Net.Http;

public async Task SendTelegramAlert(string message)
{
    string botToken = "YOUR_BOT_TOKEN";
    string chatId = "YOUR_CHAT_ID";
    string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={message}";

    using (var client = new HttpClient())
    {
        await client.GetAsync(url);
    }
}
```

### 2.3. 장점 및 단점
*   👍 **장점**: 완전 무료, 무제한 발송, 구현 난이도 최하(Easy), 유지보수 불필요.
*   👎 **단점**: 텔레그램 앱을 설치해야 함.

---

## 3. 카카오톡 (KakaoTalk) - "나에게 보내기" API

카카오톡은 국민 메신저지만, 개발자 관점에서는 **보안 인증(OAuth 2.0)** 절차가 있어 구현 난이도가 높습니다.

### 3.1. 준비 사항
1.  **카카오 디벨로퍼스 가입**: 앱 생성 및 '카카오 로그인', '메시지 권한' 활성화.
2.  **REST API 키** 확인.
3.  **사용자 인증**: 사용자가 브라우저를 통해 로그인을 하고, **Access Token**과 **Refresh Token**을 발급받아야 함.

### 3.2. 기술 구현 로직
단순한 HTTP 호출이 아니라, **토큰 관리 시스템**이 필요합니다.

1.  **로그인 (최초 1회)**: 웹브라우저 팝업 -> 카카오 로그인 -> 인증 코드(Code) 수신.
2.  **토큰 발급**: 인증 코드로 `Access Token`(6시간 유효)과 `Refresh Token`(2달 유효) 요청.
3.  **메시지 전송**:
    *   API: `POST https://kapi.kakao.com/v2/api/talk/memo/default/send`
    *   Header: `Authorization: Bearer {Access_Token}`
4.  **자동 갱신 (핵심)**: 메시지 전송 전 `Access Token` 만료 여부 확인 -> 만료 시 `Refresh Token`으로 재발급 -> 전송.

### 3.3. C# 구현 예시 (메시지 전송 부분만)

```csharp
public async Task SendKakaoToMe(string accessToken, string text)
{
    string url = "https://kapi.kakao.com/v2/api/talk/memo/default/send";
    
    // 템플릿 JSON (Text 형태)
    var payload = new 
    {
        object_type = "text",
        text = text,
        link = new { web_url = "http://google.com" }
    };
    
    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(new { template_object = jsonString });

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("template_object", payload.ToString())
        });
        
        await client.PostAsync(url, content);
    }
}
```

### 3.4. 장점 및 단점
*   👍 **장점**: 별도 앱 설치 없이 확인 가능 (접근성 우수).
*   👎 **단점**:
    *   **토큰 만료 이슈**: PC가 꺼져 있거나 오랫동안 노드가 잘 돌아가면 토큰이 만료되어 정작 중요할 때 알림 실패 가능성 있음.
    *   구현 복잡도 높음 (OAuth 로그인 구현 필요).

---

## 4. UI 설계 및 종합 비교

### 4.1. 설정 화면 (Settings Form) UI 제안
*   **알림 채널 선택**: [ 라디오 버튼 ] Telegram / KakaoTalk
*   **Telegram 선택 시**: `Bot Token` 입력창, `Chat ID` 입력창.
*   **KakaoTalk 선택 시**: `카카오 로그인` 버튼 (토큰 발급용), `현재 상태: 인증됨/만료됨` 라벨.

### 4.2. 종합 추천
| 평가 항목 | 텔레그램 (Telegram) | 카카오톡 (KakaoTalk) |
| :--- | :--- | :--- |
| **구현 난이도** | ⭐ (매우 쉬움) | ⭐⭐⭐⭐⭐ (어려움) |
| **안정성** | ⭐⭐⭐⭐⭐ (영구적) | ⭐⭐ (토큰 만료 위험) |
| **비용** | 무료 | 무료 (나에게 보내기) |
| **추천** | **강력 추천** | 사용자 편의성이 필수일 때만 |

**결론**: Pi Node Monitor와 같은 24시간 백그라운드 프로그램은 인증 풀림 걱정이 없는 **텔레그램** 방식이 훨씬 안정적입니다.
