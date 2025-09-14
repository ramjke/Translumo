// -----------------------------------------------------------------------------
//  Lookupper (https://lookupper.com)
//  Copyright (c) 2025 Lookupper Team. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lookupper.Services
{
    /// <summary>
    /// Singleton service for managing monitor information and operations.
    /// </summary>
    public class MonitorService
    {
        private static MonitorService _instance;

        private readonly List<RECT> _monitors = new();
        private RECT _virtualScreen;
        private RECT _primaryMonitor;

        private MonitorService() { }

        public static MonitorService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MonitorService();
                return _instance;
            }
        }

        /// <summary>
        /// Clears and reinitializes monitor list.
        /// </summary>
        public void ResetMonitors()
        {
            _monitors.Clear();
            InitializeMonitors();
        }

        /// <summary>
        /// Gets the monitor rectangle that contains the specified point.
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>RECT of monitor</returns>
        public RECT GetMonitorFromPoint(POINT point)
        {
            var existingMonitor = _monitors.FirstOrDefault(m => IsPointInRectangle(m, point));

            if (!existingMonitor.IsEmpty) // now works
                return existingMonitor;

            RECT? newMonitor = GetRectangleFromPoint(point);
            if (!newMonitor.HasValue)
                throw new Exception("No monitor found at the specified point.");

            _monitors.Add(newMonitor.Value);
            return newMonitor.Value;
        }

        private void InitializeMonitors()
        {
            SetPrimaryMonitor();
            SetVirtualScreen();
        }

        private void SetPrimaryMonitor()
        {
            _primaryMonitor = new RECT
            {
                Left = 0,
                Top = 0,
                Right = GetSystemMetrics(SystemMetric.SM_CXSCREEN),
                Bottom = GetSystemMetrics(SystemMetric.SM_CYSCREEN)
            };
        }

        private void SetVirtualScreen()
        {
            _virtualScreen = new RECT
            {
                Left = GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN),
                Top = GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN),
                Right = GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN) + GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN),
                Bottom = GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN) + GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN)
            };
        }

        private static bool IsPointInRectangle(RECT rect, POINT point)
        {
            return point.X >= rect.Left && point.Y >= rect.Top
                && point.X <= rect.Right && point.Y <= rect.Bottom;
        }

        private static RECT? GetRectangleFromPoint(POINT point)
        {
            IntPtr hMonitor = MonitorFromPoint(point, MonitorDefaultToNull);
            if (hMonitor == IntPtr.Zero)
                return null;

            MONITORINFO mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hMonitor, ref mi))
                return mi.rcMonitor;

            return null;
        }

        #region Win32 Imports & Structs

        private const int MonitorDefaultToNull = 0x00000000;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, int dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool IsEmpty => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        private enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
            SM_XVIRTUALSCREEN = 76,
            SM_YVIRTUALSCREEN = 77,
            SM_CXVIRTUALSCREEN = 78,
            SM_CYVIRTUALSCREEN = 79
        }

        #endregion
    }
}