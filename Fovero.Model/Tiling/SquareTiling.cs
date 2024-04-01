namespace Fovero.Model.Tiling;

public sealed class SquareTiling(ushort columns, ushort rows) : GridTiling(columns, rows)
{
    protected override ITile CreateTile(ushort ordinal, Location location, IReadOnlyDictionary<Location, ITile> lookup)
    {
        return new RectangleTile(this, ordinal, location, lookup);
    }
}
