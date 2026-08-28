using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AudioSlicerPro.Models;
using Microsoft.Extensions.Logging;

namespace AudioSlicerPro.Services;

public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsFilePath;

    public AppSettings CurrentSettings { get; private set; } = new AppSettings();

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        string appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AudioSlicerPro"
        );

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, "settings.json");

        // Set default temp and dest folders if empty
        CurrentSettings.TmpFolderPath = Path.Combine(appDataFolder, "tmp");
        CurrentSettings.DestFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "AudioSlicerOutput"
        );
        CurrentSettings.Mp3DestFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "AudioSlicerOutput",
            "MP3"
        );
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    CurrentSettings = loaded;

                    // Ensure fallback paths if empty
                    if (string.IsNullOrWhiteSpace(CurrentSettings.TmpFolderPath))
                    {
                        CurrentSettings.TmpFolderPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "AudioSlicerPro", "tmp"
                        );
                    }

                    if (string.IsNullOrWhiteSpace(CurrentSettings.DestFolderPath))
                    {
                        CurrentSettings.DestFolderPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                            "AudioSlicerOutput"
                        );
                    }

                    if (string.IsNullOrWhiteSpace(CurrentSettings.Mp3DestFolderPath))
                    {
                        CurrentSettings.Mp3DestFolderPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                            "AudioSlicerOutput", "MP3"
                        );
                    }
                }
                _logger.LogInformation("Loaded settings successfully from {Path}", _settingsFilePath);
            }
            else
            {
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings file. Using default settings.");
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            string json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
            _logger.LogInformation("Saved settings to {Path}", _settingsFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings file to {Path}", _settingsFilePath);
        }
    }
}
