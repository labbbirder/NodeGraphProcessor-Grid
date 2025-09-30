using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    /// <summary>
    /// Compactly arranged LinkedList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerTypeProxy(typeof(DenseLink<>.SparseLinkDebugView))]
    public class DenseLink<T> : IEnumerable<T>, IDisposable
    {
        static readonly Node[] s_empty = Array.Empty<Node>();
        static readonly ArrayPool<Node> ArrayPool = ModuleProps<Node>.ArrayPool;

        private Node[] _dense = s_empty;
        private int _count;
        private int _headIndex = -1;
        private int _tailIndex = -1;

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new IndexOutOfRangeException($"index {index} out of range[0..{_count}]");
                }

                return _dense[index].Value;
            }
        }

        private void EnsureSize(int size)
        {
            if (_dense.Length >= size) return;

            if (size == 0)
            {
                ReleaseArray(ref _dense);
                _dense = s_empty;
            }
            else
            {
                size = CollectionUtility.LargerSizeInPowOf2(size);
                var newArr = ArrayPool.Rent(size);
                // var newArr = new T[size];
                if (_count > 0)
                {
                    Array.Copy(_dense, newArr, _count);
                }

                ReleaseArray(ref _dense);
                _dense = newArr;
            }
        }

        public int IndexOf(T value)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(_dense[i].Value, value))
                {
                    return i;
                }
            }

            return ~0;
        }

        public bool Contains(T action)
        {
            return IndexOf(action) != ~0;
        }

        public void Add(T value)
        {
            EnsureSize(_count + 1);
            if (_tailIndex >= 0)
            {
                _dense[_tailIndex].NextIndex = _count;
            }

            _dense[_count] = new Node()
            {
                Value = value,
                PrevIndex = _tailIndex,
                NextIndex = ~0,
            };

            _tailIndex = _count;
            if (_headIndex == ~0)
            {
                _headIndex = _count;
            }

            _count++;
        }

        private void Swap(int i, int j)
        {
            if (i == j) return;

            var iNode = _dense[i];
            var jNode = _dense[j];

            if (iNode.PrevIndex != ~0)
            {
                _dense[iNode.PrevIndex].NextIndex = j;
            }

            if (iNode.NextIndex != ~0)
            {
                _dense[iNode.NextIndex].PrevIndex = j;
            }

            if (jNode.PrevIndex != ~0)
            {
                _dense[jNode.PrevIndex].NextIndex = i;
            }

            if (jNode.NextIndex != ~0)
            {
                _dense[jNode.NextIndex].PrevIndex = i;
            }

            ; (_dense[i], _dense[j]) = (_dense[j], _dense[i]);

            if (_headIndex == i)
            {
                _headIndex = j;
            }
            else if (_headIndex == j)
            {
                _headIndex = i;
            }

            if (_tailIndex == i)
            {
                _tailIndex = j;
            }
            else if (_tailIndex == j)
            {
                _tailIndex = i;
            }
        }

        private void Disconnect(int i)
        {
            var node = _dense[i];
            if (node.PrevIndex != ~0)
            {
                _dense[node.PrevIndex].NextIndex = node.NextIndex;
            }

            if (node.NextIndex != ~0)
            {
                _dense[node.NextIndex].PrevIndex = node.PrevIndex;
            }

            if (_headIndex == i)
            {
                _headIndex = node.NextIndex;
            }

            if (_tailIndex == i)
            {
                _tailIndex = node.PrevIndex;
            }
        }

        private void InternalRemoveAt(int index)
        {
            var countMinusOne = _count - 1;
            Swap(index, countMinusOne);
            Disconnect(countMinusOne);
            _dense[countMinusOne] = default;
            _count = countMinusOne;
        }

        public void RemoveAt(int index)
        {
            CollectionUtility.AssertIndexInRange(index, _count);
            InternalRemoveAt(index);
        }

        public bool Remove(T value)
        {
            var index = IndexOf(value);
            if (index == ~0)
            {
                return false;
            }

            InternalRemoveAt(index);
            return true;
        }

        public void Clear()
        {
            ReleaseArray(ref _dense);
            _count = 0;
            _headIndex = -1;
            _tailIndex = -1;
        }

        private void ReleaseArray(ref Node[] array)
        {
            if (array != null && !ReferenceEquals(array, s_empty))
            {
                ArrayPool.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }

            array = s_empty;
        }

        /// <summary>
        /// Release and return buffers to shared pool immediately
        /// </summary>
        /// <remarks>
        /// Optional Disposal: GC works well without `Dispose()`. No leak will happen.
        /// </remarks>
        public void Dispose()
        {
            Clear();
        }

        public Enumerator GetEnumerator()
        {
            return new(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Node
        {
            public int PrevIndex;
            public int NextIndex;
            public T Value;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private DenseLink<T> _inner;
            private Node _currentNode;
            private bool _firstIter;
            public T Current => _currentNode.Value;
            object IEnumerator.Current => _currentNode;

            public Enumerator(DenseLink<T> inner)
            {
                _inner = inner;
                _currentNode = default;
                _firstIter = true;
            }

            public void Dispose()
            {
                _inner = null;
            }

            public bool MoveNext()
            {
                var nextIndex = _firstIter
                    ? _inner._headIndex
                    : _currentNode.NextIndex;
                _firstIter = false;
                if (nextIndex != ~0)
                {
                    _currentNode = _inner._dense[nextIndex];
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                _currentNode = default;
                _firstIter = true;
            }
        }

        internal class SparseLinkDebugView
        {
            DenseLink<T> link;
            public SparseLinkDebugView(DenseLink<T> link)
            {
                this.link = link;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Values => link.ToArray();
        }
    }
}
