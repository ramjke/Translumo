using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Translumo.Infrastructure.Language;

namespace Translumo.OCR.WindowsOCR
{
    public class WindowsOCREngine : IOCREngine
    {
        public Languages DetectionLanguage => LanguageDescriptor.Language;
        public virtual int Confidence => 5;
        public virtual byte PrimaryPriority => 10;
        public bool SecondaryPrimaryCheck => true;

        protected readonly OcrEngine MsEngine;
        protected readonly LanguageDescriptor LanguageDescriptor;

        protected ImageFormat ImgFormat;

        public WindowsOCREngine(LanguageDescriptor languageDescriptor)
        {
            LanguageDescriptor = languageDescriptor;
            MsEngine = OcrEngine.TryCreateFromLanguage(new Language(LanguageDescriptor.Code));
            ImgFormat = ImageFormat.Tiff;
        }

        public string[] GetTextLines(byte[] image)
        {
            if (MsEngine == null) 
                return [];

            using var stream = new MemoryStream(image);
            Bitmap bitmap = PreProcess(new Bitmap(stream));

            SoftwareBitmap softwareBitmap;
            using (var randStream = new InMemoryRandomAccessStream())
            {
                bitmap.Save(randStream.AsStream(), ImgFormat);
                var decoder = BitmapDecoder.CreateAsync(randStream).AsTask().Result;
                softwareBitmap = decoder.GetSoftwareBitmapAsync().AsTask().Result;
            }

            OcrResult result = MsEngine.RecognizeAsync(softwareBitmap).AsTask().Result;

            return result.Lines
                .Select(line => LanguageDescriptor.UseSpaceRemover ? line.Text.Replace(" ", string.Empty) : line.Text)
                .ToArray();
        }

        protected virtual Bitmap PreProcess(Bitmap bitmap)
        {
            return bitmap;
        }
    }
}
