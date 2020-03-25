using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace FM.GeoLocation.Client
{
    public class GeoLocationClient : IGeoLocationClient
    {
        private readonly IGeoLocationClientConfiguration _config;
        private readonly ILogger _logger;

        public GeoLocationClient(IGeoLocationClientConfiguration config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        public List<CacheEntry> Cache { get; set; } = new List<CacheEntry>();

        public async Task<GeoLocationDto> LookupAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

            if (_config.UseMemoryCache)
            {
                var cachedEntry = Cache.SingleOrDefault(c => c.Address == address);

                if (cachedEntry?.Created > DateTime.UtcNow.AddMinutes(-_config.CacheEntryLifeInMinutes))
                {
                    _logger?.Debug("Returning location for {address} from memory cache", address);
                    return cachedEntry.GeoLocationDto;
                }

                Cache.Remove(cachedEntry);
            }

            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_config.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.Warning("Failed to get location for {address} - retry count: {count}", address,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationDto(address));

                Cache.Add(new CacheEntry(address, DateTime.UtcNow, locationResult));

                return locationResult;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Failed to get location for address {address}", address);
                throw;
            }
        }

        public async Task<List<GeoLocationDto>> LookupAddressBatch(List<string> addresses)
        {
            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_config.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.Warning("Failed to get locations for {addresses} - retry count: {count}",
                                addresses,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationBatchDto(addresses));

                return locationResult;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Failed to get locations for addresses {addresses}", addresses);
                throw;
            }
        }

        private async Task<GeoLocationDto> GetGeoLocationDto(string address)
        {
            using (var wc = new WebClient())
            {
                var locationString =
                    await wc.DownloadStringTaskAsync(
                        $"{_config.BaseUrl}/api/LookupAddress?code={_config.ApiKey}&address={address}");
                var deserializeLocation = JsonConvert.DeserializeObject<GeoLocationDto>(locationString);

                _logger?.Debug("{@location} retrieved for {address}", deserializeLocation, address);

                return deserializeLocation;
            }
        }

        private async Task<List<GeoLocationDto>> GetGeoLocationBatchDto(List<string> addresses)
        {
            using (var wc = new WebClient())
            {
                var addressesJson = JsonConvert.SerializeObject(addresses);

                var locationsString =
                    await wc.UploadStringTaskAsync(
                        $"{_config.BaseUrl}/api/LookupAddressBatch?code={_config.ApiKey}", addressesJson);
                var deserializeLocations = JsonConvert.DeserializeObject<List<GeoLocationDto>>(locationsString);

                _logger?.Debug("{@locations} retrieved for {addresses}", deserializeLocations, addresses);

                return deserializeLocations;
            }
        }

        public class CacheEntry
        {
            public CacheEntry(string address, DateTime created, GeoLocationDto geoLocationDto)
            {
                Address = address;
                Created = created;
                GeoLocationDto = geoLocationDto;
            }

            public string Address { get; }
            public DateTime Created { get; }
            public GeoLocationDto GeoLocationDto { get; }
        }
    }
}