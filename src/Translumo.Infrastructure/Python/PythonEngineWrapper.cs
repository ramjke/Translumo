using System;
using System.IO;
using Python.Runtime;
using Translumo.Infrastructure.Constants;

namespace Translumo.Infrastructure.Python;

public class PythonEngineWrapper : IDisposable
{
    private int _countUsage;

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

        if (!PythonEngine.IsInitialized)
        {
            // TODO: move to common place, also used in EasyOCR
            Runtime.PythonDLL = Path.Combine(Global.PythonPath, "python38.dll");
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }

    public void Dispose()
    {
        if (_countUsage > 0)
        {
            _countUsage--;
        }
    }
}
