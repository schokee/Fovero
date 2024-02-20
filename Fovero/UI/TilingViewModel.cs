﻿using System.Reactive.Disposables;
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
        Solution.Clear();
        VisitedCells.Clear();

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
        VisitedCells.Clear();

        using (BeginWork())
        {
            var tileLookup = Tiles.ToDictionary(x => x.Ordinal);

            var pathways = Walls
                .Where(x => x.IsOpen)
                .SelectMany(wall => wall.SelectPathways(n => tileLookup[n]))
                .ToLookup(x => x.From, x => x.To);

            var origin = new Cell(Tiles[start], pathways);
            var goal = new Cell(Tiles[end], pathways);

            await SolutionSequence.Play(SelectedSolver
                .FindPath(origin, goal)
                .Prepend(Path.Empty)
                .Pairwise((prev, next) => prev.SwitchTo(next))
                .SelectMany(move => move)
                .TakeWhile(_ => IsBusy), step =>
                {
                    switch (step)
                    {
                        case Retreat:
                            Solution.RemoveAt(Solution.Count - 1);
                            break;
                        case Advance advance:
                            Solution.Add(advance.NextCell);
                            VisitedCells.Add(advance.NextCell);
                            break;
                    }
                });
        }
    }

    private IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared).Distinct();

    private IDisposable BeginWork()
    {
        IsBusy = true;
        return Disposable.Create(() => IsBusy = false);
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
