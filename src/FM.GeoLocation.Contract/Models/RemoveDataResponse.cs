﻿namespace FM.GeoLocation.Contract.Models
{
    public class RemoveDataResponse
    {
        public string Address { get; set; }
        public string TranslatedAddress { get; set; }

        public string RemovalStatus { get; set; }
    }
}