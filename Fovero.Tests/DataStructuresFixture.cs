using Fovero.Model.DataStructures;
using MoreLinq;

namespace Fovero.Tests;

public class DataStructuresFixture
{
    [Test]
    public void PredecessorSequenceTest()
    {
        var s = new PredecessorSequence<int>();

        s.Insert([0, 1, 2, 3, 4, 5]);
        s.InsertAfter(6, 3);
        s.InsertAfter(7, 6);

        Assert.That(s.GetPredecessors(6), Is.EqualTo(new[] { 3, 2, 1, 0 }));
        Assert.That(s.GetSequenceTo(5), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void ExtendedQueueTest_QueueUseCase()
    {
        var q = new ExtendedQueue<int>();

        int[] list = [1, 2, 3, 4, 5];
        list.ForEach(i => q.Enqueue(i, 0));

        var output = new List<int>();
        while (q.Count > 0)
        {
            output.Add(q.Dequeue());
        }

        Assert.That(output, Is.EqualTo(list));
    }

    [Test]
    public void ExtendedQueueTest_PriorityQueueUseCase()
    {
        var q = new ExtendedQueue<int>();

        int[] list = [1, 2, 3, 4, 5];
        float[] priorities = [2f, 1f, 3f, 1.5f, 2.5f];

        list.Zip(priorities).ForEach(i => q.Enqueue(i.First, i.Second));

        var output = new List<int>();
        while (q.Count > 0)
        {
            output.Add(q.Dequeue());
        }

        int[] expected = [2, 4, 1, 5, 3];
        Assert.That(output, Is.EqualTo(expected));
    }

    [Test]
    public void ExtendedQueueTest_MixedUseCase()
    {
        var q = new ExtendedQueue<int>();

        int[] list = [1, 2, 3, 4, 5];
        float[] priorities = [2f, 1f, 0, 1.5f, 0];

        list.Zip(priorities).ForEach(i => q.Enqueue(i.First, i.Second));

        var output = new List<int>();
        while (q.Count > 0)
        {
            output.Add(q.Dequeue());
        }

        int[] expected = [3, 5, 2, 4, 1];
        Assert.That(output, Is.EqualTo(expected));
    }
}
