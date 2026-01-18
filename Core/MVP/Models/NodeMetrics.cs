using System;

namespace PiNodeMonitorWinForm.Core.MVP.Models
{
    public class NodeMetrics
    {
        public bool IsDockerRunning { get; set; }
        public string ActiveContainerName { get; set; } = "N/A";
        public string ConsensusState { get; set; } = "Unknown";
        public string LocalBlock { get; set; } = "0";
        public string LatestBlock { get; set; } = "0";
        public TimeSpan Uptime { get; set; }
        public int IncomingConnections { get; set; }
        public int OutgoingConnections { get; set; }
        public bool IsSupporting { get; set; }
        public string ProtocolVersion { get; set; } = "N/A";
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        
        // Port Status
        public bool IsPort31401Open { get; set; }
        public bool IsPort31402Open { get; set; }
        public bool IsPort31403Open { get; set; }
    }
}
