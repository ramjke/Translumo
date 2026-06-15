using System;
using System.IO;
using Python.Runtime;
using Translumo.Infrastructure.Constants;

namespace Translumo.Infrastructure.Python;

public class PythonEngineWrapper : IDisposable
{
    private bool _disposedValue;
    private int _countUsage;
    private readonly IntPtr _threadState;

    public PythonEngineWrapper()
    {
        Runtime.PythonDLL = Path.Combine(Global.PythonPath, "python38.dll");
        PythonEngine.PythonHome = Global.PythonPath;
    }

    public PyObject Import(string libName) => Py.Import(libName);

    public void Execute(Action action)
    {
        Execute<object?>(() => { action(); return null; });
    }

    public T Execute<T>(Func<T> func)
    {
        using (Py.GIL())
        {
            return func();
        }
    }

    public void Init()
    {
        if (_countUsage++ > 0)
        {
            return;
        }

        InitInternal();
    }

    private void InitInternal()
    {
        _disposedValue = false;

        if (!PythonEngine.IsInitialized)
        {
            // TODO: move to common place, also used in EasyOCR
            Runtime.PythonDLL = Path.Combine(Global.PythonPath, "python38.dll");
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            if (PythonEngine.IsInitialized)
            {
                // Causes PythonEngine.Shutdown() hanging (https://github.com/pythonnet/pythonnet/issues/1701)
                // PythonEngine.EndAllowThreads(_threadState);
                PythonEngine.Shutdown();
            }

            _disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~PythonEngineWrapper()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        if (--_countUsage > 0)
        {
            return;
        }

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
