using System;
using System.Collections.Generic;
using CourseWork.Framework.Core.Services;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Common;

namespace CourseWork.Framework.Components.Modules.Concrete;

internal sealed class DisposeModule : Module
{
    private int totalDisposedTasksCount;

    internal DisposeModule(string identifier) : base(identifier)
    {
        // this.DisposedTasksCount = new Dictionary<int, int>();
        // this.DisposedTasksLifetime = new Dictionary<int, float>();
    }

    // internal IDictionary<int, int> DisposedTasksCount { get; private init; }

    // internal IDictionary<int, float> DisposedTasksLifetime { get; private init; }

    // internal int DisposedTasksCountTotal => this.DisposedTasksCount.Values.Sum();

    // internal float DisposedTasksLifetimeMean => this.DisposedTasksLifetime.Values.Sum() / this.DisposedTasksCount.Values.Sum();

    internal override sealed float TimeCurrent { get; set; }

    internal override sealed void CompleteTask()
    {
        throw new InvalidOperationException($"{base.Identifier} ({this.GetType()}) is not able to complete tasks.");
    }

    internal override sealed void AcceptTask(DummyTask task)
    {
        ++this.totalDisposedTasksCount;
        SystemLoggerService.Instance.AppendLogEntry("LOG", "TRACE", $"[{base.Identifier}] disposes task #{task.Id}");
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        throw new InvalidOperationException($"{base.GetType()} {base.Identifier}a is not able to move the timeline.");
    }

    public override sealed void PrintIntermediateStatistics()
    {
        Console.Write($"|LOG| (STATS) [{base.Identifier}] ");
        Console.WriteLine($"Tasks (total): {this.totalDisposedTasksCount}");
    }

    public override sealed void PrintFinalStatistics()
    {
        Console.Write($"|REPORT| [{base.Identifier}] ");
        // Console.WriteLine($"Tasks (total): {this.DisposedTasksCountTotal}");

        // foreach (KeyValuePair<int, int> entry in this.DisposedTasksCount)
        //     Console.WriteLine($"|REPORT| [{base.Identifier}] (Type:Count) {entry.Key} : {entry.Value}");

        // foreach (KeyValuePair<int, float> entry in this.DisposedTasksLifetime)
        //     Console.WriteLine($"|REPORT| [{base.Identifier}] (Type:Lifetime) {entry.Key} : {entry.Value / this.DisposedTasksCount[entry.Key]}");
    }
}
