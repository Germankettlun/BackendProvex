using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

namespace ProvexApi.Configuration
{
    public class DbJsonConfigurationSource : IConfigurationSource
    {
        public string ConnectionString { get; }
        public string Environment { get; }

        public DbJsonConfigurationSource(string connectionString, string environment)
        {
            ConnectionString = connectionString;
            Environment = environment;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new DbJsonConfigurationProvider(this);

    }
}
