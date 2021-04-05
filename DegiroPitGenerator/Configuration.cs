using System;
using Microsoft.Extensions.Configuration;

namespace DegiroPitGenerator
{
    internal static class Configuration
    {
        private static readonly IConfigurationRoot _configurationRoot;

        static Configuration()
        {
            _configurationRoot = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();
        }

        internal static T GetSection<T>()
        {
            return _configurationRoot.GetSection(typeof(T).Name).Get<T>();
        }
    }
}
