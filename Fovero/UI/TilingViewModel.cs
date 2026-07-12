using System.Reactive.Disposables;
using Caliburn.Micro;
using CommunityToolkit.Mvvm.Input;
using Fovero.Model;
using Fovero.Model.Generators;
using Fovero.Model.Presentation;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using Fovero.UI.Editors;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed partial class TilingViewModel : Screen
{
    private int _seed;

    public TilingViewModel()
    {
        _seed = Random.Shared.Next();

        DisplayName = "Tiling";

        Builders = BuildingStrategy<SharedBorder>.All;
        SelectedBuilder = Builders[0];

        Solvers = SolvingStrategy.All;
        SelectedSolver = Solvers[0];

        AvailableFormats =
        [
            new RegularFormatEditor("Square", (c, r) => new SquareTiling(c, r)) { Columns = 32, Rows = 16 },
            new DijonFormatEditor { Columns = 16, Rows = 10 },
            new PalazzoFormatEditor { Columns = 16, Rows = 10 },
            new LatticeFormatEditor { Columns = 16, Rows = 12 },
            new RegularFormatEditor("Truncated Square Tile", (c, r) => new TruncatedSquareTiling(c, r)) { Columns = 17, Rows = 17 },
            new RegularFormatEditor("Hexagonal", (c, r) => new HexagonalTiling(c, r)) { Columns = 23, Rows = 23 },
            new PyramidFormatEditor { Rows = 20 },
            new RegularFormatEditor("Triangular", (c, r) => new TriangularTiling(c, r)) { Columns = 43, Rows = 20 },
            new CircularFormatEditor { Rings = 14 }
        ];

        SelectedFormat = AvailableFormats[0];
    }

    public bool IsIdle => !IsBusy;

    public bool IsBusy
    {
        get;
        private set
        {
            if (Set(ref field, value))
            {
                NotifyOfPropertyChange(nameof(IsIdle));
                NotifyOfPropertyChange(nameof(CanGenerate));
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

    public bool HasGenerated
    {
        get;
        private set
        {
            if (Set(ref field, value))
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
        get;
        set
        {
            if (Set(ref field, value))
            {
                NotifyOfPropertyChange(nameof(Scaling));
                NotifyOfPropertyChange(nameof(StrokeThickness));
            }
        }
    } = 22;

    #region ICanvas

    public double Scaling => Zoom / Model.Tiling.Scaling.Unit;

    public double StrokeThickness => Math.Max(0.001, 3 / Scaling);

    #endregion

    public IReadOnlyList<IFormatEditor> AvailableFormats { get; }

    public IFormatEditor SelectedFormat
    {
        get;
        set
        {
            if (Set(ref field, value))
            {
                if (field is not null)
                {
                    field.FormatChanged -= OnFormatChanged;
                }

                field = value;

                if (field is not null)
                {
                    field.FormatChanged += OnFormatChanged;
                }

                OnFormatChanged();
            }
        }
    }

    public IReadOnlyList<BuildingStrategy<SharedBorder>> Builders { get; }

    public BuildingStrategy<SharedBorder> SelectedBuilder
    {
        get;
        set => Set(ref field, value);
    }

    public IReadOnlyList<SolvingStrategy> Solvers { get; }

    public SolvingStrategy SelectedSolver
    {
        get;
        set
        {
            if (Set(ref field, value))
            {
                Clear();
                NotifyOfPropertyChange(nameof(CanSolve));
            }
        }
    }

    public Maze Maze
    {
        get;
        private set
        {
            if (Set(ref field, value))
            {
                TrailMap = Maze?.CreateTrailMap();
                HasGenerated = false;
                NotifyOfPropertyChange(nameof(CanGenerate));
            }
        }
    }

    public ITrailMap TrailMap
    {
        get;
        private set => Set(ref field, value);
    }

    [UsedImplicitly]
    public void Reset()
    {
        Clear();

        Maze?.ResetBorders();
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

        Reset();

        using (BeginWork())
        {
            var random = new Random(Seed);

            await BuildingSequence.Play(SelectedBuilder
                .SelectBordersToBeOpened(Maze.SharedBorders.ToList(), random)
                .ToScript(border => border.IsOpen = true)
                .TakeWhile(_ => IsBusy));

            if (!IsSeedLocked)
            {
                _seed = random.Next();
                NotifyOfPropertyChange(nameof(Seed));
            }

            HasGenerated = true;
        }
    }

    public bool CanSolve => IsIdle && HasGenerated && TrailMap is not null && SelectedSolver is not null;

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
                .EnumerateSolutionSteps(SelectedSolver)
                .TakeWhile(_ => IsBusy));
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
        Maze = SelectedFormat?.CreateLayout();
    }
}
