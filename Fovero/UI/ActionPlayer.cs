using Caliburn.Micro;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed class ActionPlayer : PropertyChangedBase
{
    public bool IsAnimated
    {
        get;
        set => Set(ref field, value);
    } = true;

    public int AnimationSpeed
    {
        get;
        set
        {
            if (Set(ref field, value))
            {
                NotifyOfPropertyChange(nameof(AnimationDelay));
            }
        }
    } = 75;

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
