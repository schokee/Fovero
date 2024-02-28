using Fovero.Model.Geometry;

namespace Fovero.Model.Tiling
{
    internal static class Projection
    {
        public const float Unit = 100;

        //public static Point2D InModelUnits(this Point2D p) => new(p.X * Unit, p.Y * Unit);

        public static Size2D InModelUnits(this Size2D s) => new(s.Width * Unit, s.Height * Unit);

        public static Rectangle InModelUnits(this Rectangle r) => r.ScaledBy(Unit);
    }
}
