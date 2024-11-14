using System;
using System.Collections.Generic;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;
using LabWork3.Framework.Components.Modules.Concrete;

namespace LabWork3.Framework.Components.Schemes.Concrete;

internal sealed class QueuePriorityScheme : IScheme
{
    private readonly ICollection<ProcessorModule> neighbours;
    private readonly Func<ProcessorModule?>? customLogicHandler;

    internal QueuePriorityScheme(Module fallback, Func<ProcessorModule?> customLogicHandler) : this(fallback)
    {
        if (customLogicHandler == null)
            throw new ArgumentNullException($"{nameof(customLogicHandler)} cannot be null.");

        this.customLogicHandler = customLogicHandler;
    }

    internal QueuePriorityScheme(Module fallback)
    {
        this.Fallback = fallback;
        this.neighbours = new LinkedList<ProcessorModule>();
    }

    public Module? Fallback { get; init; }

    public Module? GetNextModule(Task task)
    {
        int minQueueLength = Int32.MaxValue;
        ProcessorModule? nextProcessor = null;

        foreach (ProcessorModule processorModule in this.neighbours)
        {
            if (processorModule.Queue.Count < minQueueLength)
            {
                nextProcessor = processorModule;
                minQueueLength = processorModule.Queue.Count;
            }
        }

        return this.customLogicHandler?.Invoke() ?? nextProcessor ?? this.Fallback;
    }

    internal void Attach(ProcessorModule processor)
    {
        if (processor == null)
            throw new ArgumentNullException($"{nameof(processor)} cannot be null.");

        this.neighbours.Add(processor);
    }
}