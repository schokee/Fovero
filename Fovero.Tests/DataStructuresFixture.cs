using Fovero.Model.DataStructures;
using MoreLinq;

namespace Fovero.Tests;

public class DataStructuresFixture
{
    [Test]
    public void InvertedTreeInsertTest()
    {
        var s = new InvertedTree<int>();

        s.Insert([0, 1, 2, 3, 4, 5]);

        Assert.That(s.GetAncestors(5), Is.EqualTo(new[] { 4, 3, 2, 1, 0 }));
        Assert.That(s.GetPathTo(5), Is.EqualTo(new[] {0, 1, 2, 3, 4, 5}));
    }

    [Test]
    public void InvertedTreeInsertAfterTest()
    {
        var s = new InvertedTree<int>();

        s.Insert([0, 1, 2, 3, 4, 5]);
        s.InsertAfter(6, 3);
        s.InsertAfter(7, 6);

        Assert.That(s.GetAncestors(7), Is.EqualTo(new[] { 6, 3, 2, 1, 0 }));
        Assert.That(s.GetPathTo(7), Is.EqualTo(new[] { 0, 1, 2, 3, 6, 7 }));
    }

    [Test]
    public void InvertedTreeInsertManyAfterTest()
    {
        var s = new InvertedTree<int>();

        s.Insert([0, 1, 2, 3, 4, 5]);
        s.InsertManyAfter([6, 7], 3);

        Assert.That(s.GetAncestors(7), Is.EqualTo(new[] { 6, 3, 2, 1, 0 }));
        Assert.That(s.GetPathTo(7), Is.EqualTo(new[] { 0, 1, 2, 3, 6, 7 }));
    }
}
