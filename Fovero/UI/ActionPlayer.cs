﻿using Caliburn.Micro;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed class ActionPlayer : PropertyChangedBase
{
    private bool _isAnimated = true;
    private int _animationSpeed = 75; // 10ms delay

    public bool IsAnimated
    {
        get => _isAnimated;
        set => Set(ref _isAnimated, value);
    }

    public int AnimationSpeed
    {
        get => _animationSpeed;
        set
        {
            if (Set(ref _animationSpeed, value))
            {
                NotifyOfPropertyChange(nameof(AnimationDelay));
            }
        }
    }

    [UsedImplicitly]
    public int MaximumSpeed { get; } = 100;

    public double AnimationDelay => AnimationSpeed == MaximumSpeed ? 0 : Math.Pow(10, CalculateExponent());

    public async Task Play(IEnumerable<System.Action> script)
    {
        foreach (var action in script)
        {
            action.Invoke();

            if (IsAnimated && AnimationDelay > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(AnimationDelay));
            }
        }
    }

    private double CalculateExponent()
    {
        var x = (MaximumSpeed - AnimationSpeed) / 50f;

        return x switch
        {
            <= 1 => 2 * x,
            _ => x + 1
        };
    }
}
