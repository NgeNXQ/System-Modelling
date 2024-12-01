using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Tasks.Concrete;
using CourseWork.Framework.Components.Tasks.Utilities.Factories.Common;

namespace CourseWork.Framework.Components.Tasks.Utilities.Factories.Concrete;

internal sealed class PacketDummyTaskFactory : IDummyTaskFactory
{
    public DummyTask CreateDummyTask(float timeCurrent)
    {
        return new PacketDummyTask(timeCurrent);
    }
}
