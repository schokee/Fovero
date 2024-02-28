using Caliburn.Micro;
using Fovero.Model.Geometry;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using JetBrains.Annotations;

namespace Fovero.UI;

public sealed partial class Maze
{
    private sealed class Cell(ITile tile, Func<Cell, IEnumerable<Cell>> selectAdjacent) : PropertyChangedBase, IMazeCell
    {
        private uint _visitCount;

        public ushort Ordinal => tile.Ordinal;

        public Point2D Location => tile.Center;

        public Rectangle Bounds => tile.Bounds;

        public IEnumerable<ICell> AccessibleAdjacentCells => selectAdjacent(this);

        [UsedImplicitly]
        public string PathData
        {
            get
            {
                var path = string.Join(" ", tile.Edges.Select((edge, n) => n == 0 ? edge.PathData : edge.DrawData));
                return path;
            }
        }

        public uint VisitCount
        {
            get => _visitCount;
            set
            {
                if (Set(ref _visitCount, value))
                {
                    NotifyOfPropertyChange(nameof(HasBeenVisited));
                }
            }
        }

        public bool HasBeenVisited => VisitCount > 0;

        public override bool Equals(object obj)
        {
            return obj is IMazeCell cell && Ordinal == cell.Ordinal;
        }

        public override int GetHashCode()
        {
            return Ordinal.GetHashCode();
        }

        public override string ToString()
        {
            return Ordinal.ToString();
        }
    }
}
