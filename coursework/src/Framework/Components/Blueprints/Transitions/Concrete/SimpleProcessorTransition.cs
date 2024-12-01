using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Concrete;
using CourseWork.Framework.Components.Blueprints.Transitions.Common;

namespace CourseWork.Framework.Components.Blueprints.Transitions.Concrete;

internal sealed class SimpleProcessorTransition : Transition
{
    internal SimpleProcessorTransition(ProcessorModule processor) : base(processor)
    {
    }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        return true;
    }
}
