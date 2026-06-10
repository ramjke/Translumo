using System;
using System.Threading.Tasks;

namespace Translumo.Update
{
    public interface IReleasesClient
    {
        Task<Version> GetLastVersionAsync();
    }
}
