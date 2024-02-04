using System.Reactive.Disposables;
using Caliburn.Micro;
using Fovero.Model.Generators;
using Fovero.Model.Tiling;
using JetBrains.Annotations;

namespace Fovero.UI;

using MoreLinq;

public sealed class TilingViewModel : Screen
{
    private BuildingStrategy<Wall> _selectedBuilder;
    private ITiling _selectedTiling;
    private bool _isBusy;

    public TilingViewModel()
    {
        DisplayName = "Tiling";

        AvailableTilings =
        [
            new SquareTiling(16, 16),
            new TruncatedSquareTiling(17, 17),
            new HexagonalTiling(8, 8),
            new PyramidTiling(16),
            new TriangularTiling(8, 8)
        ];

        SelectedTiling = AvailableTilings[0];

        Builders = new []
        {
            BuildingStrategy<Wall>.Kruskal
        };

        SelectedBuilder = Builders[0];
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
            }
        }
    }

    public IObservableCollection<ITile> Tiles { get; } = new BindableCollection<ITile>();

    public IObservableCollection<Wall> Walls { get; } = new BindableCollection<Wall>();

    public IReadOnlyList<ITiling> AvailableTilings { get; }

    public ITiling SelectedTiling
    {
        get => _selectedTiling;
        set
        {
            if (Set(ref _selectedTiling, value))
            {
                IsBusy = false;

                Tiles.Clear();
                Tiles.AddRange(SelectedTiling.Generate());

                Walls.Clear();
                Walls.AddRange(Tiles
                    .SelectMany(x => x.Edges)
                    .DistinctBy(x => x.Ordinal)
                    .Select(x => new Wall(x, 22))
                    .OrderByDescending(x => x.IsShared));
            }
        }
    }

    public IReadOnlyList<BuildingStrategy<Wall>> Builders { get; }

    public BuildingStrategy<Wall> SelectedBuilder
    {
        get => _selectedBuilder;
        set => Set(ref _selectedBuilder, value);
    }

    [UsedImplicitly]
    public void Reset()
    {
        IsBusy = false;

        foreach (Wall wall in SharedWalls.ToList())
        {
            wall.IsOpen = false;
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
            foreach (var wall in SelectedBuilder.SelectWallsToBeOpened(SharedWalls.ToList(), new Random()).TakeWhile(_ => IsBusy))
            {
                wall.IsOpen = true;
                await Task.Delay(TimeSpan.FromMilliseconds(40));
            }
        }
    }

    private IEnumerable<Wall> SharedWalls => Walls.Where(x => x.IsShared);

    private IDisposable BeginWork()
    {
        IsBusy = true;
        return Disposable.Create(() => IsBusy = false);
    }
}
