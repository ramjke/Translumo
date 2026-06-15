using Translumo.Infrastructure;

namespace Translumo.Processing.Interfaces
{
    public interface IChatTextMediator
    {
        void SendText(string text, bool successful);

        void SendText(string text, TextTypes textType);

        void ClearTexts();
    }
}
