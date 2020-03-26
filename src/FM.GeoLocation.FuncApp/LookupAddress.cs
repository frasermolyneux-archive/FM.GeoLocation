using System;
using System.Threading.Tasks;
using FM.GeoLocation.Client;
using FM.GeoLocation.Contract.Models;
using FM.GeoLocation.Repositories;
using FM.GeoLocation.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FM.GeoLocation.FuncApp
{
    public class LookupAddress
    {
        private readonly IAddressValidator _addressValidator;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IMaxMindLocationsRepository _maxMindLocationsRepository;

        public LookupAddress(
            IMaxMindLocationsRepository maxMindLocationsRepository,
            ILocationsRepository locationsRepository,
            IAddressValidator addressValidator)
        {
            _maxMindLocationsRepository = maxMindLocationsRepository ??
                                          throw new ArgumentNullException(nameof(maxMindLocationsRepository));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _addressValidator = addressValidator ?? throw new ArgumentNullException(nameof(addressValidator));
        }

        [FunctionName("LookupAddress")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            string address = req.Query["address"];

            if (string.IsNullOrWhiteSpace(address))
                return new BadRequestObjectResult(new
                    {error = "true", message = "An address parameter is required for this function"});

            if (!_addressValidator.ConvertAddress(address, out var validatedAddress))
                return new BadRequestObjectResult(new {error = "true", message = "Invalid ip address or hostname"});

            log.LogInformation($"Processing request for address {validatedAddress}");

            GeoLocationEntity location;
            try
            {
                location = await _locationsRepository.GetGeoLocationEntity(validatedAddress) ??
                           await _maxMindLocationsRepository.GetGeoLocationEntity(validatedAddress);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Error retrieving geo location data from downstream services for {address}", address);
                return new BadRequestObjectResult(new
                    {error = "true", message = "Error retrieving geo location data from downstream services"});
            }

            if (location != null)
                return new OkObjectResult(new GeoLocationDto
                {
                    Address = address,
                    TranslatedAddress = location.RowKey,
                    Country = location.Country,
                    City = location.City,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                });

            return new BadRequestObjectResult(
                new {error = "true", message = "Could not determine location for address"});
        }
    }
}