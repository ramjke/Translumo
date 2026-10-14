using System.Drawing;
using Translumo.Utils.IntertopStruct;

namespace Translumo.Utils
{
    /// <summary>
    /// Bounds of the whole desktop spanning all monitors, in virtual-screen coordinates.
    /// Left/Top are negative when a monitor sits left of / above the primary one.
    /// </summary>
    public static class VirtualScreen
    {
        public static Rectangle GetBounds()
        {
            return new Rectangle(
                Win32Interfaces.GetSystemMetrics(SystemMetricTypes.SM_XVIRTUALSCREEN),
                Win32Interfaces.GetSystemMetrics(SystemMetricTypes.SM_YVIRTUALSCREEN),
                Win32Interfaces.GetSystemMetrics(SystemMetricTypes.SM_CXVIRTUALSCREEN),
                Win32Interfaces.GetSystemMetrics(SystemMetricTypes.SM_CYVIRTUALSCREEN));
        }
    }
}
