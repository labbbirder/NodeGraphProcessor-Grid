using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BBBirder.FlowField
{
    public unsafe static class NativeAPI
    {
        public static nuint Malloc(int bytes, bool zerofill = true)
        {
#if NET6_0_OR_GREATER
            return zerofill ? (nuint)NativeMemory.AllocZeroed((nuint)bytes)
                : (nuint)NativeMemory.Alloc((nuint)bytes);
#else
            var ptr = (nuint)Marshal.AllocHGlobal(bytes).ToPointer();
            if (zerofill)
            {
                Unsafe.InitBlockUnaligned((void*)ptr, 0, (uint)bytes);
            }

            return ptr;
#endif
        }

        public static void Free(nuint ptr)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free((void*)ptr);
#else
            Marshal.FreeHGlobal(new IntPtr((long)ptr));
#endif
        }

        public static nuint MallocAligned(int bytes, int alignment, bool zerofill = true)
        {
#if NET6_0_OR_GREATER
            return (nuint)NativeMemory.AlignedAlloc((nuint)(bytes / alignment), (nuint)alignment);
#else
            var ptr = AlignedMalloc_Compatible(bytes, alignment);
#endif
            if (zerofill)
            {
                // initblk. See ECMA-335, Sec. III.2.5 unaligned.(prefix) for more information
                Unsafe.InitBlock((void*)ptr, 0, (uint)bytes);
            }

            return ptr;
        }

        public static void FreeAligned(nuint ptr)
        {
#if NET6_0_OR_GREATER
            NativeMemory.AlignedFree((void*)ptr);
#else
            AlignedFree_Compatible(ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            return Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignmentOf<T>()
        {
            return SizeOf<AlignmentHelper<T>>() - SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(nuint ptr)
        {
            return ref Unsafe.AsRef<T>((void*)ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* ptr)
        {
            return ref Unsafe.AsRef<T>(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AddressOf<T>(ref T obj)
        {
            return (nuint)Unsafe.AsPointer(ref obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint HeapAddressOf<T>(ref T obj) where T : class
        {
            return *(nuint*)Unsafe.AsPointer(ref obj);
        }

        private static nuint AlignedMalloc_Compatible(int size, int alignment)
        {
            if (size <= 0)
                throw new ArgumentException("Size must be positive.");
            if (alignment <= 0 || (alignment & ~-alignment) != 0)
                throw new ArgumentException("Alignment must be a power of two.");

            if (alignment < sizeof(nint))
            {
                alignment = sizeof(nint); // natural alignment
            }

            int totalSize = size + alignment + sizeof(nint) - 1;
            IntPtr rawPtr = Marshal.AllocHGlobal(totalSize);

            long alignedAddr = ((long)rawPtr + sizeof(nint) + alignment - 1) & -alignment;

            IntPtr originalPtrLocation = new IntPtr(alignedAddr - sizeof(nint));
            Marshal.WriteIntPtr(originalPtrLocation, rawPtr);

            return (nuint)alignedAddr;
        }

        private static void AlignedFree_Compatible(nuint alignedPtr)
        {
            if (alignedPtr == 0) return;

            IntPtr originalPtrLocation = new IntPtr((long)alignedPtr - sizeof(nint));
            IntPtr originalPtr = Marshal.ReadIntPtr(originalPtrLocation);

            Marshal.FreeHGlobal(originalPtr);
        }

        public static void Memset(nuint ptr, int len, byte value)
        {
            Unsafe.InitBlockUnaligned((void*)ptr, value, (uint)len);
        }

        public static void Memzero(nuint ptr, int len)
        {
            // unaligned.1 initblk. See ECMA-335, Sec. III.2.5 unaligned.(prefix) for more information
            Unsafe.InitBlockUnaligned((void*)ptr, 0, (uint)len);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AlignmentHelper<T>
        {
            public T value;
            public byte suffix;
        }
    }
}
