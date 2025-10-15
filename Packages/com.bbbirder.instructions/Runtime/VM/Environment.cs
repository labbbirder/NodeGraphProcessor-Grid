using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BBBirder.Instructions
{
    /// <summary>
    /// STACKFRAMES LAYOUT SKETCH
    ///  
    ///   iArg        iTrace
    ///       ┏━━━━━━┓ -1 
    ///     -1┃ arg0 ┃   
    ///       ┠──────┨
    ///     -2┃ arg1 ┃   │
    ///       ┠──────┨   │        ↑
    ///     -3┃ arg2 ┃   ↓        │
    ///       ┠──────┨            │
    ///       ┃ ...  ┃ top frame  │
    ///   ━━━━╋━━━━━━╋━━━━        │
    ///     -1┃ arg0 ┃ -2         │
    ///       ┠──────┨            │
    ///     -2┃ arg1 ┃
    ///       ┠──────┨
    ///     -3┃ arg2 ┃
    ///       ┠──────┨
    ///       ┃ ...  ┃
    /// </summary>
    internal class Environment
    {
        private static int s_id;

        private int id;
        private StackFrame topFrame;
        private readonly List<StackFrame> trace;
        public readonly List<IColumnBuffer> heapBuffers;

        public Environment()
        {
            this.id = Interlocked.Increment(ref s_id) - 1;
            this.trace = new();
            this.heapBuffers = new();
        }

        public void PushStackFrame(StackFrame stackFrame)
        {
            stackFrame.traceDepth = trace.Count;
            trace.Add(topFrame = stackFrame);
        }

        public void PopStackFrame(out StackFrame stackFrame)
        {
            CheckTopFrame();

            ; (stackFrame = topFrame).Clear();

            trace.RemoveAt(trace.Count - 1);
            topFrame = trace.Count == 0 ? null : trace[^1];

            if (topFrame is null)
            {
                foreach (var buffer in heapBuffers)
                {
                    if (buffer.Count != 0)
                    {
                        throw new($"{buffer.Count} leak(s) in {buffer}");
                    }
                }
            }
        }

        private ColumnBuffer<T> GetBuffer<T>()
        {
            var token = IdTokens.AsType<T>.GetToken(this);
            return (ColumnBuffer<T>)heapBuffers[token];
        }

        public void PushArgumentT<T>(T value)
        {
            CheckTopFrame();

            if (typeof(T).IsValueType)
            {
                var heap = GetBuffer<T>();
                heap.Push(value, out var token);

                var handler = new RuntimeArgument()
                {
                    reference = heap,
                    subkey = token,
                    parameterType = typeof(T)
                };

                topFrame.Push(handler);
            }
            else
            {
                var handler = new RuntimeArgument()
                {
                    reference = value,
                    subkey = default,
                    parameterType = typeof(T)
                };

                topFrame.Push(handler);
            }
        }

        public void PushArgument<T>(T value, ValueSignatureBump bump = default) where T : struct
        {
            CheckTopFrame();

            var heap = GetBuffer<T>();
            heap.Push(value, out var token);

            var handler = new RuntimeArgument()
            {
                reference = heap,
                subkey = token,
                parameterType = typeof(T)
            };

            topFrame.Push(handler);
        }

        public void PushArgument<T>(T value) where T : class
        {
            CheckTopFrame();

            var handler = new RuntimeArgument()
            {
                reference = value,
                subkey = default,
                parameterType = typeof(T)
            };

            topFrame.Push(handler);
        }

        public T GetArgument<T>(int traceOffset, int stackOffset)
        {
            if (traceOffset >= 0 || -traceOffset > trace.Count)
            {
                throw new IndexOutOfRangeException($"trace out of range: {traceOffset}");
            }

            var frame = trace[trace.Count + traceOffset];
            var argument = frame.GetArgumentAt(stackOffset);
            return argument.GetValue<T>();
        }

        [Conditional("DEBUG")]
        private void CheckTopFrame()
        {
            if (topFrame == null)
            {
                throw new System.Exception("stack frame empty.");
            }
        }

        private struct IdTokens
        {
            internal static class AsType<T>
            {
                static int[] indices = new int[1] { -1 };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static int GetToken(Environment inst)
                {
                    var id = inst.id;
                    EnsureSize(ref indices, id + 1, -1);
                    ref var idx = ref indices[id];
                    if (idx == -1)
                    {
                        idx = inst.heapBuffers.Count;
                        inst.heapBuffers.Add(new ColumnBuffer<T>());
                    }

                    return idx;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnsureSize(ref int[] arr, int size, int defaultValue = -1)
            {
                var len = arr.Length;
                if (len >= size)
                {
                    return;
                }

                // extend with default value
                var newArr = ArrayPool<int>.Shared.Rent(size);
                ArrayPool<int>.Shared.Return(arr);
                Array.Copy(arr, newArr, arr.Length);
                Array.Fill(newArr, defaultValue, len, newArr.Length - len);
                arr = newArr;
            }
        }
    }

    public class ValueSignatureBump
    {
        private ValueSignatureBump() => throw null;
    }
}
