using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;

namespace FM.GeoLocation.FuncApp
{
    public class LookupAddressBatch
    {
        private readonly IAddressValidator _addressValidator;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IMaxMindLocationsRepository _maxMindLocationsRepository;

        public LookupAddressBatch(
            IMaxMindLocationsRepository maxMindLocationsRepository,
            ILocationsRepository locationsRepository,
            IAddressValidator addressValidator)
        {
            _maxMindLocationsRepository = maxMindLocationsRepository ??
                                          throw new ArgumentNullException(nameof(maxMindLocationsRepository));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _addressValidator = addressValidator ?? throw new ArgumentNullException(nameof(addressValidator));
        }

        [FunctionName("LookupAddressBatch")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var addressData = await new StreamReader(req.Body).ReadToEndAsync();

            var model = new LookupAddressBatchResponse();

            if (string.IsNullOrWhiteSpace(addressData))
            {
                model.ErrorMessage = "You must provide a line separated list of addresses. IP or DNS is acceptable.";
                return new BadRequestObjectResult(model);
            }

            List<string> addresses;
            try
            {
                addresses = JsonConvert.DeserializeObject<List<string>>(addressData);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Could not deserialize request body");

                model.ErrorMessage =
                    "Invalid data, you must provide a line separated list of addresses. IP or DNS is acceptable.";
                return new BadRequestObjectResult(model);
            }

            model.LookupAddressResponses = new List<LookupAddressResponse>();

            foreach (var address in addresses)
            {
                var addressModel = new LookupAddressResponse();

                if (string.IsNullOrWhiteSpace(address))
                {
                    addressModel.ErrorMessage =
                        "You must provide an address to query against. IP or DNS is acceptable.";
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
                }

                if (!_addressValidator.ConvertAddress(address, out var validatedAddress))
                {
                    addressModel.ErrorMessage = "The address provided is invalid. IP or DNS is acceptable.";
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
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
                    addressModel.ErrorMessage = ex.Message;
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
                }
                catch (GeoIP2Exception ex)
                {
                    addressModel.ErrorMessage = ex.Message;
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error retrieving geo location data from downstream services for {address}.",
                        address);

                    addressModel.ErrorMessage =
                        "There was a problem retrieving the data from the downstream MaxMind service.";
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
                }

                if (location != null)
                {
                    addressModel.GeoLocationDto = new GeoLocationDto
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
                    model.LookupAddressResponses.Add(addressModel);
                    continue;
                }

                addressModel.ErrorMessage = "There was a problem looking up the geo-data for the address";
                model.LookupAddressResponses.Add(addressModel);
            }

            return new OkObjectResult(model);
        }
    }
}