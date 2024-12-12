using Coursework.Framework.Components.Tasks.Common;

namespace Coursework.Framework.Components.Queues.Common;

internal interface IQueue
{
    public int Count { get; }
    public bool IsFull { get; }
    public bool IsEmpty { get; }
    public int MaxCapacity { get; }

    public void Clear();
    public DummyTask Dequeue();
    public void Enqueue(DummyTask task);
}
