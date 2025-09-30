using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public struct ReadOnlyHashSet<T> : IReadOnlyCollection<T>
    {
        private HashSet<T> hashset;

        public bool HasValue => hashset != null;

        public int Count => hashset.Count;

        public ReadOnlyHashSet(HashSet<T> hashset)
        {
            this.hashset = hashset;
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return hashset.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T value)
        {
            return hashset.Contains(value);
        }
    }
}
