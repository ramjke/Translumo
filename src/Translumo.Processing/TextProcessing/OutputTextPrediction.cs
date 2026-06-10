using Microsoft.ML.Data;

namespace Translumo.Processing.TextProcessing
{
    public class OutputTextPrediction
    {
        [ColumnName("Score")]
        public float Validity;
    }
}
