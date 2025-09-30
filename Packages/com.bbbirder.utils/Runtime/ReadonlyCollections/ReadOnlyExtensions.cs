using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    public static class ReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyList<T> ToReadOnly<T>(this List<T> values)
        {
            return new ReadOnlyList<T>(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyHashSet<T> ToReadOnly<T>(this HashSet<T> values)
        {
            return new ReadOnlyHashSet<T>(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyQueue<T> ToReadOnly<T>(this Queue<T> values)
        {
            return new ReadOnlyQueue<T>(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySimpleList<T> ToReadOnly<T>(this SimpleList<T> values)
        {
            return new ReadOnlySimpleList<T>(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyActiveList<T> ToReadOnly<T>(this ActiveList<T> values) where T : IIndexElement
        {
            return new ReadOnlyActiveList<T>(values);
        }
    }
}
