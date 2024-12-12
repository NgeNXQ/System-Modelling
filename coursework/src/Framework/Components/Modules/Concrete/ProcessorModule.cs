using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Core.Controllers;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Queues.Common;
using Coursework.Framework.Components.Workers.Common;
using Coursework.Framework.Components.Modules.Common;
using Coursework.Framework.Components.Blueprints.Schemes.Common;

namespace Coursework.Framework.Components.Modules.Concrete;

internal sealed class ProcessorModule : Module
{
    private readonly IQueue queue;
    private readonly Scheme scheme;
    private readonly IWorker worker;

    private DummyTask? currentTask;

    private float currentBusyness;
    private float currentQueueMean;

    internal ProcessorModule(string identifier, Scheme scheme, IWorker worker, IQueue queue) : base(identifier)
    {
        this.queue = queue ?? throw new ArgumentNullException($"{nameof(queue)} cannot be null.");
        this.scheme = scheme ?? throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");
        this.worker = worker ?? throw new ArgumentNullException($"{nameof(worker)} cannot be null.");
    }

    internal bool IsBusy { get; private set; }

    internal int FailedTasksCount { get; private set; }

    internal int SucceededTasksCount { get; private set; }

    internal int ProcessedTasksCount => this.FailedTasksCount + this.SucceededTasksCount;

    internal float Busyness => this.currentBusyness / SystemModelController.Instance.TimeCurrent;

    internal float QueueMean => this.currentQueueMean / SystemModelController.Instance.TimeCurrent;

    internal override sealed void CompleteTask()
    {
        ++this.SucceededTasksCount;
        DummyTask finishedTask = this.currentTask!;

        if (this.queue.IsEmpty)
        {
            this.IsBusy = false;
            this.currentTask = null;
            this.TimeNext = Single.MaxValue;
        }
        else
        {
            this.MoveTimeline(this.worker.Delay);
            this.currentTask = this.queue.Dequeue();
        }

        Module? nextModule = this.scheme.GetNextModule(finishedTask);
        SystemLoggerService.Instance.AppendLogEntry(this, "LOG", "TRACE", $"[{base.Identifier}] sends [{finishedTask}] to the [{nextModule?.Identifier}]");
        nextModule?.AcceptTask(finishedTask);
    }

    internal override sealed void AcceptTask(DummyTask task)
    {
        if (task == null)
            throw new ArgumentNullException($"{nameof(task)} cannot be null.");

        if (this.IsBusy)
        {
            if (!this.queue.IsFull)
                this.queue.Enqueue(task);
            else
                ++this.FailedTasksCount;
        }
        else
        {
            this.IsBusy = true;
            this.currentTask = task;
            this.MoveTimeline(this.worker.Delay);
        }
    }

    internal override sealed void UpdateTimeline(float currentTime)
    {
        base.TimeCurrent = currentTime;
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        base.TimeNext = this.TimeCurrent + deltaTime;
    }

    public override sealed void PrintStatistics()
    {
        string formattedLogMessage = $"[{base.Identifier}]: Successes: {this.SucceededTasksCount}; Failures: {this.FailedTasksCount}; Queue (mean): {this.QueueMean}; Busyness: {this.Busyness}";
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", formattedLogMessage);
        this.scheme.PrintStatistics();
    }

    public override sealed void ResetStatistics()
    {
        this.FailedTasksCount = 0;
        this.SucceededTasksCount = 0;
    }

    public override sealed void UpdateStatistics(float deltaTime)
    {
        this.currentQueueMean += this.queue.Count * deltaTime;
        this.currentBusyness += (this.currentTask != null ? 1 : 0) * deltaTime;
    }
}
