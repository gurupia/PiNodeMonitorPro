using System;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public class SmsConfigModel
    {
        public string SelectedProvider { get; set; } = "SOLAPI";
        public string TargetPhone { get; set; } = "";
        public string Template { get; set; } = "[PiNode] Deposit! +{amount} Pi. Total: {total} Pi";
        public bool IsNodeAlertEnabled { get; set; } = false;
        
        // Telegram Config
        public bool EnableTelegram { get; set; } = false;
        public string TelegramBotToken { get; set; } = "";
        public string TelegramChatId { get; set; } = "";

        public ProviderConfig Solapi { get; set; } = new ProviderConfig();
        public ProviderConfig Twilio { get; set; } = new ProviderConfig();
    }

    public class ProviderConfig
    {
        public string Key1 { get; set; } = ""; // ApiKey or SID
        public string Key2 { get; set; } = ""; // Secret or Token
        public string SenderPhone { get; set; } = "";
    }
}
