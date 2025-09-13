using System.Diagnostics;
using System.Globalization;
using System.Speech.Synthesis;

namespace Translumo.TTS
{
    public static class WindowsTTSHelper
    {
        public static async Task<bool> InstallTTSLanguageCapatibility(string languageCode)
        {
            var process = CreateTTSCapabilityInstallProcess(languageCode);
            if (process == null)
                return true;

            process.Start();

            // Wait asynchronously for the process to exit
            await Task.Run(() => process.WaitForExit());

            // Optional: check exit code (0 usually means success)
            return process.ExitCode == 0;
        }

        public static bool IsLanguageTTSCapabilityInstalled(string languageTag, bool exactMatching = false)
        {
            using var synth = new SpeechSynthesizer();

            try
            {
                // Try exact match
                var voices = synth.GetInstalledVoices(new CultureInfo(languageTag));
                if (voices.Count != 0)
                    return true;
            }
            catch
            {
                // Culture not valid — ignore
            }

            if (!exactMatching)
            {
                try
                {
                    // Fallback: check only the two-letter language code
                    var shortTag = languageTag.Split('-')[0];
                    var voices = synth.GetInstalledVoices(new CultureInfo(shortTag));
                    if (voices.Count != 0)
                        return true;
                }
                catch
                {
                    // Still not valid — ignore
                }
            }

            return false;
        }

        private static Process CreateTTSCapabilityInstallProcess(string nameTag)
        {
            string psCommand = $@"
            $env:TERM = 'xterm';
            $Host.UI.RawUI.WindowTitle = 'Installing TTS Language...';
            Write-Host 'Installing {nameTag} TTS Language Capability. Please wait...' -ForegroundColor Yellow;
            Write-Host 'This can take up to 20 minutes. Please be patient.' -ForegroundColor Yellow;

            $cap = Get-WindowsCapability -Online | Where-Object {{ $_.Name -like 'Language.TextToSpeech~~~{nameTag}~*' }} | Select-Object -First 1;

            if ($cap) {{
                try {{
                    Add-WindowsCapability -Online -Name $cap.Name -ErrorAction Stop;
                    Write-Host 'Installation complete!' -ForegroundColor Green;
                    Start-Sleep -Seconds 2;
                }} catch {{
                    Write-Host 'Installation failed. Please try again or use the manual method.' -ForegroundColor Red;
                    Write-Host $_ -ForegroundColor Red;
                    Start-Sleep -Seconds 60;
                }}
            }} else {{
                Write-Host 'Capability not found. Try installing the language manually.' -ForegroundColor Red;
                Start-Sleep -Seconds 60;
            }}
            ";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{psCommand}\"",
                    Verb = "runas", // enable elevation
                    UseShellExecute = true, // enable elevation prompt
                },
                EnableRaisingEvents = true, // enable process.Exited event
            };

            return process;  // Return the process after completion
        }       
    }
}