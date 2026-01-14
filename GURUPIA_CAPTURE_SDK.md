# GurupiaCapture SDK Guide

**Version**: 1.0.0  
**Module**: `GurupiaCapture.Core.dll`  
**Platform**: Windows (x64)

## 1. Overview
`GurupiaCapture.Core.dll` is a high-performance native library designed for screen capture, video recording, and image processing. It bridges the gap between managed languages (like C#) and low-level system APIs, providing access to:

*   **Hybrid Capture Engine**: Seamless switching between **GDI+** (Maximum Compatibility) and **DXGI Desktop Duplication** (High Performance, Low Latency).
*   **Screen Recording**: Record screen activity to **GIF**, **WebP** (Animated), or **MP4**.
*   **Advanced Export**: optimized WebP encoding and standard image formats.
*   **Audio Loopback**: System audio recording support.

---

## 2. Integration Basics

The DLL exports standard C-style functions (`extern "C"`) calling convention `__cdecl`.

### Loading the Library
Ensure `GurupiaCapture.Core.dll` is in the same directory as your executable or in the system PATH.

### C# / .NET (P/Invoke)
```csharp
using System.Runtime.InteropServices;

[DllImport("GurupiaCapture.Core.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int Engine_Initialize();
```

### C++ (Dynamic Loading)
```cpp
typedef int(*FuncInit)();
HMODULE hDll = LoadLibrary(L"GurupiaCapture.Core.dll");
FuncInit init = (FuncInit)GetProcAddress(hDll, "Engine_Initialize");
init();
```

---

## 3. Data Structures

### ImageBuffer
Raw image data container. **Important**: You must release this memory using `Image_Free`.

| Field  | Type | Description |
|:---:|:---:|:---|
| `width` | `int` | Image width in pixels |
| `height` | `int` | Image height in pixels |
| `stride` | `int` | Bytes per row (usually `width * 4`) |
| `data` | `IntPtr` | Pointer to raw BGRA pixel data |
| `size` | `IntPtr` | Total size of buffer in bytes |

### CaptureOptions
Configuration for single frame capture.

| Field | Type | Description |
|:---:|:---:|:---|
| `includeCursor` | `bool` | Draw the mouse cursor on the image |
| `delayMs` | `int` | Delay before capture (optional) |
| `engineType` | `int` | **0 = GDI+** (Safe), **1 = DXGI** (Fast) |

### RecordOptions
Configuration for screen recording.

| Field | Type | Description |
|:---:|:---:|:---|
| `fps` | `int` | Target Frames Per Second |
| `quality` | `int` | Image Quality (1-100) |
| `maxFrames` | `int` | Stop after N frames (0 = infinite) |
| `maxSizeMB` | `int` | Stop after N MB usage (0 = infinite) |
| `recordAudio` | `bool` | Enable system audio recording |
| `outputFilePath` | `wchar_t*` | Direct file path for real-time encoding (optional) |
| `engineType` | `int` | 0=Auto, 1=GDI, 2=DXGI |

---

## 4. API Reference

### Initialization
Check `GC_OK` (0) for success.

*   `int Engine_Initialize()`: Starts GDI+/DXGI subsystems. Must call once.
*   `void Engine_Shutdown()`: Cleans up resources.
*   `int Engine_GetLastError()`: Returns the last error code.

### Screen Capture

*   `int Capture_FullScreen(ref ImageBuffer buffer, ref CaptureOptions opts)`
    *   Captures the primary monitor.
    *   Supports high-performance DXGI path.
    
*   `int Capture_Region(int x, int y, int w, int h, ref ImageBuffer buffer, ref CaptureOptions opts)`
    *   Captures a specific rectangle of the screen.

### Screen Recording

*   `int Recorder_Start(int x, int y, int w, int h, ref RecordOptions opts)`
*   `int Recorder_Stop()`
*   `int Recorder_Pause()` / `int Recorder_Resume()`
*   `int Recorder_SaveGif(string path)`: Save recorded buffer as Animated GIF.
*   `int Recorder_SaveWebP(string path)`: Save recorded buffer as Animated WebP.
*   `int Recorder_SaveMP4(string path)`: Save recorded buffer as MP4 Video.

### Memory Management

*   `void Image_Free(ref ImageBuffer buffer)`
    *   **CRITICAL**: You must call this for every `ImageBuffer` filled by usage of Capture APIs to prevent memory leaks.

---

## 5. Error Codes

| Code | Name | Meaning |
|:---:|:---|:---|
| `0` | `GC_OK` | Success |
| `-1` | `GC_ERR_INVALID_PARAM` | Null pointer or invalid argument |
| `-2` | `GC_ERR_OUT_OF_MEMORY` | Memory allocation failed |
| `-3` | `GC_ERR_CAPTURE_FAILED` | Screen capture failed (e.g. UAC protected window) |
| `-9` | `GC_ERR_NOT_INITIALIZED` | Engine not initialized (Call Engine_Initialize) |

---

## 6. C# Integration Example

```csharp
public void CaptureScreen()
{
    // 1. Initialize
    NativeCapture.Engine_Initialize();

    try 
    {
        // 2. Prepare Structures
        var buffer = new ImageBuffer();
        var options = new CaptureOptions 
        { 
            includeCursor = true, 
            engineType = 1 // Try DXGI first
        };

        // 3. Capture
        int result = NativeCapture.Capture_FullScreen(ref buffer, ref options);

        if (result == NativeCapture.GC_OK)
        {
            // 4. Process (Convert IntPtr to Bitmap)
            using (var bmp = new Bitmap(
                buffer.width, buffer.height, buffer.stride, 
                PixelFormat.Format32bppRgb, buffer.data))
            {
                bmp.Save("screenshot.jpg", ImageFormat.Jpeg);
            }
        }
    }
    finally
    {
        // 5. Cleanup
        NativeCapture.Image_Free(ref buffer);
    }
}
```
