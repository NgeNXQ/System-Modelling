using System;
using LabWork2.Framework.Components.Modules.Common;
using LabWork2.Framework.Components.Workers.Common;

namespace LabWork2.Framework.Components.Modules.Concrete;

internal sealed class ProcessorModule : Module
{
    private readonly int maxQueueLength;
    private readonly IMockWorker mockWorker;

    private int queueLength;
    private float queueLengthSum;

    private float timeCurrent;
    private int failuresCount;
    private int successesCount;
    private float totalDelayPayloads;

    internal ProcessorModule(string identifier, IMockWorker mockWorker, int maxQueueLength) : base(identifier)
    {
        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        if (maxQueueLength < 0)
            throw new ArgumentException($"{nameof(maxQueueLength)} cannot be less than 0.");

        this.mockWorker = mockWorker;
        this.maxQueueLength = maxQueueLength;
    }

    internal bool IsBusy { get; private set; }

    internal override sealed float TimeCurrent
    {
        get => this.timeCurrent;
        set
        {
            this.queueLengthSum += (value - this.timeCurrent) * this.queueLength;
            this.timeCurrent = value;
        }
    }

    internal sealed override void AcceptTask()
    {
        if (this.IsBusy)
        {
            if (this.queueLength < this.maxQueueLength)
                ++this.queueLength;
            else
                ++this.failuresCount;
        }
        else
        {
            this.IsBusy = true;
            this.MoveTimeline();
        }
    }

    internal override sealed void CompleteTask()
    {
        ++this.successesCount;

        if (this.queueLength == 0)
        {
            this.IsBusy = false;
            base.TimeNext = Single.MaxValue;
        }
        else
        {
            --this.queueLength;
            this.MoveTimeline();
        }

        base.NextModule?.AcceptTask();
    }

    private protected override sealed void MoveTimeline()
    {
        float delayPayload = this.mockWorker.DelayPayload;
        this.TimeNext = this.TimeCurrent + delayPayload;
        this.totalDelayPayloads += delayPayload;
    }

    public override sealed void PrintFinalStatistics()
    {
        float averageQueueLength = this.queueLengthSum / this.TimeCurrent;
        float averageDelayPayload = this.totalDelayPayloads / this.successesCount;
        float failureProbability = (this.successesCount == 0 ? 0 : (float)this.failuresCount / (this.failuresCount + this.successesCount));

        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.WriteLine($"Delay mean: {averageDelayPayload}; Successes: {this.successesCount}; Queue mean: {averageQueueLength}; Failures: {this.failuresCount}; Failure probability: {failureProbability}; ");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Queue: {this.queueLength}; Failures: {this.failuresCount}; Time: {this.TimeNext}.");
    }
}
