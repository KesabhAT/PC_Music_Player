using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PC_Music_Player.Service;
using PC_Music_Player.ViewModels;
using PC_Music_Player.Views;

namespace PC_Music_Player;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<IAudioService, AudioService>();

            //Instructing the dependency Injection container to provide the MainWindow instance everytime a Window instance is required by a class, here FilePickerService requires Window  
            services.AddSingleton<Window>(sp => sp.GetRequiredService<MainWindow>());

            var provider = services.BuildServiceProvider();
            var window = provider.GetRequiredService<MainWindow>();
            window.DataContext = provider.GetRequiredService<MainViewModel>();
            desktop.MainWindow = window;

        }

        base.OnFrameworkInitializationCompleted();
    }
}