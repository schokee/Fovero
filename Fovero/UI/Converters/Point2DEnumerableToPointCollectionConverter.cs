using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Fovero.Model.Geometry;

namespace Fovero.UI.Converters;

[ValueConversion(typeof(IEnumerable<Point2D>), typeof(PointCollection))]
internal class Point2DEnumerableToPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is IEnumerable<Point2D> points
            ? new PointCollection(points.Select(p => new Point(p.X, p.Y)))
            : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
