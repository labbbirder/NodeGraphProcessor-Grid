using System.Threading;

namespace BBBirder
{
    internal class TypeIdHelper
    {
        internal static int s_id;
    }

    public class TypeId<T>
    {
        public static readonly int Id;

        static TypeId()
        {
            Id = Interlocked.Increment(ref TypeIdHelper.s_id);
        }
    }
}
