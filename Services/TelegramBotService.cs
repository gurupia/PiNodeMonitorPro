using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PiNodeMonitorWinForm.Services
{
    public class TelegramBotService
    {
        private string _token;
        private string _allowedChatId; 
        private readonly HttpClient _client;
        private CancellationTokenSource _cts;
        private long _lastUpdateId = 0;

        public event Func<string> OnStatusRequested;
        public event Action OnRestartRequested;

        public bool IsRunning { get; private set; }

        public TelegramBotService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        public void Start(string token, string chatId)
        {
            if (IsRunning) Stop();
            
            _token = token;
            _allowedChatId = chatId;
            _cts = new CancellationTokenSource();
            IsRunning = true;

            Task.Run(() => PollingLoop(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null) _cts.Cancel();
            IsRunning = false;
        }

        private async Task PollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string url = string.Format("https://api.telegram.org/bot{0}/getUpdates?offset={1}&timeout=10", _token, _lastUpdateId + 1);
                    var response = await _client.GetStringAsync(url);
                    
                    var updates = JsonConvert.DeserializeObject<TelegramUpdateResponse>(response);
                    
                    if (updates != null && updates.Result != null)
                    {
                        foreach (var update in updates.Result)
                        {
                            _lastUpdateId = update.UpdateId;
                            ProcessMessage(update.Message);
                        }
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception) 
                { 
                    try { await Task.Delay(5000, token); } catch { break; }
                }
            }
        }

        private async void ProcessMessage(TelegramMessage msg)
        {
            if (msg == null || msg.Text == null) return;
            if (msg.Chat.Id.ToString() != _allowedChatId) return;

            string text = msg.Text.Trim().ToLower();
            string reply = "";

            if (text == "/start" || text == "/help")
            {
                reply = "🤖 **Pi Node Monitor Bot**\n\nCommand List:\n✅ `/status` - Check Node Health\n🔄 `/reboot` - Restart Node Container\n👋 `/ping` - Check Connection";
            }
            else if (text == "/status")
            {
                reply = OnStatusRequested != null ? OnStatusRequested() : "No status available.";
            }
            else if (text == "/reboot")
            {
                if (OnRestartRequested != null) OnRestartRequested();
                reply = "🔄 Reboot command received. Attempting to restart container...";
            }
            else if (text == "/ping")
            {
                reply = "Pong! 🏓 I am alive.";
            }
            else
            {
                return; 
            }

            if (!string.IsNullOrEmpty(reply))
            {
                await SendMessageAsync(msg.Chat.Id.ToString(), reply);
            }
        }

        public async Task SendMessageAsync(string chatId, string text)
        {
            try
            {
                string url = string.Format("https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}&parse_mode=Markdown", _token, chatId, Uri.EscapeDataString(text));
                await _client.GetAsync(url);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[TelegramBot] SendMessage error: {ex.Message}"); }
        }

        public class TelegramUpdateResponse
        {
            [JsonProperty("result")]
            public List<TelegramUpdate> Result { get; set; }
        }

        public class TelegramUpdate
        {
            [JsonProperty("update_id")]
            public long UpdateId { get; set; }
            [JsonProperty("message")]
            public TelegramMessage Message { get; set; }
        }

        public class TelegramMessage
        {
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("chat")]
            public TelegramChat Chat { get; set; }
        }

        public class TelegramChat
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }
    }
}
