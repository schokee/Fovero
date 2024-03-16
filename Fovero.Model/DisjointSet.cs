using System.Collections;

namespace Fovero.Model;

// See also: https://red-green-rewrite.github.io/2016/09/30/Curious-case-of-disjoint-set/
public sealed class DisjointSet<T> : IEnumerable<T> where T : IEquatable<T>
{
    private readonly Dictionary<T, NodeInfo> _nodes = new();

    public DisjointSet()
    {
    }

    public DisjointSet(IEnumerable<T> nodes)
    {
        foreach (T node in nodes)
        {
            Add(node);
        }
    }

    public bool Add(T item)
    {
        if (_nodes.ContainsKey(item))
        {
            return false;
        }

        _nodes[item] = new NodeInfo();
        return true;
    }

    public bool AreDisjoint(T itemA, T itemB)
    {
        return !ReferenceEquals(Find(_nodes[itemA]), Find(_nodes[itemB]));
    }

    public bool Merge(T itemA, T itemB)
    {
        var representativeA = _nodes.TryGetValue(itemA, out NodeInfo? nodeA)
            ? nodeA.Representative
            : throw new KeyNotFoundException($"Key {itemA} not found");

        var representativeB = _nodes.TryGetValue(itemB, out NodeInfo? nodeB)
            ? nodeB.Representative
            : throw new KeyNotFoundException($"Key {itemB} not found");

        if (ReferenceEquals(representativeA, representativeB))
        {
            return false;
        }

        if (representativeA.Height < representativeB.Height)
        {
            (representativeA, representativeB) = (representativeB, representativeA);
        }

        representativeB.Representative = representativeA;

        if (representativeA.Height == representativeB.Height)
        {
            representativeA.Height++;
        }

        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _nodes.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static NodeInfo Find(NodeInfo nodeInfo)
    {
        if (nodeInfo.Representative != nodeInfo)
        {
            nodeInfo.Representative = Find(nodeInfo.Representative);
        }

        return nodeInfo.Representative;
    }

    private record NodeInfo
    {
        public NodeInfo()
        {
            Representative = this;
            Height = 0;
        }

        public NodeInfo Representative { get; set; }

        public int Height { get; set; }
    }
}
