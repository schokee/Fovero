using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors;

namespace Fovero.UI.Behaviors;

internal sealed class CaptureBehavior : Behavior<Button>
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(UIElement), typeof(CaptureBehavior), new PropertyMetadata(default(UIElement)));

    public UIElement Source
    {
        get { return (UIElement)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(CaptureBehavior), new PropertyMetadata(default(ICommand)));

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Click += OnClick;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (Source is null || Command is null)
        {
            return;
        }

        Command.Execute(RenderVisual(Source));
    }

    private static BitmapSource RenderVisual(Visual visual, double dpi = 96)
    {
        var bounds = VisualTreeHelper.GetDescendantBounds(visual);

        var width = (int)Math.Ceiling(bounds.Width);
        var height = (int)Math.Ceiling(bounds.Height);

        var drawingVisual = new DrawingVisual();

        using (var dc = drawingVisual.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(-bounds.X, -bounds.Y));
            dc.DrawRectangle(
                new VisualBrush(visual),
                null,
                new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height));
        }

        var bitmap = new RenderTargetBitmap(
            width,
            height,
            dpi,
            dpi,
            PixelFormats.Pbgra32);

        bitmap.Render(drawingVisual);

        return bitmap;
    }
}
