using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace PiNodeMonitorWinForm.Services
{
    /// <summary>
    /// Gurupia Thread Director (GTD) Engine
    /// 하이브리드 아키텍처 최적화를 위한 범용 프로세스 제어 유틸리티
    /// </summary>
    public class ThreadDirectorEngine
    {
        public class ManagementRule
        {
            public string ProcessPattern { get; set; }
            public PerfUtility.CpuGroup CpuGroup { get; set; } = PerfUtility.CpuGroup.All;
            public ProcessPriorityClass? Priority { get; set; }
            public string Description { get; set; }
        }

        private List<ManagementRule> _rules = new List<ManagementRule>();
        private static string RulesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gtd_rules.json");

        public void LoadRules()
        {
            try
            {
                if (File.Exists(RulesPath))
                {
                    _rules = JsonConvert.DeserializeObject<List<ManagementRule>>(File.ReadAllText(RulesPath));
                }
                else
                {
                    // 기본 규칙 생성 (예시)
                    _rules = new List<ManagementRule>
                    {
                        new ManagementRule { ProcessPattern = "vmmem*", CpuGroup = PerfUtility.CpuGroup.All, Priority = ProcessPriorityClass.AboveNormal, Description = "WSL 연산 엔진 우선순위 강화" },
                        new ManagementRule { ProcessPattern = "PiNodeMonitor*", CpuGroup = PerfUtility.CpuGroup.ECoresOnly, Description = "모니터링 앱은 E-코어로 격리" },
                        new ManagementRule { ProcessPattern = "Docker Desktop*", CpuGroup = PerfUtility.CpuGroup.ECoresOnly, Description = "도커 관리 툴은 저전력 코어로" }
                    };
                    SaveRules();
                }
            }
            catch { }
        }

        public void SaveRules()
        {
            try { File.WriteAllText(RulesPath, JsonConvert.SerializeObject(_rules, Formatting.Indented)); } catch { }
        }

        /// <summary>
        /// 활성화된 모든 규칙을 현재 실행 중인 프로세스에 실시간 적용
        /// </summary>
        public void ApplyRules()
        {
            var allProcesses = Process.GetProcesses();

            foreach (var rule in _rules)
            {
                var targets = allProcesses.Where(p => 
                    p.ProcessName.IndexOf(rule.ProcessPattern.Replace("*",""), StringComparison.OrdinalIgnoreCase) >= 0);

                foreach (var p in targets)
                {
                    try
                    {
                        ApplyRuleToProcess(p, rule);
                    }
                    catch { /* Access Denied 등 예외 처리 */ }
                }
            }
        }

        private void ApplyRuleToProcess(Process p, ManagementRule rule)
        {
            // 1. CPU Affinity 적용
            long mask = 0;
            int processorCount = Environment.ProcessorCount;

            if (rule.CpuGroup == PerfUtility.CpuGroup.All)
            {
                for (int i = 0; i < processorCount; i++) mask |= (1L << i);
            }
            else if (rule.CpuGroup == PerfUtility.CpuGroup.PCoresOnly)
            {
                int limit = Math.Min(processorCount, 12);
                for (int i = 0; i < limit; i++) mask |= (1L << i);
            }
            else if (rule.CpuGroup == PerfUtility.CpuGroup.ECoresOnly)
            {
                if (processorCount > 12)
                    for (int i = 12; i < processorCount; i++) mask |= (1L << i);
                else
                    for (int i = 0; i < processorCount; i++) mask |= (1L << i);
            }

            if (mask != 0 && p.ProcessorAffinity != (IntPtr)mask)
            {
                p.ProcessorAffinity = (IntPtr)mask;
                Debug.WriteLine($"[GTD] Applied Affinity {rule.CpuGroup} to {p.ProcessName}");
            }

            // 2. 우선순위 적용
            if (rule.Priority.HasValue && p.PriorityClass != rule.Priority.Value)
            {
                p.PriorityClass = rule.Priority.Value;
                Debug.WriteLine($"[GTD] Applied Priority {rule.Priority} to {p.ProcessName}");
            }
        }
    }
}
