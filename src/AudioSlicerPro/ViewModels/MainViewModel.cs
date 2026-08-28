using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioSlicerPro.Models;
using AudioSlicerPro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace AudioSlicerPro.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAudioService _audioService;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<MainViewModel> _logger;
    private DispatcherQueue? _dispatcherQueue;

    private CancellationTokenSource? _cts;

    [ObservableProperty]
    public partial AudioFileInfo? SelectedAudioFile { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial double OverallProgress { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial OutputFileItem? SelectedOutputFile { get; set; }

    [ObservableProperty]
    public partial string? PreviewAudioPath { get; set; }

    public ObservableCollection<string> LogEntries { get; } = new();
    public ObservableCollection<OutputFileItem> OutputFiles { get; } = new();

    public string AppTitle => _localizationService.GetString("AppTitle");
    public string LabelDropFile => _localizationService.GetString("DropFileHere");
    public string LabelBrowseFile => _localizationService.GetString("BrowseFile");
    public string LabelStartProcessing => _localizationService.GetString("StartProcessing");
    public string LabelCancel => _localizationService.GetString("Cancel");
    public string LabelDuration => _localizationService.GetString("DurationLabel");
    public string LabelSize => _localizationService.GetString("SizeLabel");
    public string LabelProcessingLog => _localizationService.GetString("ProcessingLog");
    public string LabelOutputFiles => _localizationService.GetString("OutputFiles");
    public string LabelClickToPreview => _localizationService.GetString("ClickToPreview");
    public string LabelOpenFolder => _localizationService.GetString("OpenFolder");

    public MainViewModel(
        IAudioService audioService,
        ISettingsService settingsService,
        ILocalizationService localizationService,
        ILogger<MainViewModel> logger)
    {
        _audioService = audioService;
        _settingsService = settingsService;
        _localizationService = localizationService;
        _logger = logger;

        _localizationService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(AppTitle));
            OnPropertyChanged(nameof(LabelDropFile));
            OnPropertyChanged(nameof(LabelBrowseFile));
            OnPropertyChanged(nameof(LabelStartProcessing));
            OnPropertyChanged(nameof(LabelCancel));
            OnPropertyChanged(nameof(LabelDuration));
            OnPropertyChanged(nameof(LabelSize));
            OnPropertyChanged(nameof(LabelProcessingLog));
            OnPropertyChanged(nameof(LabelOutputFiles));
            OnPropertyChanged(nameof(LabelClickToPreview));
            OnPropertyChanged(nameof(LabelOpenFolder));
        };

        StatusMessage = _localizationService.GetString("StatusReady");
    }

    public async Task SetSelectedFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

        try
        {
            SelectedAudioFile = await _audioService.GetAudioFileInfoAsync(filePath);
            AddLog($"Selected audio file: {SelectedAudioFile.FileName} ({SelectedAudioFile.FormattedDuration}, {SelectedAudioFile.FormattedSize})");
            StatusMessage = $"Selected {SelectedAudioFile.FileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inspect audio file {Path}", filePath);
            AddLog($"Error loading file: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task StartProcessingAsync()
    {
        if (SelectedAudioFile == null || string.IsNullOrWhiteSpace(SelectedAudioFile.FilePath))
        {
            AddLog("Warning: No audio file selected.");
            return;
        }

        IsProcessing = true;
        OverallProgress = 0;
        LogEntries.Clear();
        OutputFiles.Clear();
        PreviewAudioPath = null;
        SelectedOutputFile = null;
        _cts = new CancellationTokenSource();

        AppSettings settings = _settingsService.CurrentSettings;
        string tmpDir = Path.Combine(settings.TmpFolderPath, Guid.NewGuid().ToString("N"));
        string mp4DestFolder = settings.DestFolderPath;
        string mp3DestFolder = settings.Mp3DestFolderPath;
        string audioSubFolder = Path.GetFileNameWithoutExtension(SelectedAudioFile.FileName);

        AddLog($"Starting audio processing: {SelectedAudioFile.FileName}");
        AddLog($"Segment Duration: {settings.SegmentDurationSeconds}s | Aspect Ratio: {settings.AspectRatio} | Trim Silence: {settings.TrimSilence}");
        AddLog($"Temporary Dir: {tmpDir}");
        AddLog($"MP4 Destination Dir: {Path.Combine(mp4DestFolder, audioSubFolder)}");
        AddLog($"MP3 Destination Dir: {Path.Combine(mp3DestFolder, audioSubFolder)}");

        try
        {
            StatusMessage = _localizationService.GetString("Processing");

            // Step 1: Slice audio
            AddLog("Step 1/2: Slicing audio file (with trim silence check)...");
            var progressHandler = new Progress<double>(val =>
            {
                _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    if (SelectedAudioFile.Duration.TotalSeconds > 0)
                    {
                        OverallProgress = Math.Min(50.0, (val / SelectedAudioFile.Duration.TotalSeconds) * 50.0);
                    }
                });
            });

            var segments = await _audioService.SliceAudioAsync(
                SelectedAudioFile.FilePath,
                tmpDir,
                settings.SegmentDurationSeconds,
                settings.TrimSilence,
                settings.SilenceThresholdDb,
                progressHandler,
                _cts.Token
            );

            AddLog($"Slicing complete: Created {segments.Count} segment(s).");
            OverallProgress = 50.0;

            // Step 2: Save MP3 segments and convert each segment to MP4
            AddLog("Step 2/2: Exporting MP3 & MP4 output files...");
            double stepWeight = 50.0 / Math.Max(1, segments.Count);

            for (int i = 0; i < segments.Count; i++)
            {
                _cts.Token.ThrowIfCancellationRequested();
                string segmentFile = segments[i];

                AddLog($"Exporting segment [{i + 1}/{segments.Count}]: {Path.GetFileName(segmentFile)}");

                // Save MP3
                string mp3Path = await _audioService.SaveMp3SegmentAsync(
                    segmentFile,
                    mp3DestFolder,
                    audioSubFolder,
                    _cts.Token
                );

                // Convert to MP4
                string mp4Path = await _audioService.ConvertToMp4Async(
                    segmentFile,
                    mp4DestFolder,
                    audioSubFolder,
                    settings.AspectRatio,
                    null,
                    _cts.Token
                );

                OverallProgress = 50.0 + ((i + 1) * stepWeight);
                AddLog($"  -> MP3: {mp3Path}");
                AddLog($"  -> MP4: {mp4Path}");

                // Add MP4 to output file preview list
                if (File.Exists(mp4Path))
                {
                    var fi = new FileInfo(mp4Path);
                    var outputItem = new OutputFileItem
                    {
                        FilePath = mp4Path,
                        FileName = fi.Name,
                        FormattedSize = $"{fi.Length / (1024.0 * 1024.0):F2} MB"
                    };

                    _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
                    _dispatcherQueue?.TryEnqueue(() => OutputFiles.Add(outputItem));
                }

                // Add MP3 to output file preview list
                if (File.Exists(mp3Path))
                {
                    var fi = new FileInfo(mp3Path);
                    var outputItem = new OutputFileItem
                    {
                        FilePath = mp3Path,
                        FileName = fi.Name,
                        FormattedSize = $"{fi.Length / (1024.0 * 1024.0):F2} MB"
                    };

                    _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
                    _dispatcherQueue?.TryEnqueue(() => OutputFiles.Add(outputItem));
                }
            }

            OverallProgress = 100.0;
            StatusMessage = _localizationService.GetString("StatusCompleted");
            AddLog("SUCCESS: All segments processed and saved to destination folder!");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = _localizationService.GetString("StatusCancelled");
            AddLog("CANCELLED: Processing was cancelled by user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio");
            StatusMessage = _localizationService.GetString("StatusError");
            AddLog($"ERROR: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
            _ = _audioService.CleanTmpDirectoryAsync(tmpDir);
        }
    }

    [RelayCommand]
    private void PlayOutputFile(OutputFileItem file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.FilePath) || !File.Exists(file.FilePath)) return;

        foreach (var item in OutputFiles)
        {
            item.IsPlaying = (item == file);
        }

        SelectedOutputFile = file;
        PreviewAudioPath = file.FilePath;
        AddLog($"Previewing file: {file.FileName}");
    }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        try
        {
            AppSettings settings = _settingsService.CurrentSettings;
            string destFolder = settings.DestFolderPath;

            if (SelectedAudioFile != null)
            {
                string audioSubFolder = Path.GetFileNameWithoutExtension(SelectedAudioFile.FileName);
                string targetDir = Path.Combine(destFolder, audioSubFolder);
                if (Directory.Exists(targetDir))
                {
                    destFolder = targetDir;
                }
            }

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = destFolder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open output directory");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            AddLog("Cancel requested. Cleaning up...");
        }
    }

    private void AddLog(string message)
    {
        string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
        if (_dispatcherQueue != null)
            _dispatcherQueue.TryEnqueue(() => LogEntries.Add(entry));
        else
            LogEntries.Add(entry);
    }
}
