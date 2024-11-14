using System;
using System.Collections.Generic;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Queues.Common;

namespace LabWork3.Framework.Components.Queues.Concrete
{
    internal sealed class DefaultQueue : IQueue
    {
        private readonly int maxCapacity;
        private readonly LinkedList<Task> items;

        internal DefaultQueue(int maxCapacity)
        {
            if (maxCapacity < 0)
                throw new ArgumentException($"{nameof(maxCapacity)} cannot be less than 0.");

            this.maxCapacity = maxCapacity;
            this.items = new LinkedList<Task>();
        }

        public event EventHandler<EventArgs>? TaskAdded;
        public event EventHandler<EventArgs>? TaskRemoved;

        public int Count => this.items.Count;
        public int MaxCapacity => this.maxCapacity;
        public bool IsEmpty => this.items.Count == 0;
        public bool IsFull => this.items.Count >= this.maxCapacity;

        public void AddLast(Task task)
        {
            if (task == null)
                throw new ArgumentNullException($"{nameof(task)} cannot be null.");

            if (IsFull)
                throw new InvalidOperationException("Queue is full.");

            this.items.AddLast(task);
            this.TaskAdded?.Invoke(this, EventArgs.Empty);
        }

        public Task RemoveLast()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Queue is empty.");

            LinkedListNode<Task>? lastNode = this.items.Last;

            if (lastNode == null)
                throw new InvalidOperationException("Last node is null.");

            this.items.RemoveLast();
            this.TaskRemoved?.Invoke(this, EventArgs.Empty);
            return lastNode.Value;
        }

        public void AddFirst(Task task)
        {
            if (task == null)
                throw new ArgumentNullException($"{nameof(task)} cannot be null.");

            if (IsFull)
                throw new InvalidOperationException("Queue is full.");

            this.items.AddFirst(task);
            this.TaskAdded?.Invoke(this, EventArgs.Empty);
        }

        public Task RemoveFirst()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Queue is empty.");

            LinkedListNode<Task>? firstNode = this.items.First;

            if (firstNode == null)
                throw new InvalidOperationException("First node is null.");

            this.items.RemoveFirst();
            this.TaskRemoved?.Invoke(this, EventArgs.Empty);
            return firstNode.Value;
        }
    }
}
