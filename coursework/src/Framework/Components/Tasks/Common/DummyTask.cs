namespace CourseWork.Framework.Components.Tasks.Common;

internal abstract class DummyTask
{
    private protected static int nextId;

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
}
