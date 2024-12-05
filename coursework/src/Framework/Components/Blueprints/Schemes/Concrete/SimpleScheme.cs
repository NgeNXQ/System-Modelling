using System;
using System.Collections.Generic;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Common;
using Coursework.Framework.Components.Blueprints.Schemes.Common;
using Coursework.Framework.Components.Blueprints.Transitions.Common;

namespace Coursework.Framework.Components.Blueprints.Schemes.Concrete;

internal sealed class SimpleScheme : Scheme
{
    private readonly ICollection<Transition> transitions;

    internal SimpleScheme(string identifier, params Transition[] transitions) : base(identifier)
    {
        this.transitions = new LinkedList<Transition>();

        foreach (Transition transition in transitions)
            this.transitions.Add(transition ?? throw new ArgumentNullException($"{nameof(transition)} cannot be null."));
    }

    internal int FailuresCount { get; private set; }

    internal int SuccessesCount { get; private set; }

    internal int ProcessedCount => this.SuccessesCount + this.FailuresCount;

    internal float FailureProbability => (float)this.FailuresCount / this.ProcessedCount;

    internal override sealed Module? GetNextModule(DummyTask task)
    {
        if (task == null)
            throw new ArgumentNullException($"{nameof(task)} cannot be null.");

        Module? nextModule = null;

        foreach (Transition transition in this.transitions)
        {
            if (transition.CheckIsRunnable(task))
            {
                if (nextModule != null)
                    throw new InvalidOperationException("Invalid scheme. More than 1 runnable transition has been detected.");

                nextModule = transition.Destination;
                transition.UpdateStatistics(TransitionStatus.Active, task);
                continue;
            }

            transition.UpdateStatistics(TransitionStatus.Inactive, task);
        }

        if (nextModule == null)
            ++this.FailuresCount;
        else
            ++this.SuccessesCount;

        return nextModule;
    }

    public override sealed void PrintStatistics()
    {
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", $"[{base.Identifier}]: Successes: {this.SuccessesCount}; Failures: {this.FailuresCount}");

        foreach (Transition transition in this.transitions)
            transition.PrintStatistics();
    }

    internal void Attach(Transition transition)
    {
        this.transitions.Add(transition ?? throw new ArgumentNullException($"{nameof(transition)} cannot be null."));
    }
}
