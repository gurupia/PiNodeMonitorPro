using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public class SmsService
    {
        private ISmsProvider _provider;
        private SmsConfigModel _config;
        private const string CONFIG_FILE = "sms_config.json";

        public bool IsEnabled => _provider != null && !string.IsNullOrEmpty(_config?.TargetPhone);
        public bool IsNodeAlertEnabled => _config?.IsNodeAlertEnabled ?? false;

        public SmsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    _config = JsonSerializer.Deserialize<SmsConfigModel>(json);
                }
                else if (File.Exists("sms_config.txt")) // Migration Support
                {
                    MigrateLegacyConfig();
                }

                if (_config == null) _config = new SmsConfigModel();

                // Setup Provider
                if (_config.SelectedProvider == "SOLAPI")
                {
                    if (!string.IsNullOrEmpty(_config.Solapi.Key1))
                        _provider = new SolapiSmsProvider(_config.Solapi.Key1, _config.Solapi.Key2, _config.Solapi.SenderPhone);
                }
                else if (_config.SelectedProvider == "TWILIO")
                {
                     if (!string.IsNullOrEmpty(_config.Twilio.Key1))
                        _provider = new TwilioProvider(_config.Twilio.Key1, _config.Twilio.Key2, _config.Twilio.SenderPhone);
                }
            }
            catch 
            { 
                _config = new SmsConfigModel();
                _provider = null; 
            }
        }

        private void MigrateLegacyConfig()
        {
            try
            {
                var line = File.ReadAllText("sms_config.txt").Trim();
                var parts = line.Split('|');
                if (parts.Length >= 5)
                {
                    _config = new SmsConfigModel();
                    _config.SelectedProvider = parts[0].ToUpper() == "COOL" ? "SOLAPI" : parts[0].ToUpper();
                    
                    var keys = new ProviderConfig 
                    { 
                        Key1 = parts[1], 
                        Key2 = parts[2], 
                        SenderPhone = parts[3] 
                    };
                    
                    if (_config.SelectedProvider == "SOLAPI") _config.Solapi = keys;
                    else _config.Twilio = keys;

                    _config.TargetPhone = parts[4];
                    if (parts.Length >= 6) _config.Template = parts[5];
                    if (parts.Length >= 7) _config.IsNodeAlertEnabled = bool.Parse(parts[6]);

                    // Save new format immediately
                    SaveSettings(_config);
                }
            }
            catch { }
        }

        public static void SaveSettings(SmsConfigModel config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }

        public SmsConfigModel GetConfig() => _config;

        public async Task SendAlertAsync(decimal amount, decimal balance)
        {
            string msg = _config.Template;
            if (string.IsNullOrWhiteSpace(msg)) 
                msg = $"[PiNode] Deposit! +{amount:0.#####} Pi. Total: {balance:N2} Pi";
            else
                msg = msg.Replace("{amount}", amount.ToString("0.#####")).Replace("{total}", balance.ToString("N2"));

            // 1. SMS
            if (IsEnabled)
            {
                try { await _provider.SendSmsAsync(_config.TargetPhone, msg); } catch { }
            }
            
            // 2. Telegram
            await SendTelegramAsync(msg);
        }

        public async Task SendAlertAsync(string message)
        {
             // 1. SMS
             if (IsEnabled)
             {
                 try { await _provider.SendSmsAsync(_config.TargetPhone, message); } catch { }
             }

             // 2. Telegram
             await SendTelegramAsync(message);
        }

        private async Task SendTelegramAsync(string message)
        {
            if (!_config.EnableTelegram || string.IsNullOrEmpty(_config.TelegramBotToken) || string.IsNullOrEmpty(_config.TelegramChatId))
                return;

            try
            {
                using var client = new System.Net.Http.HttpClient();
                string url = $"https://api.telegram.org/bot{_config.TelegramBotToken}/sendMessage?chat_id={_config.TelegramChatId}&text={System.Uri.EscapeDataString(message)}";
                await client.GetAsync(url);
            }
            catch { }
        }
    }
}
