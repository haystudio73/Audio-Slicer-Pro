using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AudioSlicerPro.Converters;

/// <summary>
/// Converts any non-null object to Visible, null to Collapsed.
/// Also converts bool: true -> Visible, false -> Collapsed.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts bool: true -> Collapsed, false -> Visible.
/// Inverse of NullToVisibilityConverter for bool inputs.
/// </summary>
public class BoolNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            // If target type is Visibility, return Visibility value
            if (targetType == typeof(Visibility))
                return b ? Visibility.Collapsed : Visibility.Visible;
            // Otherwise return negated bool (for IsEnabled bindings)
            return !b;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        if (value is Visibility v) return v != Visibility.Visible;
        return false;
    }
}

/// <summary>
/// Converts OutputFileItem.IsPlaying to Button Background Brush (Green when playing, default when idle)
/// </summary>
public class PlayingToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPlaying && isPlaying)
        {
            // Active playing state: Vibrant Emerald Green
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
        }
        // Default subtle accent button brush
        return Application.Current.Resources["AccentButtonBackground"] as Microsoft.UI.Xaml.Media.Brush 
               ?? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts OutputFileItem.IsPlaying to Button Text Foreground Brush
/// </summary>
public class PlayingToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts OutputFileItem.IsPlaying to FontIcon Glyph: "" (Volume/Sound) when playing, "" (Play) when idle
/// </summary>
public class PlayingToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPlaying && isPlaying)
        {
            return "\uE767"; // Volume / Playing icon
        }
        return "\uE768"; // Play icon
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts OutputFileItem.IsPlaying to Button Label Text: "Đang phát..." when playing, "Nghe thử" when idle
/// </summary>
public class PlayingToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPlaying && isPlaying)
        {
            return "Đang phát...";
        }
        return "Nghe thử";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
