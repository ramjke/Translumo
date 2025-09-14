[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Github All Releases](https://img.shields.io/github/downloads/ramjke/Translumo/total.svg)]()

<p align="center">
  <img width="670" src="https://github.com/ramjke/Translumo/assets/29047281/8985049f-ea1c-428e-94be-042ece66cb54">
</p>
  <h2 align="center" style="border: 0">Advanced Real-Time Screen Translator</h2>

<p align="center"><strong>English</strong> | <a href="docs/README-RU.md"><strong>Русский</strong></a></p>

## Project Status Update
Translumo has a new maintainer. The project is no longer abandoned — development is active, improvements are ongoing, and contributions are welcome. Expect fresh updates, bug fixes, and new features as the project continues to grow.

## Download Translumo

**Direct download link to the latest version:**  
[Translumo_1.0.1.zip](https://github.com/ramjke/Translumo/releases/download/v.1.0.1/Translumo_1.0.1.zip)   
After downloading, unzip the archive and run `Translumo.exe`.

Version 1.0.x includes many changes and improvements compared to versions 0.9.x. You can view the full list of updates on the [Releases page](https://github.com/ramjke/Translumo/releases). 

## Main Features

- **High text recognition precision**  
  Translumo allows combining multiple OCR engines simultaneously. It uses a machine learning model to score each OCR result and selects the best one.  

  <p align="center">
    <img width="740" src="https://github.com/ramjke/Translumo/assets/29047281/649e5fab-a5de-4c54-a3d8-f7ea95b8f218">
  </p>

- **Game oriented**  
  Designed for real-time translation in PC games, but works anywhere on the screen with any application.

- **Low latency**  
  Several optimizations reduce system impact and minimize latency between text appearance and translation.

- **Integrated modern OCR engines**: Tesseract 5.2, Windows OCR, EasyOCR

- **Available translators**: Google Translate, Yandex Translate, Naver Papago, DeepL

- **Supported recognition languages**: English, Russian, Japanese, Chinese (Simplified), Korean

- **Supported translation languages**: English, Russian, Japanese, Chinese (Simplified), Korean, French, Spanish, German, Portuguese, Italian, Vietnamese, Thai, Turkish, Arabic, Greek, Brazilian Portuguese, Polish, Belarusian, Persian, Indonesian

## System Requirements

### Minimal requirements to use Tesseract and Windows OCR
- Windows 10 version 2004 (build 19041) or later, or Windows 11
- DirectX 11 compatible GPU
- 2 GB RAM

### Minimal requirements to use EasyOCR
- NVIDIA GPU with CUDA SDK 11.8 support (GTX 750, 8xxM, 9xx series or newer)
- 8 GB RAM
- At least 5 GB of free storage space

## How to Use

![Preview](https://github.com/ramjke/Translumo/blob/7f4a73ffba0e5a0090ea0bfc3d72acb99832a0f4/docs/preview-EN.gif)

1. Open the Settings (**Alt+G**)
2. Select languages: source language for OCR and translation language
3. Select text recognition engines (see Usage Tips for recommended modes)
4. Define the capture area: press **Alt+Q** and select an area on the screen
5. Run translation (press **~**)

## Usage Tips

- Keep Windows OCR turned on — it’s the fastest and most effective for primary text detection with minimal performance impact.

### Recommended Combinations of OCR Engines
- **Tesseract + Windows OCR + EasyOCR** — advanced mode with highest precision
- **Tesseract + Windows OCR** — lower system impact; suitable for simple text backgrounds and common fonts
- **Windows OCR + EasyOCR** — for specific complex cases; disabling Tesseract can reduce text noise

### Select Minimum Capture Area
Reducing the capture area decreases the chance of picking up random letters from the background. Larger frames take longer to process.

### Use Proxy List to Avoid Blocking by Translation Services
Some translators may block clients sending many requests. Configure personal or shared IPv4 proxies (1-2 is usually enough) under **Languages → Proxy tab**. The app will alternate proxies to reduce requests from a single IP.

### Use Borderless or Windowed Modes in Games (Not Fullscreen)
These modes are required for correct translation overlay display. If your game does not support them, use tools like [Borderless Gaming](https://github.com/Codeusa/Borderless-Gaming).

### Install the Application on an SSD
Reduces cold launch time with the EasyOCR engine as large models are loaded into RAM.

## FAQ

**Q: I get "Failed to capture screen" or nothing happens after translation starts**  
A: Ensure the target window is active. Restart Translumo or reopen the target window if needed.

**Q: I get "Text translation failed" after successful translation**  
A: Translation service may have temporarily blocked requests from your IP. Change translator or configure a proxy list.

**Q: Can't enable Windows OCR**  
A: Run the application as Administrator. Translumo checks installed Windows language packs via PowerShell.

**Q: Borderless/windowed mode is set, but the translation window is under the game**  
A: With the game running and focused, press the hotkey (**Alt+T** by default) to hide and show the translation window.

**Q: EasyOCR package download failed**  
A: Try reinstalling while connected to a VPN.

**Q: Hotkeys don't work**  
A: Other applications may be intercepting hotkeys.

**Q: Text detection failed (TesseractOCREngine)**  
A: Ensure the application path contains only Latin letters.

## Build

*Visual Studio 2022 and .NET 8 SDK are required.*

1. Clone the repository (the **master** branch always corresponds to the latest release):

    ```bash
    git clone https://github.com/ramjke/Translumo.git
    ```

> Note: During the build, **binaries_extract.bat** will automatically download and extract models and Python binaries (~400 MB) to the target output directory.

## Credits

- [Material Design In XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)  
- [Tesseract .NET wrapper](https://github.com/charlesw/tesseract)  
- [OpenCvSharp](https://github.com/shimat/opencvsharp)  
- [Python.NET](https://github.com/pythonnet/pythonnet)  
- [EasyOCR](https://github.com/JaidedAI/EasyOCR)  
- [Silero TTS](https://github.com/snakers4/silero-models)  

## Alternative Solutions

- [Lookupper](https://lookupper.com) — on-screen dictionary and translator for language learning.
- [ScreTran](https://github.com/PavlikBender/ScreTran) — simple screen translator.
- [ScreenTranslator](https://github.com/OneMoreGres/ScreenTranslator) - screen capture, OCR and translation tool.

