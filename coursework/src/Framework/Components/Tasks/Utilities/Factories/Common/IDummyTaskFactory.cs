using Coursework.Framework.Components.Tasks.Common;

namespace Coursework.Framework.Components.Tasks.Utilities.Factories.Common;

internal interface IDummyTaskFactory
{
    public DummyTask CreateDummyTask(float timeCurrent);
}
