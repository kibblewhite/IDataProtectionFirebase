namespace Microsoft.Extensions.DependencyInjection;

public static class DataProtectionBuilderExtensions
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="json_credentials">These are the JSON credentials provided by the Google Cloud Platform under the "service account - keys"</param>
    /// <param name="project_id">This is the name of the Google Project ID.</param>
    /// <param name="remove_snapshots_that_fail_to_parse">If records are found in the firebase db which can not be read or parsed, the system can attempt to delete that record, default is false</param>
    /// <returns></returns>
    public static IDataProtectionBuilder PersistKeysToFirebase(this IDataProtectionBuilder builder, string service_name, string json_credentials, string project_id, bool remove_snapshots_that_fail_to_parse = false)
    {

        builder.Services.TryAddFirestoreDb(json_credentials, project_id);

        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
        {
            FirestoreDb? firestore_db = services.GetService<FirestoreDb>();
            ArgumentNullException.ThrowIfNull(firestore_db, nameof(firestore_db));
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new FirestoreDbRepository(firestore_db, service_name, remove_snapshots_that_fail_to_parse);
            });
        });

        return builder;
    }

    internal static IServiceCollection TryAddFirestoreDb(this IServiceCollection collection, string json_credentials, string collection_name, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Func<IServiceProvider, FirestoreDb> factory = new FirestoreDbFactory(json_credentials, collection_name).CreateInstance;
        ServiceDescriptor descriptor = new(typeof(FirestoreDb), factory, lifetime);
        collection.TryAdd(descriptor);
        return collection;
    }

}

