*** Begin Patch
*** Update File: src/Translumo.Processing/TranslationProcessingService.cs
@@
-                if (!string.IsNullOrWhiteSpace(translation) && !_textResultCacheService.IsTranslatedCached(translation, iterationId))
-                {
-                    Interlocked.Exchange(ref _lastTranslatedTextTicks, DateTime.UtcNow.Ticks);
-                    _chatTextMediator.SendText(translation, true);
-                    _ttsEngine.SpeechText(translation);
-                }
+                if (!string.IsNullOrWhiteSpace(translation) && !_textResultCacheService.IsTranslatedCached(translation, iterationId))
+                {
+                    Interlocked.Exchange(ref _lastTranslatedTextTicks, DateTime.UtcNow.Ticks);
+                    _chatTextMediator.SendText(translation, true);
+                    _ttsEngine.SpeechText(translation);
+
+                    // send original + translation event for overlay/anki
+                    try
+                    {
+                        _chatTextMediator.SendText(text, translation);
+                    }
+                    catch { }
+                }
*** End Patch
