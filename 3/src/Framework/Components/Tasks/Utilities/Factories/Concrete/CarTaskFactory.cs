using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Tasks.Concrete;
using LabWork3.Framework.Components.Tasks.Utilities.Factories.Common;

namespace LabWork3.Framework.Components.Tasks.Utilities.Factories.Concrete;

internal sealed class CarTaskFactory : TaskFactory
{
    internal override sealed Task CreateTask(float timeCreation)
    {
        return new CarTask(timeCreation);
    }
}
