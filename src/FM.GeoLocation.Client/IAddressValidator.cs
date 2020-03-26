namespace FM.GeoLocation.Client
{
    public interface IAddressValidator
    {
        bool ConvertAddress(string address, out string validatedAddress);
    }
}