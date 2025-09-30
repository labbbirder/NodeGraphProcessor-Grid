using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public struct ReadOnlyActiveList<T> : IReadOnlyCollection<T> where T : IIndexElement
    {
        private ActiveList<T> list;

        public bool HasValue => list != null;

        public int Count => list.Count;

        public T this[int index]
        {
            get => list[index];
        }

        public ReadOnlyActiveList(ActiveList<T> list)
        {
            this.list = list;
        }

        public bool Contains(T item) => list.Contains(item);

        public ActiveList<T>.Enumerator GetEnumerator() => list.GetEnumerator();

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
