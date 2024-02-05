using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.Tests;

public sealed class TilingFixture
{
    [TestCaseSource(nameof(Tilings))]
    public void EdgeIdsShouldBeDistinct(ITiling tiling)
    {
        var unexpected = tiling
            .Generate()
            .SelectMany(x => x.Edges)
            .OrderBy(x => x.Id)
            .Pairwise((a, b) => (Edge: a, Diff: b.Id - a.Id))
            .Where(x => x.Diff is > 0 and < 3);

        Assert.That(unexpected, Is.Empty);
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
            const int size = 20;

            yield return new TestCaseData(new PyramidTiling(size));
            yield return new TestCaseData(new SquareTiling(size, size));
            yield return new TestCaseData(new HexagonalTiling(size, size));
            yield return new TestCaseData(new TriangularTiling(size, size));
            yield return new TestCaseData(new TruncatedSquareTiling(size, size));
        }
    }
}
