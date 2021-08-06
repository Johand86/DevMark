using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DevMark.Core.SysIdle
{
    public class WinSystemIdlePreventionProvider : IDisposable, ISystemIdlePreventionProvider
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        Thread _workerThread;

        public WinSystemIdlePreventionProvider()
        {
            _workerThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(15000);
                        SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
                    }
                }
                catch (ThreadInterruptedException)
                {

                }
            });
            _workerThread.Start();
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
                _workerThread?.Interrupt();
                _workerThread = null;
            }
        }

        ~WinSystemIdlePreventionProvider()
        {
            Dispose(false);
        }

        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }
    }
}
