using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Models;
using FM.GeoLocation.Repositories;
using FM.GeoLocation.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FM.GeoLocation.FuncApp
{
    public class LookupAddressBatch
    {
        private readonly IAddressHelper _addressHelper;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IMaxMindLocationsRepository _maxMindLocationsRepository;

        public LookupAddressBatch(
            IMaxMindLocationsRepository maxMindLocationsRepository,
            ILocationsRepository locationsRepository,
            IAddressHelper addressHelper)
        {
            _maxMindLocationsRepository = maxMindLocationsRepository ??
                                          throw new ArgumentNullException(nameof(maxMindLocationsRepository));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _addressHelper = addressHelper ?? throw new ArgumentNullException(nameof(addressHelper));
        }

        [FunctionName("LookupAddressBatch")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var addressData = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(addressData))
                return new BadRequestObjectResult(new
                    {error = "true", message = "A request body is required for this function"});

            List<string> addresses;
            try
            {
                addresses = JsonConvert.DeserializeObject<List<string>>(addressData);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Could not deserialize request body");

                return new BadRequestObjectResult(new
                    {error = "true", message = "Unable to deserialize request body"});
            }

            var results = new List<GeoLocationDto>();

            foreach (var address in addresses)
            {
                if (!_addressHelper.ConvertAddress(address, out var validatedAddress))
                    results.Add(null);

                log.LogInformation($"Processing request for address {validatedAddress}");

                GeoLocationEntity location;
                try
                {
                    location = await _locationsRepository.GetGeoLocationEntity(validatedAddress) ??
                               await _maxMindLocationsRepository.GetGeoLocationEntity(validatedAddress);
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, "Error retrieving geo location data from downstream services for {address}",
                        address);
                    results.Add(null);
                    continue;
                }

                results.Add(new GeoLocationDto
                {
                    Address = address,
                    TranslatedAddress = location.RowKey,
                    Country = location.Country,
                    City = location.City,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                });
            }

            return new OkObjectResult(results);
        }
    }
}