# 웹 기반 원격 제어 및 드래그 앤 드롭 구현 가이드 (Web-Based Remote Control Implementation Guide)

이 문서는 Pi Node Monitor의 웹 기반 원격 제어 기능, 특히 **PC 환경에서의 드래그 앤 드롭(Drag & Drop) 문제**를 해결하기 위해 적용된 기술적 솔루션을 상세히 기술합니다.

## 1. 문제 상황 (Problem Context)

### 1.1 MJPEG 스트리밍과 DOM의 한계
- **상황**: 서버는 MJPEG (`multipart/x-mixed-replace`) 방식으로 화면을 프레임 단위로 전송합니다.
- **현상**: 브라우저는 `<img>` 태그의 소스를 계속 새로고침합니다. 이 과정에서 이미지에 직접 연결된 마우스 이벤트 리스너가 끊기거나, 브라우저가 이미지를 "로딩 중" 상태로 인식하여 이벤트를 무시하는 현상이 발생합니다.

### 1.2 PC 브라우저의 기본 동작 간섭
- **현상**: PC 브라우저에서 이미지를 클릭하고 드래그하면, 웹 브라우저 고유의 **"이미지 끌기(Ghost Image Drag)"** 기능이 작동합니다.
- **결과**: `mousemove` 이벤트가 원격 제어 로직으로 전달되지 않고, 브라우저의 네이티브 드래그 동작이 실행되어 원격 창 이동이 불가능했습니다.

### 1.3 Win32 API의 한계 (`SetCursorPos`)
- **현상**: 서버 측에서 단순히 `SetCursorPos(x, y)` API만 사용하여 마우스 좌표를 옮겼습니다.
- **결과**: 윈도우 OS는 이를 "순간 이동(Teleport)"으로 인식할 뿐, 사용자가 마우스를 쥐고 "이동(Move)"하는 것으로 인식하지 않았습니다. 때문에 **클릭 상태에서의 이동(드래그)** 이벤트가 무시되었습니다.

---

## 2. 해결 솔루션 (Solutions)

이 문제는 **프론트엔드(입력 감지)**와 **백엔드(입력 주입)** 양쪽 모두를 근본적으로 개선하여 해결되었습니다.

### 2.1 프론트엔드: 투명 오버레이 실드 (Overlay Shield Pattern)

이미지 태그(`<img>`)에 직접 이벤트를 걸지 않고, 그 위에 **투명한 막(`<div>`)**을 씌워 입력을 전담시켰습니다.

#### 구조 변경
```html
<div id="videoWrapper" style="position:relative;">
    <!-- 1. MJPEG 스트리밍 (계속 새로고침됨) -->
    <img id="liveStream" style="z-index:1;" draggable="false" />

    <!-- 2. 투명 입력 실드 (절대 움직이지 않음, z-index 높음) -->
    <div id="inputShield" 
         style="position:absolute; top:0; left:0; width:100%; height:100%; z-index:10; cursor:default;"
         oncontextmenu="return false;">
    </div>
</div>
```

#### 효과
1.  **입력 안정성 100%**: 이미지가 깜빡이거나 로딩 중이어도, 상위 레이어인 `inputShield`는 항상 존재하므로 클릭/이동 이벤트를 놓치지 않습니다.
2.  **브라우저 간섭 차단**: 사용자는 기술적으로 '이미지'가 아닌 '빈 div'를 드래그하는 것이므로, 브라우저의 "이미지 파일 드래그" 기능이 발동하지 않습니다.

### 2.2 프론트엔드: 글로벌 이벤트 & 좌표 보정 (Global & Clamping)

마우스가 화면 밖으로 나갔을 때 드래그가 끊기는 문제를 방지했습니다.

1.  **Global Listener**: `mousemove`, `mouseup` 이벤트는 `div`가 아니라 `window` 전역 객체에 걸어, 마우스가 브라우저 밖으로 튀어나가도 추적합니다.
2.  **Coordinate Clamping**: 좌표가 화면 영역(`0 ~ Width`)을 벗어나면, 강제로 `0` 또는 `Max` 값으로 보정하여 서버에 전송합니다. 이를 통해 창을 화면 끝까지 밀어붙이는 동작이 가능해집니다.

### 2.3 백엔드: 하드웨어 레벨 입력 주입 (`mouse_event`)

단순 `SetCursorPos`를 버리고, `mouse_event`의 절대 좌표 모드를 사용하여 하드웨어 신호를 시뮬레이션했습니다.

#### 핵심 코드 (C#)
```csharp
// 화면 해상도를 65535 단위로 정규화 (전체 화면 절대 좌표계)
int absX = (int)((x * 65535) / screenW);
int absY = (int)((y * 65535) / screenH);

// MOUSEEVENTF_ABSOLUTE (0x8000) | MOUSEEVENTF_MOVE (0x0001)
// 이 플래그를 함께 사용해야 OS가 "물리적 마우스 이동"으로 인식함
mouse_event(0x8001 | actionFlag, absX, absY, 0, 0);
```

#### 효과
- OS는 이 신호를 실제 마우스 하드웨어 인터럽트와 동일하게 처리합니다.
- `MouseDown` 상태에서 이 `Move` 신호가 들어오면, OS는 정확하게 **"드래그(Drag)"** 동작으로 해석하여 창 이동, 텍스트 선택 등을 수행합니다.

---

## 3. 요약 (Summary)

| 구분 | 기존 방식 | **최종 개선 방식** |
| :--- | :--- | :--- |
| **입력 타겟** | `<img>` 태그 직접 클릭 | **투명 `<div>` 오버레이** |
| **드래그 방해** | 브라우저 Ghost 이미지 발생 | **CSS/구조적으로 원천 차단** |
| **이벤트 범위** | 요소 내부 국한 | **`window` 전역 감지 (Out of bounds 처리)** |
| **서버 입력** | `SetCursorPos` (순간 이동) | **`mouse_event` (Absolute Move 신호)** |
| **결과** | 클릭은 되지만 드래그 불가 | **완벽한 PC/모바일 드래그 앤 드롭 지원** |

이 아키텍처는 MJPEG 기반의 웹 원격 제어 솔루션에서 **표준(Standard)**으로 사용될 수 있는 가장 강력하고 안정적인 모델입니다.
