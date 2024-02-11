namespace Fovero.Model.Tiling;

public abstract class RegularTiling(ushort columns, ushort rows) : ITiling
{
    public ushort Columns { get; } = columns;

    public ushort Rows { get; } = rows;

    public virtual Rectangle Bounds => new(0, 0, Columns, Rows);

    public IEnumerable<ITile> Generate()
    {
        return Enumerable
            .Range(0, Rows)
            .SelectMany(r => Enumerable
                .Range(0, Columns)
                .Select(c => CreateTile(c, r)));
    }

    protected abstract ITile CreateTile(int col, int row);

    protected bool Contains(Location location)
    {
        return location.Column >= 0 && location.Column < Columns && location.Row >= 0 && location.Row < Rows;
    }

    protected readonly struct Location(int column, int row)
    {
        public int Column { get; init; } = column;
        public int Row { get; init; } = row;
    }
}
