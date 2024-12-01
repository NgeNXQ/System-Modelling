using System;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Concrete;
using CourseWork.Framework.Components.Blueprints.Transitions.Common;

namespace CourseWork.Framework.Components.Blueprints.Transitions.Concrete;

internal sealed class AdvancedProcessorTransition : Transition
{
    private readonly Func<DummyTask, bool> flowPredicateHandler;

    internal AdvancedProcessorTransition(ProcessorModule processor, Func<DummyTask, bool> flowPredicateHandler) : base(processor)
    {
        this.flowPredicateHandler = flowPredicateHandler ?? throw new ArgumentNullException($"{nameof(flowPredicateHandler)} cannot be null.");
    }

    internal override sealed bool CheckIsRunnable(DummyTask task)
    {
        return this.flowPredicateHandler.Invoke(task ?? throw new ArgumentNullException($"{nameof(flowPredicateHandler)} cannot be null."));
    }
}
