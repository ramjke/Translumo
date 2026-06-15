using System;
using Microsoft.Extensions.Logging;
using Translumo.Configuration;
using Translumo.Processing.Interfaces;

namespace Translumo.Services
{
    public class ScreenCapturerFactory : ICapturerFactory
    {
        private readonly ScreenCaptureConfiguration _configuration;
        private readonly ILogger _logger;

        public ScreenCapturerFactory(ScreenCaptureConfiguration configuration, ILogger<ScreenCapturerFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IScreenCapturer CreateCapturer(bool reliabilityPrioritize)
        {
            if (reliabilityPrioritize)
            {
                return (IScreenCapturer)TryCreateCapturer<BitBltScreenCapture>() ?? TryCreateCapturer<ScreenDXCapturer>();
            }
            else
            {
                return (IScreenCapturer)TryCreateCapturer<ScreenDXCapturer>() ?? TryCreateCapturer<BitBltScreenCapture>();
            }
        }

        private TCapturer TryCreateCapturer<TCapturer>()
            where TCapturer : IScreenCapturer
        {
            TCapturer capturer = default;
            try
            {
                capturer = (TCapturer)Activator.CreateInstance(typeof(TCapturer), _configuration);
                capturer.Initialize();

                return capturer;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to create {typeof(TCapturer)}. Another capturer will be used.");
                capturer?.Dispose();
            }

            return default;
        }
    }
}
