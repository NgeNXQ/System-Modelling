using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Concrete;
using CourseWork.Framework.Components.Blueprints.Transitions.Common;

namespace CourseWork.Framework.Components.Blueprints.Transitions.Concrete;

internal sealed class DisposeTransition : Transition
{
    internal DisposeTransition(DisposeModule dispose) : base(dispose)
    {
    }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        return true;
    }
}
