using System.Globalization;
using System.Speech.Synthesis;

namespace Translumo.TTS.Engines;

public class WindowsTTSEngine : ITTSEngine
{
    private readonly VoiceInfo _voiceInfo;
    private readonly SpeechSynthesizer _synthesizer;

    public WindowsTTSEngine(string languageCode)
    {
        _synthesizer = new SpeechSynthesizer();
        _synthesizer.SetOutputToDefaultAudioDevice();
        _synthesizer.Rate = 3;

        _voiceInfo = _synthesizer.GetInstalledVoices(new CultureInfo(languageCode)).FirstOrDefault()?.VoiceInfo;
    }

    public void SpeechText(string text)
    {
        // https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/june/speech-text-to-speech-synthesis-in-net
        if (_voiceInfo == null)
        {
            return;
        }
        var builder = new PromptBuilder();
        builder.StartVoice(_voiceInfo);
        builder.AppendText(text);
        builder.EndVoice();
        _synthesizer.SpeakAsyncCancelAll();
        _synthesizer.SpeakAsync(builder);
    }

    public void Dispose()
    {
        _synthesizer.Dispose();
    }
}