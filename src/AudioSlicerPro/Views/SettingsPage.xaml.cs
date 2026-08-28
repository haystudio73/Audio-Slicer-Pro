using System;
using AudioSlicerPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioSlicerPro.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<SettingsViewModel>();
        DataContext = ViewModel;
    }

    private async void OnBrowseTmpFolderClick(object sender, RoutedEventArgs e)
    {
        string? path = await PickFolderAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ViewModel.TmpFolderPath = path;
        }
    }

    private async void OnBrowseDestFolderClick(object sender, RoutedEventArgs e)
    {
        string? path = await PickFolderAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ViewModel.DestFolderPath = path;
        }
    }

    private async void OnBrowseMp3FolderClick(object sender, RoutedEventArgs e)
    {
        string? path = await PickFolderAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ViewModel.Mp3DestFolderPath = path;
        }
    }

    private async System.Threading.Tasks.Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        IntPtr hwnd = WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");

        StorageFolder folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
