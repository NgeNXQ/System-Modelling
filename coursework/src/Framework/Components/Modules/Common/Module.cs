using System;
using CourseWork.Framework.Common;
using CourseWork.Framework.Components.Tasks.Common;

namespace CourseWork.Framework.Components.Modules.Common;

internal abstract class Module : IStatisticsPrinter
{
    private static int nextId;

    static Module()
    {
        Module.nextId = 0;
    }

    private protected Module(string identifier)
    {
        if (String.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException($"Invalid value of the {nameof(identifier)}.");

        this.Id = Module.nextId;
        this.Identifier = identifier;
        this.TimeNext = Single.MaxValue;
    }

    internal int Id { get; private init; }
    internal string Identifier { get; private init; }

    internal abstract float TimeCurrent { get; set; }
    internal float TimeNext { get; private protected set; }

    internal abstract void CompleteTask();
    internal abstract void AcceptTask(DummyTask task);
    private protected abstract void MoveTimeline(float deltaTime);

    public abstract void PrintFinalStatistics();
    public abstract void PrintIntermediateStatistics();
}
