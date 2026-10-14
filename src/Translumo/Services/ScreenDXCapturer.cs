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

        private Rectangle _outputBounds;
        private BitBltScreenCapture _gdiFallback;

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
            // Without a selected area yet, start on the monitor holding the virtual screen origin (the primary one)
            var area = _configuration.CaptureArea.IsEmpty
                ? new RectangleF(0, 0, 1, 1)
                : _configuration.CaptureArea;

            if (!TryFindOutput(area, out var adapterIndex, out var outputIndex))
            {
                adapterIndex = 0;
                outputIndex = 0;
            }

            InitializeOutput(adapterIndex, outputIndex);
        }

        // Duplicates a single monitor, so pick the adapter/output that contains the capture area
        private void InitializeOutput(int adapterIndex, int outputIndex)
        {
            if (_initialized)
            {
                Dispose();
            }

            _factory = new Factory1();
            _adapter = _factory.GetAdapter1(adapterIndex);
            //Get device from adapter
            _device = new SharpDX.Direct3D11.Device(_adapter);
            //Get front buffer of the adapter
            _output = _adapter.GetOutput(outputIndex);
            _output1 = _output.QueryInterface<Output1>();

            // Position and size of the monitor within the virtual desktop
            var desktopBounds = _output.Description.DesktopBounds;
            _outputBounds = new Rectangle(desktopBounds.Left, desktopBounds.Top,
                desktopBounds.Right - desktopBounds.Left, desktopBounds.Bottom - desktopBounds.Top);
            _width = _outputBounds.Width;
            _height = _outputBounds.Height;

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

                return CaptureArea(_configuration.CaptureArea);
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
                return CaptureArea(captureArea);
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
            _gdiFallback?.Dispose();
            _gdiFallback = null;
            _factory?.Dispose();
            _factory = null;
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

        private byte[] CaptureArea(RectangleF captureArea)
        {
            if (!_outputBounds.Contains(Rectangle.Round(captureArea)))
            {
                if (!TryFindOutput(captureArea, out var adapterIndex, out var outputIndex))
                {
                    // The area spans several monitors, which a single output duplication can't cover
                    return CaptureWithGdi(captureArea);
                }

                InitializeOutput(adapterIndex, outputIndex);
            }

            return MakeScreenshotInternal(captureArea, 1);
        }

        private byte[] CaptureWithGdi(RectangleF captureArea)
        {
            if (_gdiFallback == null)
            {
                _gdiFallback = new BitBltScreenCapture(_configuration);
                _gdiFallback.Initialize();
            }

            return _gdiFallback.CaptureScreen(captureArea);
        }

        private static bool TryFindOutput(RectangleF captureArea, out int adapterIndex, out int outputIndex)
        {
            var area = Rectangle.Round(captureArea);

            using var factory = new Factory1();
            for (int a = 0; a < factory.GetAdapterCount1(); a++)
            {
                using var adapter = factory.GetAdapter1(a);
                for (int o = 0; o < adapter.GetOutputCount(); o++)
                {
                    using var output = adapter.GetOutput(o);
                    var bounds = output.Description.DesktopBounds;
                    var outputBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
                    if (outputBounds.Contains(area))
                    {
                        adapterIndex = a;
                        outputIndex = o;
                        return true;
                    }
                }
            }

            adapterIndex = outputIndex = -1;
            return false;
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

                    // The duplicated texture starts at the monitor's corner, not at the virtual screen origin
                    var relativeArea = new RectangleF(captureArea.X - _outputBounds.X, captureArea.Y - _outputBounds.Y,
                        captureArea.Width, captureArea.Height);

                    using var croppedBitmap = bitmap.Clone(relativeArea, bitmap.PixelFormat);
                    return croppedBitmap.ToBytes(ImageFormat.Tiff);
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
