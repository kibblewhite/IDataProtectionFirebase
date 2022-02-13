using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

public static class DataProtectionBuilderExtensions
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="json_credentials">These are the JSON credentials provided by the Google Cloud Platform under the "service account - keys". When it is set to null, the system will attempt to retrieve the FirestoreDb instance from the DI service container through the IServiceProvider.</param>
    /// <param name="remove_snapshots_that_fail_to_parse">During the method call 'GetAllElements', if there are any records found in the firebase db which can not be read or parsed, the system can attempt to delete that record, default is false</param>
    /// <returns></returns>
    public static IDataProtectionBuilder PersistKeysToFirebase(this IDataProtectionBuilder builder, string service_name, string? json_credentials = null, bool remove_snapshots_that_fail_to_parse = false)
    {

        if (string.IsNullOrWhiteSpace(json_credentials) is false)
        {
            Project? project = JsonSerializer.Deserialize<Project>(json_credentials);
            ArgumentNullException.ThrowIfNull(project?.Id, $"{nameof(Project)}.{nameof(Project.Id)}");
            builder.Services.TryAddFirestoreDb(json_credentials, project.Id);
        }

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

    internal static IServiceCollection TryAddFirestoreDb(this IServiceCollection collection, string json_credentials, string project_id, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Func<IServiceProvider, FirestoreDb> factory = new FirestoreDbFactory(json_credentials, project_id).CreateInstance;
        ServiceDescriptor descriptor = new(typeof(FirestoreDb), factory, lifetime);
        collection.TryAdd(descriptor);
        return collection;
    }

}

