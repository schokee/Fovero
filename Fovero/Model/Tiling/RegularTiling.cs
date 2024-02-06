namespace Fovero.Model.Tiling;

public abstract class RegularTiling(string title, ushort columns, ushort rows) : ITiling
{
    public string Title { get; } = title;

    public ushort Columns { get; } = columns;

    public ushort Rows { get; } = rows;

    public IEnumerable<ITile> Generate()
    {
        return Enumerable
            .Range(0, (int)Rows)
            .SelectMany(r => Enumerable
                .Range(0, (int)Columns)
                .Select(c => CreateTile(c, r)));
    }

    protected abstract ITile CreateTile(int col, int row);

    protected bool Contains(Location location)
    {
        return location.Column >= 0 && location.Column < Columns && location.Row >= 0 && location.Row < Rows;
    }

    public override string ToString()
    {
        return $"{Title} ({Columns} x {Rows})";
    }

    protected readonly struct Location(int column, int row)
    {
        public int Column { get; init; } = column;
        public int Row { get; init; } = row;
    }
}
