using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Common;

namespace Coursework.Framework.Components.Modules.Concrete;

internal sealed class DisposeModule : Module
{
    internal DisposeModule(string identifier) : base(identifier)
    {
    }

    internal int DisposedTasksCount { get; private set; }

    internal override sealed void CompleteTask()
    {
        throw new InvalidOperationException($"{base.Identifier} ({this.GetType()}) is not able to complete tasks.");
    }

    internal override sealed void AcceptTask(DummyTask task)
    {
        ++this.DisposedTasksCount;
        SystemLoggerService.Instance.AppendLogEntry(this, "LOG", "TRACE", $"[{base.Identifier}] disposes [{task}]");
    }

    internal override sealed void UpdateTimeline(float timeCurrent)
    {
        base.TimeCurrent = timeCurrent;
    }

    private protected override sealed void MoveTimeline(float deltaTime)
    {
        base.TimeNext = this.TimeCurrent + deltaTime;
    }

    public override sealed void PrintStatistics()
    {
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", $"[{base.Identifier}]: Tasks (disposed) {this.DisposedTasksCount}");
    }

    public override sealed void ResetStatistics()
    {
        this.DisposedTasksCount = 0;
    }

    public override sealed void UpdateStatistics(float deltaTime)
    {
    }
}
