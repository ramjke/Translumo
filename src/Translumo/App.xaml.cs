using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SharpDX.XInput;
using Translumo.Configuration;
using Translumo.Dialog;
using Translumo.HotKeys;
using Translumo.Infrastructure.Dispatching;
using Translumo.Infrastructure.Encryption;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.MachineLearning;
using Translumo.Infrastructure.Python;
using Translumo.MVVM.Models;
using Translumo.MVVM.ViewModels;
using Translumo.OCR;
using Translumo.OCR.Configuration;
using Translumo.Processing;
using Translumo.Processing.Configuration;
using Translumo.Processing.Interfaces;
using Translumo.Processing.TextProcessing;
using Translumo.Services;
using Translumo.Translation;
using Translumo.Translation.Configuration;
using Translumo.TTS;
using Translumo.Update;
using Translumo.Utils;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Translumo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public App()
        {
            Log.Logger = CreateLogger();

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            this._serviceProvider = services.BuildServiceProvider();
            this._logger = _serviceProvider.GetService<ILogger<App>>();

            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.ExceptionObject as Exception, "Unhandled app exception");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "Unhandled app exception");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            var configurationStorage = _serviceProvider.GetService<ConfigurationStorage>();
            configurationStorage.SaveConfiguration();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configurationStorage = _serviceProvider.GetService<ConfigurationStorage>();
            configurationStorage.LoadConfiguration();

            var chatViewModel = _serviceProvider.GetService<ChatWindowViewModel>();
            var dialogService = _serviceProvider.GetService<DialogService>();
            dialogService.ShowWindowAsync(chatViewModel);

            _serviceProvider.RegisterUIInputController();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(builder => builder.AddSerilog(/*Log.Logger,*/ dispose: true));

            services.AddScoped<SettingsViewModel>();
            services.AddScoped<AppearanceSettingsViewModel>();
            services.AddScoped<HotkeysSettingsViewModel>();
            services.AddScoped<LanguagesSettingsViewModel>();
            services.AddScoped<OcrSettingsViewModel>();

            var chatWindowConfiguration = ChatWindowConfiguration.Default;
            services.AddSingleton<OcrGeneralConfiguration>(OcrGeneralConfiguration.Default);
            services.AddSingleton<TranslationConfiguration>(TranslationConfiguration.Default);
            services.AddSingleton<TtsConfiguration>(TtsConfiguration.Default);
            services.AddSingleton<ChatWindowConfiguration>(chatWindowConfiguration);
            services.AddSingleton<HotKeysConfiguration>(HotKeysConfiguration.Default);
            services.AddSingleton<SystemConfiguration>(SystemConfiguration.Default);
            services.AddSingleton<TextProcessingConfiguration>(chatWindowConfiguration.TextProcessing);

            var chatMediatorInstance = new ChatUITextMediator();
            services.AddSingleton<IChatTextMediator, ChatUITextMediator>(provider => chatMediatorInstance);
            services.AddSingleton<ChatUITextMediator>(chatMediatorInstance);
            services.AddSingleton<ChatWindowViewModel>();
            services.AddSingleton<ChatWindowModel>();
            services.AddSingleton<HotKeysServiceManager>();
            services.AddSingleton<ScreenCaptureConfiguration>();
            services.AddSingleton<DialogService>();
            services.AddSingleton<LanguageService>();
            services.AddSingleton<TextDetectionProvider>();
            services.AddSingleton<IActionDispatcher, InteractionActionDispatcher>();
            services.AddSingleton<TextValidityPredictor>();
            services.AddSingleton<IControllerService, GamepadService>();
            services.AddSingleton<IControllerInputProvider, ControllerInputProvider>();
            services.AddSingleton<ObservablePipe<Keystroke>>(new ObservablePipe<Keystroke>(Application.Current.Dispatcher));
            services.AddSingleton<UpdateManager>();
            services.AddSingleton<IReleasesClient, GithubApiClient>(provider => new GithubApiClient("ramjke", "Translumo"));
            services.AddSingleton<ICapturerFactory, ScreenCapturerFactory>();
            services.AddSingleton<PythonEngineWrapper>();

            services.AddTransient<IProcessingService, TranslationProcessingService>();
            services.AddTransient<OcrEnginesFactory>();
            services.AddTransient<TranslatorFactory>();
            services.AddTransient<TextResultCacheService>();
            services.AddTransient<IPredictor<InputTextPrediction, OutputTextPrediction>, MlPredictor<InputTextPrediction, OutputTextPrediction>>();
            services.AddTransient<IEncryptionService, AesEncryptionService>();
            services.AddTransient<LanguageDescriptorFactory>();
            services.AddTransient<TtsFactory>();


            services.AddConfigurationStorage();
        }

        private Logger CreateLogger()
        {
            var configuration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File("Logs/log.txt", LogEventLevel.Warning, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 10);

#if DEBUG
            configuration = configuration.WriteTo.File("Logs/trace.txt", LogEventLevel.Verbose, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}");
#endif

            return configuration.CreateLogger();
        }

    }
}
