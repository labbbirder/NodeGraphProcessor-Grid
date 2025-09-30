using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BBBirder.FlowField
{
    public unsafe partial class NativeActiveList<T> : IEnumerable<T>, IDisposable
        where T : unmanaged
    {
        private int _activeCount;
        private int _count;
        private T* ptr;
        private int _capacity;

        public int Count => _count;
        public int ActiveCount => _activeCount;

        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException("index", index, "must reside in [0,count)");
                }

                return ref NativeAPI.AsRef<T>(ptr + index);
            }
        }

        private void EnsureSize(int size)
        {
            if (_capacity >= size) return;

            var newSize = 8;
            if (_capacity > newSize) newSize = _capacity;
            while (newSize < size) newSize <<= 1;

            var newBufferBytes = size * NativeAPI.SizeOf<T>();
            var newPtr = (T*)NativeAPI.MallocAligned(newBufferBytes, NativeAPI.AlignmentOf<T>());
            Buffer.MemoryCopy(ptr, newPtr, newBufferBytes, _count * NativeAPI.SizeOf<T>());
            NativeAPI.FreeAligned((nuint)ptr);
            ptr = newPtr;

            GC.KeepAlive(this);
        }

        public NativeActiveList()
        {
            if (!s_TypeValid)
            {
                ThrowTypeNotValid();
            }
        }

        public void Add(in T element)
        {
            EnsureSize(_count + 1);
            ; *(ptr + _count) = element;
            ; *(int*)(ptr + _count) = _count;
            _count++;

            GC.KeepAlive(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsActiveAt(int index)
        {
            return index < _activeCount;
        }

        public void Active(int index)
        {
            if (!IsActiveAt(index))
            {
                Swap(index, _activeCount);
                _activeCount++;
            }

            GC.KeepAlive(this);
        }

        public void Deactive(int index)
        {
            if (IsActiveAt(index))
            {
                Swap(index, _activeCount - 1);
                _activeCount--;
            }

            GC.KeepAlive(this);
        }

        public void RemoveAt(int index)
        {
            if (IsActiveAt(index)) _activeCount--;
            if (index < Count)
            {
                Swap(index, _count);
                RemoveLast();
            }
            else
            {
                throw new IndexOutOfRangeException();
            }

            GC.KeepAlive(this);
        }

        private void RemoveLast()
        {
            ; *(int*)(ptr + --_count) = -1;
            GC.KeepAlive(this);
        }

        private void Swap(int i, int j)
        {
            if (i == j) return;

            ; (*(ptr + i), *(ptr + j)) = (*(ptr + j), *(ptr + i));
            ; *(int*)(ptr + i) = i;
            ; *(int*)(ptr + j) = j;
            GC.KeepAlive(this);
        }

        private void DoActualDispose()
        {
            if ((nuint)ptr == 0)
            {
                return;
            }

            NativeAPI.FreeAligned((nuint)ptr);
        }

        ~NativeActiveList()
        {
            DoActualDispose();
        }

        public void Dispose()
        {
            DoActualDispose();
            GC.SuppressFinalize(this);
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

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NativeActiveList<T> list;
            private int index;
            private T _current;

            public Enumerator(NativeActiveList<T> list)
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

        void ThrowTypeNotValid()
        {
            throw new($"{typeof(T)} is not a valid type");
        }

        void ThrowOutOfRange(int index)
        {
            throw new ArgumentOutOfRangeException("index", index, $"must reside in [0,{_count})");
        }
        // }

        // unsafe partial class NativeActiveList<T>
        // {
        static private readonly bool s_TypeValid = false;
        const string INDEX_FIELD_NAME = "__index";
        static NativeActiveList()
        {
            CheckT();
            s_TypeValid = true;
        }

        static void CheckT()
        {
            var type = typeof(T);
            T testValue = default;
            var ptr = (int*)NativeAPI.AddressOf(ref testValue);
            var fiIndex = type.GetField(INDEX_FIELD_NAME);
            if (fiIndex == null || NativeAPI.SizeOf<T>() < sizeof(int))
            {
                throw new Exception($"Type {type.FullName} dont has a field `{INDEX_FIELD_NAME}` in type int");
            }

            // It would be better if you could call ldflda opcode here.

            *ptr = -1;
            if (-1 != (int)fiIndex.GetValue(testValue))
            {
                throw new Exception($"Field `{INDEX_FIELD_NAME}` must be the first in type {type.FullName}");
            }

            *ptr = 0;
            if (0 != (int)fiIndex.GetValue(testValue))
            {
                throw new Exception($"Field `{INDEX_FIELD_NAME}` must be the first in type {type.FullName}");
            }
        }
    }
}
