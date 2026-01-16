using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PiNodeMonitorWinForm
{
    public static class PerfUtility
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessAffinityMask(IntPtr hProcess, IntPtr dwProcessAffinityMask);

        /// <summary>
        /// i5-14600K (6P + 8E = 20 Threads) 최적화:
        /// 특정 코어 그룹으로 프로세스를 할당합니다.
        /// </summary>
        public enum CpuGroup
        {
            All,
            PCoresOnly, // 0-11 (6 P-cores with HyperThreading)
            ECoresOnly  // 12-19 (8 E-cores)
        }

        public static void SetCpuAffinity(CpuGroup group)
        {
            try
            {
                int processorCount = Environment.ProcessorCount;
                long mask = 0;

                if (group == CpuGroup.All)
                {
                    for (int i = 0; i < processorCount; i++) mask |= (1L << i);
                }
                else if (group == CpuGroup.PCoresOnly)
                {
                    // P-cores are usually first. For 14600K, it's 0-11.
                    int limit = Math.Min(processorCount, 12);
                    for (int i = 0; i < limit; i++) mask |= (1L << i);
                }
                else if (group == CpuGroup.ECoresOnly)
                {
                    // E-cores for 14600K are 12-19.
                    if (processorCount > 12)
                    {
                        for (int i = 12; i < processorCount; i++) mask |= (1L << i);
                    }
                    else
                    {
                        // Fallback to all if not enough cores
                        for (int i = 0; i < processorCount; i++) mask |= (1L << i);
                    }
                }

                if (mask != 0)
                {
                    Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)mask;
                    System.Diagnostics.Debug.WriteLine($"[PerfUtility] CPU Affinity set to {group} (Mask: {mask:X})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PerfUtility] Failed to set CPU affinity: {ex.Message}");
            }
        }
    }
}
