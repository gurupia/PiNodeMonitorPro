# 📡 Pi Node Monitor Pro: 멀티 노드 관제 센터 개발 백서

## 1. 프로젝트 개요 (Overview)
**목표**: 외부 원격 제어 도구(TeamViewer 등)에 의존하지 않고, 단일 "관제 센터(Control Center)" PC에서 수십 대의 Pi Node를 중앙 집중식으로 모니터링하고 관리할 수 있도록 시스템을 구축함.

**핵심 기능**:
- **멀티 노드 대시보드 (Dashboard)**: 10대 이상의 노드 상태(블록 높이, 동기화 상태, In/Out 연결 수)를 하나의 그리드 화면에서 실시간 통합 관제.
- **스마트 폴링 (Smart Polling)**: 비동기 HTTP 폴링 방식을 사용하여 네트워크 부하를 최소화하면서 상태를 갱신.
- **원격 화면 보기 (Remote Screen - Eye)**: 1초당 1프레임(1 FPS) 수준의 실시간 화면 미러링 기능을 통해 시각적 트러블슈팅 지원.
- **원격 제어 (Click-to-Control - Hand)**: 마우스 클릭 이벤트를 원격지로 전송하여, Docker 재시작 등의 복구 작업을 원격으로 수행 가능.

---

## 2. 시스템 아키텍처 (Architecture)

이 시스템은 **P2P(Peer-to-Peer) 에이전트 아키텍처**를 따릅니다. 각 Pi Node Monitor 프로그램은 모니터링 클라이언트이자 동시에 서버 에이전트 역할을 수행합니다.

### **A. 서버 사이드 (Agent Node)**
- **컴포넌트**: `MobileServer.cs` (System.Net.HttpListener 기반 경량 웹 서버)
- **역할**: 자신의 노드 상태(JSON)를 외부에 노출하고, OS 제어 명령(화면 캡처, 마우스 클릭)을 수신하여 실행.
- **포트**: 5000 (기본값)
- **보안**: PIN 번호 기반의 인증 체계.

### **B. 클라이언트 사이드 (Control Center)**
- **컴포넌트**: `MultiMonitorForm.cs` & `RemoteViewForm.cs`
- **역할**: 여러 에이전트(노드)로부터 데이터를 수집하여 시각화하고, 사용자 명령을 전달.

---

## 3. 핵심 구현 상세 (Implementation Details)

### 3.1. REST API 확장 (`MobileServer.cs`)
기존 모바일 앱 연동용 서버를 확장하여 원격 화면 캡처 및 입력 시뮬레이션 API를 추가했습니다.

| 엔드포인트 | 방식 | 파라미터 | 설명 |
|:--- | :--- | :--- | :--- |
| `/api/status` | GET | `pin` | 노드 상태 지표(블록, 동기화 여부 등)를 JSON으로 반환. |
| `/api/screen` | GET | `pin` | 현재 메인 모니터 화면을 캡처하여 JPEG 이미지로 반환. |
| `/api/click` | GET | `pin`, `x`, `y` | 지정된 (x, y) 좌표에 마우스 좌클릭 이벤트를 발생시킴. |

**주요 기술**:
- **System.Drawing.Common**: `Graphics.CopyFromScreen`을 사용하여 바탕화면 캡처 구현.
- **user32.dll (Win32 API)**: `SetCursorPos` 및 `mouse_event` API를 P/Invoke로 호출하여 마우스 제어 구현.

### 3.2. 대시보드 UI (`MultiMonitorForm.cs`)
멀티 노드 목록을 관리하는 메인 화면입니다.

- **데이터 그리드**: `DataGridView`를 커스터마이징하여 Alias, IP, 상태, 블록 높이 등을 표시.
- **동시성 처리**: `Task.WhenAll`을 사용하여 수십 대의 노드를 병렬로 폴링(Polling)하므로, 노드가 많아져도 UI가 멈추지 않음.
- **UI 로직**:
    - **동적 색상**: 상태가 'Synced'이면 초록색, 문제가 있으면 분홍색/회색으로 행 배경색 변경.
    - **레이아웃 보정**: 상단 패널과 그리드 간의 겹침 문제를 해결하기 위해 컨테이너 패널(`pnlGridContainer`) 도입 및 Z-Order 재조정(`SendToBack`).
    - **안전한 종료**: 비동기 업데이트 중 폼이 닫힐 때 발생할 수 있는 `InvalidOperationException` 방지 로직(`IsDisposed` 체크) 적용.

### 3.3. 원격 뷰어 (`RemoteViewForm.cs`)
가벼운 원격 데스크톱 뷰어입니다.

- **스트리밍 전략**: 1000ms(1초)마다 `/api/screen`을 호출하여 이미지를 갱신. 정적 오류 메시지 확인에 최적화됨.
- **좌표 변환 알고리즘**:
    - `PictureBox`는 `SizeMode.Zoom` 모드를 사용하므로, UI상의 클릭 위치와 실제 서버 해상도 간의 차이가 발생.
    - 레터박스(Letterbox) 및 필러박스(Pillarbox) 여백을 계산하여, 사용자의 클릭 좌표를 실제 서버 화면 좌표로 정밀하게 변환하여 전송.

---

## 4. 운영 워크플로우 (User Scenario)

1.  **배포 (Deployment)**: 관리 대상이 되는 모든 노드 PC에서 `PiNodeMonitorWinForm.exe` 실행.
2.  **등록 (Registration)**: 관제용 메인 PC에서 "Multi-View"를 열고 노드 IP(예: `192.168.1.5:5000`)와 PIN 등록.
3.  **관제 (Monitoring)**: 대시보드 모니터링.
    - **초록불**: 정상 (Synced).
    - **회색/분홍불**: 노드 중단 또는 Docker 문제 발생.
4.  **조치 (Troubleshooting)**:
    - 문제 발생 노드의 **"📺 View"** 버튼 클릭.
    - **확인**: 화면을 통해 오류 메시지(예: "Check port 31400") 확인.
    - **해결**: 화면 상의 "Check Now" 버튼이나 Docker 아이콘을 직접 클릭하여 복구 시도.
    - **완료**: 뷰어 종료 후 대시보드 상태가 정상(초록색)으로 돌아오는지 확인.

---

## 5. 기술적 난제 및 해결 (Challenges & Solutions)

### **난제 1: UI 겹침 현상 (Grid hiding behind Header)**
- **문제**: `Dock=Fill` 속성의 Grid가 WinForms Z-Order 우선순위에 의해 상단 패널(`Dock=Top`) 뒤로 숨어버리는 현상 발생.
- **해결**: Grid를 별도의 컨테이너 패널(`pnlGridContainer`)에 감싸고, 상단 패널을 `SendToBack()` 처리하여 Docking 공간 계산의 우선권을 부여함.

### **난제 2: 비동기 UI 스레드 충돌 (Async UI Thread Crashes)**
- **문제**: 폴링 작업이 진행 중일 때 폼을 닫으면, 뒤늦게 도착한 응답이 이미 소멸된(Disposed) 컨트롤에 접근하려다 `Invoke` 에러 발생.
- **해결**: `this.Invoke` 호출 전 `if (this.IsDisposed || !this.IsHandleCreated) return;` 안전장치를 추가하고, `OnFormClosing`에서 타이머를 즉시 중지시킴.

### **난제 3: 원격 클릭 정확도 (Remote Click Accuracy)**
- **문제**: 뷰어 창 크기를 조절하면 이미지 비율(Zoom)이 달라져, 클릭한 위치와 실제 서버의 클릭 위치가 엇나가는 현상.
- **해결**: 이미지의 원본 비율(Aspect Ratio)과 뷰어 창의 비율을 비교하여, 실제 이미지가 렌더링된 영역(Display Rect)을 역산하는 좌표 매핑 로직 구현.

---

---

## 6. 업데이트 로그 (v1.2.1-stable)

### **[v1.2.1-stable] - 2026-01-16: Professional UI & Monitoring Consolidation**
원격 관리의 편의성과 모니터링의 정확도를 극대화한 안정화 버전입니다.

-   **모바일 대시보드 혁신**: 
    -   `Node Bonus`를 제거하고 실전 지표인 **가동률(Availability %)**과 **31401-03 개별 포트 상태** 통합.
    -   공식 앱과 동일한 **Desktop Version(0.5.4)** 및 **OS 버전** 정보 하단 배치.
-   **PC UI 정석 리팩토링**: 
    -   `HistoryForm`에 `Dock/AutoSize`를 적용하여 텍스트 가림 문제를 아키텍처적으로 해결.
-   **에이전트 수집 엔진 강화**: 
    -   메인 지표와 독립된 버전 수집 루틴 및 **Docker Fallback** 로직으로 프로토콜/빌드 정보 미표시 현상 완치.

### **[v1.2.2-stable] - 2026-01-16: Manual Node Bonus Integration**
사용자 피드백을 반영하여 자동 수집이 어려운 노드 보너스 정보를 직접 관리할 수 있게 되었습니다.

-   **수동 입력 UI**: `Set` 버튼과 입력 필드를 통해 즉시 보너스 수치 반영 및 히스토리 CSV 자동 기록.
-   **실시간 동기화**: PC에서 입력 즉시 모바일 서버 상태 엔진에 반영되어 앱 재시작 없이 확인 가능.

### **[v1.2.3-stable] - 2026-01-16: Pi Price & Asset Value**
파이 지갑 잔고의 실제 가치를 실시간으로 파악할 수 있는 금융 지표를 통합했습니다.

-   **CoinGecko API 연동**: `PriceService` 구현을 통해 실시간 USD/KRW 시세 수집 (10분 캐싱).
-   **자산 가치 자동 계산**: `보유 수량 * 현재 시세`를 합산하여 PC 및 모바일 대시보드에 표시.

### **[v1.2.4-stable] - 2026-01-16: Hybrid Price Sync Engine**
시세 정보의 신선도와 안정성을 극대화하기 위한 하이브리드 수집 환경을 구축했습니다.

-   **Dual-Source 수집**: C# 백엔드와 모바일 웹(JS)이 동시에 시세를 수집하고 상호 보완하는 구조.
-   **양방향 동기화**: 모바일에서 갱신된 최신 지표가 PC 대시보드 레이블(`[M]` 표시)로 즉시 역전송됨.

### **[v1.2.5-stable] - 2026-01-16: Cloudflare Tunnel Stability Fix**
원격 접속 시 발생하는 Error 1033 및 프로세스 충돌 문제를 해결한 인프라 패치 버전입니다.

-   **좀비 프로세스 정리**: 터널 시작 전 기존의 `cloudflared` 인스턴스를 모두 강제 종료하여 충돌 방지.
-   **UX 고도화**: 터널 URL 생성 시 클립보드 자동 복사 및 윈도우 알림 배너 연동.

### **[v1.3.0-stable] - 2026-01-16: Dual-Mode Mobile UI Architecture**
공식 파이 앱의 디자인 철학을 반영하면서도 기존의 직관적인 사용성을 보존한 대규모 UI 개편 버전입니다.

-   **듀얼 UI 엔진**: 
    -   **Classic Mode**: 기존의 올인원 스크롤 레이아웃을 유지하여 한눈에 모든 정보를 파악 가능.
    -   **Pro Mode**: 사이드바(Navigation Drawer) 기반의 모듈형 구조로 전환하여 전문적인 관리 환경 제공.
-   **Smart Sidebar**: 실시간 노드 상태 집약 헤더, 사용자 맞춤형 뷰 전환, 햅틱 피드백 시스템 통합.

### **[v1.4.0-stable] - 2026-01-16: Mobile Fleet Control Edition**
여러 대의 파이 노드를 하나의 화면에서 지휘할 수 있는 '함대 관제(Fleet Control)' 시스템이 통합되었습니다.

-   **멀티 노드 등록 엔진**: 로컬 스토리지를 활용하여 다중 터널 URL을 보안 저장하고 관리하는 인프라 구축.
-   **하이브리드 네비게이션**: 
    -   **Classic 모드**: 하단 3탭 시스템([지표], [원격], [함대])으로 노드간 전환 편의성 극대화.
    -   **Pro 모드**: 하단 탭을 제거하여 화면 공간을 100% 확보하고 사이드바에서 모든 제어 기능 수행.
-   **실시간 함대 헬스 체크**: 등록된 모든 노드의 가동 상태를 주기적으로 감시하여 대폭적인 통합 모니터링 환경 제공.
-   **사이드바 스마트 인프라**: 
    -   **Smart Header**: 사이드바 호출 시 노드 요약 상태 및 IP 등 핵심 지표 즉시 노출.
    -   **Quick Action Footer**: 한 손 조작 환경에 최적화된 긴급 재시작 및 새로고침 버튼 배치.
-   **고도화된 모바일 UX**: 
    -   **Haptic Feedback**: 메뉴 및 버튼 인터랙션 시 물리적 진동 반응 추가.
    -   **SPA Hash Routing**: 브라우저 뒤로가기 시 사이드바 자동 닫힘 등 앱 습관에 최적화된 라우팅.
-   **미래 확장성**: 사이드바 구조 도입을 통해 향후 '매크로 스튜디오' 및 '상세 로그 뷰어' 모듈 추가를 위한 기반 마련.

### **[v1.4.3-stable] - 2026-01-16: Fleet Security & Layout Hotfix**
멀티 노드 관제 시스템의 안정성과 보안을 강화한 긴급 패치입니다.

-   **개별 노드 PIN 인증**: 노드 등록 시 PIN 입력 필드(기본값 0000)를 추가하고, 노드 전환 시 자동으로 해당 PIN을 적용하여 인증 오류 차단.
-   **레이아웃 버그 수정**: Fleet View 컨테이너가 화면 하단으로 밀려나던 HTML 구조적 결함(Early Closing Tag)을 완벽 수정.
-   **초기화 로직 안정화**: 중복 선언된 `startSession` 함수를 제거하여 앱 실행 시 노드 데이터 로딩 신뢰성 확보.

### **[v1.5.0] - 2026-01-16: Performance Update**
시스템 자원 효율성과 반응성을 극대화하기 위한 코어 로직 최적화 업데이트입니다.

-   **Docker 호출 효율화 (Batching)**: 컨테이너 상태를 확인하기 위해 반복 호출되던 Docker CLI 명령을 단일 호출(`docker ps`)로 통합하여 프로세스 생성 횟수를 50% 이상 절감.
-   **Regex 엔진 최적화**: 런타임마다 반복 컴파일되던 정규식을 `static readonly` 필드로 전환하여 메모리 할당 및 파싱 오버헤드 제거.

### **[v1.6.0] - 2026-01-16: Electron App Expansion**
공식 Pi Node 앱(`pi-network-desktop`)의 소스 코드를 확장하여 편의 기능을 주입했습니다. (MIT License 기반 Customization)

-   **Auto-Launch Integration**: 공식 Pi Node 앱 실행 시, `PiNodeMonitorWinForm` 관제 도구가 자동으로 함께 실행되도록 연동. (main.js Injection)
-   **DevTools Unleashed**: `Ctrl+Shift+I` 단축키를 통해 Electron 내부 브라우저의 개발자 도구를 활성화하여 네트워크 및 렌더링 디버깅 환경 마련.

### **[v1.7.0] - 2026-01-16: Project Structure Refactoring**
`app.asar-repack` 내부에 혼재되어 있던 Electron 원본 코드와 C# 커스텀 프로젝트를 명확히 분리하여 유지보수성 향상.

-   **Source Isolation**: Electron 앱(`dist`, `package.json`)을 `app.asar-custom` 폴더로 격리.
-   **Reference Update**: 폴더 구조 변경에 따라 `main-enhancer.js`의 모니터링 툴 실행 경로를 `../../../` 깊이로 수정하여 연결성 유지.
-   **Deployment Automation**: `deploy_and_run.bat` 스크립트를 통해 `app.asar` 교체뿐만 아니라 `PiNodeMonitorWinForm` 바이너리까지 설치 경로로 자동 배포 성공.

---

## 7. 시스템 성숙도 및 기술적 결정 사항 (Finalized Decisions)

향후 개발 및 AI 협업 시 중복 분석을 방지하기 위해 현재 시스템의 확정된 상태를 기록합니다.

| 분류 | 현황 | 기술적 결정 근거 |
|:--- | :--- | :--- |
| **매크로 엔진** | **구현 완료 (v1.2.1)** | `restart_container`, `restart_docker`, `restart_pi` 3종 핵심 명령이 백엔드 및 모바일 UI에 통합 완료됨. 추가 확장이 필요한 경우에만 신규 개발 진행. |
| **알림 시스템** | **구현 완료 (v1.0.0)** | 텔레그램 봇(`/status`, `/reboot`) 및 SMS/시스템 입금·장애 알림이 서비스 레이어에 완전히 통합됨. |
| **터널 보안** | **Quick Tunnel 유지** | 사용자 편의성을 극대화하기 위해 `--url` (trycloudflare.com) 기반 익명 터널 방식을 유지함. 보안은 앱 레벨의 **PIN 인증**으로 충분하다고 판단함. |
| **앱 통합** | **인젝션 완료** | `inject_stats_saver.js`를 통해 공식 앱 내부 지표를 실시간으로 가로채어 `node_stats.json`에 기록하는 구조가 확정됨. |
| **성능 최적화** | **구현 완료 (v1.7.6)** | 하이브리드 CPU(P/E) 스케줄링 로직 강화 패치 적용. Synced 상태 시 12-19번 코어(E-core) 자동 할당 및 UI 실시간 시각화 검증 완료. |

---

## 8. 향후 로드맵 (Roadmap)
- **알림 통합**: 오류 감지 시 텔레그램으로 현재 화면 스크린샷 및 시스템 사양 정보 자동 전송.
- **보안 강화**: Cloudflare Tunnel을 통한 종단간 암호화(E2EE) 통신 표준화.
- **자동 복구**: Docker 데몬 중단 감지 시 자가 치유(Self-healing) 로직 고도화.
