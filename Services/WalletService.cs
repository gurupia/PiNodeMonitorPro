using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PiNodeMonitorWinForm.Services
{
    public class WalletService
    {
        private const string WalletFile = "wallet.dat";
        private const string ApiUrl = "https://api.mainnet.minepi.com/accounts/";
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        public string PublicKey { get; private set; }

        public WalletService()
        {
            LoadKey();
        }

        public void LoadKey()
        {
            try
            {
                if (File.Exists(WalletFile))
                {
                    PublicKey = File.ReadAllText(WalletFile).Trim();
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WalletService] LoadKey error: {ex.Message}"); PublicKey = ""; }
        }

        public void SaveKey(string key)
        {
            try
            {
                PublicKey = key?.Trim();
                if (!string.IsNullOrEmpty(PublicKey))
                {
                    File.WriteAllText(WalletFile, PublicKey);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WalletService] SaveKey error: {ex.Message}"); }
        }

        /// <summary>
        /// Fetches the current native balance from Pi Mainnet.
        /// Returns null if failed or key is invalid.
        /// </summary>
        public async Task<decimal?> GetBalanceAsync()
        {
            if (string.IsNullOrEmpty(PublicKey) || !PublicKey.StartsWith("G")) return null;

            try
            {
                var json = await _client.GetStringAsync(ApiUrl + PublicKey);
                
                // Regex parsing logic extracted from Form1
                var match = Regex.Match(json, "\"balance\"\\s*:\\s*\"([0-9.]+)\"[^}]*?\"asset_type\"\\s*:\\s*\"native\"");
                if (!match.Success) 
                    match = Regex.Match(json, "\"asset_type\"\\s*:\\s*\"native\"[^}]*?\"balance\"\\s*:\\s*\"([0-9.]+)\"");
                    
                if (match.Success)
                {
                    if (decimal.TryParse(match.Groups[1].Value, out decimal balance))
                    {
                        return balance;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WalletService] GetBalanceAsync error: {ex.Message}");
            }
            return null;
        }

        public void LogDeposit(decimal amount, decimal newBalance)
        {
            try
            {
                // Format: Timestamp, Amount, TotalBalance
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},+{amount},Total: {newBalance}{Environment.NewLine}";
                File.AppendAllText("transaction_log.csv", logLine);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WalletService] LogDeposit error: {ex.Message}"); }
        }
    }
}
