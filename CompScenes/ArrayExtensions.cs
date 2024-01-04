using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace CompScenes
{
    internal static class ArrayExtensions
    {
        internal static unsafe MemoryBuffer ToMemoryBuffer<T>(this T[] arr) where T : unmanaged
        {
            MemoryBuffer mb = new MemoryBuffer((uint)(arr.Length * sizeof(T)));
            IMemoryBufferReference mbr = mb.CreateReference();
            IMemoryBufferByteAccess mba = (IMemoryBufferByteAccess)mbr;

            byte* bytes = default;
            uint capacity;
            mba.GetBuffer(&bytes, &capacity);

            fixed (void* ptr = arr) Unsafe.CopyBlock(bytes, ptr, capacity);

            return mb;
        }

        [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMemoryBufferByteAccess
        {
            unsafe void GetBuffer(byte** bytes, uint* capacity);
        }
    }
}
