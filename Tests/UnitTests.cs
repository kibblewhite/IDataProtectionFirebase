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
        XElement element = XElement.Parse(@$"<xml>{friendly_name}</xml>");
        firestore_db_repository.StoreElement(element, friendly_name);

        XElement? retrieved_element = firestore_db_repository.GetElement(friendly_name);
        Assert.IsNotNull(retrieved_element);
        Assert.AreEqual(element.Value, retrieved_element?.Value);

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
        Assert.IsFalse(string.IsNullOrWhiteSpace(project?.Id));
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

/// <summary>
/// Bonus code: If you read or at least were decent enough to have scanned throught the unit tests and got this far, then here is a little extra treat. It will make integrating that little more slicker.
/// Don't forget to add the namespace...
/// </summary>
/// <remarks>
/// Usage:
///   WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
///   builder.Services.AddFirebaseDataProtection(builder.Configuration, "your-service-name-or-identifier", options =>
///   {
///       options.NewKeyLifetime = TimeSpan.FromDays(365);
///       options.AutoGenerateKeys = true;
///   }).UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
///   {
///       EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
///       ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
///   });
/// </remarks>
public static class IServiceCollectionExtensions
{
    public static IDataProtectionBuilder AddFirebaseDataProtection(this IServiceCollection svc, string service_application_name, Action<KeyManagementOptions> key_management_options, string json_credentials) => svc.AddDataProtection()
            .SetApplicationName(service_application_name)
            .AddKeyManagementOptions(key_management_options)
            .PersistKeysToFirebase(
                service_application_name,
                json_credentials,
                true
            );
}
