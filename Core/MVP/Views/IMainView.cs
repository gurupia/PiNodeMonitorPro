using System;
using PiNodeMonitorWinForm.Core.MVP.Models;

namespace PiNodeMonitorWinForm.Core.MVP.Views
{
    public interface IMainView
    {
        // Event forwarding
        event EventHandler ViewLoaded;
        event EventHandler ToggleNodeClicked;
        event EventHandler ShowHistoryClicked;
        event EventHandler SecureLinkClicked;
        event EventHandler MultiViewClicked;
        event EventHandler MobileConnectClicked;

        // Data Updates
        void UpdateNodeMetrics(NodeMetrics metrics);
        void UpdateWallet(WalletData wallet);
        void UpdateSystemStatus(string status, System.Drawing.Color color);
        void ShowError(string message);
        
        // Control Compatibility (Thread-safe invoke)
        void InvokeUI(Action action);
    }
}
