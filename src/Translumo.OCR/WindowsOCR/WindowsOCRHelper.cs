using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.Ocr;

namespace Translumo.OCR.WindowsOCR
{
    public static class WindowsOCRHelper
    {
        public static async Task<bool> InstallOcrLanguageCapatibility(string languageCode)
        {
            var process = CreateOcrCapabilityInstallProcess(languageCode);
            if (process == null)
                return true;

            process.Start();

            // Wait asynchronously for the process to exit
            await Task.Run(() => process.WaitForExit());

            // Optional: check exit code (0 usually means success)
            return process.ExitCode == 0;
        }

        public static bool IsLanguageOcrCapabilityInstalled(string languageTag, bool exactMatching = false)
        {
            var result = GetInstalledWindowsOCRLanguageByTag(languageTag, exactMatching) != null;

            return result;
        }

        private static Process CreateOcrCapabilityInstallProcess(string nameTag)
        {
            string psCommand = $@"
            $env:TERM = 'xterm';
            $Host.UI.RawUI.WindowTitle = 'Installing OCR Language...';
            Write-Host 'Installing {nameTag} OCR Language Capability. Please wait...' -ForegroundColor Yellow;

            $cap = Get-WindowsCapability -Online | Where-Object {{ $_.Name -like 'Language.OCR~~~{nameTag}~*' }} | Select-Object -First 1;

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

        private static Language GetInstalledWindowsOCRLanguageByTag(string languageTag, bool exactMathing = false)
        {
            var installedLanguages = OcrEngine.AvailableRecognizerLanguages;
            if (exactMathing)
            {
                return installedLanguages.FirstOrDefault(l => l.LanguageTag == languageTag);
            }
            else
            {
                var shortTag = languageTag.Split('-')[0]; // Get the part before the hyphen
                return installedLanguages.FirstOrDefault(l => l.LanguageTag.Split('-')[0] == shortTag);
            }
        }


        public static List<LanguageOCRCapability> GetAllLanguageOCRCapabilities()
        {
            return new List<LanguageOCRCapability>
            {
                new() { NameTag = "en-US", LanguageTag = "en-US", ShortTag = "en" },
                new() { NameTag = "en-GB", LanguageTag = "en-GB", ShortTag = "en" },
                new() { NameTag = "fr-FR", LanguageTag = "fr-FR", ShortTag = "fr" },
                new() { NameTag = "fr-CA", LanguageTag = "fr-CA", ShortTag = "fr" },
                new() { NameTag = "es-ES", LanguageTag = "es-ES", ShortTag = "es" },
                new() { NameTag = "es-MX", LanguageTag = "es-MX", ShortTag = "es" },
                new() { NameTag = "it-IT", LanguageTag = "it-IT", ShortTag = "it" },
                new() { NameTag = "de-DE", LanguageTag = "de-DE", ShortTag = "de" },
                new() { NameTag = "ja-JP", LanguageTag = "ja-JP", ShortTag = "ja" },
                new() { NameTag = "ko-KR", LanguageTag = "ko-KR", ShortTag = "ko" },
                new() { NameTag = "ru-RU", LanguageTag = "ru-RU", ShortTag = "ru" },
                new() { NameTag = "tr-TR", LanguageTag = "tr-TR", ShortTag = "tr" },
                new() { NameTag = "ar-SA", LanguageTag = "ar-SA", ShortTag = "ar" },
                new() { NameTag = "pt-PT", LanguageTag = "pt-PT", ShortTag = "pt" },
                new() { NameTag = "pt-BR", LanguageTag = "pt-BR", ShortTag = "pt" },
                new() { NameTag = "da-DK", LanguageTag = "da-DK", ShortTag = "da" },
                new() { NameTag = "bg-BG", LanguageTag = "bg-BG", ShortTag = "bg" },
                new() { NameTag = "bs-LATN-BA", LanguageTag = "bs-LATN-BA", ShortTag = "bs" },
                new() { NameTag = "cs-CZ", LanguageTag = "cs-CZ", ShortTag = "cs" },
                new() { NameTag = "el-GR", LanguageTag = "el-GR", ShortTag = "el" },
                new() { NameTag = "fi-FI", LanguageTag = "fi-FI", ShortTag = "fi" },
                new() { NameTag = "hr-HR", LanguageTag = "hr-HR", ShortTag = "hr" },
                new() { NameTag = "hu-HU", LanguageTag = "hu-HU", ShortTag = "hu" },
                new() { NameTag = "nb-NO", LanguageTag = "nb-NO", ShortTag = "nb" },
                new() { NameTag = "nl-NL", LanguageTag = "nl-NL", ShortTag = "nl" },
                new() { NameTag = "pl-PL", LanguageTag = "pl-PL", ShortTag = "pl" },
                new() { NameTag = "ro-RO", LanguageTag = "ro-RO", ShortTag = "ro" },
                new() { NameTag = "sk-SK", LanguageTag = "sk-SK", ShortTag = "sk" },
                new() { NameTag = "sl-SI", LanguageTag = "sl-SI", ShortTag = "sl" },
                new() { NameTag = "sr-CYRL-RS", LanguageTag = "sr-CYRL-RS", ShortTag = "sr" },
                new() { NameTag = "sr-LATN-RS", LanguageTag = "sr-LATN-RS", ShortTag = "sr" },
                new() { NameTag = "sv-SE", LanguageTag = "sv-SE", ShortTag = "sv" },
                new() { NameTag = "zh-CN", LanguageTag = "zh-CN", ShortTag = "zh" },
                new() { NameTag = "zh-HK", LanguageTag = "zh-HK", ShortTag = "zh" },
                new() { NameTag = "zh-TW", LanguageTag = "zh-TW", ShortTag = "zh" },
            };
        }
    }

    public class LanguageOCRCapability
    {
        public string NameTag { get; init; }
        public string LanguageTag { get; init; }
        public string ShortTag { get; init; }

        private Language _language;
        public Language Language => _language ??= new Language(LanguageTag);
        public string GetNameWithoutVersion()
        {
            return $"Language.OCR~~~{NameTag}";
        }
    }
}