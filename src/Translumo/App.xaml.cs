*** Begin Patch
*** Update File: src/Translumo/App.xaml.cs
@@
             services.AddSingleton<ChatUITextMediator>(chatMediatorInstance);
+            services.AddSingleton<Services.AnkiService>();
             services.AddSingleton<ChatWindowViewModel>();
*** End Patch
