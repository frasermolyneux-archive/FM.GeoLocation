using System;
using System.Reflection;
using FM.GeoLocation.Client;
using FM.GeoLocation.Contract.Interfaces;
using FM.GeoLocation.FuncApp;
using FM.GeoLocation.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace FM.GeoLocation.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddSingleton<ITableStorageConfiguration, TableStorageConfiguration>();
            builder.Services.AddSingleton<ILocationsRepository, LocationsRepository>();
            builder.Services.AddSingleton<IPartitionKeyHelper, PartitionKeyHelper>();
            builder.Services.AddSingleton<IMaxMindApiConfiguration, MaxMindApiConfiguration>();
            builder.Services.AddSingleton<IMaxMindLocationsRepository, MaxMindLocationsRepository>();
            builder.Services.AddSingleton<IAddressValidator, AddressValidator>();
        }
    }
}