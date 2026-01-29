# 🚀 고성능 DXGI 화면 캡처 엔진 통합 가이드 (C++ to C# Interop)

이 문서는 **Pi Node Monitor Pro**의 원격 화면 전송 성능을 극대화하기 위해, 기존의 C# 내장 캡처 방식(CPU)을 **C++ 기반의 고성능 DXGI 캡처 엔진(GPU)**으로 교체하는 과정을 단계별로 설명합니다.

---

---

## 1. 하이브리드 엔진 아키텍처 (Hybrid Engine Architecture)

우리는 단순히 DXGI로 GDI+를 대체하는 것이 아니라, **두 엔진을 모두 탑재하고 상황에 따라 선택**하는 **하이브리드 방식**을 채택했습니다.

### **A. DXGI 엔진 (High Performance Mode)**
- **용도**: PC 환경, 고성능 네트워크
- **특징**: GPU 가속, 60FPS, 고화질, 낮은 CPU 점유율.

### **B. GDI+ 엔진 (Safe / Mobile Mode)**
- **용도**: 모바일 환경, DXGI 호환성 문제 발생 시 (Fallback)
- **특징**: **스마트 리사이징(Smart Resizing)**을 적용하여 해상도를 FHD/HD급으로 낮춰 데이터 전송량을 대폭 줄임. 15FPS 제한으로 모바일 배터리와 데이터를 절약.

---

## 2. 준비물 (Prerequisites)

1.  **C++ DLL 파일**: `GurupiaCapture.Core.dll` (x64 Release 빌드 권장)
    - *위치*: 프로젝트 루트 폴더 (`PiNodeMonitorWinForm` 폴더 내)
2.  **개발 환경**: Visual Studio 또는 VS Code (.NET 8.0 SDK)

---

## 3. 단계별 통합 가이드 (Step-by-Step)

### **Step 1: DLL 파일 배치 및 설정**

가장 먼저 C++ 엔진(DLL)을 C# 프로젝트가 인식할 수 있는 곳에 두어야 합니다.

1.  `GurupiaCapture.Core.dll` 파일을 `PiNodeMonitorWinForm` 폴더로 복사합니다.
2.  **프로젝트 파일(.csproj) 수정**:
    - 이 DLL이 빌드(Build)나 배포(Publish)될 때 항상 따라다니도록 설정해야 합니다.
    - `PiNodeMonitorWinForm.csproj` 파일을 열고 `<Project>` 태그 안에 아래 내용을 추가합니다.

```xml
  <ItemGroup>
    <!-- DLL을 출력 폴더로 자동 복사 -->
    <None Update="GurupiaCapture.Core.dll">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```

### **Step 2: C# 래퍼(Wrapper) 클래스 만들기**

C#은 C++ 코드를 직접 알아들을 수 없습니다. 중간 번역기 역할을 하는 **Interop(상호 운용)** 클래스가 필요합니다. 이것을 **P/Invoke**라고 부릅니다.

1.  프로젝트에 `NativeCapture.cs` 파일을 새로 만듭니다.
2.  아래 코드를 복사해서 붙여넣습니다.

```csharp
using System;
using System.Runtime.InteropServices; // P/Invoke를 위한 네임스페이스

namespace PiNodeMonitorWinForm
{
    public static class NativeCapture
    {
        // C++ DLL 파일 이름 (확장자 .dll 생략 가능)
        private const string DLL_NAME = "GurupiaCapture.Core.dll";

        // 1. 엔진 초기화 함수 (한 번만 호출)
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeDxgi();

        // 2. 고속 캡처 함수
        // monitorIndex: 모니터 번호 (0: 주모니터)
        // outBuffer: 이미지 데이터가 담길 메모리 주소 (C++이 할당해줌)
        // outSize: 이미지 데이터 크기
        // quality: JPEG 품질 (1~100)
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CaptureScreenToMemory(int monitorIndex, out IntPtr outBuffer, out int outSize, int quality);

        // 3. 메모리 해제 함수 (필수!)
        // C++이 빌려준 메모리는 반드시 반납해야 메모리 누수(Leak)가 안 생깁니다.
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemory(IntPtr buffer);
    }
}
```

### **Step 3: 서버 코드(`MobileServer.cs`) 수정 - 하이브리드 로직**

사용자가 `?engine=dxgi` 파라미터로 모드를 선택할 수 있게 하고, GDI+ 모드일 때는 **스마트 리사이징**을 적용합니다.

```csharp
// 1. 요청된 엔진 확인
string? reqEngine = context.Request.Query["engine"];
bool useDxgi = (reqEngine == "dxgi");

byte[] imageBytes = null;

// 2. DXGI 시도 (사용자가 원할 경우)
if (useDxgi)
{
    if (NativeCapture.CaptureScreenToMemory(0, out IntPtr buffer, out int size, 65))
    {
        // 성공 시 버퍼 복사
        imageBytes = new byte[size];
        Marshal.Copy(buffer, imageBytes, 0, size);
        NativeCapture.FreeMemory(buffer);
        LastCaptureMode = "DXGI (High Perf)";
    }
}

// 3. GDI+ Fallback (DXGI 실패 혹은 안전 모드)
if (imageBytes == null)
{
    using (var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
    {
        using (var g = Graphics.FromImage(bmp)) g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
        
        // **Smart Resizing**: 모바일 성능을 위해 가로 1280px로 리사이징
        // 이는 데이터 전송량을 70% 이상 줄여줍니다.
        using (var thumb = bmp.GetThumbnailImage(1280, 720, null, IntPtr.Zero))
        using (var ms = new MemoryStream())
        {
            thumb.Save(ms, ImageFormat.Jpeg);
            imageBytes = ms.ToArray();
        }
    }
    LastCaptureMode = "GDI+ (Safe)";
}
```

---

## 4. 트러블슈팅 (Troubleshooting)

### **Q1. `DllNotFoundException` 오류가 떠요!**
- **원인**: `GurupiaCapture.Core.dll` 파일이 실행 파일(`PiNodeMonitorWinForm.exe`)과 같은 폴더에 없어서 그렇습니다.
- **해결**:
  1. `Step 1`의 `.csproj` 설정을 다시 확인하세요.
  2. 빌드 후 `bin/Release/net8.0-windows/win-x64` 폴더에 DLL이 들어있는지 눈으로 확인하세요.
  3. **Visual C++ Redistributable 2015-2022**가 설치되어 있는지 확인하세요. (대부분의 C++ DLL은 이게 필요합니다.)

### **Q2. `BadImageFormatException` 오류가 떠요!**
- **원인**: 32비트(x86)와 64비트(x64)가 섞여서 그렇습니다.
- **해결**:
  - C++ DLL이 x64라면, C# 프로젝트도 반드시 **Use `win-x64`** (또는 `Platform: x64`)로 빌드해야 합니다.
  - `dotnet publish -r win-x64` 명령어를 사용하세요.

### **Q3. 화면이 검게 나와요.**
- **원인**: DRM이 걸려 있는 화면(넷플릭스 등)이거나, 전체 화면 독점 모드 게임 실행 중일 때 간혹 발생합니다.
- **해결**: 이는 윈도우 보안 정책상 정상입니다. 일반 바탕화면 상태에서 테스트해보세요.

---

## 5. 결론 및 다음 단계 (Conclusion)

이제 여러분의 Pi Node Monitor는 **상용 원격 제어 프로그램급의 캡처 엔진**을 탑재했습니다.
초보 C# 개발자 단계를 넘어, **Native C++ 라이브러리를 자유자재로 다루는 중급 개발자** 영역에 진입하신 것을 축하드립니다! 🎉

### 👉 다음 단계: 입력 제어 (Input Control)
화면을 보는 것만으로 충분하지 않습니다. 마우스와 키보드로 PC를 제어하려면 다음 가이드를 참고하세요.
- **[웹 기반 원격 제어 가이드 (WEB_REMOTE_CONTROL_GUIDE.md)](WEB_REMOTE_CONTROL_GUIDE.md)**: 드래그 앤 드롭 구현, 오버레이 실드 패턴 등
