using System;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Interfaces;
using FM.GeoLocation.Contract.Models;
using FM.GeoLocation.Repositories;
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

        public RemoveDataForAddress(
            ILocationsRepository locationsRepository,
            IAddressValidator addressValidator)
        {
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

            var model = new RemoveDataForAddressResponse();

            if (string.IsNullOrWhiteSpace(address))
            {
                model.ErrorMessage = "You must provide an address to query against. IP or DNS is acceptable.";
                return new BadRequestObjectResult(model);
            }

            if (!_addressValidator.ConvertAddress(address, out var validatedAddress))
            {
                model.ErrorMessage = "The address provided is invalid. IP or DNS is acceptable.";
                return new BadRequestObjectResult(model);
            }

            log.LogInformation($"Processing purge request for address {validatedAddress}");

            var location = await _locationsRepository.GetGeoLocationEntity(validatedAddress);

            if (location == null)
            {
                model.ErrorMessage = "The address passed in could not be found within the GeoLocation database";
                return new OkObjectResult(model);
            }

            await _locationsRepository.RemoveGeoLocationEntity(location);
            return new OkObjectResult(model);
        }
    }
}