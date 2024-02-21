using Fovero.Model.Tiling;

namespace Fovero.UI;

public interface ITilingEditor
{
    event Action FormatChanged;

    ITiling CreateTiling();
}
