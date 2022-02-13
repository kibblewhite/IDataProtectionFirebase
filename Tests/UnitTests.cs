namespace Test;

/// <summary>
/// Please ensure that you have set the ProjectId correctly in the "appsettings.Tests.json" file
/// </summary>
[TestClass]
public class UnitTests
{

    protected const string default_app_settings_json_filename = "appsettings.Tests.json";
    protected const string service_name = "IDataProtectionFirebase.UnitTest";
    protected IConfiguration? configuration;
    protected string? project_id;

    [TestInitialize]
    public void TestInitialize()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)     // Assembly Microsoft.Extensions.Configuration.FileExtensions
            .AddJsonFile(default_app_settings_json_filename).Build();  // Assembly Microsoft.Extensions.Configuration.Json

        project_id = configuration["ProjectId"];
        if (project_id.Equals("project-id-from-gcp-firebase") is true)
        {
            throw new InvalidOperationException("Please ensure that you have set the ProjectId correctly in the \"appsettings.Tests.json\" file. Also read the README.md for more details.");
        }
    }


    [TestMethod]
    public void BasicIntergratedTest()
    {

        IServiceCollection service_container = new ServiceCollection();

        string json_credentials = ReadGoogleJsonCredentialsFile();
        service_container
            .AddDataProtection()
            .PersistKeysToFirebase(service_name, json_credentials, project_id ?? string.Empty, true);

        ServiceProvider service_provider = service_container.BuildServiceProvider();
        IConfigureOptions<KeyManagementOptions>? key_management_options = service_provider.GetRequiredService<IConfigureOptions<KeyManagementOptions>>();

        Assert.IsNotNull(key_management_options);
        AssertDataProtectUnprotect(service_provider);

    }

    [TestMethod]
    public void FirestoreDbFactoryTest()
    {
        string json_credentials = ReadGoogleJsonCredentialsFile();
        FirestoreDbFactory factory = new(json_credentials, project_id ?? string.Empty);
        FirestoreDb? firestore_db = factory.CreateInstance();
        Assert.IsNotNull(firestore_db);
    }


    [TestMethod]
    public void FirestoreDbRepositoryTest()
    {
        string json_credentials = ReadGoogleJsonCredentialsFile();
        FirestoreDbFactory factory = new(json_credentials, project_id ?? string.Empty);
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
