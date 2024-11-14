using LabWork3.Framework.Components.Tasks.Common;

namespace LabWork3.Framework.Components.Tasks.Concrete;

internal sealed class CarTask : Task
{
    private const int GENERAL_TYPE = 1;

    public CarTask(float timeCreation) : base(timeCreation)
    {
        base.InitialType = CarTask.GENERAL_TYPE;
        base.CurrentType = CarTask.GENERAL_TYPE;
    }
}
