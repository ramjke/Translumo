using System.Threading.Tasks;

namespace Translumo.Translation
{
    public interface ITranslator
    {
        Task<string> TranslateTextAsync(string sourceText);
    }
}
