using LabWork4.Framework.Components.Tasks.Common;

namespace LabWork4.Framework.Components.Tasks.Concrete;

internal sealed class MockTask : Task
{
    private const int GENERAL_TYPE = 1;

    public MockTask(float timeCreation) : base(timeCreation)
    {
        base.InitialType = MockTask.GENERAL_TYPE;
        base.CurrentType = MockTask.GENERAL_TYPE;
    }
}
