using Fovero.Model.Geometry;
using Fovero.Model.Tiling;
using JetBrains.Annotations;

namespace Fovero.Model.Presentation;

public sealed partial class Maze
{
    private Size2D Size { get; }
    private Func<TrailMap> TrailMapFactory { get; }

    public Maze(ITiling tiling) : this(tiling, edge => new Door(edge))
    {
    }

    public Maze(ITiling tiling, Func<IEdge, SharedBorder> createBorder)
    {
        var tiles = tiling
            .Generate()
            .ToDictionary(x => x.Ordinal);

        Boundaries = tiles.Values
            .SelectMany(x => x.Edges)
            .Distinct()
            .Select(edge => (Boundary)(edge.IsShared ? createBorder(edge) : new Wall(edge)))
            .ToList();

        Size = tiling.Bounds.Size;
        TrailMapFactory = () => new TrailMap(tiles.Values, SharedBorders.ToList());
    }

    [UsedImplicitly]
    public float Width => Size.Width;

    [UsedImplicitly]
    public float Height => Size.Height;

    public bool AreBoundariesVisible { get; set; } = true;

    public IReadOnlyList<Boundary> VisibleBoundaries => AreBoundariesVisible ? Boundaries : [];

    public IReadOnlyList<Boundary> Boundaries { get; }

    public IEnumerable<SharedBorder> SharedBorders => Boundaries.OfType<SharedBorder>();

    public void ResetBorders()
    {
        foreach (var border in SharedBorders)
        {
            border.IsOpen = false;
        }
    }

    public ITrailMap CreateTrailMap()
    {
        return TrailMapFactory.Invoke();
    }
}
