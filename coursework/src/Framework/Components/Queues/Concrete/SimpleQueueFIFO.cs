using System;
using System.Collections.Generic;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Queues.Common;

namespace Coursework.Framework.Components.Queues.Concrete
{
    internal sealed class SimpleQueueFIFO : IQueue
    {
        private readonly int maxCapacity;
        private readonly Queue<DummyTask> queue;

        internal SimpleQueueFIFO(int maxCapacity)
        {
            if (maxCapacity < 0)
                throw new ArgumentException($"{nameof(maxCapacity)} cannot be less than 0.");

            this.maxCapacity = maxCapacity;
            this.queue = new Queue<DummyTask>();
        }

        internal static SimpleQueueFIFO None => new SimpleQueueFIFO(0);
        internal static SimpleQueueFIFO Infinite => new SimpleQueueFIFO(Int32.MaxValue);

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
