﻿using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Fovero.Model;
using Microsoft.Xaml.Behaviors;

namespace Fovero.UI.Behaviors;

internal sealed class TrailDrawingBehavior : Behavior<Canvas>
{
    public static readonly DependencyProperty TrailMapProperty = DependencyProperty.Register(
        nameof(TrailMap), typeof(ITrailMap), typeof(TrailDrawingBehavior), new PropertyMetadata(default(ITrailMap)));

    public ITrailMap TrailMap
    {
        get { return (ITrailMap)GetValue(TrailMapProperty); }
        set { SetValue(TrailMapProperty, value); }
    }

    private readonly SerialDisposable _shutdown = new();

    protected override void OnAttached()
    {
        var mouseDown = Observable
            .FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => AssociatedObject.MouseLeftButtonDown += h, h => AssociatedObject.MouseLeftButtonDown -= h);

        var mouseUp = Observable
            .FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => AssociatedObject.MouseLeftButtonUp += h, h => AssociatedObject.MouseLeftButtonUp -= h);

        var endCapture = Observable
            .FromEventPattern<MouseEventHandler, MouseEventArgs>(h => AssociatedObject.LostMouseCapture += h, h => AssociatedObject.LostMouseCapture -= h);

        var escape = Observable
            .FromEventPattern<KeyEventHandler, KeyEventArgs>(h => AssociatedObject.KeyDown += h, h => AssociatedObject.KeyDown -= h)
            .Where(x => x.EventArgs.Key == Key.Escape);

        var cancel = Observable.Merge(
            mouseUp.Select(x => (EventArgs)x.EventArgs),
            endCapture.Select(x => x.EventArgs),
            escape.Select(x => x.EventArgs));

        var toJoin = mouseDown
            .Select(x => HitTest(x.EventArgs))
            .FirstAsync(cell => cell is not null)
            .SelectMany(cellInitiallyHit => Observable
                .If(AssociatedObject.CaptureMouse, Observable.Using(() => Disposable.Create(AssociatedObject.ReleaseMouseCapture), _ =>
                {
                    var solution = TrailMap.Solution;

                    var currentEnd = Observable
                        .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                            h => solution.CollectionChanged += h,
                            h => solution.CollectionChanged -= h)
                        .Select(_ => 0)
                        .StartWith(0)
                        .Select(_ => solution.LastOrDefault())
                        .DistinctUntilChanged();

                    var at = Observable
                        .FromEventPattern<MouseEventHandler, MouseEventArgs>(
                            h => AssociatedObject.MouseMove += h,
                            h => AssociatedObject.MouseMove -= h)
                        .Select(x => HitTest(x.EventArgs))
                        .Where(cell => cell is not null)
                        .StartWith(cellInitiallyHit)
                        .DistinctUntilChanged();

                    return at.CombineLatest(currentEnd);
                })))
            .TakeUntil(cancel)
            .Repeat();

        _shutdown.Disposable = toJoin
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(x => UpdateWith(x.First, x.Second));

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        _shutdown.Disposable = null;
        base.OnDetaching();
    }

    private IMazeCell HitTest(MouseEventArgs args)
    {
        var result = VisualTreeHelper.HitTest(AssociatedObject, args.GetPosition(AssociatedObject))?.VisualHit is FrameworkElement e
            ? e.DataContext as IMazeCell
            : null;

        return result;
    }

    private void UpdateWith(IMazeCell at, IMazeCell currentEnd)
    {
        var canAdd = currentEnd is null ? TrailMap.StartCell.Equals(at) : currentEnd.AccessibleAdjacentCells.Contains(at);

        if (canAdd)
        {
            TrailMap.Solution.Add(at);
        }
        else
        {
            TrailMap.Solution.TrimAllAfter(at);
        }
    }
}
