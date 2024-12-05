using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Tasks.Concrete;
using Coursework.Framework.Components.Tasks.Utilities.Factories.Common;

namespace Coursework.Framework.Components.Tasks.Utilities.Factories.Concrete;

internal sealed class PacketDummyTaskFactory : IDummyTaskFactory
{
    public DummyTask CreateDummyTask(float timeCurrent)
    {
        return new PacketDummyTask(timeCurrent);
    }
}
