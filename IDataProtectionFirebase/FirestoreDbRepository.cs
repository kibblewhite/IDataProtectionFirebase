[assembly: InternalsVisibleTo("Tests")]
namespace IDataProtectionFirebase;

public sealed class FirestoreDbRepository : IXmlRepository
{

    private readonly FirestoreDb _db;
    private readonly string _service_name;
    private readonly bool _remove_snapshots_that_fail_to_parse;

    public FirestoreDbRepository(FirestoreDb db, string serviceName, bool removeSnapshotsThatFailToParse = false)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _service_name = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _remove_snapshots_that_fail_to_parse = removeSnapshotsThatFailToParse;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return Task.Run(GetAllElementsAsync).GetAwaiter().GetResult();
    }

    private async Task<IReadOnlyCollection<XElement>> GetAllElementsAsync()
    {
        HashSet<XElement> results = new();
        DocumentSnapshot? current_snapshot = null;

        CollectionReference collection_reference = _db.Collection(nameof(DataProtectionKey));
        IAsyncEnumerable<DocumentSnapshot> document_reference = collection_reference.StreamAsync();
        await foreach (DocumentSnapshot snapshot in document_reference)
        {
            if (snapshot.Exists is false) { continue; }
            current_snapshot = snapshot;
            try
            {
                IDataProtectionKey key = snapshot.ConvertTo<DataProtectionKey>();
                XElement element = XElement.Parse(key.Value);
                results.Add(element);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (_remove_snapshots_that_fail_to_parse is true && current_snapshot is not null)
                {
                    string friendly_name = current_snapshot.Reference.Path.Split(Path.AltDirectorySeparatorChar).Last();
                    await RemoveElementAsync(friendly_name);
                }
            }
        }

        return results;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        Task.Run(() => StoreElementAsync(element, friendlyName)).Wait();
    }

    private async Task StoreElementAsync(XElement element, string friendlyName)
    {
        CollectionReference collection_reference = _db.Collection(nameof(DataProtectionKey));
        DocumentReference document_reference = collection_reference.Document(friendlyName);
        await document_reference.CreateAsync(new DataProtectionKey
        {
            Value = element.ToString(),
            ServiceName = _service_name,
            DateTimeUtc = DateTime.UtcNow
        });
    }

    internal XElement? GetElement(string friendlyName)
    {
        return Task.Run(() => GetElementAsync(friendlyName)).GetAwaiter().GetResult();
    }

    internal async Task<XElement?> GetElementAsync(string friendlyName)
    {
        try
        {
            CollectionReference collection_reference = _db.Collection(nameof(DataProtectionKey));
            DocumentReference document_reference = collection_reference.Document(friendlyName);
            DocumentSnapshot snapshot = await document_reference.GetSnapshotAsync();
            if (snapshot.Exists is false) { return null; }
            IDataProtectionKey key = snapshot.ConvertTo<DataProtectionKey>();
            XElement element = XElement.Parse(key.Value);
            return element;
        }
        catch
        {
            return null;
        }
    }

    internal Google.Cloud.Firestore.WriteResult RemoveElement(string friendlyName)
    {
        return Task.Run(() => RemoveElementAsync(friendlyName)).GetAwaiter().GetResult();
    }

    internal async Task<Google.Cloud.Firestore.WriteResult> RemoveElementAsync(string friendlyName)
    {
        CollectionReference collection_reference = _db.Collection(nameof(DataProtectionKey));
        DocumentReference document_reference = collection_reference.Document(friendlyName);
        Google.Cloud.Firestore.WriteResult write_result = await document_reference.DeleteAsync();
        return write_result;
    }

}

