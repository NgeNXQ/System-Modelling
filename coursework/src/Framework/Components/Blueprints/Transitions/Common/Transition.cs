using System;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Common;

namespace CourseWork.Framework.Components.Blueprints.Transitions.Common;

internal abstract class Transition
{
    private protected Transition(Module target)
    {
        this.Target = target ?? throw new ArgumentNullException($"{nameof(target)} cannot be null.");
    }

    internal Module Target { get; private init; }

    internal abstract bool CheckIsRunnable(DummyTask task);
}
