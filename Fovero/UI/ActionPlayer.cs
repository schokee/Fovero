using Caliburn.Micro;

namespace Fovero.UI;

public sealed class ActionPlayer : PropertyChangedBase
{
    private int _animationSpeed = 75; // 10ms delay

    public bool IsAnimated { get; set; } = true;

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

    public int MaximumSpeed { get; } = 100;

    public double AnimationDelay => AnimationSpeed == MaximumSpeed ? 0 : Math.Pow(10, CalculateExponent());

    public async Task Play<T>(IEnumerable<T> source, Action<T> doWork)
    {
        foreach (T item in source)
        {
            doWork(item);

            if (!IsAnimated)
            {
                continue;
            }

            var delay = TimeSpan.FromMilliseconds(AnimationDelay);
            await Task.Delay(delay);
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
