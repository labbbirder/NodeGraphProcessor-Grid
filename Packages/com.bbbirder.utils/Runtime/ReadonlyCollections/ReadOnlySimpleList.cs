using System;
using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public struct ReadOnlySimpleList<T> : IReadOnlyCollection<T>
    {
        private SimpleList<T> list;

        public bool HasValue => list != null;

        public int Count => list.Count;

        public T this[int index] => list[index];

        public T this[Index index] => list[index];

        public ReadOnlySimpleList(SimpleList<T> list)
        {
            this.list = list;
        }

        public SimpleList<T>.Enumerator GetEnumerator()
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
