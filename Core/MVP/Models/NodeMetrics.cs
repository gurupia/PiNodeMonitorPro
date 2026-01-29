using System;

namespace PiNodeMonitorWinForm.Core.MVP.Models
{
    public class NodeMetrics
    {
        // Core Node Info
        public bool IsDockerRunning { get; set; }
        public string ActiveContainerName { get; set; }
        public string ConsensusState { get; set; } = "---";
        public TimeSpan Uptime { get; set; }
        public string Availability { get; set; } = "---";
        public double NodeBonus { get; set; }

        // Block Info
        public string LocalBlockNum { get; set; } = "---";
        public string RemoteBlockNum { get; set; } = "---";
        public string LatestLedgerNum { get; set; } = "---";
        public string LedgerAge { get; set; } = "0s";

        // Version Info
        public string ProtocolVersion { get; set; } = "---";
        public string CoreBuild { get; set; } = "---";

        // Resource Usage
        public string LocalCpuCount { get; set; } = "---";
        public string ServerCpuCount { get; set; } = "N/A";
        public string ContainerCpuUsage { get; set; } = "---";
        public string ContainerRamUsage { get; set; } = "---";

        // Network Stats
        public string IncomingConnections { get; set; } = "0";
        public string OutgoingConnections { get; set; } = "0";
        public string IsSupporting { get; set; } = "No";

        // Port Status
        public string Port31401Status { get; set; } = "Checking...";
        public string Port31402Status { get; set; } = "Checking...";
        public string Port31403Status { get; set; } = "Checking...";
    }
}
