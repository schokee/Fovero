using Fovero.Model.Tiling;

namespace Fovero.Tests;

public sealed class TilingFixture
{
    [TestCaseSource(nameof(Tilings))]
    public void TileOrdinalsShouldBeUnique(ITiling tiling)
    {
        var allTiles = tiling.Generate();

        var duplicates = allTiles
            .Select(x => x.Ordinal)
            .GroupBy(x => x)
            .Where(x => x.Skip(1).Any())
            .Select(x => x.Key);

        Assert.That(duplicates, Is.Empty);
    }

    public static IEnumerable Tilings
    {
        get
        {
            yield return new TestCaseData(new PyramidTiling(10));
            yield return new TestCaseData(new SquareTiling(10, 10));
            yield return new TestCaseData(new HexagonalTiling(10, 10));
            yield return new TestCaseData(new TriangularTiling(10, 10));
            yield return new TestCaseData(new TruncatedSquareTiling(10, 10));
        }
    }
}
