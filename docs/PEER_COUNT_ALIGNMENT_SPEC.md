# 인커밍/아웃고잉 피어 카운트 정합성 명세서 (Peer Count Semantic Alignment Specification)

> **목적:** 본 문서는 공식 Pi Node 앱과 100% 동일한 인커밍/아웃고잉 피어 수치를 표시하기 위한 핵심 알고리즘을 명세합니다. 리팩토링 또는 기능 추가 시 이 명세를 준수하여 정합성을 유지해야 합니다.

---

## 1. 문제 정의 (Problem Statement)

### 1.1. 배경
Pi Node의 Stellar Core API (`localhost:31402/info` 또는 컨테이너 내부 `11626/info`)는 연결된 피어의 총 수(`authenticated_count`)만 제공하고, **인커밍(Incoming)과 아웃고잉(Outgoing)의 방향성 정보를 누락**하는 경우가 많습니다.

이로 인해 다음과 같은 문제가 발생했습니다:
- 모니터 앱에서 인커밍 연결이 `0`으로 표시됨
- "Supporting" 상태가 실제와 달리 `No`로 표시됨
- 공식 Pi Node 앱의 대시보드 수치와 불일치

### 1.2. 목표
공식 Pi Node 앱이 사용하는 것과 **동일한 휴리스틱**을 적용하여, 어떤 API 경로(Primary/Failover)에서 데이터를 가져오든 일관된 수치를 표시합니다.

---

## 2. Algorithm v3: 총 피어 수 기반 추론 (Total-based Inference)

### 2.1. 알고리즘 개요
Stellar API가 `authenticated_count` (총 인증된 피어 수)만 제공할 때 적용됩니다.

```
입력: T = authenticated_count (정수, 0 이상)
출력: Outbound, Inbound (각각 정수)

Outbound = min(T, 8)
Inbound  = T - Outbound
if Inbound < 0 then Inbound = 0
```

### 2.2. 핵심 가정 (Critical Assumption)
- Pi Node는 **최대 8개의 아웃바운드 연결**을 유지하도록 설계되어 있습니다.
- 총 피어 수가 8 이하이면, 모든 연결이 아웃바운드일 가능성이 높습니다.
- 총 피어 수가 8을 초과하면, 초과분은 인바운드 연결입니다.

### 2.3. 코드 참조
**파일:** `MainPresenter.cs`  
**위치:** `ParseNodeInfoJson` 메서드 내부
**라인:** 274-284

```csharp
// [Algorithm v3] Semantic Alignment with Official Pi App
// Outbound: Up to 8 authenticated peers are typically outbound.
// Inbound: The remainder of authenticated peers.
int outbound = (auth > 8) ? 8 : auth;
int inbound = auth - outbound;
if (inbound < 0) inbound = 0;

_currentMetrics.OutgoingConnections = outbound.ToString();
_currentMetrics.IncomingConnections = inbound.ToString();
_currentMetrics.IsSupporting = inbound > 0 ? "Yes" : "No";
```

### 2.4. 정합성 규칙 (Invariants)
| 조건 | Outbound | Inbound | IsSupporting |
|:---|:---:|:---:|:---:|
| `auth == 0` | 0 | 0 | No |
| `auth == 5` | 5 | 0 | No |
| `auth == 8` | 8 | 0 | No |
| `auth == 10` | 8 | 2 | Yes |
| `auth == 15` | 8 | 7 | Yes |

---

## 3. Address-Position-Aware Parser: 네트워크 명령 기반 분석

### 3.1. 알고리즘 개요
`ss -ant` 또는 `netstat -an` 명령의 출력을 분석하여 정밀한 방향성을 판별합니다. 컨테이너 환경에 따라 출력 형식(컬럼 위치)이 다를 수 있으므로, **동적으로 주소 컬럼을 탐지**합니다.

### 3.2. 판별 로직
```
1. ESTAB(또는 ESTABLISHED) 상태이고 포트 31400을 포함하는 라인만 처리
2. 공백/탭 기준으로 라인을 분리
3. IP:Port 또는 IP.Port 형식을 포함하는 컬럼 인덱스를 수집
4. 최소 2개의 주소 컬럼이 발견되어야 유효 (Local, Remote)
5. 첫 번째 주소 컬럼의 포트가 31400이면 → Incoming (내가 리스닝하는 포트로 들어온 연결)
6. 두 번째 주소 컬럼의 포트가 31400이면 → Outgoing (내가 상대방의 31400 포트로 나간 연결)
```

### 3.3. 코드 참조
**파일:** `NodeUtility.cs`  
**메서드:** `GetContainerPeerCountsAsync`
**라인:** 199-251

```csharp
// IP/주소 형식을 포함하는 컬럼들만 추출
List<int> addrIndices = new List<int>();
for (int i = 0; i < parts.Length; i++)
{
    if (parts[i].Contains(".") || parts[i].Contains(":"))
    {
        addrIndices.Add(i);
    }
}

// 최소 2개의 주소 컬럼(로컬, 리모트)이 발견되어야 함
if (addrIndices.Count >= 2)
{
    int localIdx = addrIndices[0];
    int remoteIdx = addrIndices[1];
    
    bool localIsPi = parts[localIdx].EndsWith(":31400") || parts[localIdx].EndsWith(".31400");
    bool remoteIsPi = parts[remoteIdx].EndsWith(":31400") || parts[remoteIdx].EndsWith(".31400");

    // 1. 내 주소(로컬)가 31400이면 -> 밖에서 들어온 것 (Incoming)
    if (localIsPi) incoming++;
    // 2. 상대방 주소(리모트)가 31400이면 -> 내가 나간 것 (Outgoing)
    else if (remoteIsPi) outgoing++;
}
```

### 3.4. 도구 부재 시 Fallback
`ss` 또는 `netstat`가 컨테이너에 설치되어 있지 않은 경우:
- 출력이 비어있거나 `not found` 문자열이 포함됨
- **즉시 `(0, 0)`을 반환**하고 Algorithm v3 결과를 신뢰함

```csharp
if (string.IsNullOrEmpty(output) || output.Contains("not found")) return (0, 0);
```

---

## 4. 데이터 흐름도 (Data Flow Diagram)

```mermaid
graph TD
    A[Polling Timer] --> B{Primary API<br/>localhost:31401}
    B -->|Success| C[Parse peers/incoming_count<br/>peers/outgoing_count]
    B -->|Fail| D{Stellar Failover API<br/>localhost:31402 or 11626}
    
    D -->|Success| E[Parse peers/authenticated_count]
    E --> F[Apply Algorithm v3<br/>Outbound=min(auth,8), Inbound=auth-Outbound]
    
    C -->|Both zero?| G{Container Netstat/ss}
    G -->|Tool available| H[Address-Position-Aware Parser]
    G -->|Tool missing| I[Trust Algorithm v3 Result]
    
    H --> J[Update Metrics]
    F --> J
    I --> J
    C -->|Values available| J
    
    J --> K[UI Display]
```

---

## 5. 테스트 시나리오 (Validation Scenarios)

### 5.1. Unit Test Cases for Algorithm v3

| Test Case | `authenticated_count` | Expected `Outbound` | Expected `Inbound` | Expected `IsSupporting` |
|:---|:---:|:---:|:---:|:---:|
| 연결 없음 | 0 | 0 | 0 | No |
| 아웃바운드만 (적음) | 3 | 3 | 0 | No |
| 아웃바운드 최대 | 8 | 8 | 0 | No |
| 인바운드 존재 | 12 | 8 | 4 | Yes |
| 대규모 피어 | 50 | 8 | 42 | Yes |

### 5.2. Integration Test for Netstat Parser

| 라인 예시 | Expected Result |
|:---|:---|
| `ESTAB 0 0 192.168.1.5:31400 10.0.0.1:45678` | Incoming +1 |
| `ESTAB 0 0 192.168.1.5:45678 10.0.0.1:31400` | Outgoing +1 |
| `ESTAB 0 0 192.168.1.5:8080 10.0.0.1:443` | (무시, 31400 없음) |
| `LISTEN` (상태만 다름) | (무시, ESTAB 아님) |

---

## 6. 리팩토링 가이드라인 (Refactoring Guidelines)

### 6.1. 절대 변경 금지 (Do Not Modify)
- **`8`이라는 상수 값**: Pi Node의 아웃바운드 연결 한계치입니다. 변경 시 공식 앱과 불일치가 발생합니다.
- **포트 `31400`**: Pi 네트워크의 표준 P2P 포트입니다.
- **주소 컬럼 탐지 로직**: `Contains(".")` 또는 `Contains(":")`는 IPv4/IPv6를 모두 커버합니다.

### 6.2. 허용되는 변경
- 로깅 추가
- 성능 최적화 (StringBuilder 사용 등)
- 추가 fallback 경로 구현 (새로운 API 엔드포인트 등)

### 6.3. 검증 필수 사항
코드 변경 후 반드시 다음을 확인:
1. **공식 Pi Node 앱**을 나란히 실행하여 수치가 동일한지 확인
2. 위의 테스트 시나리오를 모두 통과하는지 확인
3. Stellar Failover 상태에서도 정상 동작 확인

---

## 7. 관련 파일 목록

| 파일 | 역할 |
|:---|:---|
| `MainPresenter.cs` | Algorithm v3 구현, 메트릭 파싱 |
| `NodeUtility.cs` | Container 네트워크 분석, GetContainerPeerCountsAsync |
| `NodeMetrics.cs` | 데이터 모델 (IncomingConnections, OutgoingConnections, IsSupporting) |

---

## 버전 이력

| 버전 | 날짜 | 변경 내용 |
|:---|:---|:---|
| 1.0 | 2026-01-28 | 초기 명세 작성. Algorithm v3 및 Address-Position-Aware Parser 문서화. |
