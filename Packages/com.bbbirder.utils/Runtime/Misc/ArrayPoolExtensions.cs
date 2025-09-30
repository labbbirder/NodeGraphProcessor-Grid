using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    public static class ArrayPoolExtensions
    {
        public static ArrayPoolScope<T> RentInScope<T>(this ArrayPool<T> pool, int expectedSize, out T[] array)
        {
            array = pool.Rent(expectedSize);
            return new(pool, array);
        }

        /// <summary>
        /// Ensure the array is large enough to hold the <paramref name="size"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <param name="size"></param>
        /// <param name="array"></param>
        public static void EnsureArraySize<T>(this ArrayPool<T> pool, ref T[] array, int size)
        {
            if (array is null)
            {
                array = pool.Rent(size);
            }
            else if (array.Length < size)
            {
                pool.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                array = pool.Rent(size);
            }
            else
            {
                return;
            }
        }

        public struct ArrayPoolScope<T> : IDisposable
        {
            private ArrayPool<T> arrayPool;
            private T[] array;

            public ArrayPoolScope(ArrayPool<T> pool, T[] array)
            {
                this.arrayPool = pool;
                this.array = array;
            }

            public void Dispose()
            {
                if (array != null)
                {
                    arrayPool.Return(array);
                }
            }
        }
    }
}
