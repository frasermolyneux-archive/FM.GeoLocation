using System;
using System.Threading.Tasks;
using FM.GeoLocation.Repositories.Models;
using Microsoft.Azure.Cosmos.Table;

namespace FM.GeoLocation.Repositories
{
    public interface ILocationsRepository
    {
        Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity);
        Task<GeoLocationEntity> GetGeoLocationEntity(string address);
        Task RemoveGeoLocationEntity(GeoLocationEntity entity);
    }

    public class LocationsRepository : ILocationsRepository
    {
        private readonly IPartitionKeyHelper _partitionKeyHelper;
        private readonly ITableStorageConfiguration _tableStorageConfiguration;

        public LocationsRepository(
            ITableStorageConfiguration tableStorageConfiguration,
            IPartitionKeyHelper partitionKeyHelper)
        {
            _tableStorageConfiguration = tableStorageConfiguration ??
                                         throw new ArgumentNullException(nameof(tableStorageConfiguration));
            _partitionKeyHelper = partitionKeyHelper ?? throw new ArgumentNullException(nameof(partitionKeyHelper));
        }

        public async Task<GeoLocationEntity> StoreEntity(GeoLocationEntity entity)
        {
            var tableReference = await GetReference();

            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            var result = await tableReference.ExecuteAsync(insertOrReplaceOperation);
            var insertedGeoLocationEntity = (GeoLocationEntity) result.Result;

            return insertedGeoLocationEntity;
        }

        public async Task<GeoLocationEntity> GetGeoLocationEntity(string address)
        {
            var tableReference = await GetReference();

            var retrieveTableOperation =
                TableOperation.Retrieve<GeoLocationEntity>(_partitionKeyHelper.GetPartitionKeyFromAddress(address),
                    address);

            var result = await tableReference.ExecuteAsync(retrieveTableOperation);

            var retrievedEntity = result.Result as GeoLocationEntity;

            return retrievedEntity;
        }

        public async Task RemoveGeoLocationEntity(GeoLocationEntity entity)
        {
            var tableReference = await GetReference();

            var deleteTableOperation = TableOperation.Delete(entity);

            await tableReference.ExecuteAsync(deleteTableOperation);
        }

        private async Task<CloudTable> GetReference()
        {
            var storageAccount = CloudStorageAccount.Parse(_tableStorageConfiguration.TableStorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference("locationsv2");
            await tableReference.CreateIfNotExistsAsync();

            return tableReference;
        }
    }
}