using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Concrete;
using Coursework.Framework.Components.Blueprints.Transitions.Common;

namespace Coursework.Framework.Components.Blueprints.Transitions.Concrete;

internal sealed class ProcessorSimpleTransition : Transition
{
    internal ProcessorSimpleTransition(string identifier, ProcessorModule processor) : base(identifier, processor)
    {
    }

    internal int ForwardedTasksCount { get; private set; }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        return true;
    }

    internal sealed override void UpdateStatistics(TransitionStatus status, DummyTask task)
    {
        if (task == null)
            throw new ArgumentNullException($"{nameof(task)} cannot be null.");

        if (status == TransitionStatus.Active)
        {
            ++this.ForwardedTasksCount;
            SystemLoggerService.Instance.AppendLogEntry(this, "LOG", "TRACE", $"[{base.Identifier}] resends task #{task.Id} (lifetime: {task.Lifetime})");
        }
    }

    public override sealed void PrintStatistics()
    {
        SystemLoggerService.Instance.AppendLogEntry(this, "LOG", "STATS", $"[{base.Identifier}]: Tasks (forwarded) {this.ForwardedTasksCount}");
    }

    public override sealed void ResetStatistics()
    {
        this.ForwardedTasksCount = 0;
    }

    public override sealed void UpdateStatistics(float deltaTime)
    {
    }
}
