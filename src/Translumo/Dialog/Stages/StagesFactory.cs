using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Python;
using Translumo.Utils;

namespace Translumo.Dialog.Stages
{
    public static class StagesFactory
    {
        private const string EASYOCR_VERSION = "1.6.2";
        private const string PIL_VERSION = "9.5.0";

        public static InteractionStage CreateLanguageChangeStages(DialogService dialogService, Action changeLangAction, ILogger logger)
        {
            return new ActionInteractionStage(dialogService, () =>
                        Task.Factory.StartNew(changeLangAction), LocalizationManager.GetValue($"Str.Stages.SwitchLanguage"))
                .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "Language change error"), LocalizationManager.GetValue("Str.Stages.SwitchLanguageError")));
        }

        public static InteractionStage CreateWindowsOcrCheckingStages(
            DialogService dialogService,
            string languageCode,
            InteractionStage enableFlagStage,
            ILogger logger)
            {
            return new ConditionalInteractionStage(
                    dialogService,
                    () => Task.FromResult(OCR.WindowsOCR.WindowsOCRHelper.IsLanguageOcrCapabilityInstalled(languageCode)),
                    LocalizationManager.GetValue("Str.Stages.CheckLangPack"))
                .AddNextFalse(
                    new DialogQuestionInteractionStage(
                            dialogService,
                            string.Format(LocalizationManager.GetValue("Str.Stages.OCRLangPackQuestion", true), languageCode))
                        .AddNextStage(
                            new ConditionalInteractionStage(
                                    dialogService,
                                    async () => await OCR.WindowsOCR.WindowsOCRHelper.InstallOcrLanguageCapatibility(languageCode),
                                    LocalizationManager.GetValue("Str.Stages.InstallationLangPack"))
                                .AddNextStage(
                                    new DialogInteractionStage(
                                            dialogService,
                                            LocalizationManager.GetValue("Str.Stages.LangPackInstalledRestart"))
                                        .AddNextStage(enableFlagStage))
                                .AddException(
                                    new ExceptionInteractionStage(
                                        dialogService,
                                        ex => logger.LogError(ex, "Language windows pack install error"),
                                        LocalizationManager.GetValue("Str.Stages.InstallationLangError")))))
                .AddNextStage(enableFlagStage)
                .AddException(
                    new ExceptionInteractionStage(
                        dialogService,
                        ex => logger.LogError(ex, "Checking language pack error"),
                        LocalizationManager.GetValue("Str.Stages.CheckLangPackError", true)));
        }

        public static InteractionStage CreateWindowsTtsCheckingStages(
            DialogService dialogService,
            string languageCode,
            InteractionStage enableFlagStage,
            ILogger logger)
            {
            return new ConditionalInteractionStage(
                    dialogService,
                    () => Task.FromResult(TTS.WindowsTTSHelper.IsLanguageTTSCapabilityInstalled(languageCode)),
                    LocalizationManager.GetValue("Str.Stages.CheckLangPack"))
                .AddNextFalse(
                    new DialogQuestionInteractionStage(
                        dialogService,
                        string.Format(LocalizationManager.GetValue("Str.Stages.TTSLangPackQuestion", true), languageCode))
                    .AddNextStage(
                        new ConditionalInteractionStage(
                                dialogService,
                                async () => await TTS.WindowsTTSHelper.InstallTTSLanguageCapatibility(languageCode),
                                LocalizationManager.GetValue("Str.Stages.InstallationLangPack"))
                            .AddNextStage(
                                new DialogInteractionStage(
                                        dialogService,
                                        LocalizationManager.GetValue("Str.Stages.LangPackInstalledRestart"))
                                    .AddNextStage(enableFlagStage))
                            .AddException(
                                new ExceptionInteractionStage(
                                    dialogService,
                                    ex => logger.LogError(ex, "Language windows pack install error"),
                                    LocalizationManager.GetValue("Str.Stages.InstallationLangError")))))
                .AddNextStage(enableFlagStage)
                .AddException(
                    new ExceptionInteractionStage(
                        dialogService,
                        ex => logger.LogError(ex, "Checking language pack error"),
                        LocalizationManager.GetValue("Str.Stages.CheckLangPackError", true)));
        }

        public static InteractionStage CreateEasyOcrCheckingStages(DialogService dialogService, InteractionStage enableFlagStage, ILogger logger)
        {
            return new ConditionalInteractionStage(
                dialogService,
                async () =>
                    await PythonProvider.ModuleIsInstalledAsync("easyocr")
                    && await PythonProvider.ModuleIsInstalledAsync("torch")
                    && await PythonProvider.ModuleIsInstalledAsync("torchvision"),
                LocalizationManager.GetValue("Str.Stages.CheckPyModules"))
                .AddNextFalse(new DialogQuestionInteractionStage(dialogService, LocalizationManager.GetValue("Str.Stages.PyModulesQuestion", true))
                    .AddNextStage(new DialogQuestionInteractionStage(dialogService, LocalizationManager.GetValue("Str.Stages.PyModulesQuestion2", true))
                        .AddNextStage(new ActionInteractionStage(dialogService, () => PythonProvider.InstallModuleAsync("torch torchvision --index-url https://download.pytorch.org/whl/cu118"), LocalizationManager.GetValue("Str.Stages.InstallationPyModule1"))
                            .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "PyTorch installation error"), "{0}"))
                            .AddNextStage(new ActionInteractionStage(dialogService, () => PythonProvider.InstallModuleAsync($"easyocr=={EASYOCR_VERSION}"), LocalizationManager.GetValue("Str.Stages.InstallationPyModule2"))
                                .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "EasyOCR installation error"), "{0}"))
                                .AddNextStage(new ActionInteractionStage(dialogService, () => PythonProvider.InstallModuleAsync($"Pillow=={PIL_VERSION}", true), LocalizationManager.GetValue("Str.Stages.InstallationPyModule3"))
                                    .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "Pillow reinstall error"), "{0}"))
                                    .AddNextStage(new DialogInteractionStage(dialogService, LocalizationManager.GetValue("Str.Stages.PyModulesInstalled"))
                                        .AddNextStage(enableFlagStage)))))))
                .AddNextStage(enableFlagStage)
                .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "Easy OCR installation checking error"), LocalizationManager.GetValue("Str.Stages.PyModulesCheckError")));
        }

        //public static InteractionStage CreateSileroTtsCheckingStages(LanguageDescriptor languageDescriptor, DialogService dialogService, InteractionStage enableFlagStage, ILogger logger)
        //{

        //    var warningMessage = string.Format(
        //        LocalizationManager.GetValue("Str.Stages.TtsNotSupportLanguageTemplate", true),
        //        LocalizationManager.GetValue("Str.LangSettings.TtsSystem", true),
        //        //TTSEngines.SileroTTS.ToString(),
        //        LocalizationManager.GetValue($"Str.Languages.{languageDescriptor.Language}", true));


        //    InteractionStage InstallPythonModuleStage(string moduleName) =>
        //        new ActionInteractionStage(
        //            dialogService,
        //            () => PythonProvider.InstallModuleAsync(moduleName),
        //            string.Format(LocalizationManager.GetValue("Str.Stages.InstallationPyModuleTemplate"), moduleName))
        //            .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, $"{moduleName} installation error"), "{0}"));

        //    return new ConditionalInteractionStage(
        //        dialogService,
        //        () => Task.FromResult(SileroTTSEngine.IsLanguageSupported(languageDescriptor.Code)))
        //        .AddNextFalse(new ExceptionInteractionStage(
        //            dialogService,
        //            _ => { return; },
        //            warningMessage)
        //        { InputException = new NotSupportedException() })
        //        .AddNextStage(new ConditionalInteractionStage(
        //            dialogService,
        //            async () =>
        //                await PythonProvider.ModuleIsInstalledAsync("numpy")
        //                && await PythonProvider.ModuleIsInstalledAsync("torch")
        //                && await PythonProvider.ModuleIsInstalledAsync("IPython"),
        //            LocalizationManager.GetValue("Str.Stages.CheckPyModules"))
        //            .AddNextFalse(
        //                new DialogQuestionInteractionStage(dialogService, LocalizationManager.GetValue("Str.Stages.PyModulesQuestion2", true))
        //                    .AddNextStage(InstallPythonModuleStage("torch")
        //                        .AddNextStage(InstallPythonModuleStage("numpy")
        //                            .AddNextStage(InstallPythonModuleStage("IPython")
        //                                .AddNextStage(new DialogInteractionStage(dialogService, LocalizationManager.GetValue("Str.Stages.PyModulesInstalled"))
        //                                    .AddNextStage(enableFlagStage))))))
        //            .AddNextStage(enableFlagStage)
        //            .AddException(new ExceptionInteractionStage(dialogService, (ex) => logger.LogError(ex, "Silero TTS installation checking error"), LocalizationManager.GetValue("Str.Stages.PyModulesCheckError"))));
        //}
    }
}
