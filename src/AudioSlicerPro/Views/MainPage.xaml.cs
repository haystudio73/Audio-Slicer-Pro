using System;
using System.IO;
using AudioSlicerPro.Models;
using AudioSlicerPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioSlicerPro.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.PreviewAudioPath))
            {
                if (!string.IsNullOrEmpty(ViewModel.PreviewAudioPath) && File.Exists(ViewModel.PreviewAudioPath))
                {
                    AudioPreviewPlayer.Source = MediaSource.CreateFromUri(new Uri(ViewModel.PreviewAudioPath));
                    AudioPreviewPlayer.MediaPlayer.Play();
                }
            }
        };

        AudioPreviewPlayer.MediaPlayer.MediaEnded += (sender, args) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.SelectedOutputFile != null)
                {
                    ViewModel.SelectedOutputFile.IsPlaying = false;
                }
            });
        };
    }

    private void OnPlayOutputItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OutputFileItem item)
        {
            ViewModel.PlayOutputFileCommand.Execute(item);
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        if (e.DragUIOverride != null)
        {
            e.DragUIOverride.Caption = "Kéo thả file audio vào đây";
            e.DragUIOverride.IsContentVisible = true;
        }
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items[0] is StorageFile file)
            {
                await ViewModel.SetSelectedFileAsync(file.Path);
            }
        }
    }

    private async void OnBrowseFileClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        
        IntPtr hwnd = WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        picker.FileTypeFilter.Add(".mp3");
        picker.FileTypeFilter.Add(".wav");
        picker.FileTypeFilter.Add(".flac");
        picker.FileTypeFilter.Add(".m4a");
        picker.FileTypeFilter.Add("*");

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await ViewModel.SetSelectedFileAsync(file.Path);
        }
    }
}
