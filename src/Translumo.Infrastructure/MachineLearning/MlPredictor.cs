using System;
using System.IO;
using Microsoft.ML;

namespace Translumo.Infrastructure.MachineLearning
{
    public class MlPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
        where TInput : class
        where TOutput : class, new()
    {
        public bool Loaded { get; private set; }

        private readonly MLContext _context;
        private ITransformer _model;
        private PredictionEngine<TInput, TOutput> _predictor;

        public MlPredictor()
        {
            _context = new MLContext();
            this.Loaded = false;
        }

        public void LoadModel(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"Model '{path}' is not found");
            }

            if (Loaded)
            {
                UnloadModel();
            }

            _model = _context.Model.Load(path, out var schema);
            _predictor = _context.Model.CreatePredictionEngine<TInput, TOutput>(_model);
            Loaded = true;
        }

        public void UnloadModel()
        {
            Dispose();
            Loaded = false;
            _model = null;
            _predictor = null;
        }

        public TOutput PredictResult(TInput input)
        {
            if (!Loaded)
            {
                throw new InvalidOperationException("Model is not loaded");
            }
            // TODO: predict isnot return result, just freezing
            return _predictor.Predict(input);
        }

        public void Dispose()
        {
            _predictor?.Dispose();
        }
    }
}
