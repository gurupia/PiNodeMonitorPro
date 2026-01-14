using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public class TwilioProvider : ISmsProvider
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _senderPhone;

        public TwilioProvider(string accountSid, string authToken, string senderPhone)
        {
            _accountSid = accountSid;
            _authToken = authToken;
            _senderPhone = senderPhone;
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken)) return false;

            using (var client = new HttpClient())
            {
                string url = string.Format("https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json", _accountSid);
                
                var authBytes = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _accountSid, _authToken));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("From", _senderPhone),
                    new KeyValuePair<string, string>("To", to),
                    new KeyValuePair<string, string>("Body", message)
                });

                var response = await client.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
