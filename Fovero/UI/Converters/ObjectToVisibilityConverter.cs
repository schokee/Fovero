using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;

namespace Fovero.UI.Converters;

[ValueConversion(typeof(object), typeof(Visibility))]
public class ObjectToVisibilityConverter : MarkupExtension, IValueConverter, IMultiValueConverter
{
    public bool HiddenOnly { get; set; }

    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var show = value switch
        {
            Visibility v => v == Visibility.Visible,
            bool b => b,
            int i => i != 0,
            string s => !string.IsNullOrEmpty(s),
            _ => value != null
        };

        return ToVisibility(show);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var show = values?.All(x => x is true) == true;
        return ToVisibility(show);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Visibility ToVisibility(bool show)
    {
        return show ^ Invert ? Visibility.Visible : HiddenOnly ? Visibility.Hidden : Visibility.Collapsed;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
