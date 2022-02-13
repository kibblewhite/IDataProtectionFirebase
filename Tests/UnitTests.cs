namespace Test;

[TestClass]
public class UnitTests
{

    protected const string service_name = "IDataProtectionFirebase.UnitTest";

    [TestMethod]
    public void BasicIntegratedTest()
    {

        IServiceCollection service_container = new ServiceCollection();

        string json_credentials = ReadGoogleJsonCredentialsFile();
        service_container
            .AddDataProtection()
            .PersistKeysToFirebase(service_name, json_credentials, true);

        ServiceProvider service_provider = service_container.BuildServiceProvider();
        IConfigureOptions<KeyManagementOptions>? key_management_options = service_provider.GetRequiredService<IConfigureOptions<KeyManagementOptions>>();

        Assert.IsNotNull(key_management_options);
        AssertDataProtectUnprotect(service_provider);

    }

    [TestMethod]
    public void FirestoreIntegratedTest()
    {
        IServiceCollection service_container = new ServiceCollection();

        string json_credentials = ReadGoogleJsonCredentialsFile();
        Project? project = JsonSerializer.Deserialize<Project>(json_credentials);

        service_container.TryAddFirestoreDb(json_credentials, project?.Id ?? string.Empty);

        service_container
            .AddDataProtection()
            .PersistKeysToFirebase(service_name);

        ServiceProvider service_provider = service_container.BuildServiceProvider();
        IConfigureOptions<KeyManagementOptions>? key_management_options = service_provider.GetRequiredService<IConfigureOptions<KeyManagementOptions>>();

        Assert.IsNotNull(key_management_options);
        AssertDataProtectUnprotect(service_provider);
    }

    [TestMethod]
    public void FirestoreDbFactoryTest()
    {
        string json_credentials = ReadGoogleJsonCredentialsFile();
        Project? project = JsonSerializer.Deserialize<Project>(json_credentials);

        FirestoreDbFactory factory = new(json_credentials, project?.Id ?? string.Empty);
        FirestoreDb? firestore_db = factory.CreateInstance();
        Assert.IsNotNull(firestore_db);
    }


    [TestMethod]
    public void FirestoreDbRepositoryTest()
    {
        string json_credentials = ReadGoogleJsonCredentialsFile();
        Project? project = JsonSerializer.Deserialize<Project>(json_credentials);

        FirestoreDbFactory factory = new(json_credentials, project?.Id ?? string.Empty);
        FirestoreDb? firestore_db = factory.CreateInstance();
        ArgumentNullException.ThrowIfNull(firestore_db, nameof(firestore_db));

        FirestoreDbRepository firestore_db_repository = new(firestore_db, service_name, true);

        string friendly_name = "TestElement";
        XElement element = XElement.Parse(@"<xml></xml>");
        firestore_db_repository.StoreElement(element, friendly_name);

        XElement? retrieved_element = firestore_db_repository.GetElement(friendly_name);
        Assert.IsNotNull(retrieved_element);

        WriteResult? write_result = firestore_db_repository.RemoveElement(friendly_name);
        Assert.IsNotNull(write_result);
        Assert.IsNotNull(write_result.UpdateTime);
    }

    [TestMethod]
    public void SimpleJsonTest()
    {
        string json_credentials = ReadGoogleJsonCredentialsFile();
        Project? project = JsonSerializer.Deserialize<Project>(json_credentials);
        Assert.IsNotNull(project?.Id);
    }

    private static void AssertDataProtectUnprotect(ServiceProvider services)
    {
        IDataProtector? data_protector = services.GetDataProtector("test-purpose");
        Assert.IsNotNull(data_protector);

        string plain_data = Guid.NewGuid().ToString();
        string? encrypted_data = data_protector.Protect(plain_data);
        string? decrypted_data = data_protector.Unprotect(encrypted_data);

        Assert.AreEqual(plain_data, decrypted_data);
    }

    private static string ReadGoogleJsonCredentialsFile(string filepath = "credentials.json")
    {
        string json_credentials = File.ReadAllText(filepath);
        return json_credentials;
    }

}
