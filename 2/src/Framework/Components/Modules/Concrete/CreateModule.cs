using System;
using LabWork2.Framework.Components.Workers.Common;
using LabWork2.Framework.Components.Modules.Common;

namespace LabWork2.Framework.Components.Modules.Concrete;

internal sealed class CreateModule : Module
{
    private readonly IMockWorker mockWorker;

    private int tasksCount;
    private float totalDelayPayloads;

    internal CreateModule(string identifier, IMockWorker mockWorker) : base(identifier)
    {
        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        this.mockWorker = mockWorker;

        this.MoveTimeline();
    }

    internal override sealed float TimeCurrent { get; set; }

    internal sealed override void AcceptTask()
    {
        throw new InvalidOperationException($"{base.Identifier} ({this.GetType()}) is not able to accept tasks.");
    }

    internal override sealed void CompleteTask()
    {
        base.NextModule?.AcceptTask();
        this.MoveTimeline();
        ++this.tasksCount;
    }

    private protected override sealed void MoveTimeline()
    {
        float delayPayload = this.mockWorker.DelayPayload;
        this.TimeNext = this.TimeCurrent + delayPayload;
        this.totalDelayPayloads += delayPayload;
    }

    public override sealed void PrintFinalStatistics()
    {
        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.WriteLine($"Tasks: {this.tasksCount}; Delay mean: {this.totalDelayPayloads / this.tasksCount}.");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Tasks: {this.tasksCount}; Time: {base.TimeNext}.");
    }
}
