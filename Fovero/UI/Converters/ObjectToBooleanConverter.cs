using System.Globalization;
using System.Windows.Data;

namespace Fovero.UI.Converters;

[ValueConversion(typeof(object), typeof(bool))]
public sealed class ObjectToBooleanConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = value switch
        {
            bool b => b,
            string s => string.IsNullOrEmpty(s),
            _ => value is not null
        };

        return Invert ? !result : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
