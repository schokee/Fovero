using System.ComponentModel;

namespace Fovero.UI;

public enum VisitHighlightingStyle
{
    [Description("None")]
    Hidden,
    [Description("Visited")]
    AnyVisit,
    [Description("Heat Map")]
    VisitFrequency
}
