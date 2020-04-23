using System;
using FM.GeoLocation.Client.Configuration;
using FM.GeoLocation.Contract.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FM.GeoLocation.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGeoLocationClient(this IServiceCollection serviceCollection,
            Action<IGeoLocationClientOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new GeoLocationClientOptions();
            configureOptions.Invoke(options);

            options.Validate();

            serviceCollection.AddSingleton<IGeoLocationClientOptions>(options);
            serviceCollection.AddSingleton<IGeoLocationClient, GeoLocationClient>();
            serviceCollection.AddSingleton<IAddressValidator, AddressValidator>();
        }
    }
}