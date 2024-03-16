using Caliburn.Micro;

namespace Fovero.UI;

public sealed class MainViewModel : Screen
{
    public MainViewModel(TilingViewModel tiling)
    {
        DisplayName = "Fovero v" + GitVersionInformation.MajorMinorPatch;
        Workspace = tiling;
    }

    public TilingViewModel Workspace { get; }
}
