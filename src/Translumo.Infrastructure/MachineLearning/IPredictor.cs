using System;

namespace Translumo.Infrastructure.MachineLearning
{
    public interface IPredictor<TInput, TOutput> : IDisposable
    {
        bool Loaded { get; }

        void LoadModel(string path);
        void UnloadModel();
        TOutput PredictResult(TInput input);
    }
}
