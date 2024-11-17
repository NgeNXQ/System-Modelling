using System;
using System.Linq;
using System.Collections.Generic;
using LabWork4.Framework.Components.Tasks.Common;
using LabWork4.Framework.Components.Schemes.Common;
using LabWork4.Framework.Components.Modules.Common;
using LabWork4.Framework.Components.Modules.Concrete;

namespace LabWork4.Framework.Components.Schemes.Concrete;

internal sealed class PayloadDistributionScheme : IScheme
{
    private readonly IList<ProcessorModule> processors;

    internal PayloadDistributionScheme(IList<ProcessorModule> processors, Module? fallback)
    {
        if (processors == null)
            throw new ArgumentNullException($"{nameof(processors)} cannot be null.");

        if (fallback == null)
            throw new ArgumentNullException($"{nameof(fallback)} cannot be null.");

        this.Fallback = fallback;
        this.processors = processors;
    }

    public Module? Fallback { get; init; }

    public Module? GetNextModule(Task task)
    {
        return this.processors.Where(processor => !processor.IsBusy).FirstOrDefault() ?? this.Fallback;
    }
}
