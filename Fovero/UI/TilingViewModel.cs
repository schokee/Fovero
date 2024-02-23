using System.Reactive.Disposables;
using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Geometry;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using Fovero.UI.Editors;
using JetBrains.Annotations;
using MoreLinq;

namespace Fovero.UI;

public sealed class TilingViewModel : Screen, ICanvas
{
    private BuildingStrategy<Wall> _selectedBuilder;
    private SolvingStrategy _selectedSolver;
    private ITilingEditor _selectedTiling;
    private Rectangle _bounds;
    private bool _isBusy;
    private bool _hasGenerated;
    private int _zoom = 22;
    private int _seed;

    public TilingViewModel()
    {
        _seed = Random.Shared.Next();

        DisplayName = "Tiling";
        Solution.CollectionChanged += delegate { NotifyOfPropertyChange(nameof(CanClearSolution)); };
        SearchEnds.CollectionChanged += delegate { NotifyOfPropertyChange( nameof(CanSolve)); };

        Builders = BuildingStrategy<Wall>.All;
        SelectedBuilder = Builders[0];

        Solvers = SolvingStrategy.All;
        SelectedSolver = Solvers[0];

        AvailableTilings =
        [
            new RegularTilingEditor("Square", (c, r) => new SquareTiling(c, r)) { Columns = 32, Rows = 16 },
            new RegularTilingEditor("Truncated Square Tile", (c, r) => new TruncatedSquareTiling(c, r)) { Columns = 17, Rows = 17 },
            new RegularTilingEditor("Hexagonal", (c, r) => new HexagonalTiling(c, r)) { Columns = 23, Rows = 23 },
            new PyramidTilingEditor(),
            new RegularTilingEditor("Triangular", (c, r) => new TriangularTiling(c, r)) { Columns = 17, Rows = 17 },
            new CircularTilingEditor()
        ];

        SelectedTiling = AvailableTilings[0];
    }

    public bool IsIdle => !IsBusy;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
            {
                NotifyOfPropertyChange(nameof(IsIdle));
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

    public bool ShowHotPaths { get; set; }

    public bool CanSolve => IsIdle && HasGenerated && SearchEnds.Count == 2;

    public bool HasGenerated
    {
        get => _hasGenerated;
        private set
        {
            if (Set(ref _hasGenerated, value))
            {
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

    public int Seed
    {
        get => _seed;
        set
        {
            if (Set(ref _seed, value))
            {
                Reset();
            }
        }
    }

    public bool IsSeedLocked { get; set; }

    public ActionPlayer BuildingSequence { get; } = new();

    public ActionPlayer SolutionSequence { get; } = new();

    public int Zoom
    {
        get => _zoom;
        set
        {
            if (Set(ref _zoom, value))
            {
                NotifyOfPropertyChange(nameof(Scaling));
                NotifyOfPropertyChange(nameof(StrokeThickness));
            }
        }
    }

    public Rectangle Bounds
    {
        get => _bounds;
        set => Set(ref _bounds, value);
    }

    #region ICanvas

    public double Scaling => Zoom;

    public double StrokeThickness => Math.Max(0.001, 3 / Scaling);

    #endregion

    public IObservableCollection<ITile> Tiles { get; } = new BindableCollection<ITile>();

    public IObservableCollection<Marker> SearchEnds { get; } = new BindableCollection<Marker>();

    public IObservableCollection<Wall> Walls { get; } = new BindableCollection<Wall>();

    public IObservableCollection<ICell> VisitedCells { get; } = new BindableCollection<ICell>();

    public IObservableCollection<ICell> Solution { get; } = new BindableCollection<ICell>();

    public IReadOnlyList<ITilingEditor> AvailableTilings { get; }

    public ITilingEditor SelectedTiling
    {
        get => _selectedTiling;
        set
        {
            if (Set(ref _selectedTiling, value))
            {
                if (_selectedTiling is not null)
                {
                    _selectedTiling.FormatChanged -= OnFormatChanged;
                }

                _selectedTiling = value;

                if (_selectedTiling is not null)
                {
                    _selectedTiling.FormatChanged += OnFormatChanged;
                }

                OnFormatChanged();
            }
        }
    }

    public IReadOnlyList<BuildingStrategy<Wall>> Builders { get; }

    public BuildingStrategy<Wall> SelectedBuilder
    {
        get => _selectedBuilder;
        set => Set(ref _selectedBuilder, value);
    }

    public IReadOnlyList<SolvingStrategy> Solvers { get; }

    public SolvingStrategy SelectedSolver
    {
        get => _selectedSolver;
        set => Set(ref _selectedSolver, value);
    }

    [UsedImplicitly]
    public void Reset()
    {
        IsBusy = false;
        HasGenerated = false;

        ClearSolution();

        Tiles.Clear();
        Walls.Clear();

        if (SelectedTiling is not null)
        {
            var tiling = SelectedTiling.CreateTiling();

            Bounds = tiling.Bounds;

            Tiles.AddRange(tiling.Generate());

            Walls.AddRange(Tiles
                .SelectMany(x => x.Edges)
                .Distinct()
                .Select(x => new Wall(this, x)));
        }
    }

    [UsedImplicitly]
    public async Task Clear()
    {
        if (IsBusy)
        {
            return;
        }

        using (BeginWork())
        {
            var toDo = SharedWalls
                .Where(x => !x.IsOpen)
                .Shuffle()
                .ToList();

            foreach (Wall wall in toDo.TakeWhile(_ => IsBusy))
            {
                wall.IsOpen = true;
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    }

    [UsedImplicitly]
    public async Task Generate()
    {
        Reset();

        using (BeginWork())
        {
            var random = new Random(Seed);

            await BuildingSequence.Play(SelectedBuilder
                .SelectWallsToBeOpened(SharedWalls.ToList(), random)
                .TakeWhile(_ => IsBusy), x => x.IsOpen = true);

            if (!IsSeedLocked)
            {
                _seed = random.Next();
                NotifyOfPropertyChange(nameof(Seed));
            }

            HasGenerated = true;
        }
    }

    public bool CanClearSolution => Solution.Count > 0;

    public void ClearSolution()
    {
        VisitedCells.Clear();
        Solution.Clear();
    }

    [UsedImplicitly]
    public Task Solve()
    {
        return Solve(SearchEnds[0], SearchEnds[1]);
    }

    [UsedImplicitly]
    public Task ReverseSolve()
    {
        return Solve(SearchEnds[1], SearchEnds[0]);
    }

    [UsedImplicitly]
    private async Task Solve(Marker from, Marker to)
    {
        ClearSolution();

        using (BeginWork())
        {
            var tileLookup = Tiles.ToDictionary(x => x.Ordinal);

            var pathways = Walls
                .Where(x => x.IsOpen)
                .SelectMany(wall => wall.SelectPathways(n => tileLookup[n]))
                .ToLookup(x => x.From, x => x.To);

            var origin = new Cell(Tiles[from.Tile], pathways);
            var goal = new Cell(Tiles[to.Tile], pathways);
            var trackVisit = TrackingRule;

            await SolutionSequence.Play(SelectedSolver
                .FindPath(origin, goal)
                .Prepend(Array.Empty<ICell>())
                .Pairwise((prev, next) => prev.SwitchTo(next))
                .SelectMany(move => move)
                .TakeWhile(_ => IsBusy), change =>
                {
                    switch (change)
                    {
                        case RemoveLast:
                            Solution.RemoveAt(Solution.Count - 1);
                            break;
                        case Append<ICell> append:
                            Solution.Add(append.Item);
                            if (trackVisit(append.Item))
                            {
                                VisitedCells.Add(append.Item);
                            }
                            break;
                    }
                });
        }
    }

    [UsedImplicitly]
    public void TileClicked(ITile tile)
    {
        if (IsIdle)
        {
            var currentStart = SearchEnds.FirstOrDefault();

            if (currentStart is not null)
            {
                if (currentStart.Tile == tile.Ordinal)
                {
                    return;
                }

                SearchEnds[0] = EndMarker(Tiles[currentStart.Tile]);
                SearchEnds.RemoveRange(SearchEnds.Skip(1));
            }

            SearchEnds.Insert(0, StartMarker(tile));
            ClearSolution();
        }
    }

    private IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared).Distinct();

    private Predicate<ICell> TrackingRule
    {
        get
        {
            if (ShowHotPaths)
            {
                return _ => true;
            }

            var visitedCells = new HashSet<ICell>();
            return visitedCells.Add;
        }
    }

    private IDisposable BeginWork()
    {
        IsBusy = true;
        return Disposable.Create(() => IsBusy = false);
    }

    private void OnFormatChanged()
    {
        Reset();

        SearchEnds.Clear();

        if (Tiles.Count > 1)
        {
            SearchEnds.Add(StartMarker(Tiles.First()));
            SearchEnds.Add(EndMarker(Tiles.Last()));
        }
    }

    public static Marker StartMarker(ITile tile)
    {
        var bounds = MarkerBounds(tile);
        var radius = bounds.Width / 2;

        return new Marker(tile.Ordinal, $"M {bounds.Left} {tile.Center.Y} a {radius} {radius} 0 0 0 {bounds.Width} 0 a {radius} {radius} 0 0 0 {-bounds.Width} 0 z");
    }

    public static Marker EndMarker(ITile tile)
    {
        var bounds = MarkerBounds(tile);
        var radius = bounds.Width / 2;

        return new Marker(tile.Ordinal, $"M {bounds.Center.X} {bounds.Top} l {radius} {radius} l {-radius} {radius} l {-radius} {-radius} z");
    }

    private static Rectangle MarkerBounds(ITile tile)
    {
        return new Rectangle(0, 0, 0.45f, 0.45f).CenteredAt(tile.Center);
    }

    private sealed class Cell(ITile tile, ILookup<ITile, ITile> adjacentTiles) : ICell
    {
        private readonly ITile _tile = tile;

        public Point2D Location => _tile.Center;

        public IEnumerable<ICell> AccessibleAdjacentCells => adjacentTiles[_tile].Select(tile => new Cell(tile, adjacentTiles));

        [UsedImplicitly]
        public string PathData => _tile.PathData;

        public override bool Equals(object obj)
        {
            return obj is Cell cell && _tile.Ordinal == cell._tile.Ordinal;
        }

        public override int GetHashCode()
        {
            return _tile.Ordinal.GetHashCode();
        }

        public override string ToString()
        {
            return _tile.Ordinal.ToString();
        }
    }
}
