using System.Drawing;

namespace Translumo.Processing
{
    public interface IProcessingService
    {
        bool IsStarted { get; }

        void StartProcessing();

        void ProcessOnce(RectangleF captureArea);

        void StopProcessing();
    }
}
