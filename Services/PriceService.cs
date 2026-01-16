using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PiNodeMonitorWinForm.Services
{
    public class PriceService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string CoinGeckoUrl = "https://api.coingecko.com/api/v3/simple/price?ids=pi-network-iou&vs_currencies=usd,krw";
        
        private double _lastPriceUsd = 0;
        private double _lastPriceKrw = 0;
        private DateTime _lastUpdate = DateTime.MinValue;

        public double CurrentPriceUsd => _lastPriceUsd;
        public double CurrentPriceKrw => _lastPriceKrw;

        public async Task<(double usd, double krw)> FetchPriceAsync()
        {
            // Cache for 10 minutes to avoid rate limits
            if ((DateTime.Now - _lastUpdate).TotalMinutes < 10 && _lastPriceUsd > 0)
            {
                return (_lastPriceUsd, _lastPriceKrw);
            }

            try
            {
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                var response = await _client.GetStringAsync(CoinGeckoUrl);
                var json = JObject.Parse(response);

                var piData = json["pi-network-iou"];
                if (piData != null)
                {
                    _lastPriceUsd = piData["usd"]?.Value<double>() ?? 0;
                    _lastPriceKrw = piData["krw"]?.Value<double>() ?? 0;
                    _lastUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PriceService] FetchPriceAsync error: {ex.Message}");
            }

            return (_lastPriceUsd, _lastPriceKrw);
        }
    }
}
