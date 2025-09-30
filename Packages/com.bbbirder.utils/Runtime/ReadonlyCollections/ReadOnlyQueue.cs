using System;
using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public struct ReadOnlyQueue<T> : IReadOnlyCollection<T>
    {
        private Queue<T> queue;

        public bool HasValue => queue != null;

        public int Count => queue.Count;

        public ReadOnlyQueue(Queue<T> queue)
        {
            this.queue = queue;
        }

        public Queue<T>.Enumerator GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
