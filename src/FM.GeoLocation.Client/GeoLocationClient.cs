using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

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

        public async Task<LookupAddressResponse> LookupAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

            if (_config.UseMemoryCache)
            {
                var cachedEntry = Cache.SingleOrDefault(c => c.Address == address);

                if (cachedEntry?.Created > DateTime.UtcNow.AddMinutes(-_config.CacheEntryLifeInMinutes))
                {
                    _logger?.LogDebug("Returning location for {address} from memory cache", address);
                    return cachedEntry.LookupAddressResponse;
                }

                Cache.Remove(cachedEntry);
            }

            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_config.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.LogWarning("Failed to get location for {address} - retry count: {count}", address,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationDto(address));

                Cache.Add(new CacheEntry(address, DateTime.UtcNow, locationResult));

                return locationResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get location for address {address}", address);
                throw;
            }
        }

        public async Task<LookupAddressBatchResponse> LookupAddressBatch(List<string> addresses)
        {
            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_config.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.LogWarning("Failed to get locations for {addresses} - retry count: {count}",
                                addresses,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationBatchDto(addresses));

                return locationResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get locations for addresses {addresses}", addresses);
                throw;
            }
        }

        public async Task<RemoveDataForAddressResponse> RemoveDataForAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

            try
            {
                var removeDataResponse = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_config.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.LogWarning("Failed remove data for {address} - retry count: {count}", address,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await RemoveAddressData(address));

                return removeDataResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to remove data for address {address}", address);
                throw;
            }
        }

        private async Task<LookupAddressResponse> GetGeoLocationDto(string address)
        {
            using (var client = new HttpClient())
            {
                var response =
                    await client.PostAsync(
                        $"{_config.BaseUrl}/api/LookupAddress?code={_config.ApiKey}&address={address}", null);

                var responseText = await response.Content.ReadAsStringAsync();
                var deserializeResponse = JsonConvert.DeserializeObject<LookupAddressResponse>(responseText);

                _logger?.LogDebug("{@location} retrieved for {address}", deserializeResponse, address);

                return deserializeResponse;
            }
        }

        private async Task<LookupAddressBatchResponse> GetGeoLocationBatchDto(List<string> addresses)
        {
            using (var client = new HttpClient())
            {
                var addressesJson = JsonConvert.SerializeObject(addresses);

                var response =
                    await client.PostAsync($"{_config.BaseUrl}/api/LookupAddressBatch?code={_config.ApiKey}",
                        new StringContent(addressesJson));

                var responseText = await response.Content.ReadAsStringAsync();
                var deserializeResponse = JsonConvert.DeserializeObject<LookupAddressBatchResponse>(responseText);

                _logger?.LogDebug("{@locations} retrieved for {addresses}", deserializeResponse, addresses);

                return deserializeResponse;
            }
        }

        private async Task<RemoveDataForAddressResponse> RemoveAddressData(string address)
        {
            using (var client = new HttpClient())
            {
                var response =
                    await client.DeleteAsync(
                        $"{_config.BaseUrl}/api/RemoveDataForAddress?code={_config.ApiKey}&address={address}");

                var responseText = await response.Content.ReadAsStringAsync();
                var deserializeResponse = JsonConvert.DeserializeObject<RemoveDataForAddressResponse>(responseText);

                _logger?.LogDebug("{@location} retrieved for {address}", deserializeResponse, address);

                return deserializeResponse;
            }
        }

        public class CacheEntry
        {
            public CacheEntry(string address, DateTime created, LookupAddressResponse lookupAddressResponse)
            {
                Address = address;
                Created = created;
                LookupAddressResponse = lookupAddressResponse;
            }

            public string Address { get; }
            public DateTime Created { get; }
            public LookupAddressResponse LookupAddressResponse { get; }
        }
    }
}