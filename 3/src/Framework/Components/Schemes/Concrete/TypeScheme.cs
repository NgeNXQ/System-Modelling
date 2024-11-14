using System;
using System.Collections.Generic;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;

namespace LabWork3.Framework.Components.Schemes.Concrete;

internal sealed class TypeScheme : IScheme
{
    private readonly IDictionary<int, Module> neighbours;

    internal TypeScheme(Module fallback)
    {
        if (fallback == null)
            throw new ArgumentNullException($"{nameof(fallback)} cannot be null.");

        this.Fallback = fallback;
        this.neighbours = new Dictionary<int, Module>();
    }

    public Module? Fallback { get; init; }

    public Module? GetNextModule(Task task)
    {
        if (!this.neighbours.ContainsKey(task.CurrentType))
            throw new InvalidOperationException($"Specified {task.CurrentType} is not inside the collection.");

        return this.neighbours[task.CurrentType];
    }

    internal void Attach(Module module, int targetType)
    {
        if (module == null)
            throw new ArgumentNullException($"{nameof(module)} cannot be null.");

        if (this.neighbours.ContainsKey(targetType))
            throw new ArgumentException($"{nameof(module)} is already among the neighbours.");

        this.neighbours.Add(targetType, module);
    }
}
