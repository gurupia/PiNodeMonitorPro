# ♻️ Reusable Modules for Crypto Node Monitors
이 문서는 **Pi Node Monitor Pro** 프로젝트에서 개발된 코드 중, **다른 암호화폐(Ethereum, Bitcoin, Solana 등)의 노드 모니터링 시스템**을 개발할 때 즉시 재사용 가능한 핵심 모듈들을 정의합니다.

---

## 1. 📣 통합 알림 시스템 (Notification System)
가장 활용도가 높은 모듈입니다. 코인 종류와 무관하게 "특정 이벤트 발생 시 알림"을 보내는 기능은 100% 동일합니다.

### 📦 주요 파일
*   `Services/Sms/ISmsProvider.cs` (인터페이스)
*   `Services/Sms/TwilioProvider.cs` (트윌리오 구현체)
*   `Services/Sms/SolapiSmsProvider.cs` (국내 문자 구현체)
*   `Services/Sms/SmsService.cs` (통합 관리자 - 텔레그램 포함)
*   `Services/Sms/SmsConfigModel.cs` (설정 데이터 모델)
*   `SmsSettingsForm.cs` (설정 UI)

### 🚀 재활용 포인트
1.  **멀티 채널 지원**: SMS(Solapi, Twilio)와 메신저(Telegram)가 이미 통합되어 있습니다.
2.  **설정 UI 완비**: API Key 입력, 저장, **테스트 발송** 기능이 포함된 `SmsSettingsForm`을 그대로 가져다 쓸 수 있습니다.
3.  **확장성**: 슬랙(Slack), 디스코드(Discord) 등을 추가하고 싶다면 `ISmsProvider`를 상속받거나 `SmsService`에 메서드만 추가하면 됩니다.

---

## 2. 🛡️ 노드 헬스 체크 엔진 (Node Health Monitor)
블록체인 노드의 "멈춤(Stall)" 현상을 감지하는 로직은 보편적입니다.

### 📦 주요 파일
*   `Services/NodeMonitorService.cs`

### 🚀 재활용 포인트
1.  **Stuck Detection 알고리즘**:
    *   `CheckNodeStatus(currentBlock)` 메서드는 "블록 높이(Block Height)"가 특정 시간(`STUCK_THRESHOLD_MINUTES`) 동안 변하지 않으면 경고를 반환합니다.
    *   이더리움의 `BlockNumber`, 비트코인의 `Height` 등 어떤 숫자형 데이터만 주입하면 바로 작동합니다.
2.  **쿨타임(Cooldown) 로직**:
    *   장애 발생 시 알림 메세지 폭탄(Spam)을 방지하기 위한 `ALERT_COOLDOWN_HOURS` 로직이 이미 구현되어 있습니다.

---

## 3. 🛠️ 시스템 유틸리티 (System Utilities)
윈도우 환경에서 서버/노드를 관리하기 위한 필수 기능들입니다.

### 📦 주요 코드 (Form1.cs 및 Helper 내부)
1.  **Docker 제어 (`RunDockerCommandAsync`)**:
    *   `Process.Start`를 래핑하여 Docker 컨테이너의 상태(`inspect`), 로그, 통계(`stats`)를 비동기로 가져오는 코드는 모든 컨테이너 기반 노드에 재사용 가능합니다.
    *   예: `stellar-core` → `geth` (Ethereum) 또는 `solana-validator`로 이름만 바꾸면 됩니다.
2.  **포트 스캔 (`CheckPortOpenAsync`)**:
    *   특정 포트(예: 31401)가 외부에서 열려있는지 확인하는 `TcpClient` 로직은 P2P 노드 구동 필수 점검 항목입니다.
3.  **하이브리드 CPU 최적화 (`PerfUtility.cs`)**:
    *   인텔 12세대 이상 하이브리드 아키텍처(P/E 코어) 대응을 위한 프로세스 Affinity 제어 로직입니다.
    *   **재활용 포인트**: CPU 집약적인 동기화 작업 시에는 P-코어에, 저부하 감시 상태일 때는 E-코어에 프로세스를 강제 할당하여 전력 효율과 성능을 극대화할 수 있습니다.

---

## 4. 📱 모바일 연동 서버 (Micro Web Server)
PC 상태를 모바일 앱에서 확인하기 위한 초경량 웹 서버입니다.

### 📦 주요 파일
*   `MobileServer.cs`

### 🚀 재활용 포인트
*   별도의 IIS나 Apache 설정 없이, C# 코드 단 몇 줄로 `HttpListener`를 띄워 **JSON 데이터를 서빙**합니다.
*   `MakeJson()` 메서드의 내용만 해당 코인의 데이터(예: 해시레이트, 피어 수)로 바꿔주면 **"나만의 코인 대시보드 앱"** 백엔드가 완성됩니다.

---

## 📝 구축 가이드 (How to Adapt)
새로운 코인(예: `MyCoin`)의 모니터를 만들 때 아래 순서로 진행하세요.

1.  **UI 복사**: `SmsSettingsForm`과 `SmsService` 폴더를 통째로 복사합니다.
2.  **데이터 소스 교체**:
    *   `Form1`의 `timer_Tick`에서 파싱하는 데이터를 `MyCoin`의 API(RPC)로 변경합니다.
    *   예: `http://localhost:31403/metrics` (Pi) → `http://localhost:8545` (Eth RPC).
3.  **모니터 연결**:
    *   가져온 블록 높이를 `_monitorService.CheckNodeStatus(myBlockHeight)`에 넣어줍니다.
4.  **완료**:
    *   나머지 알림 전송, 설정 저장, 쿨타임 관리는 기존 코드가 알아서 수행합니다.
