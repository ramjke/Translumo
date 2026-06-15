using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Constants;

namespace Translumo.Update
{
    public class UpdateManager
    {
        private readonly IReleasesClient _releasesClient;
        private readonly ILogger _logger;

        public UpdateManager(IReleasesClient releasesClient, ILogger<UpdateManager> logger)
        {
            _releasesClient = releasesClient;
            _logger = logger;
        }


        public async Task<bool> CheckNewVersionAsync()
        {
            try
            {
                var lastVersion = await _releasesClient.GetLastVersionAsync();
                if (lastVersion == null)
                {
                    return false;
                }

                return lastVersion > Global.GetVersion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to check available versions");

                return false;
            }
        }
    }
}
