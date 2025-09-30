using System;
using System.Runtime.CompilerServices;

namespace BBBirder
{
    /// <summary>
    /// CRC32 Computer
    /// <example>
    /// <code>
    /// <![CDATA[
    ///     // create in-line
    ///     Crc32.Compute(bytes);
    /// 
    ///     // create in update mode
    ///     uint crc = Crc32.BeginContext()
    ///         .Update(bytes1)
    ///         .UpdateByte(127)
    ///         .Update(bytes2)
    ///         .ToDigest();
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public static class Crc32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Crc32BuildContext BeginContext()
        {
            var ctx = new Crc32BuildContext();
            ctx.Init();
            return ctx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(byte[] bytes, int offset, int size)
        {
            var ctx = new Crc32BuildContext();
            ctx.Init();
            ctx.Update(bytes, offset, size);
            return ctx.ToDigest();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(ReadOnlySpan<byte> bytes)
        {
            var ctx = new Crc32BuildContext();
            ctx.Init();
            ctx.Update(bytes);
            return ctx.ToDigest();
        }

        public struct Crc32BuildContext
        {
            static readonly uint[] s_lut;
            static Crc32BuildContext()
            {
                s_lut = new uint[256];
                for (uint num = 0u; num < 256; num++)
                {
                    uint num2 = num;
                    for (int i = 0; i < 8; i++)
                    {
                        num2 = ((num2 & 1) == 0) ? (num2 >> 1) : ((num2 >> 1) ^ 0xEDB88320u);
                    }

                    s_lut[num] = num2;
                }
            }

            private uint _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Init()
            {
                _value = uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Crc32BuildContext UpdateByte(byte b)
            {
                _value = s_lut[(byte)_value ^ b] ^ (_value >> 8);
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Crc32BuildContext Update(byte[] bytes, int offset, int size)
            {
                for (int i = 0; i < size; i++)
                {
                    _value = s_lut[(byte)_value ^ bytes[offset + i]] ^ (_value >> 8);
                }

                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Crc32BuildContext Update(ReadOnlySpan<byte> bytes)
            {
                var size = bytes.Length;
                for (int i = 0; i < size; i++)
                {
                    _value = s_lut[(byte)_value ^ bytes[i]] ^ (_value >> 8);
                }

                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint ToDigest()
            {
                return _value ^ 0xFFFFFFFFu;
            }
        }
    }
}

