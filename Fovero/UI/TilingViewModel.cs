using System.Reactive.Disposables;
using Caliburn.Micro;
using Fovero.Model;
using Fovero.Model.Generators;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using Fovero.UI.Editors;
using JetBrains.Annotations;

namespace Fovero.UI;

using MoreLinq;

public sealed class TilingViewModel : Screen, ICanvas
{
    private BuildingStrategy<Wall> _selectedBuilder;
    private SolvingStrategy _selectedSolver;
    private ITilingEditor _selectedTiling;
    private bool _isBusy;
    private bool _hasGenerated;
    private int _zoom = 22;
    private int _seed;
    private Rectangle _bounds;

    public TilingViewModel()
    {
        _seed = Random.Shared.Next();

        DisplayName = "Tiling";

        AvailableTilings =
        [
            new RegularTilingEditor("Square", (c, r) => new SquareTiling(c, r)) { Columns = 32, Rows = 16 },
            new RegularTilingEditor("Truncated Square Tile", (c, r) => new TruncatedSquareTiling(c, r)) { Columns = 17, Rows = 17 },
            new RegularTilingEditor("Hexagonal", (c, r) => new HexagonalTiling(c, r)) { Columns = 23, Rows = 23 },
            new PyramidTilingEditor(),
            new RegularTilingEditor("Triangular", (c, r) => new TriangularTiling(c, r)) { Columns = 17, Rows = 17 }
        ];
        SelectedTiling = AvailableTilings[0];

        Builders = [BuildingStrategy<Wall>.Kruskal];
        SelectedBuilder = Builders[0];

        Solvers =
        [
            SolvingStrategy.AStarEuclidean,
            SolvingStrategy.AStarManhattan,
            SolvingStrategy.BreadthFirstSearch
        ];
        SelectedSolver = Solvers[0];
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

    public bool IsSeedLocked { get; set; }

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

    public IObservableCollection<ITile> VisitedTiles { get; } = new BindableCollection<ITile>();

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

    private void OnFormatChanged()
    {
        Reset();
    }

    [UsedImplicitly]
    public void Reset()
    {
        IsBusy = false;
        HasGenerated = false;

        Tiles.Clear();
        Walls.Clear();
        VisitedTiles.Clear();

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

            foreach (var wall in SelectedBuilder.SelectWallsToBeOpened(SharedWalls.ToList(), random).TakeWhile(_ => IsBusy))
            {
                wall.IsOpen = true;
                await Task.Delay(TimeSpan.FromMilliseconds(40));
            }

            if (!IsSeedLocked)
            {
                _seed = random.Next();
                NotifyOfPropertyChange(nameof(Seed));
            }

            HasGenerated = true;
        }
    }

    [UsedImplicitly]
    public async Task Solve()
    {
        VisitedTiles.Clear();

        using (BeginWork())
        {
            foreach (ITile tile in SelectedSolver.Solve(Tiles[0], Tiles[^1], Walls.ToList()).TakeWhile(_ => IsBusy))
            {
                VisitedTiles.Add(tile);
                await Task.Delay(TimeSpan.FromMilliseconds(40));
            }
        }
    }

    private IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared).Distinct();

    private IDisposable BeginWork()
    {
        IsBusy = true;
        return Disposable.Create(() => IsBusy = false);
    }
}
