using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Translumo.Services
{
    public class LibreTranslateManager : IDisposable
    {
        private System.Diagnostics.Process _libreTranslateProcess;
        private readonly ILogger _logger;

        private string _currentSourceLang;
        private string _currentTargetLang;

        public LibreTranslateManager(ILogger<LibreTranslateManager> logger)
        {
            _logger = logger;
        }

        public bool IsRunning { get; private set; }

        public void EnsureServerRunning(string sourceLang, string targetLang)
        {
            try
            {
                if (IsRunning &&
                    _libreTranslateProcess != null &&
                    !_libreTranslateProcess.HasExited &&
                    string.Equals(_currentSourceLang, sourceLang, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(_currentTargetLang, targetLang, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                KillServer();

                _currentSourceLang = sourceLang;
                _currentTargetLang = targetLang;

                _libreTranslateProcess = new System.Diagnostics.Process();
                _libreTranslateProcess.StartInfo.FileName = "cmd.exe";

                var loadLangs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sourceLang, targetLang };
                var loadOnlyStr = string.Join(",", loadLangs);

                _libreTranslateProcess.StartInfo.Arguments = $"/c libretranslate --load-only {loadOnlyStr} --threads 1";
                _libreTranslateProcess.StartInfo.UseShellExecute = false;
                _libreTranslateProcess.StartInfo.CreateNoWindow = true;
                _libreTranslateProcess.Start();

                IsRunning = true;
                _logger.LogInformation($"Started LibreTranslate server for {sourceLang}-{targetLang}");
            }
            catch (Exception ex)
            {
                IsRunning = false;
                _logger.LogError(ex, "Failed to start local LibreTranslate server");
                throw;
            }
        }

        public void KillServer()
        {
            IsRunning = false;

            if (_libreTranslateProcess != null && !_libreTranslateProcess.HasExited)
            {
                try { _libreTranslateProcess.Kill(true); } catch { }
                _libreTranslateProcess.Dispose();
                _libreTranslateProcess = null;
            }

            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c netstat -ano | findstr :5000")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process == null) return;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("LISTENING"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 4 && int.TryParse(parts[4], out int pid))
                        {
                            try
                            {
                                var proc = System.Diagnostics.Process.GetProcessById(pid);
                                proc.Kill();
                                proc.WaitForExit();
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up port 5000");
            }
        }

        public void Dispose()
        {
            KillServer();
        }
    }
}
