using System;
using System.Collections.Generic;

namespace Translumo.Infrastructure.Collections
{
    public class LimitedQueue<TEntity> : Queue<TEntity>
    {
        public int Capacity
        {
            get => _capacity;
            set
            {
                _capacity = value;
                Dequeue(Count - _capacity);
            }
        }

        private int _capacity;

        public LimitedQueue(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentException($"Invalid capacity");
            }

            this.Capacity = capacity;
        }

        public new void Enqueue(TEntity item)
        {
            base.Enqueue(item);
            if (Count >= Capacity)
            {
                this.Dequeue(Count - Capacity);
            }
        }

        public void DequeueAll(Predicate<TEntity> condition)
        {
            var curEntity = this.Peek();
            while (curEntity != null && condition.Invoke(curEntity))
            {
                this.Dequeue();
                curEntity = this.Peek();
            }
        }

        public void Dequeue(int count)
        {
            while (count > 0)
            {
                base.Dequeue();
                count--;
            }
        }
    }
}
