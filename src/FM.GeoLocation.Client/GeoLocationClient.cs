using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Interfaces;
using FM.GeoLocation.Contract.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace FM.GeoLocation.Client
{
    public class GeoLocationClient : IGeoLocationClient
    {
        private readonly ILogger<GeoLocationClient> _logger;
        private readonly IGeoLocationClientOptions _options;

        public GeoLocationClient(ILogger<GeoLocationClient> logger, IGeoLocationClientOptions options)
        {
            _logger = logger;
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (_options.RetryTimespans == null) _options.RetryTimespans = new[] {TimeSpan.FromSeconds(1)};
        }

        public List<CacheEntry> Cache { get; set; } = new List<CacheEntry>();

        public async Task<LookupAddressResponse> LookupAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return new LookupAddressResponse
                {
                    ErrorMessage = "Lookup address is null or empty"
                };
            }

            if (_options.UseMemoryCache)
            {
                var cachedEntry = Cache.SingleOrDefault(c => c.Address == address);

                if (cachedEntry?.Created > DateTime.UtcNow.AddMinutes(-_options.CacheEntryLifeInMinutes))
                {
                    _logger?.LogDebug("Returning location for {address} from memory cache", address);
                    return cachedEntry.LookupAddressResponse;
                }

                Cache.Remove(cachedEntry);
            }

            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_options.RetryTimespans,
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

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressResponse()
                    {
                        ErrorMessage = $"Failed to lookup address {address} data"
                    };
                }
            }
        }

        public async Task<LookupAddressBatchResponse> LookupAddressBatch(List<string> addresses)
        {
            try
            {
                var locationResult = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_options.RetryTimespans,
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

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressBatchResponse()
                    {
                        ErrorMessage = $"Failed to lookup locations for address batch"
                    };
                }
            }
        }

        public async Task<RemoveDataForAddressResponse> RemoveDataForAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return new RemoveDataForAddressResponse
                {
                    ErrorMessage = "Lookup address is null or empty"
                };
            }

            try
            {
                var removeDataResponse = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_options.RetryTimespans,
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

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new RemoveDataForAddressResponse()
                    {
                        ErrorMessage = $"Failed to remove address {address} data"
                    };
                }
            }
        }

        private async Task<LookupAddressResponse> GetGeoLocationDto(string address)
        {
            using (var client = new HttpClient())
            {
                var response =
                    await client.PostAsync(
                        $"{_options.BaseUrl}/api/LookupAddress?code={_options.ApiKey}&address={address}", null);

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
                    await client.PostAsync($"{_options.BaseUrl}/api/LookupAddressBatch?code={_options.ApiKey}",
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
                        $"{_options.BaseUrl}/api/RemoveDataForAddress?code={_options.ApiKey}&address={address}");

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