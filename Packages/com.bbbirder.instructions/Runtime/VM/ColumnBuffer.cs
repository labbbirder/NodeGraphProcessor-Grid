using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;

namespace BBBirder.Instructions
{
    public interface IColumnBuffer
    {
        int Count { get; }
        T GetValueOrDefaultByToken<T>(int token, T defaultValue = default);
        void RemoveByToken(int token);
    }

    [DebuggerDisplay("Length = {_denseCount}")]
    [DebuggerTypeProxy(typeof(ColumnBuffer<>.ColumnBufferDebugView))]
    public class ColumnBuffer<T> : IColumnBuffer //where T : struct
    {
        static readonly Exception checkTypeException;
        static ColumnBuffer()
        {
            if (!typeof(T).IsValueType)
            {
                checkTypeException = new("Heap buffer must be used for value types.");
            }
        }

        private int maxTokenEver;
        private int _denseCount;
        private Slot[] dense;
        private int[] sparse;

        public int Count => _denseCount;

        internal ColumnBuffer()
        {
            if (checkTypeException != null) throw checkTypeException;

            dense = Array.Empty<Slot>();
            sparse = Array.Empty<int>();
        }

        public T2 GetValueOrDefaultByToken<T2>(int token, T2 defaultValue = default)
        {
            if (TryGetSlot(token, out var slot))
            {
                var value = slot.value;
                RuntimeConverter.TryConvert<T, T2>(value, out var targetValue);
                return targetValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public T GetValueOrDefaultByToken(int token, T defaultValue = default)
        {
            if (TryGetSlot(token, out var slot))
            {
                return slot.value;
            }
            else
            {
                return defaultValue;
            }
        }

        private void EnsureSparseCapacity(int capacity)
        {
            if (sparse.Length >= capacity)
            {
                return;
            }

            var newArr = ArrayPool<int>.Shared.Rent(capacity);
            Array.Copy(sparse, newArr, maxTokenEver);
            Array.Clear(newArr, maxTokenEver, newArr.Length - maxTokenEver);
            sparse = newArr;
        }

        private void EnsureDenseCapacity(int capacity)
        {
            if (dense.Length >= capacity)
            {
                return;
            }

            var newArr = ArrayPool<Slot>.Shared.Rent(capacity);
            Array.Copy(dense, newArr, _denseCount);
            Array.Clear(newArr, _denseCount, newArr.Length - _denseCount);
            dense = newArr;
        }

        public void Push(T value, out int token)
        {
            EnsureDenseCapacity(_denseCount + 1);
            // EnsureSparseCapacity(token);
            // var index = sparse[token] - 1;
            ref var slot = ref dense[_denseCount];
            slot.value = value;
            if (slot.token != 0)
            {
                token = slot.token;
                _denseCount++;
                sparse[token - 1] = _denseCount;
            }
            else
            {
                _denseCount++;
                token = _denseCount;
                slot.token = token;
                EnsureSparseCapacity(token);
                sparse[token - 1] = _denseCount;
            }

            if (token > maxTokenEver)
            {
                maxTokenEver = token;
            }
        }

        private bool TryGetSlot(int token, out Slot result)
        {
            if (sparse.Length <= token - 1 || token <= 0)
            {
                result = default;
                return false;
            }

            var index = sparse[token - 1] - 1;

            if (index == -1 || dense.Length <= index)
            {
                result = default;
                return false;
            }

            var slot = dense[index];

            /* We have cleared sparse array, hence token-compare is redundant */

            // if (slot.token != token)
            // {
            //     result = default;
            //     return false;
            // }

            result = slot;
            return true;
        }

        public void RemoveByToken(int token)
        {
            if (sparse.Length <= token - 1 || token <= 0)
            {
                return;
            }

            var index = sparse[token - 1] - 1;

            if (index < 0 || dense.Length <= index)
            {
                return;
            }

            var tailToken = dense[_denseCount - 1].token;
            dense[index].value = default;
            ; (dense[index], dense[_denseCount - 1]) = (dense[_denseCount - 1], dense[index]);
            sparse[tailToken - 1] = token;
            sparse[token - 1] = 0;
            _denseCount--;
        }

        // public override string ToString()
        // {
        //     return $"dense {string.Join(", ", dense.Take(Count).Select(s => $"[{s.token}]{s.value}"))}\nsparse: {string.Join(", ", sparse)}";
        // }

        public struct Slot
        {
            public int token;
            public T value;
        }

        internal class ColumnBufferDebugView
        {
            ColumnBuffer<T> inner;
            public ColumnBufferDebugView(ColumnBuffer<T> inner)
            {
                this.inner = inner;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items => inner.dense.Take(inner._denseCount).Select(s => s.value).ToArray();
        }
    }
}
