using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Translumo.Configuration;
using Translumo.Processing.Exceptions;
using Translumo.Processing.Interfaces;
using Translumo.Utils;
using Translumo.Utils.IntertopStruct;

namespace Translumo.Services
{
    public class BitBltScreenCapture : IScreenCapturer
    {
        public int CaptureAttempts { get; set; } = 3;
        public int AttemptDelayMs { get; set; } = 300;

        private Rectangle _virtualScreen;

        private readonly ScreenCaptureConfiguration _configuration;

        public BitBltScreenCapture(ScreenCaptureConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void Initialize()
        {
            _virtualScreen = VirtualScreen.GetBounds();
        }

        public byte[] CaptureScreen()
        {
            if (_configuration.CaptureArea.IsEmpty)
            {
                throw new CaptureException($"Capture area is not selected");
            }

            return MakeScreenshotInternal(_configuration.CaptureArea, 1);
        }

        public byte[] CaptureScreen(RectangleF captureArea)
        {
            return MakeScreenshotInternal(captureArea, 1);
        }

        public void Dispose()
        {
        }

        private byte[] MakeScreenshotInternal(RectangleF captureArea, int curAttempt)
        {
            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;

            try
            { 
                hdcSrc = Win32Interfaces.GetDCEx(IntPtr.Zero, IntPtr.Zero,
                    DeviceContextValues.Window | DeviceContextValues.Cache | DeviceContextValues.LockWindowUpdate);
                hdcDest = Win32Interfaces.CreateCompatibleDC(hdcSrc);
                hBitmap = Win32Interfaces.CreateCompatibleBitmap(hdcSrc, _virtualScreen.Width, _virtualScreen.Height);
                var hOld = Win32Interfaces.SelectObject(hdcDest, hBitmap);
                // The screen DC uses virtual-screen coordinates, whose origin is negative when a monitor sits left of / above the primary one
                Win32Interfaces.BitBlt(hdcDest, 0, 0, _virtualScreen.Width, _virtualScreen.Height, hdcSrc,
                    _virtualScreen.X, _virtualScreen.Y, TernaryRasterOperations.SRCCOPY);
                Win32Interfaces.SelectObject(hdcDest, hOld);

                var relativeArea = new RectangleF(captureArea.X - _virtualScreen.X, captureArea.Y - _virtualScreen.Y,
                    captureArea.Width, captureArea.Height);

                using var img = Image.FromHbitmap(hBitmap);
                using var bitmap = img.Clone(relativeArea, PixelFormat.Format32bppArgb);

                return bitmap.ToBytes(ImageFormat.Tiff);
            }
            catch (Exception e)
            {
                if (curAttempt >= CaptureAttempts)
                {
                    throw new CaptureException("Failed to capture screen", e);
                }

                Thread.Sleep(AttemptDelayMs);

                return MakeScreenshotInternal(captureArea, ++curAttempt);
            }
            finally
            {
                if (hdcDest != IntPtr.Zero)
                {
                    Win32Interfaces.DeleteDC(hdcDest);
                }

                if (hdcSrc != IntPtr.Zero)
                {
                    Win32Interfaces.ReleaseDC(IntPtr.Zero, hdcSrc);
                }

                if (hBitmap != IntPtr.Zero)
                {
                    Win32Interfaces.DeleteObject(hBitmap);
                }
            }
        }
    }
}
