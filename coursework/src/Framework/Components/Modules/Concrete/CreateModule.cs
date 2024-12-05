using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Workers.Common;
using Coursework.Framework.Components.Modules.Common;
using Coursework.Framework.Components.Blueprints.Schemes.Common;
using Coursework.Framework.Components.Tasks.Utilities.Factories.Common;

namespace Coursework.Framework.Components.Modules.Concrete;

internal sealed class CreateModule : Module
{
    private readonly Scheme scheme;
    private readonly IWorker worker;
    private readonly IDummyTaskFactory dummyTaskFactory;

    internal CreateModule(string identifier, Scheme scheme, IWorker worker, IDummyTaskFactory dummyTaskFactory) : base(identifier)
    {
        this.scheme = scheme ?? throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");
        this.worker = worker ?? throw new ArgumentNullException($"{nameof(worker)} cannot be null.");
        this.dummyTaskFactory = dummyTaskFactory ?? throw new ArgumentNullException($"{nameof(dummyTaskFactory)} cannot be null.");

        this.MoveTimeline(this.worker.Delay);
    }

    internal int CreatedTasksCount { get; private set; }

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

        SystemLoggerService.Instance.AppendLogEntry(this, "LOG", "TRACE", $"[{base.Identifier}] sends [{newTask.Id}] to the [{nextModule?.Identifier}]");
    }

    internal override sealed void UpdateTimeline(float currentTime)
    {
        base.TimeCurrent = currentTime;
    }

    internal override sealed void UpdateStatistics(float deltaTime)
    {
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        base.TimeNext = this.TimeCurrent + deltaTime;
    }

    public override sealed void PrintStatistics()
    {
        this.scheme.PrintStatistics();
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", $"[{base.Identifier}]: Tasks (created) {this.CreatedTasksCount}");
    }
}
