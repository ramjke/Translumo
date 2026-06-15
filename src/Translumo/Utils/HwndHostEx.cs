using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Translumo.Utils
{
    public class HwndHostEx : HwndHost
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        private readonly IntPtr ChildHandle = IntPtr.Zero;

        public const int GWL_STYLE = (-16);
        public const int WS_CHILD = 0x40000000;

        public HwndHostEx(IntPtr handle)
        {
            ChildHandle = handle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HandleRef href = new HandleRef();

            if (ChildHandle != IntPtr.Zero)
            {
                SetWindowLong(ChildHandle, GWL_STYLE, WS_CHILD);
                SetParent(ChildHandle, hwndParent.Handle);
                href = new HandleRef(this, ChildHandle);
            }

            return href;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {

        }
    }
}
