using Caliburn.Micro;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed class ActionPlayer : PropertyChangedBase
{
    private readonly ActionPlayerSettings _settings;

    private System.Action SaveSettings { get; }

    public ActionPlayer(string settingsKey)
    {
        var userSettings = Properties.Settings.Default;

        _settings = userSettings[settingsKey] as ActionPlayerSettings ?? ActionPlayerSettings.Default;

        SaveSettings = () =>
        {
            userSettings[settingsKey] = _settings;
            userSettings.Save();
        };
    }

    public bool IsAnimated
    {
        get => _settings.IsAnimationEnabled;
        set
        {
            _settings.IsAnimationEnabled = value;
            SaveSettings();

            NotifyOfPropertyChange();
        }
    }

    public int AnimationSpeed
    {
        get => _settings.AnimationSpeed;
        set
        {
            _settings.AnimationSpeed = value;
            SaveSettings();

            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(AnimationDelay));
        }
    }

    [UsedImplicitly]
    public int MaximumSpeed { get; } = 100;

    public double AnimationDelay => AnimationSpeed == MaximumSpeed ? 0 : Math.Pow(10, CalculateExponent());

    public async Task Play<T>(IEnumerable<T> source, Action<T> doWork)
    {
        foreach (T item in source)
        {
            doWork(item);

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
