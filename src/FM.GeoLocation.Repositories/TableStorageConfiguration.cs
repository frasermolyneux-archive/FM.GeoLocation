using System;
using Microsoft.Extensions.Configuration;

namespace FM.GeoLocation.Repositories
{
    public interface ITableStorageConfiguration
    {
        string TableStorageConnectionString { get; }
    }

    public class TableStorageConfiguration : ITableStorageConfiguration
    {
        private readonly IConfiguration _configuration;

        public TableStorageConfiguration(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string TableStorageConnectionString => _configuration["Storage:TableStorageConnectionString"];
    }
}