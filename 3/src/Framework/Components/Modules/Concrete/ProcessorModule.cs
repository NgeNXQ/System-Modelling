using System;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Queues.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;
using LabWork3.Framework.Components.Workers.Common;

namespace LabWork3.Framework.Components.Modules.Concrete;

internal sealed class ProcessorModule : Module
{
    private readonly IScheme scheme;
    private readonly IMockWorker mockWorker;

    private float timeCurrent;
    private Task? currentTask;

    private float totalTimeBusy;
    private float totalDelayPayloads;
    private float totalQueueLengthSum;

    internal ProcessorModule(string identifier, IScheme scheme, IMockWorker mockWorker, IQueue queue) : base(identifier)
    {
        if (queue == null)
            throw new ArgumentNullException($"{nameof(queue)} cannot be null.");

        if (scheme == null)
            throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");

        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        this.Queue = queue;
        this.scheme = scheme;
        this.mockWorker = mockWorker;
    }

    internal bool IsBusy { get; private set; }

    internal IQueue Queue { get; private init; }

    internal int FailuresCount { get; private set; }

    internal int SuccessesCount { get; private set; }

    internal int CurrentTasksCount => this.Queue.Count + (this.IsBusy ? 1 : 0);

    internal override sealed float TimeCurrent
    {
        get => this.timeCurrent;
        set
        {
            float deltaTime = value - this.timeCurrent;
            this.totalTimeBusy += (this.IsBusy ? deltaTime : 0.0f);
            this.totalQueueLengthSum += deltaTime * this.Queue.Count;
            this.timeCurrent = value;
        }
    }

    internal override sealed void AcceptTask(Task task)
    {
        if (this.IsBusy)
        {
            if (!this.Queue.IsFull)
                this.Queue.AddLast(task);
            else
                ++this.FailuresCount;
        }
        else
        {
            this.IsBusy = true;
            this.currentTask = task;
            this.MoveTimeline(this.mockWorker.DelayPayload);
        }
    }

    internal void AcceptInitialTask(Task task, float delayPayload)
    {
        this.IsBusy = true;
        this.currentTask = task;
        base.TimeNext = delayPayload;
    }

    internal override sealed void CompleteTask()
    {
        ++this.SuccessesCount;

        if (this.Queue.IsEmpty)
        {
            this.IsBusy = false;
            this.TimeNext = Single.MaxValue;
        }
        else
        {
            this.currentTask = this.Queue.RemoveFirst();
            this.MoveTimeline(this.mockWorker.DelayPayload);
        }

        Module? nextModule = this.scheme.GetNextModule(this.currentTask!);
        nextModule?.AcceptTask(this.currentTask!);

        Console.WriteLine($"|LOG| (TRACE) [{base.Identifier}] sends task to the [{nextModule?.Identifier}]");
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        this.totalDelayPayloads += deltaTime;
        this.TimeNext = this.TimeCurrent + deltaTime;
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Busy?: {this.IsBusy}; Queue: {this.Queue.Count}; Successes: {this.SuccessesCount}; Failures: {this.FailuresCount}");
    }

    public override sealed void PrintFinalStatistics()
    {
        float busyness = this.totalTimeBusy / this.TimeCurrent;
        float queueLengthMean = this.totalQueueLengthSum / this.TimeCurrent;
        float delayPayloadMean = this.totalDelayPayloads / this.SuccessesCount;
        float failureProbability = (this.SuccessesCount == 0 ? 0 : (float)this.FailuresCount / (this.FailuresCount + this.SuccessesCount));

        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.Write($"Busyness: {busyness}; ");
        Console.Write($"Queue mean: {queueLengthMean}; ");
        Console.Write($"Delay mean: {delayPayloadMean}; ");
        Console.Write($"Failures: {this.FailuresCount}; ");
        Console.Write($"Successes: {this.SuccessesCount}; ");
        Console.WriteLine($"Failure probability: {failureProbability}");
    }
}
