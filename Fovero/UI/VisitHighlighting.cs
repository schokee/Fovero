using Fovero.Model;

namespace Fovero.UI;

internal sealed class VisitHighlighting
{
    public static IReadOnlyDictionary<VisitHighlightingStyle, string> Options { get; } = Enum<VisitHighlightingStyle>.ToDictionary();
}
