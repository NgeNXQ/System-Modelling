using System;
using System.Linq;
using System.Collections.Generic;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Queues.Common;
using LabWork3.Framework.Components.Queues.Concrete;
using LabWork3.Framework.Components.Workers.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;
using LabWork3.Framework.Components.Schemes.Concrete;

namespace LabWork3.Framework.Components.Modules.Concrete;

internal sealed class MultiProcessorModule : Module
{
    private readonly IScheme distribution;
    private readonly IMockWorker mockWorker;
    private readonly IList<ProcessorModule> subProcessors;

    private float timeCurrent;
    private float totalTimeBusy;
    private float totalQueueLengthSum;
    private float totalSubProcessorsTimeBusy;

    internal MultiProcessorModule(string identifier, IScheme scheme, IMockWorker mockWorker, IQueue queue, int subProcessorsCount) : base(identifier)
    {
        if (scheme == null)
            throw new ArgumentNullException($"{nameof(scheme)} cannot be null.");

        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        if (queue == null)
            throw new ArgumentNullException($"{nameof(queue)} cannot be null.");

        if (subProcessorsCount <= 0)
            throw new ArgumentException($"{nameof(subProcessorsCount)} cannot be less or equals 0.");

        this.Queue = queue;
        this.mockWorker = mockWorker;
        this.subProcessors = new ProcessorModule[subProcessorsCount];
        this.distribution = new PayloadDistributionScheme(this.subProcessors, scheme.Fallback);

        for (int i = 0; i < subProcessorsCount; ++i)
            this.subProcessors[i] = new ProcessorModule($"{identifier}_{i}", scheme, mockWorker, new DefaultQueue(0));
    }

    internal IQueue Queue { get; private init; }

    internal int FailuresCount { get; private set; }

    internal int SuccessesCount { get; private set; }

    internal bool IsPartiallyBusy => this.subProcessors.Any(subProcessorModule => subProcessorModule.IsBusy);

    internal bool IsCompletelyBusy => this.subProcessors.All(subProcessorModule => subProcessorModule.IsBusy);

    internal List<ProcessorModule> BusySubProcessors => this.subProcessors.Where(processor => processor.TimeNext == this.TimeNext).ToList();

    internal override sealed float TimeCurrent
    {
        get => this.timeCurrent;
        set
        {
            float deltaTime = value - this.timeCurrent;

            this.totalSubProcessorsTimeBusy += this.subProcessors.Count(processor => processor.IsBusy) * deltaTime;
            this.totalTimeBusy += (this.IsCompletelyBusy ? deltaTime : 0.0f);
            this.totalQueueLengthSum += deltaTime * this.Queue.Count;
            this.timeCurrent = value;

            foreach (ProcessorModule processor in this.subProcessors)
                processor.TimeCurrent = this.timeCurrent;
        }
    }

    internal override sealed void AcceptTask(Task task, IMockWorker? mockWorker)
    {
        if (this.IsCompletelyBusy)
        {
            if (!this.Queue.IsFull)
                this.Queue.AddLast(task);
            else
                ++this.FailuresCount;
        }
        else
        {
            Module? nextModule = this.distribution.GetNextModule(task);
            nextModule?.AcceptTask(task, null);
            Console.WriteLine($"|LOG| (TRACE) [{base.Identifier}] sends task to the [{nextModule?.Identifier}]");
            this.MoveTimeline(this.mockWorker.DelayPayload);
        }
    }

    internal override sealed void CompleteTask()
    {
        ++this.SuccessesCount;
        List<ProcessorModule> BusySubProcessors = this.BusySubProcessors;

        foreach (ProcessorModule processor in BusySubProcessors)
        {
            processor.CompleteTask();

            if (!this.Queue.IsEmpty)
            {
                Task newTask = this.Queue.RemoveFirst();
                processor.AcceptTask(newTask, null);
                Console.WriteLine($"|LOG| (TRACE) [{base.Identifier}] sends task to the [{processor.Identifier}]");
            }
        }

        this.MoveTimeline(this.mockWorker.DelayPayload);
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        this.TimeNext = this.subProcessors.Min(processor => processor.TimeNext);
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Busy sub-processors: {this.BusySubProcessors.Count}; Queue: {this.Queue.Count}; Successes: {this.SuccessesCount}; Failures: {this.FailuresCount}");
    }

    public override sealed void PrintFinalStatistics()
    {
        float busyness = this.totalTimeBusy / this.TimeCurrent;
        float queueLengthMean = this.totalQueueLengthSum / this.TimeCurrent;
        float failureProbability = (this.SuccessesCount == 0 ? 0 : (float)this.FailuresCount / (this.FailuresCount + this.SuccessesCount));

        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.Write($"Busyness: {busyness}; ");
        Console.Write($"Queue mean: {queueLengthMean}; ");
        Console.Write($"Failures: {this.FailuresCount}; ");
        Console.Write($"Successes: {this.SuccessesCount}; ");
        Console.Write($"Failure probability: {failureProbability}; ");
        Console.WriteLine($"Average busy processors: {this.totalSubProcessorsTimeBusy / this.TimeCurrent}");

        foreach (ProcessorModule processorModule in this.subProcessors)
            processorModule.PrintFinalStatistics();
    }
}