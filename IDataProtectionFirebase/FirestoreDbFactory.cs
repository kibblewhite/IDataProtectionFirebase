[assembly: InternalsVisibleTo("Tests")]
namespace IDataProtectionFirebase;

public sealed class FirestoreDbFactory
{
    public string JsonCredentials { get; private set; }
    public string ProjectId { get; private set; }

    public FirestoreDbFactory(string jsonCredentials, string projectId)
    {
        JsonCredentials = jsonCredentials ?? throw new ArgumentNullException(nameof(jsonCredentials));
        ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
    }

    internal FirestoreDb CreateInstance(IServiceProvider provider)
    {
        return CreateInstance();
    }

    internal FirestoreDb CreateInstance()
    {
        FirestoreClientBuilder firestore_client_builder = new() { JsonCredentials = JsonCredentials };
        FirestoreDb db = FirestoreDb.Create(ProjectId, firestore_client_builder.Build());
        return db;
    }

}

