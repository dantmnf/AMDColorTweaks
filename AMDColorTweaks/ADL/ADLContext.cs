using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMDColorTweaks.ADL
{
    internal class ADLContext : IDisposable
    {
        private IntPtr _handle;
        public ADLContext(bool enumConnectedOnly = false)
        {
            var result = ADLNative.ADL2_Main_Control_Create(Marshal.AllocHGlobal, enumConnectedOnly ? 1 : 0, out _handle);
            if (result != 0)
            {
                throw new SystemException("ADL2_Main_Control_Create failed");
            }
        }

        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                ADLNative.ADL2_Main_Control_Destroy(handle);
            }
        }

        public static implicit operator IntPtr(ADLContext x) => x._handle;

        public static int RaiseForError(int err)
        {
            if (err < 0)
            {
                var msg = ((ADLResultCode)err).ToString();
                throw new SystemException($"ADL Error: {msg}");
            }
            return err;
        }
    }
}
