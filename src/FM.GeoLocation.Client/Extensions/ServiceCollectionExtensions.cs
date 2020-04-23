using System;
using FM.GeoLocation.Contract.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FM.GeoLocation.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGeoLocationClient(this IServiceCollection serviceCollection,
            IGeoLocationClientOptions configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            if (string.IsNullOrWhiteSpace(configureOptions.BaseUrl))
                throw new NullReferenceException(nameof(configureOptions.BaseUrl));

            if (string.IsNullOrWhiteSpace(configureOptions.ApiKey))
                throw new NullReferenceException(nameof(configureOptions.ApiKey));

            serviceCollection.AddSingleton(configureOptions);
            serviceCollection.AddSingleton<IGeoLocationClient, GeoLocationClient>();
        }
    }
}