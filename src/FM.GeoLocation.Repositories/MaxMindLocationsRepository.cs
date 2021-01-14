using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FM.GeoLocation.Repositories.Models;
using MaxMind.GeoIP2;

namespace FM.GeoLocation.Repositories
{
    public interface IMaxMindLocationsRepository
    {
        Task<GeoLocationEntity> GetGeoLocationEntity(string address);
    }

    public class MaxMindLocationsRepository : IMaxMindLocationsRepository
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IMaxMindApiConfiguration _maxMindApiConfiguration;
        private readonly IPartitionKeyHelper _partitionKeyHelper;

        public MaxMindLocationsRepository(
            IMaxMindApiConfiguration maxMindApiConfiguration,
            ILocationsRepository locationsRepository,
            IPartitionKeyHelper partitionKeyHelper)
        {
            _maxMindApiConfiguration = maxMindApiConfiguration ??
                                       throw new ArgumentNullException(nameof(maxMindApiConfiguration));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _partitionKeyHelper = partitionKeyHelper ?? throw new ArgumentNullException(nameof(partitionKeyHelper));
        }

        public Task<GeoLocationEntity> GetGeoLocationEntity(string address)
        {
            using (var reader = new WebServiceClient(_maxMindApiConfiguration.UserId, _maxMindApiConfiguration.ApiKey))
            {
                var lookupResult = reader.City(address);

                var traits = new Dictionary<string, string>
                {
                    {"AutonomousSystemNumber", lookupResult.Traits?.AutonomousSystemNumber.ToString()},
                    {"AutonomousSystemOrganization", lookupResult.Traits?.AutonomousSystemOrganization},
                    {"ConnectionType", lookupResult.Traits?.ConnectionType},
                    {"Domain", lookupResult.Traits?.Domain},
                    {"IPAddress", lookupResult.Traits?.IPAddress},
                    {"IsAnonymous", lookupResult.Traits?.IsAnonymous.ToString()},
                    {"IsAnonymousVpn", lookupResult.Traits?.IsAnonymousVpn.ToString()},
                    {"IsHostingProvider", lookupResult.Traits?.IsHostingProvider.ToString()},
                    {"IsLegitimateProxy", lookupResult.Traits?.IsLegitimateProxy.ToString()},
                    {"IsPublicProxy", lookupResult.Traits?.IsPublicProxy.ToString()},
                    {"IsTorExitNode", lookupResult.Traits?.IsTorExitNode.ToString()},
                    {"Isp", lookupResult.Traits?.Isp},
                    {"Organization", lookupResult.Traits?.Organization},
                    {"StaticIPScore", lookupResult.Traits?.StaticIPScore.ToString()},
                    {"UserCount", lookupResult.Traits?.UserCount.ToString()},
                    {"UserType", lookupResult.Traits?.UserType}
                };

                var geoLocationEntity =
                    new GeoLocationEntity(_partitionKeyHelper.GetPartitionKeyFromAddress(address), address)
                    {
                        ContinentCode = lookupResult.Continent?.Code ?? string.Empty,
                        ContinentName = lookupResult.Continent?.Name ?? string.Empty,
                        CountryCode = lookupResult.Country?.IsoCode ?? string.Empty,
                        CountryName = lookupResult.Country?.Name ?? string.Empty,
                        IsEuropeanUnion = lookupResult.Country?.IsInEuropeanUnion ?? false,
                        CityName = lookupResult.City?.Name ?? string.Empty,
                        PostalCode = lookupResult.Postal?.Code ?? string.Empty,
                        RegisteredCountry = lookupResult.RegisteredCountry?.IsoCode ?? string.Empty,
                        RepresentedCountry = lookupResult.RepresentedCountry?.IsoCode ?? string.Empty,
                        Latitude = lookupResult.Location?.Latitude ?? 0.0,
                        Longitude = lookupResult.Location?.Longitude ?? 0.0,
                        AccuracyRadius = lookupResult.Location?.AccuracyRadius ?? 0,
                        Timezone = lookupResult.Location?.TimeZone ?? string.Empty,
                        Traits = traits
                    };

                return _locationsRepository.StoreEntity(geoLocationEntity);
            }
        }
    }
}