namespace Fovero.Model.Tiling;

public abstract class RegularTiling(string title, int columns, int rows) : ITiling
{
    public string Title { get; } = title;

    public int Columns { get; } = columns;

    public int Rows { get; } = rows;

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
