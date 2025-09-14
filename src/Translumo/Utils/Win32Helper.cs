using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Translumo.Utils
{
    internal static class Win32Helper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Gets the current cursor position in screen coordinates.
        /// </summary>
        public static Point GetCursorPosition()
        {
            if (GetCursorPos(out POINT point))
            {
                return new Point(point.X, point.Y);
            }

            throw new InvalidOperationException("Unable to get cursor position.");
        }
    }
}