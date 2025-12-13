using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public class CoolSmsProvider : ISmsProvider
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _senderPhone; // 발신번호 (사전 등록 필수)

        public CoolSmsProvider(string apiKey, string apiSecret, string senderPhone)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _senderPhone = senderPhone;
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret)) return false;

            // Generate Signature
            string salt = Guid.NewGuid().ToString().Replace("-", "");
            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string signature = CalculateSignature(_apiSecret, date, salt);

            // Payload
            var payload = new JsonObject
            {
                ["message"] = new JsonObject
                {
                    ["to"] = to.Replace("-", ""),
                    ["from"] = _senderPhone.Replace("-", ""),
                    ["text"] = message
                }
            };

            using (var client = new HttpClient())
            {
                // CoolSMS API v4
                string url = $"https://api.coolsms.co.kr/messages/v4/send";
                client.DefaultRequestHeaders.Add("Authorization", $"HMAC-SHA256 apiKey={_apiKey}, date={date}, salt={salt}, signature={signature}");

                var content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                
                return response.IsSuccessStatusCode;
            }
        }

        private string CalculateSignature(string secret, string date, string salt)
        {
            var data = date + salt;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
