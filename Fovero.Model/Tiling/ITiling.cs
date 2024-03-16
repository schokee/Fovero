using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling;

public interface ITiling
{
    Rectangle Bounds { get; }

    IEnumerable<ITile> Generate();
}
