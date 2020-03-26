namespace FM.GeoLocation.Contract.Models
{
    public class RemoveDataForAddressResponse
    {
        public bool Success => ErrorMessage == null;

        public string ErrorMessage { get; set; } = null;
    }
}