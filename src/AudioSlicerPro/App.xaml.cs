using System;
using System.IO;
using AudioSlicerPro.Services;
using AudioSlicerPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace AudioSlicerPro;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;
    public MainWindow MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Load persisted settings first
        var settingsService = Services.GetRequiredService<ISettingsService>();
        await settingsService.LoadSettingsAsync();

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IAudioService, AudioService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }
}
