namespace LabWork3.Framework.Components.Tasks.Common;

internal abstract class Task
{
    public Task(float timeCreation)
    {
        this.TimeCreation = timeCreation;
    }

    internal int CurrentType { get; set; }
    internal float TimeCreation { get; private init; }
    internal int InitialType { get; private protected set; }
}
