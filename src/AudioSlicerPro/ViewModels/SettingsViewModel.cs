using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AudioSlicerPro.Models;
using AudioSlicerPro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioSlicerPro.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string TmpFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Mp3DestFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; } = "vi-VN";

    [ObservableProperty]
    public partial int SegmentDurationSeconds { get; set; } = 15;

    [ObservableProperty]
    public partial bool TrimSilence { get; set; } = true;

    [ObservableProperty]
    public partial int SilenceThresholdDb { get; set; } = -50;

    [ObservableProperty]
    public partial string SelectedAspectRatio { get; set; } = "16:9";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public ObservableCollection<string> AvailableLanguages { get; } = new() { "vi-VN", "en-US" };
    public ObservableCollection<string> AvailableAspectRatios { get; } = new() { "16:9", "9:16" };

    public string Title => _localizationService.GetString("NavSettings");
    public string LabelTmpFolder => _localizationService.GetString("TmpFolder");
    public string LabelDestFolder => _localizationService.GetString("DestFolder");
    public string LabelMp3Folder => _localizationService.GetString("Mp3Folder");
    public string LabelLanguage => _localizationService.GetString("Language");
    public string LabelSegmentDuration => _localizationService.GetString("SegmentDuration");
    public string LabelTrimSilence => _localizationService.GetString("TrimSilence");
    public string LabelSilenceThreshold => _localizationService.GetString("SilenceThreshold");
    public string LabelSaveSettings => _localizationService.GetString("SaveSettings");
    public string LabelBrowse => _localizationService.GetString("Browse");
    public string LabelAudioSlicingParams => _localizationService.GetString("AudioSlicingParams");
    public string LabelAspectRatio => _localizationService.GetString("AspectRatio");

    public SettingsViewModel(ISettingsService settingsService, ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _localizationService = localizationService;

        _localizationService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(LabelTmpFolder));
            OnPropertyChanged(nameof(LabelDestFolder));
            OnPropertyChanged(nameof(LabelMp3Folder));
            OnPropertyChanged(nameof(LabelLanguage));
            OnPropertyChanged(nameof(LabelSegmentDuration));
            OnPropertyChanged(nameof(LabelTrimSilence));
            OnPropertyChanged(nameof(LabelSilenceThreshold));
            OnPropertyChanged(nameof(LabelSaveSettings));
            OnPropertyChanged(nameof(LabelBrowse));
            OnPropertyChanged(nameof(LabelAudioSlicingParams));
            OnPropertyChanged(nameof(LabelAspectRatio));
        };

        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        AppSettings settings = _settingsService.CurrentSettings;
        TmpFolderPath = settings.TmpFolderPath;
        DestFolderPath = settings.DestFolderPath;
        Mp3DestFolderPath = settings.Mp3DestFolderPath;
        SelectedLanguage = settings.Language;
        SegmentDurationSeconds = settings.SegmentDurationSeconds;
        TrimSilence = settings.TrimSilence;
        SilenceThresholdDb = settings.SilenceThresholdDb;
        SelectedAspectRatio = settings.AspectRatio;
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _localizationService.SetLanguage(value);
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        AppSettings settings = _settingsService.CurrentSettings;
        settings.TmpFolderPath = TmpFolderPath;
        settings.DestFolderPath = DestFolderPath;
        settings.Mp3DestFolderPath = Mp3DestFolderPath;
        settings.Language = SelectedLanguage;
        settings.SegmentDurationSeconds = SegmentDurationSeconds;
        settings.TrimSilence = TrimSilence;
        settings.SilenceThresholdDb = SilenceThresholdDb;
        settings.AspectRatio = SelectedAspectRatio;

        await _settingsService.SaveSettingsAsync();
        StatusMessage = _localizationService.GetString("SettingsSaved");
    }
}
