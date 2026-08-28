using System;
using System.IO;
using AudioSlicerPro.Services;
using AudioSlicerPro.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AudioSlicerPro;

public sealed partial class MainWindow : Window
{
    private readonly ILocalizationService _localizationService;

    public MainWindow()
    {
        InitializeComponent();
        Title = "AudioSlicer Pro";
        SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

        _localizationService = ((App)Application.Current).Services.GetRequiredService<ILocalizationService>();
        _localizationService.PropertyChanged += (s, e) => UpdateNavTitles();
        UpdateNavTitles();

        try
        {
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            if (File.Exists(iconPath))
            {
                AppWindow.SetIcon(iconPath);
            }
        }
        catch
        {
            // Ignore if setting icon fails on unsupported platforms
        }
    }

    private void UpdateNavTitles()
    {
        NavItemMain.Content = _localizationService.GetString("NavMain");
        NavItemSettings.Content = _localizationService.GetString("NavSettings");
    }

    private void OnThemeToggleClick(object sender, RoutedEventArgs e)
    {
        if (Content is FrameworkElement rootElement)
        {
            ElementTheme currentTheme = rootElement.RequestedTheme;
            if (currentTheme == ElementTheme.Default)
            {
                currentTheme = (Application.Current.RequestedTheme == ApplicationTheme.Dark) 
                    ? ElementTheme.Dark 
                    : ElementTheme.Light;
            }

            if (currentTheme == ElementTheme.Dark)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
                ThemeIcon.Glyph = "\uE706"; // Sun icon for Light mode
            }
            else
            {
                rootElement.RequestedTheme = ElementTheme.Dark;
                ThemeIcon.Glyph = "\uE708"; // Moon icon for Dark mode
            }
        }
    }

    private void OnNavViewLoaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(MainPage));
    }

    private void OnNavViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem item)
        {
            string? tag = item.Tag?.ToString();
            if (tag == "MainPage")
            {
                ContentFrame.Navigate(typeof(MainPage));
            }
            else if (tag == "SettingsPage")
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
        }
    }
}
