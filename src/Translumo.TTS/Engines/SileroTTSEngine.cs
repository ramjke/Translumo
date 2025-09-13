//using System.Media;
//using Translumo.Infrastructure.Constants;
//using Translumo.Infrastructure.Python;

//namespace Translumo.TTS.Engines;

//public class SileroTTSEngine : ITTSEngine
//{
//    private dynamic _ipython;
//    private dynamic _model;
//    private string _voice;
//    private readonly string _modelPath;
//    private readonly PythonEngineWrapper _pythonEngine;
//    private readonly List<IDisposable> _pyObjects = new();
//    private readonly SoundPlayer _player = new SoundPlayer();
//    private readonly object _lockObject = new();
//    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

//    private const string _sileroModelDirectory = "Silero";


//    private const int _wave_rate = 48000;


//    public SileroTTSEngine(PythonEngineWrapper pythonEngine, string langCode)
//    {
//        _pythonEngine = pythonEngine;
//        _modelPath = GetModelFullPath(langCode);
//        Init();
//    }

//    private void Init()
//    {
//        _pythonEngine.Init();

//        _pythonEngine.Execute(() =>
//        {
//            var np = _pythonEngine.Import("numpy");
//            _ipython = _pythonEngine.Import("IPython");
//            dynamic torch = _pythonEngine.Import("torch");

//            using dynamic device = torch.device("cpu");
//            torch.set_num_threads(4);

//            _model = torch.package.PackageImporter(_modelPath).load_pickle("tts_models", "model");
//            _model.to(device);

//            _pyObjects.Add(_model);
//            _pyObjects.Add(np);
//            _pyObjects.Add(torch);
//            _pyObjects.Add(_ipython);
//        });

//        _voice = ((string[])_model.speakers).First();
//    }

//    public void SpeechText(string text)
//    {
//        CancellationTokenSource currentToken;
//        lock (_lockObject)
//        {
//            _cancellationTokenSource.Cancel();
//            _cancellationTokenSource = new CancellationTokenSource();
//            currentToken = _cancellationTokenSource;
//        }

//        Task.Factory.StartNew(() => GenerateAudio(text), currentToken.Token)
//            .ContinueWith((bytesTask) =>
//             {
//                 if (!bytesTask.IsCompletedSuccessfully)
//                 {
//                     return;
//                 }

//                 PlayWavBytes(bytesTask.Result);
//             }, currentToken.Token);
//    }

//    private byte[] GenerateAudio(string text) =>
//        _pythonEngine.Execute(() =>
//        {
//            using var audio = _model.apply_tts(text: text, speaker: _voice, sample_rate: _wave_rate);
//            using var pyAudio = _ipython.display.Audio(audio, rate: _wave_rate);
//            return (byte[])pyAudio.data;
//        });

//    private void PlayWavBytes(byte[] wavBytes)
//    {
//        _player.Stop();
//        using (var ms = new MemoryStream(wavBytes))
//        {
//            _player.Stream = ms;
//            _player.Play();
//        }
//    }

//    public void Dispose()
//    {
//        _cancellationTokenSource.Cancel();
//        _cancellationTokenSource.Dispose();
//        _player.Stop();
//        _player.Dispose();

//        foreach (var pyObject in _pyObjects)
//        {
//            pyObject.Dispose();
//        }

//        _pythonEngine.Dispose();
//    }

//    public static bool IsLanguageSupported(string langCode) => GetModelForLanguage(langCode) != null;

//    private string GetModelFullPath(string langCode)
//    {
//        var modelUrl = GetModelForLanguage(langCode)?.FileUrl
//            ?? throw new NotSupportedException($"{nameof(SileroTTSEngine)} not support language '{langCode}'");

//        var fileName = Path.GetFileName(modelUrl);
//        var fullPath = Path.Combine(
//            Global.ModelsPath,
//            _sileroModelDirectory,
//            fileName);

//        return fullPath;
//    }

//    private static ModelDescription? GetModelForLanguage(string lang) =>
//        lang switch
//        {
//            "en-US" =>
//                new("https://models.silero.ai/models/tts/en/v3_en.pt",
//                    "Can you can a canned can into an un-canned can like a canner can can a canned can into an un-canned can?"),
//            "ru-RU" =>
//                new("https://models.silero.ai/models/tts/ru/v4_ru.pt",
//                    "В н+едрах т+ундры в+ыдры в г+етрах т+ырят в в+ёдра +ядра к+едров."),
//            "fr-FR" =>
//                new("https://models.silero.ai/models/tts/fr/v3_fr.pt",
//                    "Je suis ce que je suis, et si je suis ce que je suis, qu’est ce que je suis."),
//            "de-DE" =>
//                new("https://models.silero.ai/models/tts/de/v3_de.pt",
//                    "Fischers Fritze fischt frische Fische, Frische Fische fischt Fischers Fritze.'"),
//            "es-ES" =>
//                new("https://models.silero.ai/models/tts/es/v3_es.pt",
//                    "Hoy ya es ayer y ayer ya es hoy, ya llegó el día, y hoy es hoy."),
//            _ => null
//        };

//    private sealed record ModelDescription(string FileUrl, string WarmUpText);
//}