using System;
using LabWork3.Framework.Common;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Workers.Common;

namespace LabWork3.Framework.Components.Modules.Common;

internal abstract class Module : IStatisticsPrinter
{
    private protected Module(string identifier)
    {
        if (String.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException($"Invalid value of the {nameof(identifier)}.");

        this.Identifier = identifier;
        this.TimeNext = Single.MaxValue;
    }

    internal string Identifier { get; private init; }
    internal float TimeNext { get; private protected set; }

    internal abstract float TimeCurrent { get; set; }

    internal abstract void CompleteTask();
    private protected abstract void MoveTimeline(float deltaTime);
    internal abstract void AcceptTask(Task task, IMockWorker? mockWorker);

    public abstract void PrintFinalStatistics();
    public abstract void PrintIntermediateStatistics();
}
