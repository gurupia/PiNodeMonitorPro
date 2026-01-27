using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm.Services.Logging
{
    /// <summary>
    /// 비동기 로깅 시스템
    /// - UI 스레드 블로킹 방지
    /// - 백그라운드 큐 기반 파일 쓰기
    /// - 자동 로그 로테이션
    /// </summary>
    public sealed class AsyncLogger : IDisposable
    {
        private static readonly Lazy<AsyncLogger> _instance = new Lazy<AsyncLogger>(() => new AsyncLogger());
        public static AsyncLogger Instance => _instance.Value;

        // 설정
        private const int MAX_QUEUE_SIZE = 10000;
        private const int MAX_LOG_SIZE_MB = 10;
        private const int LOG_RETENTION_DAYS = 7;
        private const int FLUSH_INTERVAL_MS = 1000;

        // 상태
        private readonly BlockingCollection<LogEntry> _logQueue;
        private readonly CancellationTokenSource _cts;
        private readonly Task _writerTask;
        private readonly string _logDirectory;
        private string _currentLogFile;
        private bool _disposed;

        public event Action<string> LogWritten;

        private AsyncLogger()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _logQueue = new BlockingCollection<LogEntry>(MAX_QUEUE_SIZE);
            _cts = new CancellationTokenSource();

            EnsureLogDirectory();
            CleanOldLogs();

            // 백그라운드 쓰기 작업 시작
            _writerTask = Task.Factory.StartNew(
                ProcessLogQueue,
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        /// <summary>
        /// 로그 메시지 기록 (비동기, 논블로킹)
        /// </summary>
        public void Log(string message, LogLevel level = LogLevel.Info, string source = null)
        {
            if (_disposed || string.IsNullOrEmpty(message)) return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Source = source ?? "App"
            };

            // 큐가 가득 차면 가장 오래된 항목 제거 (드롭)
            if (!_logQueue.TryAdd(entry))
            {
                // 큐 오버플로우 - 새 항목을 위해 오래된 항목 제거 시도
                LogEntry dropped;
                _logQueue.TryTake(out dropped);
                _logQueue.TryAdd(entry);
            }

            LogWritten?.Invoke(entry.ToString());
        }

        /// <summary>
        /// 로그 기록 (편의 메서드들)
        /// </summary>
        public void Info(string message, string source = null) => Log(message, LogLevel.Info, source);
        public void Warning(string message, string source = null) => Log(message, LogLevel.Warning, source);
        public void Error(string message, string source = null) => Log(message, LogLevel.Error, source);
        public void Debug(string message, string source = null) => Log(message, LogLevel.Debug, source);

        /// <summary>
        /// 예외 로깅
        /// </summary>
        public void Error(Exception ex, string context = null)
        {
            string message = context != null
                ? $"{context}: {ex.Message}\r\nStack: {ex.StackTrace}"
                : $"{ex.Message}\r\nStack: {ex.StackTrace}";

            Log(message, LogLevel.Error, "Exception");
        }

        /// <summary>
        /// 백그라운드 큐 처리
        /// </summary>
        private void ProcessLogQueue()
        {
            var buffer = new StringBuilder();
            var lastFlush = DateTime.Now;

            try
            {
                foreach (var entry in _logQueue.GetConsumingEnumerable(_cts.Token))
                {
                    buffer.AppendLine(entry.ToString());

                    // 배치 쓰기 (1초마다 또는 버퍼가 충분히 찼을 때)
                    if ((DateTime.Now - lastFlush).TotalMilliseconds >= FLUSH_INTERVAL_MS ||
                        buffer.Length > 4096)
                    {
                        FlushBuffer(buffer);
                        lastFlush = DateTime.Now;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            finally
            {
                // 남은 버퍼 플러시
                if (buffer.Length > 0)
                {
                    FlushBuffer(buffer);
                }
            }
        }

        /// <summary>
        /// 버퍼를 파일에 쓰기
        /// </summary>
        private void FlushBuffer(StringBuilder buffer)
        {
            if (buffer.Length == 0) return;

            try
            {
                string logFile = GetCurrentLogFile();
                File.AppendAllText(logFile, buffer.ToString(), Encoding.UTF8);
                buffer.Clear();

                // 로그 로테이션 체크
                CheckLogRotation(logFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AsyncLogger] Flush error: {ex.Message}");
            }
        }

        /// <summary>
        /// 현재 로그 파일 경로 가져오기
        /// </summary>
        private string GetCurrentLogFile()
        {
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string expectedFile = Path.Combine(_logDirectory, $"app_{dateStr}.log");

            if (_currentLogFile != expectedFile)
            {
                _currentLogFile = expectedFile;
            }

            return _currentLogFile;
        }

        /// <summary>
        /// 로그 로테이션 체크
        /// </summary>
        private void CheckLogRotation(string logFile)
        {
            try
            {
                var info = new FileInfo(logFile);
                if (info.Exists && info.Length > MAX_LOG_SIZE_MB * 1024 * 1024)
                {
                    string rotatedFile = Path.Combine(
                        _logDirectory,
                        $"app_{DateTime.Now:yyyy-MM-dd}_{DateTime.Now:HHmmss}.log"
                    );
                    File.Move(logFile, rotatedFile);
                    _currentLogFile = null; // 다음 쓰기 시 새 파일 생성
                }
            }
            catch { }
        }

        /// <summary>
        /// 로그 디렉토리 생성
        /// </summary>
        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// 오래된 로그 정리
        /// </summary>
        private void CleanOldLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-LOG_RETENTION_DAYS);
                foreach (var file in Directory.GetFiles(_logDirectory, "*.log"))
                {
                    if (File.GetCreationTime(file) < cutoff)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 큐를 강제로 플러시 (동기)
        /// </summary>
        public void Flush()
        {
            // 큐가 비워질 때까지 대기 (최대 5초)
            var timeout = DateTime.Now.AddSeconds(5);
            while (_logQueue.Count > 0 && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logQueue.CompleteAdding();
            _cts.Cancel();

            try
            {
                _writerTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch { }

            _logQueue.Dispose();
            _cts.Dispose();
        }
    }

    /// <summary>
    /// 로그 레벨
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// 로그 항목
    /// </summary>
    internal class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level,-7}] [{Source}] {Message}";
        }
    }
}
