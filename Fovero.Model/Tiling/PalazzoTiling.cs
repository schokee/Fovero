namespace Fovero.Model.Tiling;

public sealed class PalazzoTiling(ushort columns, ushort rows, float spacing = 0.5f)
    : MultiShapeTiling(columns, rows, spacing)
{
    protected override ITile CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
    {
        return new RectangleTile(this, ordinal, location, lookup);
    }
}
