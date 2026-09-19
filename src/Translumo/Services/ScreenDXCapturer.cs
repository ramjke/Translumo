using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Translumo.Configuration;
using Translumo.Processing.Exceptions;
using Translumo.Processing.Interfaces;
using Translumo.Utils;

namespace Translumo.Services
{
    public class ScreenDXCapturer : IScreenCapturer
    {
        public int CaptureAttempts { get; set; } = 5;
        public int AttemptDelayMs { get; set; } = 250;

        private Factory1 _factory;
        private Adapter1 _adapter;
        private SharpDX.Direct3D11.Device _device;
        private Output _output;
        private Output1 _output1;
        private Texture2DDescription _textureDesc;
        private Texture2D _screenTexture;

        private OutputDuplication _duplicatedOutput;

        private int _width;
        private int _height;

        private bool _initialized = false;

        private readonly ScreenCaptureConfiguration _configuration;

        public ScreenDXCapturer(ScreenCaptureConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Initialize()
        {
            if (_initialized)
            {
                Dispose();
            }
            
            _factory = new Factory1();
            //Get first adapter
            _adapter = _factory.GetAdapter1(0);
            //Get device from adapter
            _device = new SharpDX.Direct3D11.Device(_adapter);
            //Get front buffer of the adapter
            _output = _adapter.GetOutput(0);
            _output1 = _output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            _width = _output.Description.DesktopBounds.Right;
            _height = _output.Description.DesktopBounds.Bottom;

            // Create Staging texture CPU-accessible
            _textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = _width,
                Height = _height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            _screenTexture = new Texture2D(_device, _textureDesc);
            _duplicatedOutput = _output1.DuplicateOutput(_device);

            _initialized = true;
        }

        public byte[] CaptureScreen()
        {
            try
            {
                if (_configuration.CaptureArea.IsEmpty)
                {
                    throw new CaptureException($"Capture area is not selected");
                }

                return MakeScreenshotInternal(_configuration.CaptureArea, 1);
            }
            catch (CaptureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CaptureException("Failed to capture screen", ex);
            }
        }

        public byte[] CaptureScreen(RectangleF captureArea)
        {
            try
            {
                return MakeScreenshotInternal(captureArea, 1);
            }
            catch (CaptureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CaptureException("Failed to capture screen", ex);
            }
        }

        public void Dispose()
        {
            _duplicatedOutput?.Dispose();
            _duplicatedOutput = null;
            _device?.Dispose();
            _device = null;
            _screenTexture?.Dispose();
            _screenTexture = null;
            _adapter?.Dispose();
            _adapter = null;
            _output?.Dispose();
            _output = null;
            _output1?.Dispose();
            _output1 = null;

            _initialized = false;
        }

        private byte[] MakeScreenshotInternal(RectangleF captureArea, int curAttempt)
        {
            SharpDX.DXGI.Resource screenResource = null;
            OutputDuplicateFrameInformation duplicateFrameInformation;

            try
            {
                // Try to get duplicated frame within given time is ms
                var resultAcquire =
                    _duplicatedOutput.TryAcquireNextFrame(100, out duplicateFrameInformation, out screenResource);
                if (!resultAcquire.Success)
                {
                    throw new CaptureException("Failed to acquire frame", resultAcquire.Code);
                }

                // copy resource into memory that can be accessed by the CPU
                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    _device.ImmediateContext.CopyResource(screenTexture2D, _screenTexture);

                // Get the desktop capture texture
                var mapSource = _device.ImmediateContext.MapSubresource(_screenTexture, 0, MapMode.Read,
                    SharpDX.Direct3D11.MapFlags.None);
                
                // Create Drawing.Bitmap
                using (var bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb))
                {
                    var boundsRect = new Rectangle(0, 0, _width, _height);

                    // Copy pixels from screen capture Texture to GDI bitmap
                    var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    var sourcePtr = mapSource.DataPointer;
                    var destPtr = mapDest.Scan0;
                    for (int y = 0; y < _height; y++)
                    {
                        // Copy a single line 
                        Utilities.CopyMemory(destPtr, sourcePtr, _width * 4);

                        // Advance pointers
                        sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                        destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                    }

                    // Release source and dest locks
                    bitmap.UnlockBits(mapDest);
                    _device.ImmediateContext.UnmapSubresource(_screenTexture, 0);

                    return bitmap.Clone(captureArea, bitmap.PixelFormat)
                        .ToBytes(ImageFormat.Tiff);
                }
            }
            catch (Exception)
            {
                if (curAttempt >= CaptureAttempts)
                {
                    throw;
                }

                Thread.Sleep(AttemptDelayMs);

                return MakeScreenshotInternal(captureArea, ++curAttempt);
            }
            finally
            {
                try
                {
                    screenResource?.Dispose();
                    _duplicatedOutput.ReleaseFrame();
                }
                catch { }
            }
        }
    }
}
