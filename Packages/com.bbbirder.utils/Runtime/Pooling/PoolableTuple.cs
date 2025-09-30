using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    static partial class PoolableTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2> Get<T1, T2>(T1 arg1 ,T2 arg2)
        {
            return Storage<T1, T2>.Get(arg1 ,arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2> ToPoolable<T1, T2>(in this ValueTuple<T1, T2> tuple)
        {
            return Get(tuple.Item1 ,tuple.Item2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTuple<T1, T2> ToValueTuple<T1, T2>(this PoolableTuple<T1, T2> tuple)
        {
            return (tuple.arg1 ,tuple.arg2);
        }

        internal static class Storage<T1, T2>
        {
            static Stack<PoolableTuple<T1, T2>> stack = new();
            public static PoolableTuple<T1, T2> Get(T1 arg1, T2 arg2)
            {
                if (stack.TryPop(out var result))
                {
                    result.arg1 = arg1;
                    result.arg2 = arg2;
                    return result;
                }
                else
                {
                    return new(arg1, arg2);
                }
            }

            public static void Release(PoolableTuple<T1, T2> tuple)
            {
                tuple.arg1 = default;
                tuple.arg2 = default;
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1, T2> : IDisposable
    {
        public T1 arg1;
        public T2 arg2;

        public PoolableTuple(T1 arg1, T2 arg2)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public void Deconstruct(out T1 arg1, out T2 arg2)
        {
            arg1 = this.arg1;
            arg2 = this.arg2;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1, T2>.Release(this);
        }
    }

    static partial class PoolableTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3> Get<T1, T2, T3>(T1 arg1 ,T2 arg2 ,T3 arg3)
        {
            return Storage<T1, T2, T3>.Get(arg1 ,arg2 ,arg3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3> ToPoolable<T1, T2, T3>(in this ValueTuple<T1, T2, T3> tuple)
        {
            return Get(tuple.Item1 ,tuple.Item2 ,tuple.Item3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTuple<T1, T2, T3> ToValueTuple<T1, T2, T3>(this PoolableTuple<T1, T2, T3> tuple)
        {
            return (tuple.arg1 ,tuple.arg2 ,tuple.arg3);
        }

        internal static class Storage<T1, T2, T3>
        {
            static Stack<PoolableTuple<T1, T2, T3>> stack = new();
            public static PoolableTuple<T1, T2, T3> Get(T1 arg1, T2 arg2, T3 arg3)
            {
                if (stack.TryPop(out var result))
                {
                    result.arg1 = arg1;
                    result.arg2 = arg2;
                    result.arg3 = arg3;
                    return result;
                }
                else
                {
                    return new(arg1, arg2, arg3);
                }
            }

            public static void Release(PoolableTuple<T1, T2, T3> tuple)
            {
                tuple.arg1 = default;
                tuple.arg2 = default;
                tuple.arg3 = default;
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1, T2, T3> : IDisposable
    {
        public T1 arg1;
        public T2 arg2;
        public T3 arg3;

        public PoolableTuple(T1 arg1, T2 arg2, T3 arg3)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
        }

        public void Deconstruct(out T1 arg1, out T2 arg2, out T3 arg3)
        {
            arg1 = this.arg1;
            arg2 = this.arg2;
            arg3 = this.arg3;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1, T2, T3>.Release(this);
        }
    }

    static partial class PoolableTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4> Get<T1, T2, T3, T4>(T1 arg1 ,T2 arg2 ,T3 arg3 ,T4 arg4)
        {
            return Storage<T1, T2, T3, T4>.Get(arg1 ,arg2 ,arg3 ,arg4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4> ToPoolable<T1, T2, T3, T4>(in this ValueTuple<T1, T2, T3, T4> tuple)
        {
            return Get(tuple.Item1 ,tuple.Item2 ,tuple.Item3 ,tuple.Item4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTuple<T1, T2, T3, T4> ToValueTuple<T1, T2, T3, T4>(this PoolableTuple<T1, T2, T3, T4> tuple)
        {
            return (tuple.arg1 ,tuple.arg2 ,tuple.arg3 ,tuple.arg4);
        }

        internal static class Storage<T1, T2, T3, T4>
        {
            static Stack<PoolableTuple<T1, T2, T3, T4>> stack = new();
            public static PoolableTuple<T1, T2, T3, T4> Get(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                if (stack.TryPop(out var result))
                {
                    result.arg1 = arg1;
                    result.arg2 = arg2;
                    result.arg3 = arg3;
                    result.arg4 = arg4;
                    return result;
                }
                else
                {
                    return new(arg1, arg2, arg3, arg4);
                }
            }

            public static void Release(PoolableTuple<T1, T2, T3, T4> tuple)
            {
                tuple.arg1 = default;
                tuple.arg2 = default;
                tuple.arg3 = default;
                tuple.arg4 = default;
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1, T2, T3, T4> : IDisposable
    {
        public T1 arg1;
        public T2 arg2;
        public T3 arg3;
        public T4 arg4;

        public PoolableTuple(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
        }

        public void Deconstruct(out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4)
        {
            arg1 = this.arg1;
            arg2 = this.arg2;
            arg3 = this.arg3;
            arg4 = this.arg4;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1, T2, T3, T4>.Release(this);
        }
    }

    static partial class PoolableTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>(T1 arg1 ,T2 arg2 ,T3 arg3 ,T4 arg4 ,T5 arg5)
        {
            return Storage<T1, T2, T3, T4, T5>.Get(arg1 ,arg2 ,arg3 ,arg4 ,arg5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4, T5> ToPoolable<T1, T2, T3, T4, T5>(in this ValueTuple<T1, T2, T3, T4, T5> tuple)
        {
            return Get(tuple.Item1 ,tuple.Item2 ,tuple.Item3 ,tuple.Item4 ,tuple.Item5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTuple<T1, T2, T3, T4, T5> ToValueTuple<T1, T2, T3, T4, T5>(this PoolableTuple<T1, T2, T3, T4, T5> tuple)
        {
            return (tuple.arg1 ,tuple.arg2 ,tuple.arg3 ,tuple.arg4 ,tuple.arg5);
        }

        internal static class Storage<T1, T2, T3, T4, T5>
        {
            static Stack<PoolableTuple<T1, T2, T3, T4, T5>> stack = new();
            public static PoolableTuple<T1, T2, T3, T4, T5> Get(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                if (stack.TryPop(out var result))
                {
                    result.arg1 = arg1;
                    result.arg2 = arg2;
                    result.arg3 = arg3;
                    result.arg4 = arg4;
                    result.arg5 = arg5;
                    return result;
                }
                else
                {
                    return new(arg1, arg2, arg3, arg4, arg5);
                }
            }

            public static void Release(PoolableTuple<T1, T2, T3, T4, T5> tuple)
            {
                tuple.arg1 = default;
                tuple.arg2 = default;
                tuple.arg3 = default;
                tuple.arg4 = default;
                tuple.arg5 = default;
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1, T2, T3, T4, T5> : IDisposable
    {
        public T1 arg1;
        public T2 arg2;
        public T3 arg3;
        public T4 arg4;
        public T5 arg5;

        public PoolableTuple(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
        }

        public void Deconstruct(out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4, out T5 arg5)
        {
            arg1 = this.arg1;
            arg2 = this.arg2;
            arg3 = this.arg3;
            arg4 = this.arg4;
            arg5 = this.arg5;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1, T2, T3, T4, T5>.Release(this);
        }
    }

    static partial class PoolableTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4, T5, T6> Get<T1, T2, T3, T4, T5, T6>(T1 arg1 ,T2 arg2 ,T3 arg3 ,T4 arg4 ,T5 arg5 ,T6 arg6)
        {
            return Storage<T1, T2, T3, T4, T5, T6>.Get(arg1 ,arg2 ,arg3 ,arg4 ,arg5 ,arg6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PoolableTuple<T1, T2, T3, T4, T5, T6> ToPoolable<T1, T2, T3, T4, T5, T6>(in this ValueTuple<T1, T2, T3, T4, T5, T6> tuple)
        {
            return Get(tuple.Item1 ,tuple.Item2 ,tuple.Item3 ,tuple.Item4 ,tuple.Item5 ,tuple.Item6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTuple<T1, T2, T3, T4, T5, T6> ToValueTuple<T1, T2, T3, T4, T5, T6>(this PoolableTuple<T1, T2, T3, T4, T5, T6> tuple)
        {
            return (tuple.arg1 ,tuple.arg2 ,tuple.arg3 ,tuple.arg4 ,tuple.arg5 ,tuple.arg6);
        }

        internal static class Storage<T1, T2, T3, T4, T5, T6>
        {
            static Stack<PoolableTuple<T1, T2, T3, T4, T5, T6>> stack = new();
            public static PoolableTuple<T1, T2, T3, T4, T5, T6> Get(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                if (stack.TryPop(out var result))
                {
                    result.arg1 = arg1;
                    result.arg2 = arg2;
                    result.arg3 = arg3;
                    result.arg4 = arg4;
                    result.arg5 = arg5;
                    result.arg6 = arg6;
                    return result;
                }
                else
                {
                    return new(arg1, arg2, arg3, arg4, arg5, arg6);
                }
            }

            public static void Release(PoolableTuple<T1, T2, T3, T4, T5, T6> tuple)
            {
                tuple.arg1 = default;
                tuple.arg2 = default;
                tuple.arg3 = default;
                tuple.arg4 = default;
                tuple.arg5 = default;
                tuple.arg6 = default;
                stack.Push(tuple);
            }
        }
    }

    public class PoolableTuple<T1, T2, T3, T4, T5, T6> : IDisposable
    {
        public T1 arg1;
        public T2 arg2;
        public T3 arg3;
        public T4 arg4;
        public T5 arg5;
        public T6 arg6;

        public PoolableTuple(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
        }

        public void Deconstruct(out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4, out T5 arg5, out T6 arg6)
        {
            arg1 = this.arg1;
            arg2 = this.arg2;
            arg3 = this.arg3;
            arg4 = this.arg4;
            arg5 = this.arg5;
            arg6 = this.arg6;
        }

        public void Dispose()
        {
            PoolableTuple.Storage<T1, T2, T3, T4, T5, T6>.Release(this);
        }
    }

}
