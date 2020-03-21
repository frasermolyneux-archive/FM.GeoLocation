using FM.GeoLocation.FuncApp;
using FM.GeoLocation.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace FM.GeoLocation.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITableStorageConfiguration, TableStorageConfiguration>();
            builder.Services.AddSingleton<ILocationsRepository, LocationsRepository>();
            builder.Services.AddSingleton<IPartitionKeyHelper, PartitionKeyHelper>();
            builder.Services.AddSingleton<IMaxMindApiConfiguration, MaxMindApiConfiguration>();
            builder.Services.AddSingleton<IMaxMindLocationsRepository, MaxMindLocationsRepository>();
            builder.Services.AddSingleton<IAddressHelper, AddressHelper>();
        }
    }
}