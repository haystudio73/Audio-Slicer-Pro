using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.Resources;

namespace AudioSlicerPro.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly ISettingsService _settingsService;
    private ResourceLoader? _resourceLoader;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentLanguage
    {
        get
        {
            string lang = _settingsService.CurrentSettings.Language;
            return string.IsNullOrWhiteSpace(lang) ? "vi-VN" : lang;
        }
    }

    public LocalizationService(ILogger<LocalizationService> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _resourceLoader = null; // Unpackaged mode uses FallbackDictionary
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return;

        _settingsService.CurrentSettings.Language = languageCode;
        _ = _settingsService.SaveSettingsAsync();

        _logger.LogInformation("Language changed to {Language}", languageCode);
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(string.Empty); // Notify all bindings to refresh
    }

    public string GetString(string resourceKey)
    {
        try
        {
            if (_resourceLoader != null)
            {
                string value = _resourceLoader.GetString(resourceKey);
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }
        catch
        {
            // Ignore in unpackaged mode
        }

        // Fallback string lookup
        if (FallbackDictionary.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(resourceKey, out var fallback))
        {
            return fallback;
        }

        // If not found in current language, try vi-VN fallback
        if (FallbackDictionary["vi-VN"].TryGetValue(resourceKey, out var defaultFallback))
        {
            return defaultFallback;
        }

        return resourceKey;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static readonly Dictionary<string, Dictionary<string, string>> FallbackDictionary = new()
    {
        ["vi-VN"] = new()
        {
            ["AppTitle"] = "AudioSlicer Pro",
            ["NavMain"] = "Cắt File Audio",
            ["NavSettings"] = "Cài Đặt",
            ["DropFileHere"] = "Kéo & thả file audio vào đây",
            ["BrowseFile"] = "Chọn File Audio",
            ["Processing"] = "Đang xử lý...",
            ["StartProcessing"] = "Bắt Đầu Cắt File",
            ["Cancel"] = "Hủy Bỏ",
            ["TmpFolder"] = "Thư Mục Tạm",
            ["DestFolder"] = "Thư Mục Đầu Ra MP4",
            ["Mp3Folder"] = "Thư Mục Đầu Ra MP3",
            ["Language"] = "Ngôn Ngữ",
            ["SegmentDuration"] = "Độ Dài Phân Đoạn (giây)",
            ["TrimSilence"] = "Tự Động Cắt Khoảng Lặng (Đầu & Cuối)",
            ["SilenceThreshold"] = "Ngưỡng Cắt Khoảng Lặng (dB)",
            ["SaveSettings"] = "Lưu Cài Đặt",
            ["StatusReady"] = "Sẵn Sàng",
            ["StatusCompleted"] = "Đã Cắt File Thành Công!",
            ["StatusCancelled"] = "Đã Hủy Bỏ Xử Lý",
            ["StatusError"] = "Lỗi Khi Xử Lý File Audio",
            ["DurationLabel"] = "Thời lượng:",
            ["SizeLabel"] = "Kích thước:",
            ["ProcessingLog"] = "Nhật Ký Xử Lý",
            ["OutputFiles"] = "File Kết Quả (Click để nghe thử)",
            ["ClickToPreview"] = "Click vào file để nghe thử ngay trong app",
            ["OpenFolder"] = "Mở Thư Mục",
            ["AudioSlicingParams"] = "Thông Số Cắt Audio",
            ["AspectRatio"] = "Tỉ Lệ Khung Hình MP4 (Google Flow)",
            ["Browse"] = "Duyệt...",
            ["SettingsSaved"] = "Đã lưu cài đặt thành công!",
            ["ThemeToggle"] = "Chuyển Giao Diện Sáng/Tối"
        },
        ["en-US"] = new()
        {
            ["AppTitle"] = "AudioSlicer Pro",
            ["NavMain"] = "Slice Audio",
            ["NavSettings"] = "Settings",
            ["DropFileHere"] = "Drag & Drop Audio File Here",
            ["BrowseFile"] = "Browse Audio File",
            ["Processing"] = "Processing...",
            ["StartProcessing"] = "Start Slicing",
            ["Cancel"] = "Cancel",
            ["TmpFolder"] = "Temporary Directory",
            ["DestFolder"] = "MP4 Output Directory",
            ["Mp3Folder"] = "MP3 Output Directory",
            ["Language"] = "Language",
            ["SegmentDuration"] = "Segment Duration (seconds)",
            ["TrimSilence"] = "Trim Silence (Start & End)",
            ["SilenceThreshold"] = "Silence Threshold (dB)",
            ["SaveSettings"] = "Save Settings",
            ["StatusReady"] = "Ready",
            ["StatusCompleted"] = "Slicing Completed Successfully!",
            ["StatusCancelled"] = "Operation Cancelled",
            ["StatusError"] = "Error processing audio file",
            ["DurationLabel"] = "Duration:",
            ["SizeLabel"] = "Size:",
            ["ProcessingLog"] = "Processing Log",
            ["OutputFiles"] = "Output Files (Click to preview)",
            ["ClickToPreview"] = "Click on any output file to play preview",
            ["OpenFolder"] = "Open Folder",
            ["AudioSlicingParams"] = "Audio Slicing Parameters",
            ["AspectRatio"] = "MP4 Aspect Ratio (Google Flow)",
            ["Browse"] = "Browse...",
            ["SettingsSaved"] = "Settings saved successfully!",
            ["ThemeToggle"] = "Toggle Light/Dark Theme"
        }
    };
}
