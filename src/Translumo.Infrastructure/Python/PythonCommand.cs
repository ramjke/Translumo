using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Translumo.Infrastructure.Constants;

namespace Translumo.Infrastructure.Python
{
    public struct PythonCommandResult
    {
        public string Output { get; init; }

        public string ErrorOutput { get; init; }

        public bool HasError { get; init; }
    }

    public class PythonCommand : IDisposable
    {
        public CancellationToken Token { get; set; }

        private readonly Process _process;
        private readonly StringBuilder _outputBuffer = new StringBuilder();
        private readonly StringBuilder _errorOutputBuffer = new StringBuilder();

        protected PythonCommand(ProcessStartInfo pythonStartInfo)
        {
            if (!File.Exists(Path.Combine(Global.PythonPath, "python.exe")))
            {
                throw new FileNotFoundException("Embedded Python is not found");
            }

            _process = Process.Start(pythonStartInfo);
        }

        public static PythonCommand CreatePip(string command, CancellationToken token)
        {
            return new PythonCommand(GetPythonStartInfo(@$"python.exe .\scripts\pip.exe {command}"))
            {
                Token = token
            };
        }

        public static PythonCommand Create(string command, CancellationToken token)
        {
            return new PythonCommand(GetPythonStartInfo($"python.exe {command}"))
            {
                Token = token
            };
        }

        public async Task<PythonCommandResult> TryGetResult()
        {
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            await _process.WaitForExitAsync(Token);
            bool hasErrors = _process.ExitCode != 0;

            //using (StreamReader reader = (hasErrors ? _process.StandardError : _process.StandardOutput))
            //{
            //    string output = await reader.ReadToEndAsync();

            return new PythonCommandResult()
            {
                HasError = hasErrors,
                ErrorOutput = _errorOutputBuffer.ToString(),
                Output = _outputBuffer.ToString()
            };
            //}
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _errorOutputBuffer.AppendLine(e.Data);
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _outputBuffer.AppendLine(e.Data);
            }
        }

        private static ProcessStartInfo GetPythonStartInfo(string argument)
        {
            return new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = @$"/C {argument}",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = Global.PythonPath
            };
        }

        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }

            _process.Dispose();
        }
    }
}
