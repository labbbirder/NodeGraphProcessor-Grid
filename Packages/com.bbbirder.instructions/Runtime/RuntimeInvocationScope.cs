using System;

namespace BBBirder.Instructions
{
    public struct RuntimeInvocationScope : IDisposable
    {
        readonly ExecutionEngine ee;

        public RuntimeInvocationScope(ExecutionEngine ee)
        {
            this.ee = ee;
        }

        public static RuntimeInvocationScope Create(ExecutionEngine ee)
        {
            var stackFrame = ee.RentFrame();
            ee.BeginInvoke(stackFrame);
            return new(ee);
        }

        public void Dispose()
        {
            ee.EndInvokeAndReleaseFrame();
        }
    }
}
