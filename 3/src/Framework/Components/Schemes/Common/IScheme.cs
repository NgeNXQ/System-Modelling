using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Modules.Common;

namespace LabWork3.Framework.Components.Schemes.Common;

internal interface IScheme
{
    public Module? Fallback { get; init; }
    public Module? GetNextModule(Task task);
}