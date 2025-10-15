using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BBBirder.Instructions
{
    public class ExecutionEngine
    {
        const int FRAME_POOL_LIMIT = 512;
        public static readonly ExecutionEngine RuntimeShared = new();

        private readonly Environment environment = new();
        private readonly Stack<StackFrame> framePool = new();

        public ExecutionEngine() { }

        internal StackFrame RentFrame()
        {
            Utils.CheckMainThread();
            if (framePool.Count > 0)
            {
                return framePool.Pop();
            }
            else
            {
                return new();
            }
        }

        internal void ReturnFrame(StackFrame stackFrame)
        {
            Utils.CheckMainThread();

            if (framePool.Count < FRAME_POOL_LIMIT)
            {
                stackFrame.Clear();
                framePool.Push(stackFrame);
            }
        }

        internal void BeginInvoke(StackFrame stackFrame)
        {
            Utils.CheckMainThread();

            environment.PushStackFrame(stackFrame);
        }

        internal void EndInvokeAndReleaseFrame()
        {
            Utils.CheckMainThread();

            environment.PopStackFrame(out var stackFrame);
            ReturnFrame(stackFrame);
        }

        public void PushArgumentT<T>(T value)
        {
            environment.PushArgumentT(value);
        }

        public void PushArgument<T>(T value, ValueSignatureBump bump = default) where T : struct
        {
            environment.PushArgument(value);
        }

        public void PushArgument<T>(T value) where T : class
        {
            environment.PushArgument(value);
        }

        public T GetArgument<T>(int traceDepth, int stackOffset)
        {
            return environment.GetArgument<T>(traceDepth, stackOffset);
        }

#if UNITY_EDITOR
        public static void Break()
        {
            EditorApplication.isPaused = true;
        }

        public static void Continue()
        {
            EditorApplication.isPaused = false;
        }
#endif
    }

}
