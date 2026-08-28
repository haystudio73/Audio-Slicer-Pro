using System.ComponentModel;

namespace AudioSlicerPro.Services;

public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLanguage { get; }
    void SetLanguage(string languageCode);
    string GetString(string resourceKey);
}
