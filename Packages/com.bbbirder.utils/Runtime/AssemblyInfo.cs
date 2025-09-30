using System.Buffers;

namespace BBBirder
{
    internal static class CollectionUtility<T>
    {
        public static readonly ArrayPool<T> ArrayPool = ArrayPool<T>.Create();
    }
}
