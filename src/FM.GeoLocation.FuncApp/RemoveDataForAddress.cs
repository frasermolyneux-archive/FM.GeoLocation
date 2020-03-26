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
    public class RemoveDataForAddress
    {
        private readonly IAddressValidator _addressValidator;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IMaxMindLocationsRepository _maxMindLocationsRepository;

        public RemoveDataForAddress(
            IMaxMindLocationsRepository maxMindLocationsRepository,
            ILocationsRepository locationsRepository,
            IAddressValidator addressValidator)
        {
            _maxMindLocationsRepository = maxMindLocationsRepository ??
                                          throw new ArgumentNullException(nameof(maxMindLocationsRepository));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _addressValidator = addressValidator ?? throw new ArgumentNullException(nameof(addressValidator));
        }

        [FunctionName("RemoveDataForAddress")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            string address = req.Query["address"];

            var model = new RemoveDataResponse
            {
                Address = address
            };

            if (string.IsNullOrWhiteSpace(address))
            {
                model.RemovalStatus = "An address query parameter is required for this function";
                return new BadRequestObjectResult(model);
            }

            if (!_addressValidator.ConvertAddress(address, out var validatedAddress))
            {
                model.RemovalStatus = "The address passed in is invalid, e.g. not an IP or domain";
                return new BadRequestObjectResult(model);
            }

            model.TranslatedAddress = validatedAddress;

            log.LogInformation($"Processing purge request for address {validatedAddress}");

            var location = await _locationsRepository.GetGeoLocationEntity(validatedAddress);

            if (location == null)
            {
                model.RemovalStatus = "The address passed in could not be found within the GeoLocation database";
                return new OkObjectResult(model);
            }

            await _locationsRepository.RemoveGeoLocationEntity(location);

            model.RemovalStatus = "The address and geo-data has been purged from the GeoLocation database";
            return new OkObjectResult(model);
        }
    }
}