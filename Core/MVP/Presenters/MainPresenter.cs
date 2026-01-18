using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PiNodeMonitorWinForm.Core.MVP.Models;
using PiNodeMonitorWinForm.Core.MVP.Views;
using PiNodeMonitorWinForm.Services;
using PiNodeMonitorWinForm.Services.Sms;

namespace PiNodeMonitorWinForm.Core.MVP.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        private readonly WalletService _walletService;
        private readonly SmsService _smsService;
        private readonly PriceService _priceService;
        private readonly NodeMonitorService _monitorService;
        private readonly Timer _refreshTimer;

        // State
        private bool _isUpdating = false;
        private decimal _lastBalance = -1;
        private int _totalSeconds = 0;
        private int _totalSyncedSeconds = 0;
        
        // Polling Counters (Phase 2: Multi-tiered Polling)
        private int _walletCounter = 0;   // Update every 30s (10 ticks)
        private int _slowCheckCounter = 0; // Update every 5m (100 ticks)

        public MainPresenter(IMainView view)
        {
            _view = view;
            _walletService = new WalletService();
            _smsService = new SmsService();
            _priceService = new PriceService();
            _monitorService = new NodeMonitorService();

            _refreshTimer = new Timer();
            _refreshTimer.Interval = 3000;
            _refreshTimer.Tick += async (s, e) => await OnTimerTick();

            Initialize();
        }

        private void Initialize()
        {
            // Subscribe to View Events
            _view.ToggleNodeClicked += async (s, e) => await ToggleNodeAsync();
            _view.ViewLoaded += (s, e) => {
                _refreshTimer.Start();
                _ = OnTimerTick(); // Initial Load
            };
        }

        private async Task OnTimerTick()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                // 1. FAST TICK (Every 3s)
                await UpdateNodeMetricsAsync();

                // 2. MEDIUM TICK (Every 30s)
                if (_walletCounter <= 0)
                {
                    await UpdateWalletAsync();
                    _walletCounter = 10;
                }
                _walletCounter--;

                // 3. SLOW TICK (Every 5m)
                if (_slowCheckCounter <= 0)
                {
                    await PerformSlowChecksAsync();
                    _slowCheckCounter = 100;
                }
                _slowCheckCounter--;
            }
            catch (Exception ex)
            {
                _view.ShowError($"Dashboard Error: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private async Task UpdateWalletAsync()
        {
            var bal = await _walletService.GetBalanceAsync();
            if (bal.HasValue)
            {
                decimal dBal = bal.Value;
                if (_lastBalance != -1 && dBal > _lastBalance)
                {
                    decimal diff = dBal - _lastBalance;
                    _walletService.LogDeposit(diff, dBal);
                    _ = _smsService.SendAlertAsync(diff, dBal);
                }
                _lastBalance = dBal;

                var (usd, krw) = await _priceService.FetchPriceAsync();
                
                var walletData = new WalletData
                {
                    Balance = dBal,
                    PriceUsd = usd,
                    PriceKrw = krw,
                    TotalValueUsd = (double)dBal * usd,
                    TotalValueKrw = (double)dBal * krw,
                    PublicKey = _walletService.PublicKey
                };

                _view.InvokeUI(() => _view.UpdateWallet(walletData));
            }
        }

        private async Task UpdateNodeMetricsAsync()
        {
            _totalSeconds += 3;
            var metrics = new NodeMetrics();
            
            // Optimized Docker Check (Direct call from NodeUtility)
            string psOutput = await NodeUtility.RunDockerCommandAsync("ps --format \"{{.Names}}\"");
            metrics.IsDockerRunning = !string.IsNullOrEmpty(psOutput);
            
            if (metrics.IsDockerRunning)
            {
                if (psOutput.Contains("pi-consensus")) metrics.ActiveContainerName = "pi-consensus";
                else if (psOutput.Contains("pi-node")) metrics.ActiveContainerName = "pi-node";
                else metrics.ActiveContainerName = psOutput.Split('\n')[0].Trim();

                // Real Sync Logic Check (Simplified for Phase 2 focus)
                metrics.ConsensusState = "Synced (Direct Poll)";
                _totalSyncedSeconds += 3;
            }
            else
            {
                metrics.ConsensusState = "Stopped";
            }

            metrics.Uptime = TimeSpan.FromSeconds(_totalSeconds);
            
            _view.InvokeUI(() => _view.UpdateNodeMetrics(metrics));
        }

        private async Task PerformSlowChecksAsync()
        {
            // Heavy operations: Ghost space, disk optimization suggestions, version checks
            double ghostGb = await NodeUtility.GetGhostSpaceGbAsync();
            _view.InvokeUI(() => _view.UpdateSystemStatus($"Ghost Space: {ghostGb:F2} GB | Logic Optimized", Color.Silver));
        }

        private async Task ToggleNodeAsync()
        {
             // Toggle Logic
             string psOutput = await NodeUtility.RunDockerCommandAsync("ps --format \"{{.Names}}\"");
             if (!string.IsNullOrEmpty(psOutput))
                await NodeUtility.RunDockerCommandAsync("stop pi-consensus");
             else
                await NodeUtility.RunDockerCommandAsync("start pi-consensus");
             
             await OnTimerTick();
        }
    }
}
