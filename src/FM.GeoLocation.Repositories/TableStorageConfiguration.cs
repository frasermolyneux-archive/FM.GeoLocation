using System;
using System.Configuration;

namespace FM.GeoLocation.Repositories
{
    public interface ITableStorageConfiguration
    {
        string TableStorageConnectionString { get; }
    }

    public class TableStorageConfiguration : ITableStorageConfiguration
    {
        public string TableStorageConnectionString =>
            ConfigurationManager.AppSettings["TableStorageConnectionString"] ??
            Environment.GetEnvironmentVariable("TableStorageConnectionString");
    }
}