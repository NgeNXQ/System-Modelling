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

    internal int FailedTasksCount { get; private set; }

    internal int UserFailedTasksCount { get; private set; }

    internal float TasksLifetimeSum { get; private set; }

    internal int SucceededTasksCount { get; private set; }

    internal int UserSucceededTasksCount { get; private set; }

    internal float TaskLifetimeMean => this.TasksLifetimeSum / this.ProcessedTasksCount;

    internal int ProcessedTasksCount => this.SucceededTasksCount + this.FailedTasksCount;

    internal int UserProcessedTasksCount => this.UserSucceededTasksCount + this.UserFailedTasksCount;

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
        {
            ++this.FailedTasksCount;
            ++this.UserFailedTasksCount;
        }
        else
        {
            ++this.SucceededTasksCount;
            ++this.UserSucceededTasksCount;
        }

        this.TasksLifetimeSum += task.Lifetime;

        return nextModule;
    }

    internal void Attach(Transition transition)
    {
        this.transitions.Add(transition ?? throw new ArgumentNullException($"{nameof(transition)} cannot be null."));
    }

    public override sealed void PrintStatistics()
    {
        string formattedLogMessage = $"[{base.Identifier}]: Successes: {this.SucceededTasksCount}; Failures: {this.FailedTasksCount}; Tasks' lifetime (mean): {this.TaskLifetimeMean}";
        SystemLoggerService.Instance.AppendLogEntry(this, "REPORT", "STATS", formattedLogMessage);

        foreach (Transition transition in this.transitions)
            transition.PrintStatistics();
    }

    public override sealed void ResetStatistics()
    {
        this.UserFailedTasksCount = 0;
        this.UserSucceededTasksCount = 0;
    }

    public override sealed void UpdateStatistics(float deltaTime)
    {
    }
}
