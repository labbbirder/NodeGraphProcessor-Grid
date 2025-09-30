using System.Collections;
using System.Collections.Generic;

namespace BBBirder
{
    public class SmartHashSet<T> : ICollection<T>
    {
        const int HASH_THRESHOLD = 12;
        private ICollection<T> inner;
        public int Count => inner.Count;

        public bool IsReadOnly => false;

        public SmartHashSet() : this(0) { }

        public SmartHashSet(int capacity)
        {
            if (capacity >= HASH_THRESHOLD)
            {
                inner = CollectionPool.Get<HashSet<T>>();
            }
            else
            {
                inner = CollectionPool.Get<SimpleList<T>>();
            }
        }

        public bool TryPeek(out T item)
        {
            if (inner.Count == 0)
            {
                item = default;
                return false;
            }

            if (inner is SimpleList<T> list)
            {
                item = list[0];
            }
            else
            {
                item = default;
                foreach (var e in inner)
                {
                    item = e;
                    break;
                }
            }

            return true;
        }

        public bool Add(T item)
        {
            if (inner is SimpleList<T> list)
            {
                if (!list.Contains(item))
                {
                    if (list.Count + 1 >= HASH_THRESHOLD)
                    {
                        Advance();
                        inner.Add(item);
                    }
                    else
                    {
                        list.Add(item);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return (inner as HashSet<T>).Add(item);
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        private void Advance()
        {
            var hash = CollectionPool.Get<HashSet<T>>();
            var list = (SimpleList<T>)inner;
            foreach (var item in list)
            {
                hash.Add(item);
            }

            CollectionPool.Release(list);
            inner = hash;
        }

        public void Clear()
        {
            inner.Clear();
        }

        public bool Contains(T item)
        {
            return inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            inner.CopyTo(array, arrayIndex);
        }

        // TODO: custom struct Enumerator
        public IEnumerator<T> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return inner.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
