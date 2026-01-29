using System;
using System.Runtime.InteropServices;

namespace PiNodeMonitorWinForm
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageBuffer
    {
        public int width;
        public int height;
        public int stride;
        public IntPtr data;
        public IntPtr size; 
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CaptureOptions
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool includeCursor;
        public int delayMs;
        public int engineType; // 0=GDI, 1=DXGI
    }

    public static class NativeCapture
    {
        private const string DLL_NAME = "GurupiaCapture.Core.dll";
        
        public const int GC_OK = 0;

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Engine_Initialize();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Engine_Shutdown();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Capture_FullScreen(ref ImageBuffer buffer, ref CaptureOptions options);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Image_Free(ref ImageBuffer buffer);
    }
}
