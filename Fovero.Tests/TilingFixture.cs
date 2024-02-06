using Fovero.Model;
using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.Tests;

public sealed class TilingFixture
{
    [Test]
    public void TestEdgeCreation()
    {
        var hexagonalTiling = new HexagonalTiling(3, 3);

        var distinctEdges = hexagonalTiling
            .Generate()
            .SelectMany(x => x.Edges)
            .Distinct()
            .Count();

        Assert.That(distinctEdges, Is.EqualTo(38));
    }

    [TestCaseSource(nameof(Tilings))]
    public void TileOrdinalsShouldBeUnique(ITiling tiling)
    {
        var duplicates = SelectNonUnique(tiling.Generate().Select(x => x.Ordinal));
        Assert.That(duplicates, Is.Empty);
    }

    private static IEnumerable<T> SelectNonUnique<T>(IEnumerable<T> source)
    {
        return source
            .GroupBy(x => x)
            .Where(x => x.Skip(1).Any())
            .Select(x => x.Key);
    }

    public static IEnumerable Tilings
    {
        get
        {
            const int size = 32;

            yield return new TestCaseData(new PyramidTiling(size));
            yield return new TestCaseData(new SquareTiling(size, size));
            yield return new TestCaseData(new HexagonalTiling(size, size));
            yield return new TestCaseData(new TriangularTiling(size, size));
            yield return new TestCaseData(new TruncatedSquareTiling(size, size));
        }
    }
}
