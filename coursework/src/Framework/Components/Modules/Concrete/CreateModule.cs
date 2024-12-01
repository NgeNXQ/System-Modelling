using System;
using CourseWork.Framework.Core.Services;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Workers.Common;
using CourseWork.Framework.Components.Modules.Common;
using CourseWork.Framework.Components.Blueprints.Schemes.Common;
using CourseWork.Framework.Components.Tasks.Utilities.Factories.Common;

namespace CourseWork.Framework.Components.Modules.Concrete;

internal sealed class CreateModule : Module
{
    private readonly IScheme scheme;
    private readonly IWorker worker;
    private readonly IDummyTaskFactory dummyTaskFactory;

    internal CreateModule(string identifier, IScheme scheme, IWorker worker, IDummyTaskFactory dummyTaskFactory) : base(identifier)
    {
        this.scheme = scheme ?? throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");
        this.worker = worker ?? throw new ArgumentNullException($"{nameof(worker)} cannot be null.");
        this.dummyTaskFactory = dummyTaskFactory ?? throw new ArgumentNullException($"{nameof(dummyTaskFactory)} cannot be null.");

        this.MoveTimeline(this.worker.Delay);
    }

    internal override sealed float TimeCurrent { get; set; }

    internal int CreatedTasksCount { get; private set; }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        base.TimeNext = this.TimeCurrent + deltaTime;
    }

    internal override sealed void AcceptTask(DummyTask task)
    {
        throw new InvalidOperationException($"{base.GetType()} {base.Identifier} is not able to accept tasks.");
    }

    internal override sealed void CompleteTask()
    {
        ++this.CreatedTasksCount;

        DummyTask newTask = this.dummyTaskFactory.CreateDummyTask(this.TimeCurrent);
        Module nextModule = this.scheme.GetNextModule(newTask)!;
        this.MoveTimeline(this.worker.Delay);
        nextModule.AcceptTask(newTask);

        SystemLoggerService.Instance.AppendLogEntry("LOG", "TRACE", $"[{base.Identifier}] sends task #{newTask.Id} to the [{nextModule?.Identifier}]");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        // Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        // Console.WriteLine($"Tasks: {this.totalCreatedTasksCount}; Time: {base.TimeNext}.");
    }

    public override sealed void PrintFinalStatistics()
    {
        // Console.Write($"|REPORT| [{base.Identifier}] ");
        // Console.WriteLine($"Tasks total: {this.totalCreatedTasksCount};");
    }
}
