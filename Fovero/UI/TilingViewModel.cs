using System.Reactive.Disposables;
using Caliburn.Micro;
using CommunityToolkit.Mvvm.Input;
using Fovero.Model.Generators;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using Fovero.UI.Editors;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed partial class TilingViewModel : Screen, ICanvas
{
    private BuildingStrategy<Wall> _selectedBuilder;
    private SolvingStrategy _selectedSolver;
    private ITilingEditor _selectedTiling;
    private Maze _maze;
    private ITrailMap _trailMap;
    private bool _isBusy;
    private bool _hasGenerated;
    private int _seed;
    private int _zoom = 22;

    public TilingViewModel()
    {
        _seed = Random.Shared.Next();

        DisplayName = "Tiling";

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
                NotifyOfPropertyChange(nameof(CanGenerate));
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

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

    #region ICanvas

    public double Scaling => Zoom / Model.Tiling.Scaling.Unit;

    public double StrokeThickness => Math.Max(0.001, 3 / Scaling);

    #endregion

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
        set
        {
            if (Set(ref _selectedSolver, value))
            {
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

    public Maze Maze
    {
        get => _maze;
        private set
        {
            if (Set(ref _maze, value))
            {
                TrailMap = Maze?.CreateTrailMap();
                HasGenerated = false;
                NotifyOfPropertyChange(nameof(CanGenerate));
            }
        }
    }

    public ITrailMap TrailMap
    {
        get => _trailMap;
        private set
        {
            void OnSolvedStateChanged(object _, bool solved)
            {
                NotifyOfPropertyChange(nameof(CanSolve));
            }

            if (_trailMap is not null)
            {
                _trailMap.SolvedStateChanged -= OnSolvedStateChanged;
            }

            Set(ref _trailMap, value);
            NotifyOfPropertyChange(nameof(CanSolve));

            if (_trailMap is not null)
            {
                _trailMap.SolvedStateChanged += OnSolvedStateChanged;
            }
        }
    }

    [UsedImplicitly]
    public void Reset()
    {
        Clear();

        Maze?.ResetWalls(clearLocks: true);
        HasGenerated = false;
    }

    [UsedImplicitly]
    public void Clear()
    {
        IsBusy = false;
        TrailMap?.Reset();
    }

    public bool CanGenerate => IsIdle && Maze is not null;

    [UsedImplicitly]
    public async Task Generate()
    {
        if (!CanGenerate)
        {
            return;
        }

        Clear();

        Maze.ResetWalls();
        HasGenerated = false;

        using (BeginWork())
        {
            var random = new Random(Seed);

            await BuildingSequence.Play(SelectedBuilder
                .SelectWallsToBeOpened(Maze.SharedWalls.ToList(), random)
                .TakeWhile(_ => IsBusy), x => x.IsOpen = true);

            if (!IsSeedLocked)
            {
                _seed = random.Next();
                NotifyOfPropertyChange(nameof(Seed));
            }

            HasGenerated = true;
        }
    }

    public bool CanSolve => IsIdle && HasGenerated && TrailMap?.IsSolved == false && SelectedSolver is not null;

    [UsedImplicitly]
    public async Task Solve()
    {
        if (!CanSolve)
        {
            return;
        }

        TrailMap.Reset();

        using (BeginWork())
        {
            await SolutionSequence.Play(TrailMap
                .FindSolution(SelectedSolver)
                .TakeWhile(_ => IsBusy), TrailMap.Update);
        }
    }

    private IDisposable BeginWork()
    {
        IsBusy = true;
        return Disposable.Create(() => IsBusy = false);
    }

    private bool CanSetStart(IMazeCell cell)
    {
        return TrailMap?.IsValidStart(cell) == true;
    }

    [RelayCommand(CanExecute = nameof(CanSetStart))]
    private void SetStart(IMazeCell cell)
    {
        TrailMap.StartCell = cell;
    }

    private bool CanSetEnd(IMazeCell cell)
    {
        return TrailMap?.IsValidEnd(cell) == true;
    }

    [RelayCommand(CanExecute = nameof(CanSetEnd))]
    private void SetEnd(IMazeCell cell)
    {
        TrailMap.EndCell = cell;
    }

    [RelayCommand]
    private void ReverseEndPoints()
    {
        TrailMap?.ReverseEndPoints();
    }

    private void OnFormatChanged()
    {
        Reset();
        Maze = SelectedTiling is null ? null : new Maze(SelectedTiling.CreateTiling());
    }
}
