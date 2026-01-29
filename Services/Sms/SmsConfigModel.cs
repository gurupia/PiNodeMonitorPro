using System;
using Newtonsoft.Json;
using PiNodeMonitorWinForm.Services.Security;

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

        // 암호화된 토큰 저장 필드 (JSON 직렬화용)
        [JsonProperty("TelegramBotToken")]
        public string TelegramBotTokenEncrypted { get; set; } = "";

        public string TelegramChatId { get; set; } = "";

        // 복호화된 토큰 접근자 (런타임용, JSON 무시)
        [JsonIgnore]
        public string TelegramBotToken
        {
            get => SecureStorageService.Decrypt(TelegramBotTokenEncrypted);
            set => TelegramBotTokenEncrypted = SecureStorageService.Encrypt(value);
        }

        public ProviderConfig Solapi { get; set; } = new ProviderConfig();
        public ProviderConfig Twilio { get; set; } = new ProviderConfig();

        /// <summary>
        /// 기존 평문 설정을 암호화 버전으로 마이그레이션
        /// </summary>
        public void MigrateToEncrypted()
        {
            // Telegram 토큰 마이그레이션
            if (!string.IsNullOrEmpty(TelegramBotTokenEncrypted) &&
                !SecureStorageService.IsEncrypted(TelegramBotTokenEncrypted))
            {
                TelegramBotTokenEncrypted = SecureStorageService.Encrypt(TelegramBotTokenEncrypted);
            }

            // Provider 설정 마이그레이션
            Solapi?.MigrateToEncrypted();
            Twilio?.MigrateToEncrypted();
        }
    }

    public class ProviderConfig
    {
        // 암호화된 키 저장 필드 (JSON 직렬화용)
        [JsonProperty("Key1")]
        public string Key1Encrypted { get; set; } = ""; // ApiKey or SID (Encrypted)

        [JsonProperty("Key2")]
        public string Key2Encrypted { get; set; } = ""; // Secret or Token (Encrypted)

        public string SenderPhone { get; set; } = "";

        // 복호화된 키 접근자 (런타임용, JSON 무시)
        [JsonIgnore]
        public string Key1
        {
            get => SecureStorageService.Decrypt(Key1Encrypted);
            set => Key1Encrypted = SecureStorageService.Encrypt(value);
        }

        [JsonIgnore]
        public string Key2
        {
            get => SecureStorageService.Decrypt(Key2Encrypted);
            set => Key2Encrypted = SecureStorageService.Encrypt(value);
        }

        /// <summary>
        /// 기존 평문 설정을 암호화 버전으로 마이그레이션
        /// </summary>
        public void MigrateToEncrypted()
        {
            if (!string.IsNullOrEmpty(Key1Encrypted) &&
                !SecureStorageService.IsEncrypted(Key1Encrypted))
            {
                Key1Encrypted = SecureStorageService.Encrypt(Key1Encrypted);
            }

            if (!string.IsNullOrEmpty(Key2Encrypted) &&
                !SecureStorageService.IsEncrypted(Key2Encrypted))
            {
                Key2Encrypted = SecureStorageService.Encrypt(Key2Encrypted);
            }
        }
    }
}
