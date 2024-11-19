using LabWork4.Framework.Components.Tasks.Common;
using LabWork4.Framework.Components.Tasks.Concrete;
using LabWork4.Framework.Components.Tasks.Utilities.Factories.Common;

namespace LabWork4.Framework.Components.Tasks.Utilities.Factories.Concrete;

internal sealed class MockTaskFactory : TaskFactory
{
    internal override sealed Task CreateTask(float timeCreation)
    {
        return new MockTask(timeCreation);
    }
}
