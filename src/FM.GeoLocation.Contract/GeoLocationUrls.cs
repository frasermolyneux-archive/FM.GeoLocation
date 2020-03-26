namespace FM.GeoLocation.Contract
{
    public static class GeoLocationUrls
    {
        public static string LookupAddressBase()
        {
            return "https://geo-location.net/Home/LookupAddress/";
        }

        public static string LookupAddress(string address)
        {
            return $"https://geo-location.net/Home/LookupAddress/{address}";
        }
    }
}