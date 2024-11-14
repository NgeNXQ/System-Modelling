using System;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Workers.Common;
using LabWork3.Framework.Components.Modules.Common;
using LabWork3.Framework.Components.Tasks.Utilities.Factories.Common;

namespace LabWork3.Framework.Components.Modules.Concrete;

internal sealed class CreateModule : Module
{
    private readonly IScheme scheme;
    private readonly IMockWorker mockWorker;
    private readonly TaskFactory taskFactory;

    private int createdTasksCount;
    private float totalDelayPayloads;

    internal CreateModule(string identifier, IScheme scheme, IMockWorker mockWorker, TaskFactory taskFactory, float initialTime = 0.0f) : base(identifier)
    {
        if (scheme == null)
            throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");

        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        if (taskFactory == null)
            throw new ArgumentNullException($"{nameof(taskFactory)} cannot be null.");

        this.scheme = scheme;
        this.mockWorker = mockWorker;
        this.taskFactory = taskFactory;
        this.MoveTimeline((initialTime != 0.0f ? initialTime : mockWorker.DelayPayload));
    }

    internal int CreatedTasksCount => this.createdTasksCount;

    internal override sealed float TimeCurrent { get; set; }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        this.totalDelayPayloads += deltaTime;
        base.TimeNext = this.TimeCurrent + deltaTime;
    }

    internal override sealed void AcceptTask(Task task)
    {
        throw new InvalidOperationException($"{base.Identifier} ({this.GetType()}) is not able to accept tasks.");
    }

    internal override sealed void CompleteTask()
    {
        ++this.createdTasksCount;

        Task newTask = this.taskFactory.CreateTask(this.TimeCurrent);
        Module? nextModule = this.scheme.GetNextModule(newTask);
        nextModule?.AcceptTask(newTask);
        this.MoveTimeline(this.mockWorker.DelayPayload);

        Console.WriteLine($"|LOG| (TRACE) [{base.Identifier}] sends task to the [{nextModule?.Identifier}]");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Tasks: {this.createdTasksCount}; Time: {base.TimeNext}.");
    }

    public override sealed void PrintFinalStatistics()
    {
        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.WriteLine($"Tasks total: {this.createdTasksCount}; Delay mean: {this.totalDelayPayloads / this.createdTasksCount}");
    }
}
