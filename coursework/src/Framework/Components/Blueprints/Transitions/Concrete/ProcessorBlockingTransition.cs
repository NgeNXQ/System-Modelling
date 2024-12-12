using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Core.Controllers;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Concrete;
using Coursework.Framework.Components.Blueprints.Transitions.Common;

namespace Coursework.Framework.Components.Blueprints.Transitions.Concrete;

internal sealed class ProcessorBlockingTransition : Transition
{
    private readonly Func<DummyTask, bool> flowPredicateHandler;

    private float activationTime;
    private TransitionStatus previousBlockingStatus;

    internal ProcessorBlockingTransition(string identifier, ProcessorModule processor, Func<DummyTask, bool> flowPredicateHandler) : base(identifier, processor)
    {
        this.previousBlockingStatus = TransitionStatus.None;
        this.flowPredicateHandler = flowPredicateHandler ?? throw new ArgumentNullException($"{nameof(flowPredicateHandler)} cannot be null.");
    }

    internal float TimeUsageTotal { get; private set; }

    internal int ActivationsCount { get; private set; }

    internal int DeactivationsCount { get; private set; }

    internal sealed override void UpdateStatistics(TransitionStatus status, DummyTask task)
    {
        if (status != this.previousBlockingStatus)
        {
            if (status == TransitionStatus.Active)
            {
                ++this.ActivationsCount;
                this.activationTime = SystemModelController.Instance.TimeCurrent;
            }
            else if (status == TransitionStatus.Inactive)
            {
                ++this.DeactivationsCount;
            }
        }

        if (this.previousBlockingStatus != TransitionStatus.Inactive)
        {
            this.TimeUsageTotal += SystemModelController.Instance.TimeCurrent - this.activationTime;
            this.activationTime = SystemModelController.Instance.TimeCurrent;
        }

        this.previousBlockingStatus = status;
    }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        return !this.flowPredicateHandler.Invoke(task ?? throw new ArgumentNullException($"{nameof(task)} cannot be null."));
    }

    public override sealed void PrintStatistics()
    {
        string formattedLogMessage = $"[{base.Identifier}]: Activations: {this.ActivationsCount}; Deactivations: {this.DeactivationsCount}; Usage time: {this.TimeUsageTotal}";
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", formattedLogMessage);
    }

    public override sealed void ResetStatistics()
    {
        this.TimeUsageTotal = 0;
        this.ActivationsCount = 0;
        this.DeactivationsCount = 0;
    }

    public override sealed void UpdateStatistics(float deltaTime)
    {
    }
}
