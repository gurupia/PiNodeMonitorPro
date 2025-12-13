# Pi Wallet & Blockchain Integration Plan

본 문서는 Pi Node Monitor Pro에 **"지갑 잔액 조회"** 및 **"거래 감지"**, **"결제(기부)"** 기능을 통합하기 위한 상세 기술 가이드입니다. 
Pi Network는 **[Stellar Consensus Protocol (SCP)](https://developers.stellar.org/docs)** 기반이므로, Stellar SDK를 활용하면 생각보다 쉽게 구현할 수 있습니다.

---

## 1단계: 단순 잔액 및 상태 조회 (Read-Only)

사용자의 **지갑 비밀키(Passphrase)가 전혀 필요 없는** 안전한 단계입니다.
오직 **공개키(Public Key, G로 시작하는 주소)**만 있으면 됩니다.

### 1.1 기술 원리
Pi 블록체인(Testnet/Mainnet)은 Stellar Core의 포크(Fork) 버전입니다. 따라서 표준 Stellar API(Horizon)와 호환됩니다.
*   **API Endpoint**: `https://api.testnet.minepi.com` (테스트넷) / `https://api.mainnet.minepi.com` (메인넷)

### 1.2 구현 방법 (C# Code Snippet)
`csharp-stellar-sdk` 또는 단순 `HttpClient`를 사용합니다.

```csharp
// GET https://api.mainnet.minepi.com/accounts/{PublicKey}
public async Task<decimal> GetPiBalance(string publicKey)
{
    string url = $"https://api.mainnet.minepi.com/accounts/{publicKey}";
    using (var client = new HttpClient())
    {
        var json = await client.GetStringAsync(url);
        // JSON 파싱: balances 배열 -> asset_type: native 항목의 balance 값 추출
        var data = JObject.Parse(json);
        var balances = data["balances"];
        foreach (var balance in balances)
        {
            if (balance["asset_type"].ToString() == "native")
            {
                return decimal.Parse(balance["balance"].ToString());
            }
        }
    }
    return 0;
}
```

### 1.3 기대 효과
*   프로그램 대시보드 한구석에 **"현재 지갑 잔액: 1,234 π"** 표시 가능.
*   **"오늘 채굴 보상"** 계산 가능 (어제 잔액과 비교).

---

## 2단계: 거래 내역(Transaction) 실시간 알림

"노드 보상"이 들어왔거나, 누군가 나에게 파이를 보냈을 때 알림을 띄우는 기능입니다.

### 2.1 기술 원리
Stellar Horizon API는 **SSE (Server-Sent Events)**를 지원합니다. 즉, 새로운 거래가 발생하자마자 연결된 프로그램에 "푸시"를 해줍니다.

### 2.2 구현 로직
*   `HttpClient`로 `/accounts/{PublicKey}/payments?cursor=now&stream=true` 주소에 연결해 둡니다.
*   연결을 끊지 않고 기다리면, 입출금이 발생할 때마다 JSON 데이터가 한 줄씩 들어옵니다.
*   들어온 데이터가 `type: payment` 이고 `to: 내주소` 라면 -> **"입금 알림"** 발생!

### 2.3 활용 시나리오
*   **24시간 감시**: PC 켜두면 "띠링! 3.14 Pi가 입금되었습니다." 알림 팝업.

---

## 3단계: Pi Apps Platform 연동 (기부 기능)

이 단계부터는 단순 조회를 넘어, 사용자가 **직접 결제(전송)**를 하게 만드는 기능입니다. 이를 위해선 Pi Network 공식 **Platform API**를 타야 합니다.

### 3.1 개념
Pi Browser가 아닌 외부 앱(데스크탑 앱)에서 결제를 일으키려면 **[Pi AppLink](https://github.com/pi-apps/pi-platform-docs)** 규격을 써야 할 수도 있지만, 더 쉬운 방법은 **QR코드**입니다.

### 3.2 구현 프로세스 (Donation System)
1.  **개발자**: Pi Developer Portal에 앱 등록 (API Key 발급).
2.  **앱**: "후원하기" 버튼 클릭 -> API로 **결제 요청(Payment Object)** 생성.
3.  **앱**: 생성된 결제의 `redirect_url` 등을 담은 **QR 코드**를 화면에 띄움.
4.  **사용자**: 핸드폰으로 Pi Browser를 켜서 QR 스캔 -> 승인 버튼 터치.
5.  **앱**: Payment API를 폴링(Polling)하며 "승인됨(COMPLETED)" 상태 대기 -> 확인 시 "감사합니다!" 메시지 출력.

---

## 4. 보안 및 주의사항 (Critical)

1.  **비밀키(Sxxxx...) 절대 요구 금지**:
    *   사용자에게 비밀 구절(Passphrase) 입력을 요구하는 순간 **스캠(사기) 앱**으로 의심받고 매장당합니다.
    *   우리는 오직 **공개키(Public Key)**만 받아서 조회용으로 써야 합니다.
    *   사용자의 송금은 반드시 **Pi Browser(핸드폰)**를 통해서만 이루어지도록 유도해야 합니다(QR 방식).

2.  **테스트넷 활용**:
    *   개발 중에는 무조건 `api.testnet.minepi.com`을 사용하여, 실수로 실제 코인을 날리는 일이 없도록 해야 합니다.

---

## 5. 결론 및 로드맵 제안

*   **P1 (지금 당장)**: **공개키 입력란** 만들고, 메인넷 API 찔러서 **"잔액 표시"** 기능 추가. (난이도: 하)
*   **P2 (다음 달)**: **거래 내역 리스트** UI 추가. (난이도: 중)
*   **P3 (최종 목표)**: **기부 QR 코드** 생성 및 서버 연동. (난이도: 상)

일단 **P1 (잔액 조회)**부터 시작하면, "오? 내 지갑이랑 연결됐네?" 라는 느낌을 주면서 사용자 경험이 확 달라질 것입니다.
