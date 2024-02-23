using System.Reactive.Disposables;
using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Geometry;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using Fovero.Properties;
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
    private bool _showVisitedCells;
    private bool _showHotPaths;
    private bool _isSeedLocked;
    private int _zoom = 22;
    private int _seed;
    private bool _showCells;

    private static Settings Settings => Settings.Default;

    public TilingViewModel()
    {
        _seed = Random.Shared.Next();
        _isSeedLocked = Settings.LockSeed;
        _showVisitedCells = Settings.ShowVisitedCells;
        _showHotPaths = Settings.ShowHotPaths;
        _showCells = Settings.ShowCells;

        DisplayName = "Tiling";
        Solution.CollectionChanged += delegate { NotifyOfPropertyChange(nameof(CanClearSolution)); };

        Builders = BuildingStrategy<Wall>.All;
        _selectedBuilder = Builders.FirstOrDefault(x => x.Name == Settings.Generator) ?? Builders[0];

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

    public bool ShowVisitedCells
    {
        get => _showVisitedCells;
        set
        {
            if (Set(ref _showVisitedCells, value))
            {
                NotifyOfPropertyChange(nameof(ShowHotPaths));

                Settings.ShowVisitedCells = value;
                Settings.Save();
            }
        }
    }

    public bool ShowHotPaths
    {
        get => _showHotPaths;
        set
        {
            if (Set(ref _showHotPaths, value))
            {
                Settings.ShowHotPaths = value;
                Settings.Save();
            }
        }
    }

    public bool ShowCells
    {
        get => _showCells;
        set
        {
            if (Set(ref _showCells, value))
            {
                Settings.ShowCells = value;
                Settings.Save();
            }
        }
    }

    public bool CanSolve => IsIdle && HasGenerated;

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
                OnFormatChanged();
            }
        }
    }

    public bool IsSeedLocked
    {
        get => _isSeedLocked;
        set
        {
            if (Set(ref _isSeedLocked, value))
            {
                Settings.LockSeed = value;
                Settings.Save();
            }
        }
    }

    public ActionPlayer BuildingSequence { get; } = new(nameof(BuildingSequence));

    public ActionPlayer SolutionSequence { get; } = new(nameof(SolutionSequence));

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

    public IObservableCollection<Wall> Walls { get; } = new BindableCollection<Wall>();

    public IObservableCollection<ICell> VisitedCells { get; } = new BindableCollection<ICell>();

    public IObservableCollection<ICell> HotPaths { get; } = new BindableCollection<ICell>();

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
        set
        {
            if (Set(ref _selectedBuilder, value))
            {
                Settings.Generator = SelectedBuilder.Name;
                Settings.Save();
            }
        }
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
        HotPaths.Clear();
        Solution.Clear();
    }

    [UsedImplicitly]
    public Task Solve()
    {
        return Solve(Tiles.First().Ordinal, Tiles.Last().Ordinal);
    }

    [UsedImplicitly]
    public Task ReverseSolve()
    {
        return Solve(Tiles.Last().Ordinal, Tiles.First().Ordinal);
    }

    [UsedImplicitly]
    private async Task Solve(ushort start, ushort end)
    {
        ClearSolution();

        using (BeginWork())
        {
            var tileLookup = Tiles.ToDictionary(x => x.Ordinal);

            var pathways = Walls
                .Where(x => x.IsOpen)
                .SelectMany(wall => wall.SelectPathways(n => tileLookup[n]))
                .ToLookup(x => x.From, x => x.To);

            var origin = new Cell(Tiles[start], pathways);
            var goal = new Cell(Tiles[end], pathways);
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
                            (trackVisit(append.Item) ? VisitedCells : HotPaths).Add(append.Item);
                            break;
                    }
                });
        }
    }

    private IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared).Distinct();

    private Predicate<ICell> TrackingRule
    {
        get
        {
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
    }

    private sealed class Cell(ITile tile, ILookup<ITile, ITile> adjacentTiles) : ICell
    {
        private readonly ITile _tile = tile;

        public Point2D Location => _tile.Center;

        public IEnumerable<ICell> AccessibleAdjacentCells => adjacentTiles[_tile].Select(tile => new Cell(tile, adjacentTiles));

        [UsedImplicitly]
        public string PathData
        {
            get
            {
                var path = string.Join(" ", _tile.Edges.Select((edge, n) => n == 0 ? edge.PathData : edge.DrawData));
                return path;
            }
        }

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
