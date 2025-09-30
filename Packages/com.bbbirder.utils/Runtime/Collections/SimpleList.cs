using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    public class SimpleList<T> : IList<T>, IDisposable
    {
        static readonly EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        static readonly T[] s_empty = Array.Empty<T>();
        static readonly ArrayPool<T> ArrayPool = ArrayPool<T>.Shared;

        private int _count = 0;
        private T[] _values = s_empty;

        T IList<T>.this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public ref T this[int index] => ref _values[index];

        public ref T this[Index index] => ref _values[index.GetOffset(_count)];

        public int Count => _count;

        public bool IsReadOnly => false;

        public virtual void EnsureCapacity(int size)
        {
            if (_values.Length >= size) return;

            var newArr = ArrayPool.Rent(size);
            if (_count > 0)
            {
                Array.Copy(_values, newArr, _count);
            }

            ArrayPool.Return(_values);
            _values = newArr;
        }

        public Span<T> AsSpan()
        {
            return _values.AsSpan(.._count);
        }

        public Span<T> AsSpan(Range range)
        {
            return _values.AsSpan(range);
        }

        public void Add(T item)
        {
            EnsureCapacity(_count + 1);
            _values[_count] = item;
            _count++;
        }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_values, 0, _values.Length);
            }

            _count = 0;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(item, _values[i])) return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _count; i++)
            {
                if (arrayIndex + i >= array.Length) break;
                array[arrayIndex + i] = _values[i];
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(item, _values[i])) return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            EnsureCapacity(_count + 1);
            for (int i = _count; i > index; i--)
            {
                _values[i] = _values[i - 1];
            }

            _values[index] = item;
            _count++;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            else
            {
                RemoveAt(index);
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            for (int i = index; i < _count - 1; i++)
            {
                _values[i] = _values[i + 1];
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _values[_count - 1] = default;
            }

            _count--;
        }

        public void RemoveAt(Index index)
        {
            RemoveAt(index.GetOffset(_count));
        }

        public void EmplacedRemoveAt(Index index)
        {
            this[index] = this[_count - 1];
            RemoveAt(_count - 1);
        }

        public void EmplacedRemoveAt(int index)
        {
            this[index] = this[_count - 1];
            RemoveAt(_count - 1);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            ArrayPool.Return(_values);
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly IList<T> _list;
            int _index;
            T _current;

            public T Current => _current;
            object IEnumerator.Current => _current;

            public Enumerator(IList<T> list)
            {
                _list = list;
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_index < _list.Count)
                {
                    _current = _list[_index];
                    _index++;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }
}
