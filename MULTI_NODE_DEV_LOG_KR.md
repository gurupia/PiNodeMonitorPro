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
- **컴포넌트**: `MobileServer.cs` (내장 Kestrel 웹 서버)
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
-   **코드 최적화**: 전문가급 FFI Pack 및 Zero-copy(Span<T>) 지향 설계를 통한 오버헤드 최소화.

---

## 7. 향후 로드맵 (Roadmap)
- **매크로 기능**: 자주 쓰는 복구 작업(예: "Docker 재시작", "포트 체크 스킵")을 버튼 하나로 자동 수행.
- **알림 통합**: 오류 감지 시 텔레그램으로 현재 화면 스크린샷 및 시스템 사양 정보 자동 전송.
- **보안 강화**: Cloudflare Tunnel을 통한 종단간 암호화(E2EE) 통신 표준화.
