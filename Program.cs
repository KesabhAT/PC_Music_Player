using Avalonia;
using System;
using System.Diagnostics;

namespace PC_Music_Player;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                UseDBusFilePicker = false
            })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont();
}
