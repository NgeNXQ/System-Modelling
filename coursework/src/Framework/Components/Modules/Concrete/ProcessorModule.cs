using System;
using CourseWork.Framework.Core.Services;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Queues.Common;
using CourseWork.Framework.Components.Workers.Common;
using CourseWork.Framework.Components.Modules.Common;
using CourseWork.Framework.Components.Blueprints.Schemes.Common;

namespace CourseWork.Framework.Components.Modules.Concrete;

internal sealed class ProcessorModule : Module
{
    private readonly IQueue queue;
    private readonly IScheme scheme;
    private readonly IWorker worker;

    private DummyTask? currentTask;

    private float timeCurrent;
    private float totalTimeBusy;
    private float totalDelayPayloads;
    private float totalQueueLengthSum;

    internal ProcessorModule(string identifier, IScheme scheme, IWorker worker, IQueue queue) : base(identifier)
    {
        this.queue = queue ?? throw new ArgumentNullException($"{nameof(queue)} cannot be null.");
        this.scheme = scheme ?? throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");
        this.worker = worker ?? throw new ArgumentNullException($"{nameof(worker)} cannot be null.");
    }

    internal bool IsBusy { get; private set; }

    internal int TotalFailuresCount { get; private set; }

    internal int TotalSuccessesCount { get; private set; }

    // internal int CurrentTasksCount => this.queue.Count + (this.IsBusy ? 1 : 0);

    internal override sealed float TimeCurrent
    {
        get => this.timeCurrent;
        set
        {
            // float deltaTime = value - this.timeCurrent;
            // this.totalTimeBusy += (this.IsBusy ? deltaTime : 0.0f);
            // this.totalQueueLengthSum += deltaTime * this.queue.Count;
            this.timeCurrent = value;
        }
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
                ++this.TotalFailuresCount;
        }
        else
        {
            this.IsBusy = true;
            this.currentTask = task;
            this.MoveTimeline(this.worker.Delay);
        }
    }

    internal override sealed void CompleteTask()
    {
        ++this.TotalSuccessesCount;
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
        SystemLoggerService.Instance.AppendLogEntry("LOG", "TRACE", $"[{base.Identifier}] sends task #{finishedTask.Id} to the [{nextModule?.Identifier}]");
        nextModule?.AcceptTask(finishedTask);
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        // this.totalDelayPayloads += deltaTime;
        this.TimeNext = this.TimeCurrent + deltaTime;
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Busy?: {this.IsBusy}; queue: {this.queue.Count}; Successes: {this.TotalSuccessesCount}; Failures: {this.TotalFailuresCount}");
    }

    public override sealed void PrintFinalStatistics()
    {
        float busyness = this.totalTimeBusy / this.TimeCurrent;
        float queueLengthMean = this.totalQueueLengthSum / this.TimeCurrent;
        float delayPayloadMean = this.totalDelayPayloads / this.TotalSuccessesCount;
        float failureProbability = (this.TotalSuccessesCount == 0 ? 0 : (float)this.TotalFailuresCount / (this.TotalFailuresCount + this.TotalSuccessesCount));

        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.Write($"Busyness: {busyness}; ");
        Console.Write($"queue mean: {queueLengthMean}; ");
        Console.Write($"Delay mean: {delayPayloadMean}; ");
        Console.Write($"Failures: {this.TotalFailuresCount}; ");
        Console.Write($"Successes: {this.TotalSuccessesCount}; ");
        Console.WriteLine($"Failure probability: {failureProbability}");
    }
}
