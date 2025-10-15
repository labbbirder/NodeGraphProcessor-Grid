using System.Diagnostics;
using System.Threading;

namespace BBBirder.Instructions
{
    internal static class Utils
    {

        [Conditional("DEBUG")] // Equivalent to `UNITY_EDITOR || DEVELOPMENT_BUILD`
        public static void CheckMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                throw new("ExecutionVM must runs in main thread.");
            }
        }

    }
}
