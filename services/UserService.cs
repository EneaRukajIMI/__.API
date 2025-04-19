using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Core.Exceptions.KeyValue; 


namespace MyCryptoAPI.Services
{
    public class UserService
    {
        private readonly INamedBucketProvider _bucketProvider;
        private readonly IClusterProvider _clusterProvider;

        public UserService(INamedBucketProvider bucketProvider, IClusterProvider clusterProvider)
        {
            _bucketProvider = bucketProvider;
            _clusterProvider = clusterProvider;
        }

        public async Task CreateUserAsync(User user)
        {
            var bucket = await _bucketProvider.GetBucketAsync();
            var collection = await bucket.DefaultCollectionAsync();
            await collection.InsertAsync(user.Id, user);
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var cluster = await _clusterProvider.GetClusterAsync();
            var query = $"SELECT META().id, * FROM `Bucketname` WHERE email = $email";

            var result = await cluster.QueryAsync<dynamic>(query, options =>
            {
                options.Parameter("email", email);
            });

            await foreach (var row in result)
            {
                var user = row["Bucketname"].ToObject<User>();
                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return user;
                }
            }

            return null;
        }

         public async Task<User?> GetUserByUsernameAsync(string username)
        {
         var cluster = await _clusterProvider.GetClusterAsync();

         var query = @"
            SELECT u.*
            FROM `bucketname` AS u
            WHERE u.username = $username
            LIMIT 1;
             ";

         var result = await cluster.QueryAsync<User>(query, options =>
        {
          options.Parameter("username", username);
        });
         await foreach (var user in result)
       {
         return user;
       }
         return null;
        }
        public async Task UpdateUserAsync(User user)
{
    var bucket = await _bucketProvider.GetBucketAsync();
    var collection = await bucket.DefaultCollectionAsync();
    await collection.UpsertAsync(user.Id, user);
}       

public async Task SavePaymentMethodAsync(string userId, PaymentMethod method)
{
    var bucket = await _bucketProvider.GetBucketAsync();
    var collection = await bucket.DefaultCollectionAsync();
    IGetResult result;

    try
    {
        result = await collection.GetAsync(userId);
    }
    catch (DocumentNotFoundException)
    {
        throw new Exception("User not found");
    }

    var user = result.ContentAs<User>();

    if (user == null)
    {
        throw new Exception("User document found, but deserialized to null.");
    }

    if (user.PaymentMethods == null)
    {
        user.PaymentMethods = new List<PaymentMethod>();
    }

    user.PaymentMethods.Add(method);

    await collection.UpsertAsync(userId, user);
}


    }
}

