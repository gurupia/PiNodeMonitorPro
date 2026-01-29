using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PiNodeMonitorWinForm.Services.Security
{
    /// <summary>
    /// Windows DPAPI를 사용한 민감 데이터 암호화 서비스
    /// API 키, 토큰, 비밀번호 등을 안전하게 저장합니다.
    /// </summary>
    public static class SecureStorageService
    {
        // 추가 엔트로피 (선택적 보안 강화)
        private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("PiNodeMonitorPro_v2.0_Security");

        /// <summary>
        /// 문자열을 DPAPI로 암호화하여 Base64 문자열로 반환
        /// </summary>
        /// <param name="plainText">암호화할 평문</param>
        /// <returns>암호화된 Base64 문자열 (실패 시 빈 문자열)</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    AdditionalEntropy,
                    DataProtectionScope.CurrentUser // 현재 사용자만 복호화 가능
                );
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] Encryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// DPAPI로 암호화된 Base64 문자열을 복호화
        /// </summary>
        /// <param name="encryptedBase64">암호화된 Base64 문자열</param>
        /// <returns>복호화된 평문 (실패 시 빈 문자열)</returns>
        public static string Decrypt(string encryptedBase64)
        {
            if (string.IsNullOrEmpty(encryptedBase64))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
                byte[] decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    AdditionalEntropy,
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (FormatException)
            {
                // Base64가 아닌 경우 = 이전 버전의 평문 데이터일 가능성
                // 마이그레이션을 위해 원본 반환
                return encryptedBase64;
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] Decryption failed: {ex.Message}");
                // 복호화 실패 시 원본 반환 (마이그레이션 호환성)
                return encryptedBase64;
            }
        }

        /// <summary>
        /// 값이 이미 암호화되어 있는지 확인
        /// </summary>
        /// <param name="value">확인할 값</param>
        /// <returns>암호화 여부</returns>
        public static bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                // Base64 디코딩 시도
                byte[] data = Convert.FromBase64String(value);

                // DPAPI 암호화 데이터는 최소 길이가 있음
                if (data.Length < 20)
                    return false;

                // 복호화 시도 - 성공하면 암호화된 데이터
                ProtectedData.Unprotect(data, AdditionalEntropy, DataProtectionScope.CurrentUser);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 평문이면 암호화하고, 이미 암호화되어 있으면 그대로 반환
        /// 기존 설정 마이그레이션에 유용
        /// </summary>
        /// <param name="value">처리할 값</param>
        /// <returns>암호화된 값</returns>
        public static string EnsureEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (IsEncrypted(value))
                return value;

            return Encrypt(value);
        }

        /// <summary>
        /// 파일에 암호화된 데이터 저장
        /// </summary>
        /// <param name="filePath">저장 경로</param>
        /// <param name="plainText">암호화할 데이터</param>
        /// <returns>성공 여부</returns>
        public static bool SaveEncrypted(string filePath, string plainText)
        {
            try
            {
                string encrypted = Encrypt(plainText);
                if (string.IsNullOrEmpty(encrypted))
                    return false;

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, encrypted, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] SaveEncrypted failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일에서 암호화된 데이터 읽기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>복호화된 데이터 (실패 시 빈 문자열)</returns>
        public static string LoadDecrypted(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return string.Empty;

                string encrypted = File.ReadAllText(filePath, Encoding.UTF8);
                return Decrypt(encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SecureStorage] LoadDecrypted failed: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
