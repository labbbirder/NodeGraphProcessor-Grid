using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    public static class RuntimeConverter
    {
        private static MethodInfo miCastHelper_1;
        private static MethodInfo miCastHelper_0;
        private delegate bool ObjectCaster_1<TTo>(object source, out TTo target);
        private delegate bool ObjectCaster_0(object source, Type targetType, out object target);

        private static readonly object[] numericConversions = new object[16 * 16];
        private static readonly Dictionary<(Type, Type), object> overriddenConversions = new();

        // generic instance methods of TryCastImpl<,>
        private static readonly Dictionary<(Type, Type), object> finalCasters_1 = new();
        private static readonly Dictionary<(Type, Type), object> finalCasters_0 = new();

        private static readonly HashSet<(Type, Type)> disallowedCasters = new();

        static RuntimeConverter()
        {
            RegisterNumericConversion<char, bool>((v) => v != 0);
            RegisterNumericConversion<char, byte>((v) => (byte)v);
            RegisterNumericConversion<char, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<char, short>((v) => (short)v);
            RegisterNumericConversion<char, ushort>((v) => (ushort)v);
            RegisterNumericConversion<char, int>((v) => (int)v);
            RegisterNumericConversion<char, uint>((v) => (uint)v);
            RegisterNumericConversion<char, long>((v) => (long)v);
            RegisterNumericConversion<char, ulong>((v) => (ulong)v);
            RegisterNumericConversion<char, decimal>((v) => (decimal)v);

            RegisterNumericConversion<byte, bool>((v) => v != 0);
            RegisterNumericConversion<byte, char>((v) => (char)v);
            RegisterNumericConversion<byte, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<byte, short>((v) => (short)v);
            RegisterNumericConversion<byte, ushort>((v) => (ushort)v);
            RegisterNumericConversion<byte, int>((v) => (int)v);
            RegisterNumericConversion<byte, uint>((v) => (uint)v);
            RegisterNumericConversion<byte, long>((v) => (long)v);
            RegisterNumericConversion<byte, ulong>((v) => (ulong)v);
            RegisterNumericConversion<byte, double>((v) => (double)v);
            RegisterNumericConversion<byte, float>((v) => (float)v);
            RegisterNumericConversion<byte, decimal>((v) => (decimal)v);

            RegisterNumericConversion<sbyte, bool>((v) => v != 0);
            RegisterNumericConversion<sbyte, char>((v) => (char)v);
            RegisterNumericConversion<sbyte, byte>((v) => (byte)v);
            RegisterNumericConversion<sbyte, short>((v) => (short)v);
            RegisterNumericConversion<sbyte, ushort>((v) => (ushort)v);
            RegisterNumericConversion<sbyte, int>((v) => (int)v);
            RegisterNumericConversion<sbyte, uint>((v) => (uint)v);
            RegisterNumericConversion<sbyte, long>((v) => (long)v);
            RegisterNumericConversion<sbyte, ulong>((v) => (ulong)v);
            RegisterNumericConversion<sbyte, double>((v) => (double)v);
            RegisterNumericConversion<sbyte, float>((v) => (float)v);
            RegisterNumericConversion<sbyte, decimal>((v) => (decimal)v);

            RegisterNumericConversion<short, bool>((v) => v != 0);
            RegisterNumericConversion<short, char>((v) => (char)v);
            RegisterNumericConversion<short, byte>((v) => (byte)v);
            RegisterNumericConversion<short, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<short, ushort>((v) => (ushort)v);
            RegisterNumericConversion<short, int>((v) => (int)v);
            RegisterNumericConversion<short, uint>((v) => (uint)v);
            RegisterNumericConversion<short, long>((v) => (long)v);
            RegisterNumericConversion<short, ulong>((v) => (ulong)v);
            RegisterNumericConversion<short, double>((v) => (double)v);
            RegisterNumericConversion<short, float>((v) => (float)v);
            RegisterNumericConversion<short, decimal>((v) => (decimal)v);

            RegisterNumericConversion<ushort, bool>((v) => v != 0);
            RegisterNumericConversion<ushort, char>((v) => (char)v);
            RegisterNumericConversion<ushort, byte>((v) => (byte)v);
            RegisterNumericConversion<ushort, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<ushort, short>((v) => (short)v);
            RegisterNumericConversion<ushort, int>((v) => (int)v);
            RegisterNumericConversion<ushort, uint>((v) => (uint)v);
            RegisterNumericConversion<ushort, long>((v) => (long)v);
            RegisterNumericConversion<ushort, ulong>((v) => (ulong)v);
            RegisterNumericConversion<ushort, double>((v) => (double)v);
            RegisterNumericConversion<ushort, float>((v) => (float)v);
            RegisterNumericConversion<ushort, decimal>((v) => (decimal)v);

            RegisterNumericConversion<int, bool>((v) => v != 0);
            RegisterNumericConversion<int, char>((v) => (char)v);
            RegisterNumericConversion<int, byte>((v) => (byte)v);
            RegisterNumericConversion<int, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<int, short>((v) => (short)v);
            RegisterNumericConversion<int, ushort>((v) => (ushort)v);
            RegisterNumericConversion<int, uint>((v) => (uint)v);
            RegisterNumericConversion<int, long>((v) => (long)v);
            RegisterNumericConversion<int, ulong>((v) => (ulong)v);
            RegisterNumericConversion<int, double>((v) => (double)v);
            RegisterNumericConversion<int, float>((v) => (float)v);
            RegisterNumericConversion<int, decimal>((v) => (decimal)v);

            RegisterNumericConversion<uint, bool>((v) => v != 0);
            RegisterNumericConversion<uint, char>((v) => (char)v);
            RegisterNumericConversion<uint, byte>((v) => (byte)v);
            RegisterNumericConversion<uint, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<uint, short>((v) => (short)v);
            RegisterNumericConversion<uint, ushort>((v) => (ushort)v);
            RegisterNumericConversion<uint, int>((v) => (int)v);
            RegisterNumericConversion<uint, long>((v) => (long)v);
            RegisterNumericConversion<uint, ulong>((v) => (ulong)v);
            RegisterNumericConversion<uint, double>((v) => (double)v);
            RegisterNumericConversion<uint, float>((v) => (float)v);
            RegisterNumericConversion<uint, decimal>((v) => (decimal)v);

            RegisterNumericConversion<long, bool>((v) => v != 0);
            RegisterNumericConversion<long, char>((v) => (char)v);
            RegisterNumericConversion<long, byte>((v) => (byte)v);
            RegisterNumericConversion<long, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<long, short>((v) => (short)v);
            RegisterNumericConversion<long, ushort>((v) => (ushort)v);
            RegisterNumericConversion<long, int>((v) => (int)v);
            RegisterNumericConversion<long, uint>((v) => (uint)v);
            RegisterNumericConversion<long, ulong>((v) => (ulong)v);
            RegisterNumericConversion<long, double>((v) => (double)v);
            RegisterNumericConversion<long, float>((v) => (float)v);
            RegisterNumericConversion<long, decimal>((v) => (decimal)v);

            RegisterNumericConversion<ulong, bool>((v) => v != 0);
            RegisterNumericConversion<ulong, char>((v) => (char)v);
            RegisterNumericConversion<ulong, byte>((v) => (byte)v);
            RegisterNumericConversion<ulong, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<ulong, short>((v) => (short)v);
            RegisterNumericConversion<ulong, ushort>((v) => (ushort)v);
            RegisterNumericConversion<ulong, int>((v) => (int)v);
            RegisterNumericConversion<ulong, uint>((v) => (uint)v);
            RegisterNumericConversion<ulong, long>((v) => (long)v);
            RegisterNumericConversion<ulong, double>((v) => (double)v);
            RegisterNumericConversion<ulong, float>((v) => (float)v);
            RegisterNumericConversion<ulong, decimal>((v) => (decimal)v);

            RegisterNumericConversion<float, bool>((v) => v != 0);
            RegisterNumericConversion<float, char>((v) => (char)v);
            RegisterNumericConversion<float, byte>((v) => (byte)v);
            RegisterNumericConversion<float, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<float, short>((v) => (short)v);
            RegisterNumericConversion<float, ushort>((v) => (ushort)v);
            RegisterNumericConversion<float, int>((v) => (int)v);
            RegisterNumericConversion<float, uint>((v) => (uint)v);
            RegisterNumericConversion<float, long>((v) => (long)v);
            RegisterNumericConversion<float, ulong>((v) => (ulong)v);
            RegisterNumericConversion<float, double>((v) => (double)v);
            RegisterNumericConversion<float, decimal>((v) => (decimal)v);

            RegisterNumericConversion<double, bool>((v) => v != 0);
            RegisterNumericConversion<double, char>((v) => (char)v);
            RegisterNumericConversion<double, byte>((v) => (byte)v);
            RegisterNumericConversion<double, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<double, short>((v) => (short)v);
            RegisterNumericConversion<double, ushort>((v) => (ushort)v);
            RegisterNumericConversion<double, int>((v) => (int)v);
            RegisterNumericConversion<double, uint>((v) => (uint)v);
            RegisterNumericConversion<double, long>((v) => (long)v);
            RegisterNumericConversion<double, ulong>((v) => (ulong)v);
            RegisterNumericConversion<double, float>((v) => (float)v);
            RegisterNumericConversion<double, decimal>((v) => (decimal)v);

            RegisterNumericConversion<decimal, bool>((v) => v != 0);
            RegisterNumericConversion<decimal, char>((v) => (char)v);
            RegisterNumericConversion<decimal, byte>((v) => (byte)v);
            RegisterNumericConversion<decimal, sbyte>((v) => (sbyte)v);
            RegisterNumericConversion<decimal, short>((v) => (short)v);
            RegisterNumericConversion<decimal, ushort>((v) => (ushort)v);
            RegisterNumericConversion<decimal, int>((v) => (int)v);
            RegisterNumericConversion<decimal, uint>((v) => (uint)v);
            RegisterNumericConversion<decimal, long>((v) => (long)v);
            RegisterNumericConversion<decimal, ulong>((v) => (ulong)v);
            RegisterNumericConversion<decimal, float>((v) => (float)v);
            RegisterNumericConversion<decimal, double>((v) => (double)v);
        }

        private static void RegisterNumericConversion<T1, T2>(Func<T1, T2> castFunc)
        {
            var code1 = Type.GetTypeCode(typeof(T1));
            var code2 = Type.GetTypeCode(typeof(T2));
            var code = (int)code1 * 16 + (int)code2;
            numericConversions[code] = castFunc;
        }

        public static void RegisterDisallowedConversion(Type tfrom, Type tto)
        {
            disallowedCasters.Add((tfrom, tto));
        }

        public static void RegisterDisallowedConversion<TFrom, TTo>()
        {
            RegisterDisallowedConversion(typeof(TFrom), typeof(TTo));
        }

        private static ObjectCaster_1<TTo> ConvertHelper_1<TFrom, TTo>()
        {
            return Impl;
            static bool Impl(object value, out TTo target)
            {
                if (value is TFrom source)
                {
                    return TryConvertImpl(source, out target);
                }
                else
                {
                    throw new("Runtime error");
                }
            }
        }

        private static ObjectCaster_0 ConvertHelper_0<TFrom, TTo>()
        {
            return Impl;
            static bool Impl(object value, Type targetType, out object target)
            {
                if (value is TFrom source)
                {
                    bool success = TryConvertImpl(source, out TTo targetOrigin);
                    target = targetOrigin;
                    return success;
                }
                else
                {
                    throw new("Runtime error");
                }
            }
        }

        private static bool TryConvertImpl<TFrom, TTo>(TFrom source, out TTo target)
        {
            if (typeof(TFrom) == typeof(TTo))
            {
                target = Unsafe.As<TFrom, TTo>(ref source);
                return true;
            }

            if (disallowedCasters.Contains((typeof(TFrom), typeof(TTo))))
            {
                target = default;
                return false;
            }

            if (typeof(TTo) == typeof(string))
            {
                var str = source?.ToString();
                target = Unsafe.As<string, TTo>(ref str);
                return true;
            }

            if (typeof(TTo) == typeof(object))
            {
                object obj = source;
                target = Unsafe.As<object, TTo>(ref obj);
                return true;
            }

            if (source is null)// is reference type and null
            {
                if (default(TTo) is null)
                {
                    target = default;
                    return true;
                }
                else // casting from null to value type
                {
                    target = default;
                    return true;
                }
            }
            else // is value type or not-null reference
            {
                if (default(TFrom) is null) // is not-null reference
                {
                    if (default(TTo) is null) // cast reference to another reference
                    {
                        if (typeof(TFrom).IsSubclassOf(typeof(TTo))) // cast from subtype
                        {
                            target = Unsafe.As<TFrom, TTo>(ref source);
                            return true;
                        }
                        else
                        {
                            return TryCustomConvert(source, out target);
                        }
                    }
                    else // cast reference to value type
                    {
                        return TryCustomConvert(source, out target);
                    }
                }
                else // is value type
                {
                    if (default(TTo) is null) // cast value type to another reference
                    {
                        return TryCustomConvert(source, out target);
                    }
                    else // cast value type to value type
                    {
                        var code1 = Type.GetTypeCode(typeof(TFrom));
                        var code2 = Type.GetTypeCode(typeof(TTo));
                        var code = (int)code1 * 16 + (int)code2;

                        if (code < 16 * 16 && numericConversions[code] is Func<TFrom, TTo> castFunc) // numeric cast
                        {
                            target = castFunc(source);
                            return true;
                        }
                        else
                        {
                            return TryCustomConvert(source, out target);
                        }
                    }
                }
            }
        }

        private static object PrepareCustomConversionImpl(Type tfrom, Type tto, Type casterType = null)
        {
            const string ExplicitKeyword = "op_Explicit";
            const string ImplicitKeyword = "op_Implicit";
            const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var handlePtr1 = (nint)tfrom.TypeHandle.Value;
            var handlePtr2 = (nint)tto.TypeHandle.Value;
            var key = handlePtr1 > handlePtr2 ? (tto, tfrom) : (tfrom, tto);
            if (!overriddenConversions.TryGetValue(key, out var conversion))
            {
                foreach (var method in tfrom.GetMethods(Flags))
                {
                    if (method.Name is ExplicitKeyword or ImplicitKeyword)
                    {
                        if (method.ReturnType == tto)
                        {
                            casterType ??= typeof(Func<,>).MakeGenericType(tfrom, tto);
                            overriddenConversions[key] = conversion = method.CreateDelegate(casterType);
                            return conversion;
                        }
                    }
                }

                foreach (var method in tto.GetMethods(Flags))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1) continue;

                    if (parameters[0].ParameterType == tfrom)
                    {
                        casterType ??= typeof(Func<,>).MakeGenericType(tfrom, tto);
                        overriddenConversions[key] = conversion = method.CreateDelegate(casterType);
                        return conversion;
                    }
                }

                overriddenConversions[key] = conversion = null;
            }

            return conversion;
        }

        private static object PrepareCustomConversion<TFrom, TTo>()
        {
            return PrepareCustomConversionImpl(typeof(TFrom), typeof(TTo), typeof(Func<TFrom, TTo>));
        }

        private static bool TryCustomConvert<TFrom, TTo>(TFrom value, out TTo target)
        {
            var conversion = PrepareCustomConversion<TFrom, TTo>();
            if (conversion is Func<TFrom, TTo> castFunc)
            {
                target = castFunc(value);
                return true;
            }

            target = default;
            return false;
        }

        public static bool TryConvert<TFrom, TTo>(TFrom source, out TTo target)
        {
            return TryConvertImpl(source, out target);
        }

        public static bool TryConvert<TTo>(object source, out TTo target)
        {
            if (source is null)
            {
                target = default;
                return true;
            }

            var sourceType = source.GetType();
            var handlePtr1 = (nint)sourceType.TypeHandle.Value;
            var handlePtr2 = (nint)typeof(TTo).TypeHandle.Value;
            var key = handlePtr1 > handlePtr2 ? (typeof(TTo), sourceType) : (sourceType, typeof(TTo));
            if (!finalCasters_1.TryGetValue(key, out var caster))
            {
                miCastHelper_1 ??= typeof(RuntimeConverter).GetMethod(nameof(ConvertHelper_1), BindingFlags.Static | BindingFlags.NonPublic);
                finalCasters_1[key] = caster = miCastHelper_1.MakeGenericMethod(sourceType, typeof(TTo)).Invoke(null, Array.Empty<object>());
            }

            return (caster as ObjectCaster_1<TTo>).Invoke(source, out target);
        }

        public static bool TryConvert(object source, Type targetType, out object target)
        {
            if (source is null)
            {
                target = default;
                return true;
            }

            var sourceType = source.GetType();

            var handlePtr1 = (nint)sourceType.TypeHandle.Value;
            var handlePtr2 = (nint)targetType.TypeHandle.Value;
            var key = handlePtr1 > handlePtr2 ? (targetType, sourceType) : (sourceType, targetType);
            if (!finalCasters_0.TryGetValue(key, out var caster))
            {
                miCastHelper_0 ??= typeof(RuntimeConverter).GetMethod(nameof(ConvertHelper_0), BindingFlags.Static | BindingFlags.NonPublic);
                finalCasters_0[key] = caster = miCastHelper_0.MakeGenericMethod(sourceType, targetType).Invoke(null, Array.Empty<object>());
            }

            return (caster as ObjectCaster_0).Invoke(source, targetType, out target);
        }

        public static bool CanConvert(Type tfrom, Type tto)
        {
            if (tfrom == tto)
            {
                return true;
            }

            if (disallowedCasters.Contains((tfrom, tto)))
            {
                return false;
            }

            if (tto == typeof(string))
            {
                return true;
            }

            if (tto == typeof(object))
            {
                return true;
            }

            if (tto.IsAssignableFrom(tfrom))
            {
                return true;
            }

            var code1 = Type.GetTypeCode(tfrom);
            var code2 = Type.GetTypeCode(tto);
            var code = (int)code1 * 16 + (int)code2;
            if (code < 16 * 16 && numericConversions[code] is { }) // numeric cast
            {
                return true;
            }

            var conversion = PrepareCustomConversionImpl(tfrom, tto);
            if (conversion is { })
            {
                return true;
            }

            return false;
        }

        public static TTo Convert<TFrom, TTo>(TFrom source)
        {
            if (TryConvert<TFrom, TTo>(source, out var target))
            {
                return target;

            }
            else
            {
                throw new("Invalid conversion");
            }
        }

        public static TTo Convert<TTo>(object source)
        {
            if (TryConvert<TTo>(source, out var target))
            {
                return target;
            }
            else
            {
                throw new("Invalid conversion");
            }
        }

        public static object Convert(object source, Type targetType)
        {
            if (TryConvert(source, targetType, out var target))
            {
                return target;
            }
            else
            {
                throw new("Invalid conversion");
            }
        }

        public static bool CanConvert<TFrom, TTo>()
        {
            return CanConvert(typeof(TFrom), typeof(TTo));
        }
    }
}

