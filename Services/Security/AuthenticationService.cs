using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace PiNodeMonitorWinForm.Services.Security
{
    /// <summary>
    /// 원격 접속 인증 서비스
    /// - 6자리 영숫자 PIN 생성
    /// - Rate Limiting (5회 실패 시 15분 차단)
    /// </summary>
    public class AuthenticationService
    {
        // PIN 설정
        private const int PIN_LENGTH = 6;
        private const string PIN_CHARACTERS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 혼동 문자 제외 (0,O,1,I)

        // Rate Limiting 설정
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 15;

        // 상태 저장
        private string _currentPin;
        private readonly ConcurrentDictionary<string, FailedAttemptInfo> _failedAttempts;
        private readonly object _pinLock = new object();

        public string CurrentPin => _currentPin;

        public AuthenticationService()
        {
            _failedAttempts = new ConcurrentDictionary<string, FailedAttemptInfo>();
            RegeneratePin();
        }

        /// <summary>
        /// 새로운 6자리 영숫자 PIN 생성
        /// </summary>
        /// <returns>생성된 PIN</returns>
        public string RegeneratePin()
        {
            lock (_pinLock)
            {
                _currentPin = GenerateSecurePin(PIN_LENGTH);
                return _currentPin;
            }
        }

        /// <summary>
        /// 암호학적으로 안전한 랜덤 PIN 생성
        /// </summary>
        private string GenerateSecurePin(int length)
        {
            var sb = new StringBuilder(length);

            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % PIN_CHARACTERS.Length;
                    sb.Append(PIN_CHARACTERS[index]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// PIN 검증 (Rate Limiting 포함)
        /// </summary>
        /// <param name="providedPin">사용자 입력 PIN</param>
        /// <param name="clientIp">클라이언트 IP</param>
        /// <returns>인증 결과</returns>
        public AuthResult Authenticate(string providedPin, string clientIp)
        {
            // 1. Lockout 상태 확인
            if (IsLockedOut(clientIp, out var remainingTime))
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorCode = AuthErrorCode.LockedOut,
                    Message = $"Too many failed attempts. Try again in {remainingTime.Minutes}m {remainingTime.Seconds}s",
                    RemainingLockoutTime = remainingTime
                };
            }

            // 2. PIN 검증 (대소문자 무시)
            bool isValid = string.Equals(_currentPin, providedPin?.ToUpperInvariant(), StringComparison.Ordinal);

            if (isValid)
            {
                // 성공 시 실패 카운트 초기화
                _failedAttempts.TryRemove(clientIp, out _);

                return new AuthResult
                {
                    Success = true,
                    ErrorCode = AuthErrorCode.None,
                    Message = "Authentication successful"
                };
            }

            // 3. 실패 처리
            var failInfo = _failedAttempts.AddOrUpdate(
                clientIp,
                new FailedAttemptInfo { Count = 1, FirstAttempt = DateTime.Now, LastAttempt = DateTime.Now },
                (key, existing) =>
                {
                    existing.Count++;
                    existing.LastAttempt = DateTime.Now;

                    // 첫 실패 후 lockout 시간 지났으면 초기화
                    if (DateTime.Now - existing.FirstAttempt > TimeSpan.FromMinutes(LOCKOUT_MINUTES))
                    {
                        existing.Count = 1;
                        existing.FirstAttempt = DateTime.Now;
                    }

                    return existing;
                }
            );

            int remainingAttempts = MAX_FAILED_ATTEMPTS - failInfo.Count;

            // 4. Lockout 적용 확인
            if (failInfo.Count >= MAX_FAILED_ATTEMPTS)
            {
                failInfo.LockoutUntil = DateTime.Now.AddMinutes(LOCKOUT_MINUTES);

                return new AuthResult
                {
                    Success = false,
                    ErrorCode = AuthErrorCode.LockedOut,
                    Message = $"Account locked for {LOCKOUT_MINUTES} minutes due to {MAX_FAILED_ATTEMPTS} failed attempts",
                    RemainingLockoutTime = TimeSpan.FromMinutes(LOCKOUT_MINUTES)
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorCode = AuthErrorCode.InvalidPin,
                Message = $"Invalid PIN. {remainingAttempts} attempts remaining",
                RemainingAttempts = remainingAttempts
            };
        }

        /// <summary>
        /// Lockout 상태 확인
        /// </summary>
        private bool IsLockedOut(string clientIp, out TimeSpan remainingTime)
        {
            remainingTime = TimeSpan.Zero;

            if (_failedAttempts.TryGetValue(clientIp, out var failInfo))
            {
                if (failInfo.LockoutUntil.HasValue && failInfo.LockoutUntil > DateTime.Now)
                {
                    remainingTime = failInfo.LockoutUntil.Value - DateTime.Now;
                    return true;
                }

                // Lockout 기간 만료 시 초기화
                if (failInfo.LockoutUntil.HasValue && failInfo.LockoutUntil <= DateTime.Now)
                {
                    _failedAttempts.TryRemove(clientIp, out _);
                }
            }

            return false;
        }

        /// <summary>
        /// 클라이언트의 현재 실패 횟수 조회
        /// </summary>
        public int GetFailedAttemptCount(string clientIp)
        {
            return _failedAttempts.TryGetValue(clientIp, out var info) ? info.Count : 0;
        }

        /// <summary>
        /// 특정 클라이언트의 Lockout 수동 해제
        /// </summary>
        public void ResetLockout(string clientIp)
        {
            _failedAttempts.TryRemove(clientIp, out _);
        }

        /// <summary>
        /// 모든 Lockout 초기화
        /// </summary>
        public void ResetAllLockouts()
        {
            _failedAttempts.Clear();
        }

        /// <summary>
        /// 현재 차단된 IP 목록 조회
        /// </summary>
        public string[] GetLockedOutIps()
        {
            var lockedIps = new System.Collections.Generic.List<string>();
            var now = DateTime.Now;

            foreach (var kvp in _failedAttempts)
            {
                if (kvp.Value.LockoutUntil.HasValue && kvp.Value.LockoutUntil > now)
                {
                    lockedIps.Add(kvp.Key);
                }
            }

            return lockedIps.ToArray();
        }

        /// <summary>
        /// 실패 시도 정보
        /// </summary>
        private class FailedAttemptInfo
        {
            public int Count { get; set; }
            public DateTime FirstAttempt { get; set; }
            public DateTime LastAttempt { get; set; }
            public DateTime? LockoutUntil { get; set; }
        }
    }

    /// <summary>
    /// 인증 결과
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public AuthErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
        public int RemainingAttempts { get; set; }
        public TimeSpan RemainingLockoutTime { get; set; }
    }

    /// <summary>
    /// 인증 오류 코드
    /// </summary>
    public enum AuthErrorCode
    {
        None = 0,
        InvalidPin = 1,
        LockedOut = 2,
        SessionExpired = 3
    }
}
