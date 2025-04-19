using Couchbase;
using Couchbase.Extensions.DependencyInjection;


namespace MyCryptoAPI.Services
{
    public class MyCouchbaseService
    {
        private readonly IBucket _bucket;

        public MyCouchbaseService(INamedBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucketAsync().Result;
        }

        public async Task InsertDocumentAsync<T>(string key, T document)
        {
            var collection = await _bucket.DefaultCollectionAsync();
            await collection.UpsertAsync(key, document);
        }

        public async Task<T?> GetDocumentAsync<T>(string key)
        {
            var collection = await _bucket.DefaultCollectionAsync();
            try
            {
                var result = await collection.GetAsync(key);
                return result.ContentAs<T>();
            }
            catch (KeyNotFoundException)
            {
                return default;
            }
        }
    }
}
