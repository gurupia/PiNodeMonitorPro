using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiNodeMonitorWinForm.Services
{
    public class CloudflareTunnelService
    {
        private Process _process;
        private string _tunnelUrl;
        private bool _isRunning;
        private int _retryCount = 0;
        private const int MaxRetries = 3;
        private int _lastPort;

        public event Action<string> UrlGenerated;
        public event Action<string> LogReceived;
        public event Action Stopped;

        public string TunnelUrl => _tunnelUrl;
        public bool IsRunning => _isRunning;

        public async Task<bool> StartTunnelAsync(int localPort)
        {
            _lastPort = localPort;
            if (_isRunning && _process != null && !_process.HasExited) return true;

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared.exe");
            if (!File.Exists(exePath))
            {
                LogReceived?.Invoke("Error: cloudflared.exe not found in application directory.");
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"tunnel --url http://127.0.0.1:{localPort}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                _process.Exited += async (s, e) => {
                    if (_isRunning)
                    {
                        LogReceived?.Invoke("[Tunnel] Process exited unexpectedly. Retrying...");
                        if (_retryCount < MaxRetries)
                        {
                            _retryCount++;
                            await Task.Delay(2000 * _retryCount);
                            await StartTunnelAsync(_lastPort);
                        }
                        else
                        {
                            LogReceived?.Invoke("[Tunnel] Max retries reached. Tunnel stopped.");
                            StopTunnel();
                        }
                    }
                };
                
                _process.ErrorDataReceived += (s, e) => ProcessOutput(e.Data);
                _process.OutputDataReceived += (s, e) => ProcessOutput(e.Data);

                _process.Start();
                _process.BeginErrorReadLine();
                _process.BeginOutputReadLine();

                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                LogReceived?.Invoke("Failed to start tunnel: " + ex.Message);
                return false;
            }
        }

        private void ProcessOutput(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            LogReceived?.Invoke(data);

            // Cloudflare Tunnel output looks like: 
            // |  https://your-random-name.trycloudflare.com                               |
            if (data.Contains(".trycloudflare.com"))
            {
                var match = Regex.Match(data, @"https://[a-zA-Z0-9-]+\.trycloudflare\.com");
                if (match.Success)
                {
                    _tunnelUrl = match.Value;
                    _retryCount = 0; // Reset retry count on success
                    UrlGenerated?.Invoke(_tunnelUrl);
                }
            }
        }

        public void StopTunnel()
        {
            if (!_isRunning) return;

            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch { }
            finally
            {
                _isRunning = false;
                _tunnelUrl = null;
                Stopped?.Invoke();
            }
        }
    }
}
