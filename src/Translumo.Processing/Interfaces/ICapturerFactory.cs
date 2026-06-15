namespace Translumo.Processing.Interfaces
{
    public interface ICapturerFactory
    {
        IScreenCapturer CreateCapturer(bool reliabilityPrioritize);
    }
}
