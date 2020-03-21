using System;
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

                return _locationsRepository.StoreEntity(new GeoLocationEntity(
                    _partitionKeyHelper.GetPartitionKeyFromAddress(address),
                    address,
                    (double) lookupResult.Location.Latitude,
                    (double) lookupResult.Location.Longitude,
                    lookupResult.Country.IsoCode,
                    lookupResult.City.Name));
            }
        }
    }
}