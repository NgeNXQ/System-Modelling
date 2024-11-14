using System;
using LabWork3.Framework.Components.Tasks.Common;

namespace LabWork3.Framework.Components.Queues.Common;

internal interface IQueue
{
    public int Count { get; }
    public bool IsFull { get; }
    public bool IsEmpty { get; }
    public int MaxCapacity { get; }

    public event EventHandler<EventArgs>? TaskAdded;
    public event EventHandler<EventArgs>? TaskRemoved;

    public void AddLast(Task task);
    public Task RemoveLast();
    public void AddFirst(Task task);
    public Task RemoveFirst();
}