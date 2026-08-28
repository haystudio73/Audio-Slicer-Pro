using System.Threading.Tasks;
using AudioSlicerPro.Models;

namespace AudioSlicerPro.Services;

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}
