[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Github All Releases](https://img.shields.io/github/downloads/ramjke/Translumo/total.svg)]()

<p align="center">
  <img width="670" src="https://github.com/ramjke/Translumo/assets/29047281/8985049f-ea1c-428e-94be-042ece66cb54">
</p>
  <h2 align="center" style="border: 0">Advanced Real-Time Screen Translator (Agentic Custom Build)</h2>

<p align="center"><strong>English</strong> | <a href="docs/README-RU.md"><strong>Русский</strong></a> | <strong>Indonesian (Mod)</strong></p>

---

# 🚀 Agentic Mod - What's New & Fixed?

This version of **Translumo** has been heavily customized and fixed by an AI Agent (Antigravity). It includes exclusive features and major stability fixes that are not present in the original repository.

## ✨ New Features
1. **AI Translator Integration**: Added support for translating via Advanced AI Models (e.g., Gemini, ChatGPT/OpenAI compatible APIs) to achieve highly contextual and natural game translations.
2. **Local LibreTranslate Support**: Fully integrated a completely offline, local LibreTranslate server support. Translumo will automatically manage the server startup and shutdown without you needing to open the terminal.
3. **Clear Chat Hotkey (`ALT+C`)**: A newly implemented global hotkey that allows you to instantly clear the chat bubble/translation screen when it gets too cluttered.

## 🛠️ Major Bug Fixes & Stability Improvements
1. **Proxy Error Fixes**: Resolved persistent proxy errors that blocked connections to translators.
2. **LibreTranslate Lifecycle Manager**: Fixed a bug where simply opening or minimizing the Settings menu would aggressively kill the `port 5000` LibreTranslate server. The server is now safely managed by a background singleton manager and only shuts down when you completely exit Translumo from the tray.
3. **Restored Anti Double-Click**: Re-implemented the lock on the "Run LibreTranslate" button to prevent accidental spam-clicking that could cause duplicate server spawns.
4. **Dark Theme Removal**: Systematically removed the highly unstable, experimental Dark Theme feature that was causing Translumo to force-close and crash on startup. 
5. **Setup Guide Improvements**: Added direct hyperlinks to the supported language codes inside the LibreTranslate guide within the app, making it extremely easy to copy-paste the exact `argospm` language model commands.

---

## Sibling Project
This project has a sibling called **[Lookupper](https://lookupper.com)** — an on-screen dictionary for language learning. It is similar to Translumo but built for a different purpose. Lookupper is built to help you *learn* a language, not just depend on a translator forever.

Lookupper is my commercial project with a free version. If you find it useful and decide to grab the Pro version, you'll also be supporting the development of both Lookupper and Translumo.

<a href="https://lookupper.com">
<img width="300" alt="Lookupper" src="https://github.com/user-attachments/assets/ef2f83b3-e15f-4bd3-826e-858266f36c93" />
</a>

## Download Translumo

**Direct download link to the latest version:**  
[Translumo_1.0.2.zip](https://github.com/ramjke/Translumo/releases/download/v.1.0.2/Translumo_1.0.2.zip)   
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

- **Integrated modern OCR engines**: Windows OCR (recommended), Tesseract 5.2 (legacy), EasyOCR (legacy)

- **Available translators**: DeepL (recommended), Google Translate, Yandex Translate, Naver Papago, AI Translator (Custom), LibreTranslate (Custom).

- **Supported recognition languages**: English, Russian, Japanese, Chinese (Simplified), Korean.

- **Supported translation languages**: English, Russian, Japanese, Chinese (Simplified), Korean, French, Spanish, German, Portuguese, Italian, Vietnamese, Thai, Turkish, Arabic, Greek, Brazilian Portuguese, Polish, Belarusian, Persian, Indonesian, Bulgarian, Czech, Danish, Estonian, Finnish, Hungarian, Lithuanian, Latvian, Dutch, Romanian, Slovak, Slovenian, Swedish, Ukrainian.

## System Requirements

### Minimal requirements to use Tesseract and Windows OCR
- Windows 10 version 2004 (build 19041) or later, or Windows 11
- DirectX 11 compatible GPU
- 2 GB RAM
