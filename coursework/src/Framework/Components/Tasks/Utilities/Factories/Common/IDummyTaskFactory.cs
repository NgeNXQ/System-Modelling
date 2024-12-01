using CourseWork.Framework.Components.Tasks.Common;

namespace CourseWork.Framework.Components.Tasks.Utilities.Factories.Common;

internal interface IDummyTaskFactory
{
    public DummyTask CreateDummyTask(float timeCurrent);
}
