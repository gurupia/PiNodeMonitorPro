using System;
using System.Drawing;
using PiNodeMonitorWinForm.Core.MVP.Models;

namespace PiNodeMonitorWinForm.Core.MVP.Views
{
    public interface IMainView
    {
        // Events
        event EventHandler ViewLoaded;
        event EventHandler ToggleNodeClicked;
        event EventHandler ShowHistoryClicked;
        event EventHandler SecureLinkClicked;
        event EventHandler MultiViewClicked;
        event EventHandler MobileConnectClicked;
        event EventHandler CompactDiskClicked;
        event EventHandler SaveWalletClicked;
        event EventHandler ChangeWalletClicked;
        event Action<double> ManualBonusSaved;
        
        // Menu Events
        event EventHandler DockerRestartClicked;
        event EventHandler DockerStopClicked;
        event EventHandler DockerStartClicked;
        event EventHandler DockerShowClicked;
        event EventHandler DockerMinimizeClicked;

        event EventHandler PiRestartClicked;
        event EventHandler PiStopClicked;
        event EventHandler PiStartClicked; // NEW
        event EventHandler PiShowClicked;
        event EventHandler PiMinimizeClicked;
        event EventHandler DiagnosticsClicked;

        // UI Updates
        string WalletPublicKey { get; }
        void InvokeUI(Action action);
        event Action CompactClicked;
        event Action ThemeToggleClicked;
        
        void UpdateNodeMetrics(NodeMetrics metrics);
        void ShowError(string message);
        void UpdateSystemStatus(string status, System.Drawing.Color color);
        void UpdateSecureLinkStatus(string url, bool active);
        void UpdateWallet(WalletData wallet);
        void ToggleTheme(bool isDark);
        void SetManualBonus(double bonus);
    }
}
