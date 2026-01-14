# Form1.cs Dashboard 분리 계획서

## 개요
`Form1.cs` (1,033줄, 50KB)에서 Dashboard 관련 로직을 `DashboardService.cs`로 분리하여 유지보수성을 개선합니다.

---

## 현재 문제점

| 항목 | 설명 |
|------|------|
| 파일 크기 | 50KB, 1,033줄 (과대) |
| 책임 혼재 | UI + 비즈니스 로직 + 이벤트 핸들러 혼합 |
| 테스트 불가 | UI 컨트롤 직접 참조로 단위 테스트 불가 |
| 중복 로직 | Docker 명령 실행, 상태 파싱 등 중복 |

---

## 분리 대상 메서드

### 1. `UpdateDashboardAsync()` (Line 524~820, 약 300줄)
- Docker 컨테이너 상태 확인
- CPU/RAM 사용량 조회
- 포트 체크
- Node 상태 JSON 파싱

### 2. `RunDockerCommandAsync()` (Line 925~950)
- Docker CLI 명령 실행 및 결과 반환

### 3. `ParseMetricValue()` (Line 952~985)
- Docker stats 출력 파싱

---

## 제안 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                        Form1.cs                             │
│  - UI 컨트롤 관리                                           │
│  - 이벤트 핸들러                                           │
│  - DashboardService 호출                                   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   DashboardService.cs                       │
│  - GetDashboardStatusAsync() : DashboardStatus             │
│  - RunDockerCommandAsync(cmd) : string                     │
│  - ParseMetricValue(output) : (cpu, ram)                   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    DashboardStatus.cs                       │
│  (DTO: 컨테이너 상태, CPU, RAM, 포트 등)                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 구현 단계

### Phase 1: DTO 정의
```csharp
// Services/DashboardStatus.cs
public class DashboardStatus
{
    public bool IsContainerRunning { get; set; }
    public string ActiveContainerName { get; set; }
    public string CpuUsage { get; set; }
    public string RamUsage { get; set; }
    public NodeState State { get; set; }
    public int Incoming { get; set; }
    public int Outgoing { get; set; }
    public int LocalBlock { get; set; }
    public int LedgerAge { get; set; }
    public bool IsPort31400Open { get; set; }
    public bool IsPort31401Open { get; set; }
}
```

### Phase 2: DashboardService 생성
```csharp
// Services/DashboardService.cs
public class DashboardService
{
    public async Task<DashboardStatus> GetDashboardStatusAsync()
    {
        var status = new DashboardStatus();
        
        // 1. 컨테이너 상태 확인
        status.ActiveContainerName = await DetectActiveContainerAsync();
        status.IsContainerRunning = !string.IsNullOrEmpty(status.ActiveContainerName);
        
        // 2. Docker stats 조회
        if (status.IsContainerRunning)
        {
            var (cpu, ram) = await GetContainerStatsAsync(status.ActiveContainerName);
            status.CpuUsage = cpu;
            status.RamUsage = ram;
        }
        
        // 3. Node 상태 조회
        await FetchNodeStatusAsync(status);
        
        return status;
    }
    
    private async Task<string> DetectActiveContainerAsync() { ... }
    private async Task<(string cpu, string ram)> GetContainerStatsAsync(string name) { ... }
    private async Task FetchNodeStatusAsync(DashboardStatus status) { ... }
}
```

### Phase 3: Form1.cs 수정
```csharp
// Form1.cs (수정 후)
private readonly DashboardService _dashboardService = new DashboardService();

private async Task UpdateDashboardAsync()
{
    var status = await _dashboardService.GetDashboardStatusAsync();
    
    // UI 업데이트만 수행
    UpdateContainerUI(status);
    UpdateNodeStatusUI(status);
    UpdatePortStatusUI(status);
}

private void UpdateContainerUI(DashboardStatus status)
{
    if (status.IsContainerRunning)
    {
        btnToggleNode.Text = "Node is ON (Click to OFF)";
        btnToggleNode.BackColor = Color.LightGreen;
        lblCPU.Text = status.CpuUsage;
        lblRAM.Text = status.RamUsage;
    }
    else
    {
        btnToggleNode.Text = "Node is OFF (Click to ON)";
        btnToggleNode.BackColor = Color.Salmon;
    }
}
```

---

## 예상 결과

| 항목 | Before | After |
|------|--------|-------|
| Form1.cs 크기 | 1,033줄 | ~700줄 |
| DashboardService.cs | - | ~200줄 |
| 테스트 가능성 | ❌ | ✅ |
| 코드 재사용 | ❌ | ✅ |

---

## 주의사항

1. **UI 스레드**: `DashboardService`는 UI를 직접 참조하지 않음. `Form1`에서 `Invoke` 필요 시 처리.
2. **MobileServer 연동**: `MobileServer.CurrentStatus`에 `DashboardStatus` 값 동기화 필요.
3. **점진적 적용**: 한 번에 모든 로직 이동 X, 메서드 단위로 점진적 분리.

---

## 예상 소요 시간
- DTO 정의: 5분
- DashboardService 생성: 20분
- Form1.cs 수정: 15분
- 빌드 테스트 및 오류 수정: 20분
- **총 예상: 1시간**

---

## 다음 세션 작업 체크리스트
- [ ] `Services/DashboardStatus.cs` 생성
- [ ] `Services/DashboardService.cs` 생성
- [ ] `Form1.cs`에서 `UpdateDashboardAsync` 리팩토링
- [ ] `MobileServer.CurrentStatus` 연동 확인
- [ ] 빌드 테스트 통과
- [ ] 커밋 및 푸시
