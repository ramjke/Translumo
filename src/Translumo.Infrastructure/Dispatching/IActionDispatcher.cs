using System;
using System.Threading.Tasks;

namespace Translumo.Infrastructure.Dispatching
{
    public interface IActionDispatcher
    {
        void RegisterConsumer<TArgument, TResult>(string actionName, Func<TArgument, Task<TResult>> actionHandler);

        Task<TResult> DispatchActionAsync<TArgument, TResult>(string actionName, TArgument argument);
    }
}
