using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Pokedex.Core.Localization;

namespace Pokedex.Wpf.Converters;

public class TypeToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, Color> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Normal"] = Color.FromRgb(168, 168, 120),
        ["Feu"] = Color.FromRgb(240, 128, 48),
        ["Eau"] = Color.FromRgb(104, 144, 240),
        ["Électrik"] = Color.FromRgb(248, 208, 48),
        ["Plante"] = Color.FromRgb(120, 200, 80),
        ["Glace"] = Color.FromRgb(152, 216, 216),
        ["Combat"] = Color.FromRgb(192, 48, 40),
        ["Poison"] = Color.FromRgb(160, 64, 160),
        ["Sol"] = Color.FromRgb(224, 192, 104),
        ["Vol"] = Color.FromRgb(168, 144, 240),
        ["Psy"] = Color.FromRgb(248, 88, 136),
        ["Insecte"] = Color.FromRgb(168, 184, 32),
        ["Roche"] = Color.FromRgb(184, 160, 56),
        ["Spectre"] = Color.FromRgb(112, 88, 152),
        ["Dragon"] = Color.FromRgb(112, 56, 248),
        ["Ténèbres"] = Color.FromRgb(112, 88, 72),
        ["Acier"] = Color.FromRgb(184, 184, 208),
        ["Fée"] = Color.FromRgb(238, 153, 172),
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = value?.ToString() ?? "";
        if (!Colors.TryGetValue(type, out var color))
            color = Color.FromRgb(108, 117, 125);
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class StatToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = value is int i ? i : 0;
        var color = v switch
        {
            >= 130 => Color.FromRgb(40, 167, 69),
            >= 90 => Color.FromRgb(139, 195, 74),
            >= 60 => Color.FromRgb(255, 193, 7),
            _ => Color.FromRgb(255, 112, 67)
        };
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visible = value != null;
        if (parameter?.ToString() == "invert") visible = !visible;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visible = value is true;
        if (parameter?.ToString() == "invert") visible = !visible;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b ? !b : false;
}

public class HeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int h ? $"{h / 10.0:F1} m" : "";
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class WeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int w ? $"{w / 10.0:F1} kg" : "";
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class PageLabelConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int page && values[1] is int total)
            return $"Page {page + 1} / {total}";
        return "";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var count = value switch
        {
            int i => i,
            System.Collections.ICollection c => c.Count,
            _ => 0
        };
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
