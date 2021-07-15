using System.IO;
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
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"),
                    true, false)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables();

            base.ConfigureAppConfiguration(builder);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITableStorageConfiguration, TableStorageConfiguration>();
            builder.Services.AddSingleton<ILocationsRepository, LocationsRepository>();
            builder.Services.AddSingleton<IPartitionKeyHelper, PartitionKeyHelper>();
            builder.Services.AddSingleton<IMaxMindApiConfiguration, MaxMindApiConfiguration>();
            builder.Services.AddSingleton<IMaxMindLocationsRepository, MaxMindLocationsRepository>();
            builder.Services.AddSingleton<IAddressValidator, AddressValidator>();
        }
    }
}