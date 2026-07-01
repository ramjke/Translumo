using System.Collections.Generic;
using System.Drawing;

namespace Translumo.OCR.WindowsOCR
{
    /// <summary>
    /// A single recognized text line together with its pixel bounding box inside the source image.
    /// </summary>
    public sealed class PositionalOcrLine
    {
        public string Text { get; init; }

        /// <summary>Bounding box in source-image pixels.</summary>
        public RectangleF Box { get; init; }
    }

    /// <summary>
    /// Result of recognizing a captured region: the chosen recognizer language, the recognized
    /// lines with positions, and the source image size (pixels).
    /// </summary>
    public sealed class OcrRegionResult
    {
        public string LanguageTag { get; init; }

        public IReadOnlyList<PositionalOcrLine> Lines { get; init; }

        public int ImageWidth { get; init; }

        public int ImageHeight { get; init; }
    }
}
