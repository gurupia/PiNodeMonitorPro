using System;

namespace PiNodeMonitorWinForm.Services
{
    public class NodeMonitorService
    {
        private string _lastBlock = "";
        private DateTime _lastBlockChangeTime = DateTime.MinValue;
        private DateTime _lastAlertTime = DateTime.MinValue;

        // Configuration
        private const int STUCK_THRESHOLD_MINUTES = 30; // 30분 동안 블록 안 움직이면 경고
        private const int ALERT_COOLDOWN_HOURS = 6;     // 알림 후 6시간 동안 재발송 금지

        public NodeMonitorService()
        {
            _lastBlockChangeTime = DateTime.Now;
        }

        /// <summary>
        /// Check node status and return alert message if something is wrong.
        /// Returns null if status is OK or cooldown is active.
        /// </summary>
        public string CheckNodeStatus(string currentBlock, string status)
        {
            // Update Block Time
            if (_lastBlock != currentBlock)
            {
                _lastBlock = currentBlock;
                _lastBlockChangeTime = DateTime.Now;
                return null; // Block is moving, all good.
            }

            // Check if Stuck
            if (string.IsNullOrEmpty(currentBlock) || currentBlock == "0" || currentBlock == "1") 
                return null; // Ignore startup phase

            var timeSinceLastChange = DateTime.Now - _lastBlockChangeTime;
            
            if (timeSinceLastChange.TotalMinutes >= STUCK_THRESHOLD_MINUTES)
            {
                // Verify Cooldown
                if ((DateTime.Now - _lastAlertTime).TotalHours < ALERT_COOLDOWN_HOURS)
                    return null; // In Cooldown

                // Trigger Alert
                _lastAlertTime = DateTime.Now;
                return $"[WARNING] Pi Node Stuck! Block {currentBlock} hasn't moved for {timeSinceLastChange.TotalMinutes:0} mins.";
            }

            return null;
        }
    }
}
