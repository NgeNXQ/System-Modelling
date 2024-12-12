using Coursework.Framework.Components.Common;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Common;

namespace Coursework.Framework.Components.Blueprints.Transitions.Common;

internal abstract class Transition : Element
{
    private protected Transition(string identifier, Module destination) : base(identifier)
    {
        this.Destination = destination;
    }

    internal Module Destination { get; private init; }

    internal abstract bool CheckIsRunnable(DummyTask task);
    internal abstract void UpdateStatistics(TransitionStatus status, DummyTask task);
}
