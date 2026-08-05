using System;
using System.Threading;

namespace frm_pin_remover.Core
{
    internal sealed class SingleInstanceGuard : IDisposable
    {
        private Mutex _mutex;

        public bool IsFirstInstance { get; private set; }

        public bool TryAcquire()
        {
            _mutex = new Mutex(true, AppConstants.MutexName, out bool createdNew);
            IsFirstInstance = createdNew;
            return createdNew;
        }

        public static void FocusExistingInstance()
        {
            IntPtr hWnd = NativeMethods.FindWindow(null, AppConstants.MainWindowTitle);
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            // SW_RESTORE also un-hides a window that was hidden to the tray (Visible = false),
            // not just one that was minimized.
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
            NativeMethods.SetForegroundWindow(hWnd);
        }

        public void Dispose()
        {
            if (_mutex != null && IsFirstInstance)
            {
                _mutex.ReleaseMutex();
            }

            _mutex?.Dispose();
        }
    }
}
