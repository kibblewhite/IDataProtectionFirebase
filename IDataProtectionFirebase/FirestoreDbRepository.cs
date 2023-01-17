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

    /// <inheritdoc cref="GetAllElementsAsync"/>
    public IReadOnlyCollection<XElement> GetAllElements()
        => Task.Run(GetAllElementsAsync).GetAwaiter().GetResult();

    /// <summary>
    /// This method is used to retrieve all the XElement objects from the Firebase Firestore database using the Google.Cloud.Firestore library.
    /// Any records that fail to parse will be deleted if the remove_snapshots_that_fail_to_parse flag is set to true.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a collection of all the XElement objects stored in the Firebase Firestore database for a specific service name.</returns>
    private async Task<IReadOnlyCollection<XElement>> GetAllElementsAsync()
    {
        HashSet<XElement> results = new();

        Query collection_query = _db.Collection(nameof(DataProtectionKey)).WhereEqualTo(nameof(DataProtectionKey.ServiceName), _service_name);
        IAsyncEnumerable<DocumentSnapshot> document_reference = collection_query.StreamAsync();
        await foreach (DocumentSnapshot snapshot in document_reference)
        {
            if (snapshot.Exists is false) { continue; }
            try
            {
                IDataProtectionKey key = snapshot.ConvertTo<DataProtectionKey>();
                XElement element = XElement.Parse(key.Value);
                results.Add(element);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (_remove_snapshots_that_fail_to_parse is true && snapshot is not null)
                {
                    string friendly_name = snapshot.Reference.Path.Split(Path.AltDirectorySeparatorChar).Last();
                    await RemoveElementAsync(friendly_name);
                }
            }
        }

        return results;
    }

    /// <inheritdoc cref="StoreElementAsync(XElement, string)"/>
    public void StoreElement(XElement element, string friendlyName)
        => Task.Run(() => StoreElementAsync(element, friendlyName)).Wait();

    /// <summary>
    /// This method is used to store an XElement object in the Firebase Firestore database using the Google.Cloud.Firestore library.
    /// </summary>
    /// <remarks>The collection name is DataProtectionKey, the document name is friendlyName, the document will contain the XElement object serialized to string, the service name and the current UTC time.</remarks>
    /// <param name="element">The XElement object to be stored in the Firebase Firestore database.</param>
    /// <param name="friendlyName">The friendly name to be used as the key for the stored element.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <inheritdoc cref="GetElementAsync(string)"/>
    internal XElement? GetElement(string friendlyName)
        => Task.Run(() => GetElementAsync(friendlyName)).GetAwaiter().GetResult();

    /// <summary>
    /// This method is used to retrieve an XElement object from the Firebase Firestore database using the Google.Cloud.Firestore library.
    /// </summary>
    /// <remarks>Gets a specific document from a collection, the collection name is DataProtectionKey, the document name is friendlyName, it will convert the document data to DataProtectionKey object, parse the serialized XElement and return it.</remarks>
    /// <param name="friendlyName">The friendly name of the element to be retrieved from the Firebase Firestore database.</param>
    /// <returns>A task that represents the asynchronous operation, returning the XElement object or null if it doesn't exist or there was an exception during retrieval</returns>
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

    /// <inheritdoc cref="RemoveElementAsync(string)"/>
    internal Google.Cloud.Firestore.WriteResult RemoveElement(string friendlyName)
        => Task.Run(() => RemoveElementAsync(friendlyName)).GetAwaiter().GetResult();

    /// <summary>
    /// This method is used to remove an XElement object from the Firebase Firestore database using the Google.Cloud.Firestore library.
    /// </summary>
    /// <remarks>Delete a specific document from a collection, the collection name is DataProtectionKey, the document name is friendlyName, it will return the WriteResult that contains the status of the deletion.</remarks>
    /// <param name="friendlyName">The friendly name of the element to be removed from the Firebase Firestore database.</param>
    /// <returns>A task that represents the asynchronous operation, returning the WriteResult object that contains the status of the deletion</returns>
    internal async Task<Google.Cloud.Firestore.WriteResult> RemoveElementAsync(string friendlyName)
    {
        CollectionReference collection_reference = _db.Collection(nameof(DataProtectionKey));
        DocumentReference document_reference = collection_reference.Document(friendlyName);
        Google.Cloud.Firestore.WriteResult write_result = await document_reference.DeleteAsync();
        return write_result;
    }
}
