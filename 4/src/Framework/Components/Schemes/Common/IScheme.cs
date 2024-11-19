using LabWork4.Framework.Components.Tasks.Common;
using LabWork4.Framework.Components.Modules.Common;

namespace LabWork4.Framework.Components.Schemes.Common;

internal interface IScheme
{
    public Module? Fallback { get; init; }
    public Module? GetNextModule(Task task);
}