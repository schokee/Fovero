using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace Fovero.UI;

public sealed class ReviewViewModel(IReadOnlyList<BitmapSource> images) : Screen
{
    public IReadOnlyList<BitmapSource> Images { get; } = images;
}
