using System;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;

namespace LabWork3.Framework.Components.Schemes.Concrete;

internal sealed class SingleTransitionScheme : IScheme
{
    private readonly Module neighbour;

    internal SingleTransitionScheme(Module neighbour) : this(neighbour, neighbour)
    {
    }

    internal SingleTransitionScheme(Module neighbour, Module fallback)
    {
        if (neighbour == null)
            throw new ArgumentNullException($"{nameof(neighbour)} cannot be null.");

        this.Fallback = fallback;
        this.neighbour = neighbour;
    }

    public Module? Fallback { get; init; }

    public Module? GetNextModule(Task task)
    {
        return this.neighbour ?? this.Fallback;
    }
}
