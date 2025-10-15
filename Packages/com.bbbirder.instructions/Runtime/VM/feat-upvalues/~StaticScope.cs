using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BBBirder.Instructions
{
    internal class StaticScope
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct Primitive64
        {
            [FieldOffset(0)] public bool boolean;
            [FieldOffset(0)] public sbyte i8;
            [FieldOffset(0)] public byte u8;
            [FieldOffset(0)] public short i16;
            [FieldOffset(0)] public ushort u16;
            [FieldOffset(0)] public int i32;
            [FieldOffset(0)] public uint u32;
            [FieldOffset(0)] public long i64;
            [FieldOffset(0)] public ulong u64;
            [FieldOffset(0)] public float f32;
            [FieldOffset(0)] public double f64;
            [FieldOffset(8)] public TypeCode typeCode;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Get<T>()
            {
                CheckCastType<T>(false);
                if (Type.GetTypeCode(typeof(T)) == typeCode)
                {
                    // INTRINSIC:
                    //   ldarg.0
                    //   ret
                    return Unsafe.As<Primitive64, T>(ref this);
                }
                else
                {
                    throw new($"Type {typeof(T)} cannot write to {typeCode}");
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool IsTypeValid<T>(bool strict = true)
            {
                var code = Type.GetTypeCode(typeof(T));

                if (strict)
                {
                    return typeCode == code;
                }
                else
                {
                    return 3 <= (int)code && (int)code <= 14;
                }
            }

            [Conditional("DEBUG")]
            void CheckCastType<T>(bool strict = true)
            {
                if (!IsTypeValid<T>(strict))
                {
                    throw new($"Type {typeof(T)} is not a valid primate number type");
                }
            }
        }

        internal int traceDepth;
        internal bool captureUpvalues = false;
        ParamterCategoryFlag mutatedCategoryFlags;
        internal Stack<IDictionary> mutated = new();
        Dictionary<string, IDictionary> lut = new();
        Dictionary<string, object> klassCategory;
        Dictionary<string, Primitive64> numberCategory;// frequently used
        GcFreeParameters structCategory;// slightly slower

        internal void GetParametersInType<T>(List<string> results)
        {
            var targetType = typeof(T);
            foreach (var (key, dict) in lut)
            {
                var parameterType = dict.GetType().GenericTypeArguments[1];
                if (targetType.IsAssignableFrom(parameterType))
                {
                    results.Add(key);
                }
            }
        }

        /// <summary>
        /// Set a reference value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value)
        {
            CheckRefValue(value);

            ref var container = ref klassCategory;
            var flag = ParamterCategoryFlag.REFERENCE;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = value;
            lut[key] = container;
        }

        /// <summary>
        /// Set a bool value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, bool value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { boolean = value, typeCode = TypeCode.Boolean };
            lut[key] = container;
        }

        /// <summary>
        /// Set a byte value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, byte value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { u8 = value, typeCode = TypeCode.Byte };
            lut[key] = container;
        }

        /// <summary>
        /// Set a sbyte value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, sbyte value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { i8 = value, typeCode = TypeCode.SByte };
            lut[key] = container;
        }

        /// <summary>
        /// Set a short value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, short value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { i16 = value, typeCode = TypeCode.Int16 };
            lut[key] = container;
        }

        /// <summary>
        /// Set a ushort value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, ushort value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { u16 = value, typeCode = TypeCode.UInt16 };
            lut[key] = container;
        }

        /// <summary>
        /// Set an Int32 value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, int value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }
            container[key] = new Primitive64() { i32 = value, typeCode = TypeCode.Int32 };
            lut[key] = container;
        }

        /// <summary>
        /// Set a uint value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, uint value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { u32 = value, typeCode = TypeCode.UInt32 };
            lut[key] = container;
        }

        /// <summary>
        /// Set a long value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, long value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { i64 = value, typeCode = TypeCode.Int64 };
            lut[key] = container;
        }

        /// <summary>
        /// Set a ulong value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, ulong value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { u64 = value, typeCode = TypeCode.UInt64 };
            lut[key] = container;
        }

        /// <summary>
        /// Set an Single value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, float value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { f32 = value, typeCode = TypeCode.Single };
            lut[key] = container;
        }

        /// <summary>
        /// Set a double value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, double value)
        {
            ref var container = ref numberCategory;
            var flag = ParamterCategoryFlag.NUMBER;

            container ??= new();
            if ((mutatedCategoryFlags | flag) != mutatedCategoryFlags)
            {
                mutatedCategoryFlags |= flag;
                mutated.Push(container);
            }

            container[key] = new Primitive64() { f64 = value, typeCode = TypeCode.Double };
            lut[key] = container;
        }

        /// <summary>
        /// Set a struct value with type `<typeparamref name="TValue"/>`
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue<TValue>(string key, TValue value) where TValue : struct
        {
            structCategory ??= new();
            structCategory.SetValue(key, value, out var container);
            lut[key] = container;
        }

        public T GetValue<T>(string key)
        {
            if (lut.TryGetValue(key, out var container))
            {
                if (container is Dictionary<string, T> dict)
                {
                    if (dict.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        ThrowParameterNotFoundException(key);
                        return default;
                    }
                }
                else if (container is Dictionary<string, Primitive64> dictN)
                {
                    if (dictN.TryGetValue(key, out var n64))
                    {
                        return n64.Get<T>();
                    }
                    else
                    {
                        ThrowParameterNotFoundException(key);
                        return default;
                    }
                }
                else
                {
                    // need cast
                    if (container.Contains(key))
                    {
                        return (T)container[key];
                    }
                    else
                    {
                        ThrowParameterNotFoundException(key);
                        return default;
                    }
                }
            }
            else
            {
                ThrowParameterNotFoundException(key);
                return default;
            }
        }

        public bool Contains(string key)
        {
            return lut.ContainsKey(key);
        }

        public void Remove(string key)
        {
            if (lut.TryGetValue(key, out var container))
            {
                container.Remove(key);
            }
        }

        public void InheritFrom(StaticScope prev)
        {
            foreach (var (key, container) in prev.lut)
            {
                if (!lut.ContainsKey(key))
                {
                    lut[key] = container;
                }
            }
        }

        public void InitParameters(GcFreeParameters parameters)
        {
            foreach (var parameterType in parameters.Types)
            {
                var dict = parameters.GetValuesWithType(parameterType);
                switch (Type.GetTypeCode(parameterType))
                {
                    case TypeCode.Boolean:
                        foreach (var (k, v) in (Dictionary<string, bool>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(bool));
                        break;
                    case TypeCode.Byte:
                        foreach (var (k, v) in (Dictionary<string, byte>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(byte));
                        break;
                    case TypeCode.SByte:
                        foreach (var (k, v) in (Dictionary<string, sbyte>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(sbyte));
                        break;
                    case TypeCode.Int16:
                        foreach (var (k, v) in (Dictionary<string, short>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(short));
                        break;
                    case TypeCode.UInt16:
                        foreach (var (k, v) in (Dictionary<string, ushort>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(ushort));
                        break;
                    case TypeCode.Int32:
                        foreach (var (k, v) in (Dictionary<string, int>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(int));
                        break;
                    case TypeCode.UInt32:
                        foreach (var (k, v) in (Dictionary<string, uint>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(uint));
                        break;
                    case TypeCode.Int64:
                        foreach (var (k, v) in (Dictionary<string, long>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(long));
                        break;
                    case TypeCode.UInt64:
                        foreach (var (k, v) in (Dictionary<string, ulong>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(ulong));
                        break;
                    case TypeCode.Single:
                        foreach (var (k, v) in (Dictionary<string, float>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(float));
                        break;
                    case TypeCode.Double:
                        foreach (var (k, v) in (Dictionary<string, double>)dict)
                        {
                            SetValue(k, v);
                        }
                        parameters.DropValuesWithType(typeof(double));
                        break;
                    default:
                        if (!parameterType.IsValueType)
                        {
                            foreach (var (k, v) in (Dictionary<string, object>)dict)
                            {
                                SetValue(k, v);
                            }
                            parameters.DropValuesWithType(parameterType);
                        }
                        break;
                } // end of switch

                // move remaining struct values from one to another
                structCategory.MoveParametersFrom(parameters);
            } // end of foreach parameter type
        }

        internal void ResetState()
        {
            traceDepth = 0;
            captureUpvalues = false;
            mutatedCategoryFlags = 0;

            while (mutated.TryPop(out var dict))
            {
                dict.Clear();
            }

            structCategory?.DeapClear();

            lut.Clear();
        }

        private void ThrowParameterNotFoundException(string parameterName)
        {
            throw new InvalidProgramException($"no parameter named {parameterName}");
        }

        [Conditional("DEBUG")]
        static void CheckRefValue(object value)
        {
            if (value != null && value.GetType().IsValueType)
            {
                throw new ArgumentException($"argument is in value type {value.GetType()}", "value");
            }
        }
    }

    [Flags]
    public enum ParamterCategoryFlag
    {
        REFERENCE = 1 << 0,
        // INT32 = 1 << 1,
        // SINGLE = 1 << 2,
        NUMBER = 1 << 3,
        STRUCT = 1 << 4,
    }

}
