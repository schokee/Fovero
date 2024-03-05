using Fovero.Model.Geometry;
using Fovero.Model.Tiling;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed partial class Maze
{
    private Size2D Size { get; }
    private Func<TrailMap> TrailMapFactory { get; }

    public Maze(ITiling tiling)
    {
        var tiles = tiling
            .Generate()
            .ToDictionary(x => x.Ordinal);

        Walls = tiles.Values
            .SelectMany(x => x.Edges)
            .Distinct()
            .Select(edge => new Wall(edge))
            .ToList();

        Size = tiling.Bounds.Size;
        TrailMapFactory = () => new TrailMap(tiles.Values, Walls);
    }

    [UsedImplicitly]
    public float Width => Size.Width;

    [UsedImplicitly]
    public float Height => Size.Height;

    public IReadOnlyList<Wall> Walls { get; }

    public IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared);

    public void ResetWalls()
    {
        foreach (var wall in SharedWalls)
        {
            wall.IsOpen = false;
        }
    }

    public ITrailMap CreateTrailMap()
    {
        return TrailMapFactory.Invoke();
    }
}
