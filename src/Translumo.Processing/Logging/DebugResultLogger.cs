using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translumo.Processing.TextProcessing;

namespace Translumo.Processing.Logging
{
    public class DebugResultLogger : IDisposable
    {
        private readonly string fileName = "results.txt";
        private readonly string imgPathFormat = "./screens/{0}.tiff";

        private readonly FileStream _stream;

        private int _counter;

        public DebugResultLogger()
        {
            _stream = new FileStream(fileName, FileMode.Create);
            _counter = 0;
        }

        public async Task LogResults(IEnumerable<TextDetectionResult> results, byte[] img)
        {
            var ordered = results.OrderBy(r => r).ToArray();
            var str = new StringBuilder();
            str.AppendLine($"Iteration: {_counter}");
            for (var i = 0; i < ordered.Count(); i++)
            {
                str.AppendLine($"{i + 1}p ({ordered[i].ValidityScore})({ordered[i].SourceEngine.GetType()}): '{ordered[i].Text}'");
            }

            str.AppendLine();
            await _stream.WriteAsync(Encoding.Default.GetBytes(str.ToString()));
            if (img != null)
            {
                await File.WriteAllBytesAsync(string.Format(imgPathFormat, _counter), img);
            }

            _counter++;
        }

        public void Dispose()
        {
            _stream.Close();
        }
    }
}
