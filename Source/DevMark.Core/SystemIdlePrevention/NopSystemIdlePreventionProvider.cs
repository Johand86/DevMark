using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DevMark.Core.SysIdle
{
    public class NopSystemIdlePreventionProvider : IDisposable, ISystemIdlePreventionProvider
    {
      
        public NopSystemIdlePreventionProvider()
        {
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        ~NopSystemIdlePreventionProvider()
        {
            Dispose(false);
        }

    }
}
