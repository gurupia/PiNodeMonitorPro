using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace PiNodeMonitorWinForm.Services
{
    public class WalletService
    {
        private const string WalletFile = "wallet.dat";
        private const string ApiUrl = "https://api.mainnet.minepi.com/accounts/";
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
        {
            int retryCount = 0;
            while (true)
            {
                try { return await action(); }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    await Task.Delay(1000 * retryCount);
                    System.Diagnostics.Debug.WriteLine($"[WalletService] Retry {retryCount} due to: {ex.Message}");
                }
                catch { throw; }
            }
        }

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

        public class PiBalance
        {
            [JsonProperty("balance")]
            public string Balance { get; set; }

            [JsonProperty("asset_type")]
            public string AssetType { get; set; }
        }

        public class PiAccountResponse
        {
            [JsonProperty("balances")]
            public List<PiBalance> Balances { get; set; }
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
                var response = await ExecuteWithRetryAsync(() => _client.GetStringAsync(ApiUrl + PublicKey));
                var account = JsonConvert.DeserializeObject<PiAccountResponse>(response);
                
                if (account?.Balances != null)
                {
                    var nativeBalance = account.Balances.FirstOrDefault(b => b.AssetType == "native");
                    if (nativeBalance != null && decimal.TryParse(nativeBalance.Balance, out decimal balance))
                    {
                        return balance;
                    }
                }

                // Fallback to Regex only if JSON DTO fails (for legacy or slight variations)
                var match = Regex.Match(response, "\"balance\"\\s*:\\s*\"([0-9.]+)\"[^}]*?\"asset_type\"\\s*:\\s*\"native\"");
                if (!match.Success) 
                    match = Regex.Match(response, "\"asset_type\"\\s*:\\s*\"native\"[^}]*?\"balance\"\\s*:\\s*\"([0-9.]+)\"");
                    
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal fallbackBalance))
                {
                    return fallbackBalance;
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
