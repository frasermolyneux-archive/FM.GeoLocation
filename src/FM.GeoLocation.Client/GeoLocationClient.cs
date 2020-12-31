using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                    ErrorMessage = "The lookup address is null or empty"
                };
            }

            if (_options.UseMemoryCache)
            {
                var cachedEntry = Cache.SingleOrDefault(c => c.Address == address);

                if (cachedEntry?.Created > DateTime.UtcNow.AddMinutes(-_options.CacheEntryLifeInMinutes))
                {
                    _logger?.LogDebug("Returning location for '{address}' from memory cache", address);
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
                            _logger?.LogWarning("Failed to get location for '{address}' - retry count: '{count}'", address,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationDto(address));

                if (locationResult == null)
                {
                    return new LookupAddressResponse
                    { 
                        ErrorMessage = $"Failed to get location for address '{address}'"
                    };
                }

                Cache.Add(new CacheEntry(address, DateTime.UtcNow, locationResult));

                return locationResult;
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Application exception getting location for address '{address}'", address);

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressResponse()
                    {
                        ErrorMessage = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception getting location for address '{address}'", address);

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressResponse()
                    {
                        ErrorMessage = $"Exception getting location for address '{address}'"
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
                            _logger?.LogWarning("Failed to get locations for address batch - retry count: '{count}'", retryCount);
                        })
                    .ExecuteAsync(async () => await GetGeoLocationBatchDto(addresses));

                if (locationResult == null)
                {
                    return new LookupAddressBatchResponse
                    { 
                        ErrorMessage = $"Failed to get locations for address batch"
                    };
                }

                return locationResult;
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Application exception getting locations for address batch");

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressBatchResponse()
                    {
                        ErrorMessage = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception getting locations for address batch");

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new LookupAddressBatchResponse()
                    {
                        ErrorMessage = "Exception getting locations for address batch"
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
                            _logger?.LogWarning("Failed to remove data for '{address}' - retry count: '{count}'", address,
                                retryCount);
                        })
                    .ExecuteAsync(async () => await RemoveAddressData(address));

                if (removeDataResponse == null)
                {
                    return new RemoveDataForAddressResponse
                    {
                        ErrorMessage = $"Failed to remove data for '{address}'"
                    };
                }

                return removeDataResponse;
            }
            catch(ApplicationException ex)
            {
                _logger?.LogError(ex, "Application exception removing data for address '{address}'", address);

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new RemoveDataForAddressResponse()
                    {
                        ErrorMessage = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception removing data for address '{address}'", address);

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new RemoveDataForAddressResponse()
                    {
                        ErrorMessage = $"Exception removing data for address '{address}'"
                    };
                }
            }
        }

        public async Task<Tuple<bool, string>> HealthCheck()
        {
            try
            {
                var healthCheckReponse = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(_options.RetryTimespans,
                        (result, timeSpan, retryCount, context) =>
                        {
                            _logger?.LogWarning("Failed to get health check - retry count: '{count}'", retryCount);
                        })
                    .ExecuteAsync(async () => await InternalHealthCheck());

                if (healthCheckReponse == null)
                {
                    return new Tuple<bool, string>(false, "Failed to perform health check");
                }

                return new Tuple<bool, string>(true, healthCheckReponse);
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Application exception performing health check");

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new Tuple<bool, string>(false, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception performing health check");

                if (_options.BubbleExceptions)
                {
                    throw ex;
                }
                else
                {
                    return new Tuple<bool, string>(false, "Exception performing health check");
                }
            }
        }

        private async Task<LookupAddressResponse> GetGeoLocationDto(string address)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"{_options.BaseUrl}/api/LookupAddress?code={_options.ApiKey}&address={address}";
                _logger?.LogDebug($"Request Uri: {requestUri}");

                var response = await client.PostAsync(requestUri, null);
                _logger?.LogDebug($"Response Code: '{response.StatusCode}'");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Failed to get address data for '{address}' with '{response.StatusCode}' response");
                }

                var responseText = await response.Content.ReadAsStringAsync();
                _logger?.LogDebug($"Response Text: '{responseText}'");

                var deserializeResponse = JsonConvert.DeserializeObject<LookupAddressResponse>(responseText);
                _logger?.LogInformation("'{@location}' retrieved for '{address}'", deserializeResponse, address);

                return deserializeResponse;
            }
        }

        private async Task<LookupAddressBatchResponse> GetGeoLocationBatchDto(List<string> addresses)
        {
            using (var client = new HttpClient())
            {
                var addressesJson = JsonConvert.SerializeObject(addresses);

                var requestUri = $"{_options.BaseUrl}/api/LookupAddressBatch?code={_options.ApiKey}";
                _logger?.LogDebug($"Request Uri: '{requestUri}'");

                var response = await client.PostAsync(requestUri, new StringContent(addressesJson));
                _logger?.LogDebug($"Response Code: '{response.StatusCode}'");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Failed to get batch location data for with '{response.StatusCode}' response");
                }

                var responseText = await response.Content.ReadAsStringAsync();
                _logger?.LogDebug($"Response Text: '{responseText}'");

                var deserializeResponse = JsonConvert.DeserializeObject<LookupAddressBatchResponse>(responseText);
                _logger?.LogInformation("'{@locations}' retrieved for '{addresses}'", deserializeResponse, addresses);

                return deserializeResponse;
            }
        }

        private async Task<RemoveDataForAddressResponse> RemoveAddressData(string address)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"{_options.BaseUrl}/api/RemoveDataForAddress?code={_options.ApiKey}&address={address}";
                _logger?.LogDebug($"Request Uri: '{requestUri}'");

                var response = await client.DeleteAsync(requestUri);
                _logger?.LogDebug($"Response Code: '{response.StatusCode}'");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Failed to remove address data for '{address}' with '{response.StatusCode}' response");
                }

                var responseText = await response.Content.ReadAsStringAsync();
                _logger?.LogDebug($"Response Text: '{responseText}'");

                var deserializeResponse = JsonConvert.DeserializeObject<RemoveDataForAddressResponse>(responseText);
                _logger?.LogInformation("'{@location}' retrieved for '{address}'", deserializeResponse, address);

                return deserializeResponse;
            }
        }

        private async Task<string> InternalHealthCheck()
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"{_options.BaseUrl}/api/HealthCheck?code={_options.ApiKey}";
                _logger?.LogDebug($"Request Uri: '{requestUri}'");

                var response = await client.DeleteAsync(requestUri);
                _logger?.LogDebug($"Response Code: '{response.StatusCode}'");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Failed to get health check with '{response.StatusCode}' response");
                }

                var responseText = await response.Content.ReadAsStringAsync();
                _logger?.LogDebug($"Response Text: '{responseText}'");

                return responseText;
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