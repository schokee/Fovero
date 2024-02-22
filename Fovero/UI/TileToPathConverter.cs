using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fovero.Model.Tiling;

namespace Fovero.UI;

// Workaround for binding to default interface implementation
[ValueConversion(typeof(ITile), typeof(string))]
public sealed class TileToPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ITile tile ? tile.PathData : DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
