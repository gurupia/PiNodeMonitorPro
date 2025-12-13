using System;
using System.IO;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public class SmsService
    {
        private ISmsProvider _provider;
        private string _targetPhone;

        public bool IsEnabled => _provider != null && !string.IsNullOrEmpty(_targetPhone);

        public SmsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            // Simple File-based Config for now (sms_config.txt)
            // Format: ProviderType|ApiKey|ApiSecret|SenderPhone|TargetPhone
            // Example: COOL|key|secret|01012345678|01099998888
            try
            {
                if (File.Exists("sms_config.txt"))
                {
                    var line = File.ReadAllText("sms_config.txt").Trim();
                    var parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        string type = parts[0].ToUpper();
                        string k1 = parts[1];
                        string k2 = parts[2];
                        string sender = parts[3];
                        _targetPhone = parts[4];

                        if (type == "COOL")
                        {
                            _provider = new CoolSmsProvider(k1, k2, sender);
                        }
                        else if (type == "TWILIO")
                        {
                            _provider = new TwilioProvider(k1, k2, sender);
                        }
                    }
                }
            }
            catch { _provider = null; }
        }

        public async Task SendAlertAsync(string message)
        {
            if (!IsEnabled) return;
            try
            {
                await _provider.SendSmsAsync(_targetPhone, message);
            }
            catch { }
        }
    }
}
