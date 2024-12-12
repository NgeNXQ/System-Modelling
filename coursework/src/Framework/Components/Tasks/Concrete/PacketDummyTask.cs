using Coursework.Framework.Components.Tasks.Common;

namespace Coursework.Framework.Components.Tasks.Concrete;

internal sealed class PacketDummyTask : DummyTask
{
    internal PacketDummyTask(float timeCreation) : base(timeCreation)
    {
    }

    public sealed override string ToString()
    {
        return $"#{this.Id} ({this.Lifetime})";
    }
}
