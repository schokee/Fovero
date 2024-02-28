using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Fovero.UI.Converters;

[ValueConversion(typeof(int), typeof(Brush))]
internal sealed class CountToHeatBrushConverter : IValueConverter
{
    private static SolidColorBrush[] Palette { get; } = new[]
        {
            Color.FromRgb(0x7A, 0xD2, 0x51),
            Color.FromRgb(0x8F, 0xD7, 0x44),
            Color.FromRgb(0xBC, 0xDF, 0x27),
            Color.FromRgb(0xD3, 0xE2, 0x1B),
            Color.FromRgb(0xE5, 0xE9, 0x15),
            Color.FromRgb(0xFF, 0xE7, 0x24),
        }
        .Select(x => new SolidColorBrush(Color.FromArgb(0xC0, x.R, x.G, x.B)))
        .ToArray();

    public static SolidColorBrush BaseBrush => Palette[0];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is uint i
            ? Palette[Math.Min(i, Palette.Length - 1)]
            : DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
