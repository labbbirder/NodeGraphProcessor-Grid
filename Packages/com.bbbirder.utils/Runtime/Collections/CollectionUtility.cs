#if DEBUG
#define CHECK_INDEX_IN_RANGE
#endif

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

namespace BBBirder
{
    public static class CollectionUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int CeilExponent(int n)
        {
            float f = n;
            var exp = (int)(*(uint*)&f << 1 >> 24) - 127;
            // var exp = (*(int*)&f << 1 >>> 24) - 127; // C# 11.0
            // BitOperations.RoundUpToPowerOf2(n) // .Net 7+
            if (1 << exp < n) exp++;
            return exp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LargerSizeInPowOf2(int cnt) => 1 << CeilExponent(cnt);

        [Conditional("CHECK_INDEX_IN_RANGE")]
        public static void AssertIndexInRange(int index, int count)
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException($"{index} out of range [0,{count})");
            }
        }
    }

}
