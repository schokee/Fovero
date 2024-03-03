using System.Configuration;

namespace Fovero.UI;

[SettingsSerializeAs(SettingsSerializeAs.String)]
public sealed class ActionPlayerSettings
{
    public static ActionPlayerSettings Default { get; } = new()
    {
        AnimationSpeed = 75,
        IsAnimationEnabled = true
    };

    public int AnimationSpeed { get; set; }

    public bool IsAnimationEnabled { get; set; }
}
