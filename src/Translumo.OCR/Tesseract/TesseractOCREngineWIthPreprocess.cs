using System.Drawing;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Translumo.Infrastructure.Language;
using Size = OpenCvSharp.Size;

namespace Translumo.OCR.Tesseract
{
    public sealed class TesseractOCREngineWIthPreprocess : TesseractOCREngine
    {
        public override byte PrimaryPriority => 1;
        public override int Confidence => 2;

        public TesseractOCREngineWIthPreprocess(LanguageDescriptor languageDescriptor)
            : base(languageDescriptor)
        {
        }

        protected override byte[] PreProcess(byte[] image)
        {
            using var stream = new MemoryStream(image);
            var bitmap = new Bitmap(stream);
            using (var src = bitmap.ToMat())
            {
                var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 1));
                Cv2.CvtColor(src, src, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(src, src, 150, 255, ThresholdTypes.Binary);
                Cv2.MorphologyEx(src, src, MorphTypes.Open, kernel);

                return src.ToBytes(".tif");
            }
        }
    }
}
