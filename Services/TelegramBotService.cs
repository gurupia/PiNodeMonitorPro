using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PiNodeMonitorWinForm.Services
{
    public class TelegramBotService
    {
        private string _token;
        private string _allowedChatId; // 보안: 내 ChatID가 아니면 무시
        private readonly HttpClient _client;
        private CancellationTokenSource _cts;
        private long _lastUpdateId = 0;

        // 메인 폼과 소통할 이벤트
        public event Func<string> OnStatusRequested;
        public event Action OnRestartRequested;

        public bool IsRunning { get; private set; }

        public TelegramBotService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30); // Long polling capability
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
            _cts?.Cancel();
            IsRunning = false;
        }

        private async Task PollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // getUpdates (Long Polling: timeout=10s)
                    string url = $"https://api.telegram.org/bot{_token}/getUpdates?offset={_lastUpdateId + 1}&timeout=10";
                    var response = await _client.GetStringAsync(url, token);
                    
                    var updates = JsonSerializer.Deserialize<TelegramUpdateResponse>(response);
                    
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
                    // Network error, wait a bit
                    await Task.Delay(5000, token); 
                }
            }
        }

        private async void ProcessMessage(TelegramMessage msg)
        {
            if (msg == null || msg.Text == null) return;
            
            // 보안 체크: 내 ChatID에서 온 명령만 허용
            if (msg.Chat.Id.ToString() != _allowedChatId) return;

            string text = msg.Text.Trim().ToLower();
            string reply = "";

            if (text == "/start" || text == "/help")
            {
                reply = "🤖 **Pi Node Monitor Bot**\n\nCommand List:\n✅ `/status` - Check Node Health\n🔄 `/reboot` - Restart Node Container\n👋 `/ping` - Check Connection";
            }
            else if (text == "/status")
            {
                reply = OnStatusRequested?.Invoke() ?? "No status available.";
            }
            else if (text == "/reboot")
            {
                OnRestartRequested?.Invoke();
                reply = "🔄 Reboot command received. Attempting to restart container...";
            }
            else if (text == "/ping")
            {
                reply = "Pong! 🏓 I am alive.";
            }
            else
            {
                // Unknown command
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
                string url = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(text)}&parse_mode=Markdown";
                await _client.GetAsync(url);
            }
            catch { }
        }

        // --- JSON Models ---
        public class TelegramUpdateResponse
        {
            [JsonPropertyName("result")]
            public List<TelegramUpdate> Result { get; set; }
        }

        public class TelegramUpdate
        {
            [JsonPropertyName("update_id")]
            public long UpdateId { get; set; }
            [JsonPropertyName("message")]
            public TelegramMessage Message { get; set; }
        }

        public class TelegramMessage
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("chat")]
            public TelegramChat Chat { get; set; }
        }

        public class TelegramChat
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
        }
    }
}
