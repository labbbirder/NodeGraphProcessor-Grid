using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    public class ActiveList<T> : IEnumerable<T> where T : IIndexElement
    {
        private static readonly ArrayPool<T> pool = ArrayPool<T>.Create();
        private int _activeCount;
        private int _count;
        private T[] elements;

        public int Count => _count;
        public int ActiveCount => _activeCount;

        public ref T this[int index] => ref elements[index];

        public ActiveList()
        {
            elements = Array.Empty<T>();
        }

        public ActiveList(T[] list)
        {
            EnsureSize(list.Length);
            Array.ConstrainedCopy(list, 0, elements, 0, _count);
#if DEBUG
            foreach (var element in list)
            {
                element.SetCollection(this);
            }
#endif
            _count = list.Length;
        }

        public void EnsureSize(int size)
        {
            if (elements.Length >= size)
                return;

            var newArr = pool.Rent(size);
            Array.ConstrainedCopy(elements, 0, newArr, 0, _count);
            pool.Return(elements, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            elements = newArr;
        }

        public void Add(T element)
        {
            element.Index = _count;
            element.SetCollection(this);
            EnsureSize(_count + 1);
            elements[_count++] = element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsActiveAt(int index)
        {
            return index < _activeCount;
        }

        public bool IsActive(T element)
        {
            var index = IndexOf(element);
            if (index == ~0)
            {
                throw new ArgumentException("element is not inside list");
            }

            return IsActiveAt(index);
        }

        public void Active(T element)
        {
            if (!IsActive(element))
            {
                Swap(element.Index, _activeCount);
                _activeCount++;
            }
        }

        public void Deactive(T element)
        {
            if (IsActive(element))
            {
                Swap(element.Index, _activeCount - 1);
                _activeCount--;
            }
        }

        public void ActiveAt(int index)
        {
            if (index < 0 || index >= _count)
                return;

            if (index >= _activeCount)
            {
                Swap(index, _activeCount);
                _activeCount++;
            }
        }

        public void DeactiveAt(int index)
        {
            if (index < 0 || index >= _count)
                return;

            if (index < _activeCount)
            {
                _activeCount--;
                Swap(index, _activeCount);
            }
        }

        public bool Remove(T element)
        {
            var index = IndexOf(element);
            if (index != ~0)
            {
                if (IsActiveAt(index))
                {
                    _activeCount--;
                    Swap(index, _activeCount);
                    Swap(_activeCount, _count - 1);
                    RemoveLast();
                }
                else
                {
                    Swap(index, _count - 1);
                    RemoveLast();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T element)
        {
            return IndexOf(element) != ~0;
        }

        private int IndexOf(T element)
        {
            var index = element.Index;
            if (index < 0 || index >= _count)
            {
                return ~0;
            }
            else
            {
                if (EqualityComparer<T>.Default.Equals(elements[index], element))
                {
                    return index;
                }
                else
                {
                    return ~0;
                }
            }
        }

        private void RemoveLast()
        {
            ref var last = ref elements[_count - 1];
            last.Index = -1;
            last.SetCollection(null);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                last = default;
            }

            _count--;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                return;

            if (index >= _activeCount)
            {
                Swap(index, _count - 1);
                RemoveLast();
            }
            else
            {
                _activeCount--;
                Swap(index, _activeCount);
                Swap(_activeCount, _count - 1);
                RemoveLast();
            }
        }

        private void Swap(int i, int j)
        {
            if (i == j)
                return;

            ; (elements[i], elements[j]) = (elements[j], elements[i]);
            elements[i].Index = i;
            elements[j].Index = j;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                ref var element = ref elements[i];
                if (element is null) continue;

                element.SetCollection(null);
                element.Index = -1;
            }

            pool.Return(elements, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            _activeCount = 0;
            _count = 0;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly ActiveList<T> list;
            private int index;
            private T _current;

            public Enumerator(ActiveList<T> list)
            {
                this.list = list;
                this.index = -1;
                this._current = default;
            }

            public readonly T Current => _current;

            readonly object IEnumerator.Current => _current;

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < list._count - 1)
                {
                    index++;
                    _current = list[index];
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                this.index = -1;
                this._current = default;
            }
        }
    }

}
