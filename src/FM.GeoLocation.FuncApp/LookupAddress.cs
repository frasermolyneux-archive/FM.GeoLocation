using System;
using System.Threading.Tasks;
using FM.GeoLocation.Client;
using FM.GeoLocation.Contract.Models;
using FM.GeoLocation.Repositories;
using FM.GeoLocation.Repositories.Models;
using MaxMind.GeoIP2.Exceptions;
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

            var model = new LookupAddressResponse();

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

            log.LogInformation($"Processing request for address {validatedAddress}");

            GeoLocationEntity location;
            try
            {
                location = await _locationsRepository.GetGeoLocationEntity(validatedAddress) ??
                           await _maxMindLocationsRepository.GetGeoLocationEntity(validatedAddress);
            }
            catch (AddressNotFoundException ex)
            {
                model.ErrorMessage = ex.Message;
                return new BadRequestObjectResult(model);
            }
            catch (GeoIP2Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return new BadRequestObjectResult(model);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error retrieving geo location data from downstream services for {address}.", address);

                model.ErrorMessage = "There was a problem retrieving the data from the downstream MaxMind service.";
                return new BadRequestObjectResult(model);
            }

            if (location != null)
            {
                model.GeoLocationDto = new GeoLocationDto
                {
                    Address = address,
                    TranslatedAddress = location.RowKey,

                    ContinentCode = location.ContinentCode,
                    ContinentName = location.ContinentName,
                    CountryCode = location.CountryCode,
                    CountryName = location.CountryName,
                    IsEuropeanUnion = location.IsEuropeanUnion,
                    CityName = location.CityName,
                    PostalCode = location.PostalCode,
                    RegisteredCountry = location.RegisteredCountry,
                    RepresentedCountry = location.RepresentedCountry,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    AccuracyRadius = location.AccuracyRadius,
                    Timezone = location.Timezone,
                    Traits = location.Traits
                };

                return new OkObjectResult(model);
            }

            model.ErrorMessage = "There was a problem looking up the geo-data for the address";
            return new BadRequestObjectResult(model);
        }
    }
}