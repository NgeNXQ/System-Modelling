using Coursework.Framework.Core.Controllers;

namespace Coursework.Framework.Components.Tasks.Common;

internal abstract class DummyTask
{
    private static int nextId;

    static DummyTask()
    {
        DummyTask.nextId = 0;
    }

    private protected DummyTask(float timeCreation)
    {
        this.Id = ++DummyTask.nextId;
        this.TimestampCreation = timeCreation;
    }

    internal int Id { get; private init; }

    internal float TimestampCreation { get; private init; }

    internal float Lifetime { get => SystemModelController.Instance.TimeCurrent - this.TimestampCreation; }
}
