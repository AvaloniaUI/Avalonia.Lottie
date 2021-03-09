using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;

namespace Avalonia.Lottie.Sample
{
    class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseManagedSystemDialogs()
                .With(new Win32PlatformOptions()
                {
                    AllowEglInitialization = true
                })
                .With(new X11PlatformOptions()
                {
                })
                .With(new MacOSPlatformOptions()
                {
                })
                .LogToTrace()
                .UseReactiveUI();
    }
}