using System;
using System.Linq;
using System.Collections.Generic;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Common;
using CourseWork.Framework.Components.Blueprints.Schemes.Common;
using CourseWork.Framework.Components.Blueprints.Transitions.Common;

namespace CourseWork.Framework.Components.Blueprints.Schemes.Concrete;

internal sealed class SimpleScheme : IScheme
{
    private readonly ICollection<Transition> transitions;

    internal SimpleScheme(params Transition[] transitions)
    {
        this.transitions = new LinkedList<Transition>();

        foreach (Transition transition in transitions)
            this.transitions.Add(transition ?? throw new ArgumentNullException($"{nameof(transition)} cannot be null."));
    }

    internal int FailuresCount { get; private set; }

    internal int SuccessesCount { get; private set; }

    internal float FailureProbability
    {
        get
        {
            return this.SuccessesCount == 0 ? 0.0f : this.FailuresCount / (this.SuccessesCount + this.FailuresCount);
        }
    }

    public Module? GetNextModule(DummyTask task)
    {
        if (this.transitions.Count(transition => transition.CheckIsRunnable(task)) > 1)
            throw new InvalidOperationException("Invalid scheme. More than 1 runnable transition has been detected");

        Module? nextModule = this.transitions.FirstOrDefault(transition => transition.CheckIsRunnable(task))?.Target;

        if (nextModule == null)
            ++this.FailuresCount;
        else
            ++this.SuccessesCount;

        return nextModule;
    }

    internal void Attach(Transition transition)
    {
        this.transitions.Add(transition ?? throw new ArgumentNullException($"{nameof(transition)} cannot be null."));
    }
}
