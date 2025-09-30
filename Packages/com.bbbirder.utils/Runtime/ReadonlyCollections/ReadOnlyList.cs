using System;
using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public struct ReadOnlyList<T> : IReadOnlyCollection<T>
    {
        private List<T> list;

        public bool HasValue => list != null;

        public int Count => list.Count;

        public T this[int index] => list[index];

        public T this[Index index] => list[index];

        public ReadOnlyList(List<T> list)
        {
            this.list = list;
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
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
