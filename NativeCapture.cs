using System;
using System.Runtime.InteropServices;

namespace PiNodeMonitorWinForm
{
    public static class NativeCapture
    {
        private const string DLL_NAME = "GurupiaCapture.Core.dll";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeDxgi();

        // Capture to memory buffer
        // monitorIndex: 0 for primary
        // outBuffer: Pointer to image data
        // outSize: Size of data in bytes
        // quality: JPEG Quality (1-100)
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CaptureScreenToMemory(int monitorIndex, out IntPtr outBuffer, out int outSize, int quality);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemory(IntPtr buffer);
    }
}
