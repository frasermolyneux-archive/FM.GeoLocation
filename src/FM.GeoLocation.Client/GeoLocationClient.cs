using System;
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

        public async Task<GeoLocationDto> LookupAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

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

                return locationResult;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Failed to get location for address {address}", address);
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
    }
}