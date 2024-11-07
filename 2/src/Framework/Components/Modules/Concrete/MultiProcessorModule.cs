using System;
using System.Linq;
using System.Collections.Generic;
using LabWork2.Framework.Components.Modules.Common;
using LabWork2.Framework.Components.Workers.Common;

namespace LabWork2.Framework.Components.Modules.Concrete;

internal sealed class MultiProcessorModule : Module
{
    private readonly int maxQueueLength;
    private readonly IMockWorker mockWorker;
    private readonly IList<ProcessorModule> subProcessorsModules;

    private bool isBusy;
    private int queueLength;
    private float queueLengthSum;
    private IList<ProcessorModule>? busySubProcessorsModules;

    private float timeCurrent;
    private int failuresCount;
    private int successesCount;

    internal MultiProcessorModule(string identifier, IMockWorker mockWorker, int maxQueueLength, int subProcessorsCount) : base(identifier)
    {
        if (mockWorker == null)
            throw new ArgumentNullException($"{nameof(mockWorker)} cannot be null.");

        if (maxQueueLength < 0)
            throw new ArgumentException($"{nameof(maxQueueLength)} cannot be less than 0.");

        if (subProcessorsCount <= 0)
            throw new ArgumentException($"{nameof(subProcessorsCount)} cannot be less or equals 0.");

        this.mockWorker = mockWorker;
        this.maxQueueLength = maxQueueLength;
        this.subProcessorsModules = new List<ProcessorModule>();

        for (int i = 0; i < subProcessorsCount; ++i)
            this.subProcessorsModules.Add(new ProcessorModule($"{identifier}_{i}", mockWorker, 0));
    }

    internal override sealed float TimeCurrent
    {
        get => this.timeCurrent;
        set
        {
            this.queueLengthSum += (value - this.timeCurrent) * this.queueLength;
            this.timeCurrent = value;

            foreach (ProcessorModule processorModule in this.subProcessorsModules)
                processorModule.TimeCurrent = this.timeCurrent;
        }
    }

    internal sealed override void AcceptTask()
    {
        if (this.isBusy)
        {
            if (this.queueLength < this.maxQueueLength)
                ++this.queueLength;
            else
                ++this.failuresCount;
        }
        else
        {
            ProcessorModule? processorModule = this.subProcessorsModules.Where(processorModule => !processorModule.IsBusy).FirstOrDefault();

            if (processorModule != null)
            {
                Console.WriteLine($"|LOG| (TRACE) {this.Identifier} sends task to the {processorModule.Identifier}.");
                processorModule.AcceptTask();
            }
            else
            {
                Console.WriteLine($"|LOG| (TRACE) {this.Identifier} disposes task.");
            }

            // this.subProcessorsModules.Where(processorModule => !processorModule.IsBusy).FirstOrDefault()?.AcceptTask();
            this.isBusy = this.subProcessorsModules.All(subProcessorModule => subProcessorModule.IsBusy);
            this.MoveTimeline();
        }
    }

    internal override sealed void CompleteTask()
    {
        ++this.successesCount;
        this.busySubProcessorsModules = this.subProcessorsModules.Where(processorModule => processorModule.TimeNext == this.TimeNext).ToList();

        foreach (ProcessorModule processorModule in this.busySubProcessorsModules)
        {
            processorModule.CompleteTask();

            if (this.queueLength != 0)
            {
                --this.queueLength;
                processorModule.AcceptTask();
            }
        }

        this.isBusy = this.subProcessorsModules.All(subProcessorModule => subProcessorModule.IsBusy);
        this.MoveTimeline();
        base.NextModule?.AcceptTask();
    }

    private protected override sealed void MoveTimeline()
    {
        this.TimeNext = this.subProcessorsModules.Min(processorModule => processorModule.TimeNext);
    }

    public override sealed void PrintFinalStatistics()
    {
        float averageQueueLength = this.queueLengthSum / this.TimeCurrent;
        float failureProbability = (this.successesCount == 0 ? 0 : (float)this.failuresCount / (this.failuresCount + this.successesCount));

        Console.Write($"|REPORT| [{base.Identifier}] ");
        Console.WriteLine($"Failures: {this.failuresCount}; Successes: {this.successesCount}; Failure probability: {failureProbability}; Average queue: {averageQueueLength}");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"busy?: {this.isBusy}; Queue: {this.queueLength}; Failures: {this.failuresCount}; Time: {this.TimeNext}.");
    }
}
