using System;
using System.Collections.Generic;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Queues.Common;

namespace CourseWork.Framework.Components.Queues.Concrete
{
    internal sealed class SimpleQueue : IQueue
    {
        private readonly int maxCapacity;
        private readonly Queue<DummyTask> queue;

        internal SimpleQueue(int maxCapacity)
        {
            if (maxCapacity < 0)
                throw new ArgumentException($"{nameof(maxCapacity)} cannot be less than 0.");

            this.maxCapacity = maxCapacity;
            this.queue = new Queue<DummyTask>();
        }

        public int Count => this.queue.Count;
        public int MaxCapacity => this.maxCapacity;
        public bool IsEmpty => this.queue.Count == 0;
        public bool IsFull => this.queue.Count >= this.maxCapacity;

        public void Clear()
        {
            this.queue.Clear();
        }

        public DummyTask Dequeue()
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Queue is empty.");

            return this.queue.Dequeue();
        }

        public void Enqueue(DummyTask task)
        {
            if (this.IsFull)
                throw new InvalidOperationException("Queue is full.");

            if (task == null)
                throw new ArgumentNullException($"{nameof(task)} cannot be null.");

            this.queue.Enqueue(task);
        }
    }
}
