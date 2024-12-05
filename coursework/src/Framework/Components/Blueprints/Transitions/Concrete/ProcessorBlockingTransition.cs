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
    private bool isBlockedCurrently;
    private bool isBlockedPreviously;

    internal ProcessorBlockingTransition(string identifier, ProcessorModule processor, Func<DummyTask, bool> flowPredicateHandler) : base(identifier, processor)
    {
        this.flowPredicateHandler = flowPredicateHandler ?? throw new ArgumentNullException($"{nameof(flowPredicateHandler)} cannot be null.");
    }

    internal float TimeUsageTotal { get; private set; }

    internal int ActivationsCount { get; private set; }

    internal int DeactivationsCount { get; private set; }

    private bool hasRedirectionOccurred => this.isBlockedCurrently != this.isBlockedPreviously;

    internal sealed override void UpdateStatistics(TransitionStatus status, DummyTask task)
    {
        if (this.hasRedirectionOccurred)
        {
            if (status == TransitionStatus.Active)
            {
                ++this.ActivationsCount;
                this.activationTime = SystemModelController.Instance.TimeCurrent;
            }
            else
            {
                ++this.DeactivationsCount;
                this.activationTime = 0.0f;
            }
        }
        else
        {
            if (status == TransitionStatus.Active)
            {
                this.TimeUsageTotal += SystemModelController.Instance.TimeCurrent - this.activationTime;
                this.activationTime = SystemModelController.Instance.TimeCurrent;
            }
        }

        this.isBlockedPreviously = this.isBlockedCurrently;
    }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        this.isBlockedCurrently = !this.flowPredicateHandler.Invoke(task ?? throw new ArgumentNullException($"{nameof(task)} cannot be null."));
        return this.isBlockedCurrently;
    }

    public override sealed void PrintStatistics()
    {
        string formattedLogMessage = $"[{base.Identifier}]: Activations: {this.ActivationsCount}; Deactivations: {this.DeactivationsCount}; Usage time: {this.TimeUsageTotal}";
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", formattedLogMessage);
    }
}
