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

    [TestCaseSource(nameof(Tilings))]
    public void TileOrdinalsShouldBeConsecutive(ITiling tiling)
    {
        var nonConsecutive = tiling
            .Generate()
            .Select(x => x.Ordinal)
            .Pairwise((a, b) => (At: a, Diff: b - a))
            .Where(x => x.Diff != 1);

        Assert.That(nonConsecutive, Is.Empty);
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
            yield return new TestCaseData(new DijonTiling(size, size));
            yield return new TestCaseData(new PalazzoTiling(size, size));
            yield return new TestCaseData(new SquareTiling(size, size));
            yield return new TestCaseData(new HexagonalTiling(size, size));
            yield return new TestCaseData(new TriangularTiling(size, size));
            yield return new TestCaseData(new TruncatedSquareTiling(size, size));
            yield return new TestCaseData(new SlicedCircularTiling(size, size, true));
            yield return new TestCaseData(new AdaptiveCircularTiling(size, size, true));
        }
    }
}
