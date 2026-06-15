using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Encryption;
using Translumo.Utils.Extensions;

namespace Translumo.Configuration
{
    public class ConfigurationStorage
    {
        private const string ENCRYPTION_PASSWORD = "p@wd!";

        private readonly IServiceProvider _serviceProvider;
        private readonly IEncryptionService _encryptionService;
        private readonly IList<Type> _configurationTypes;
        private readonly ILogger _logger;

        public ConfigurationStorage(IServiceProvider serviceProvider, IEncryptionService encryptionService, ILogger<ConfigurationStorage> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _encryptionService = encryptionService;
            _configurationTypes = new List<Type>();
        }

        public void RegisterConfiguration<TConfiguration>()
            where TConfiguration : class
        {
            if (!_configurationTypes.Contains(typeof(TConfiguration)))
            {
                _configurationTypes.Add(typeof(TConfiguration));
            }
        }

        public void LoadConfiguration()
        {
            List<object> configurations = _configurationTypes.Select(type => _serviceProvider.GetService(type)).ToList();
            var serializer = new XmlSerializer(typeof(List<object>), _configurationTypes.ToArray());
            List<object> savedConfigs;
            try
            {
                var confPath = GetConfigurationPath();
                _logger.LogTrace($"Loading configuration from '{confPath}'");
                using (FileStream fs = new FileStream(confPath, FileMode.Open))
                {
                    var decryptedConfig = _encryptionService.Decrypt(fs, ENCRYPTION_PASSWORD);
                    using (var textReader = new StringReader(decryptedConfig))
                    {
                        savedConfigs = serializer.Deserialize(textReader) as List<object>;
                    }
                }

                foreach (var configuration in configurations)
                {
                    var savedConfig = savedConfigs.FirstOrDefault(dc => dc.GetType() == configuration.GetType());
                    if (savedConfig == null)
                    {
                        continue;
                    }

                    savedConfig.MapTo(configuration);
                }
                _logger.LogTrace("Configuration loaded");
            }
            catch (FileNotFoundException)
            {
                //IGNORE
            }
            catch (Exception)
            {
                _logger.LogError($"Unexpected error loading configuration");
            }
        }


        public void SaveConfiguration()
        {
            List<object> configurations = _configurationTypes.Select(type => _serviceProvider.GetService(type)).ToList();

            var serializer = new XmlSerializer(typeof(List<object>), _configurationTypes.ToArray());
            var savePath = GetConfigurationPath();
            _logger.LogTrace($"Saving configuration to '{savePath}'");
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.Serialize(ms, configurations);
                    ms.Position = 0;

                    byte[] encryptedConfig = _encryptionService.Encrypt(ms, ENCRYPTION_PASSWORD);
                    File.WriteAllBytes(savePath, encryptedConfig);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save configuration to '{savePath}'");
            }
        }


        private string GetConfigurationPath()
        {
            const string CONFIGURATION_FILE = "settings";

            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appName = AppDomain.CurrentDomain.FriendlyName.Split('.').First();
            string appDirectory = Path.Combine(appDataDirectory, appName);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            return Path.Combine(appDirectory, CONFIGURATION_FILE);
        }
    }
}
