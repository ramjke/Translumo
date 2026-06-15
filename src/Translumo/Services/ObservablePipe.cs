using System;
using System.Windows.Threading;

namespace Translumo.Services
{
    public class ObservablePipe<T>
    {
        public event EventHandler<T> ItemHasArrived;

        private readonly Dispatcher _dispatcher;

        public ObservablePipe(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Send(T item)
        {
            if (_dispatcher == null)
            {
                ItemHasArrived?.Invoke(this, item);
            }
            else
            {
                _dispatcher.InvokeAsync(() => ItemHasArrived?.Invoke(this, item));
            }
        }
    }
}
