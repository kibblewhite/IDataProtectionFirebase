namespace IDataProtectionFirebase;

[FirestoreData]
public sealed class DataProtectionKey : IDataProtectionKey
{
    [FirestoreProperty]
    [JsonPropertyName("Value")]
    public required string Value { get; init; }

    [FirestoreProperty]
    [JsonPropertyName("ServiceName")]
    public required string ServiceName { get; init; }

    [FirestoreProperty]
    [JsonPropertyName("DateTimeUtc")]
    public required DateTime DateTimeUtc { get; init; }
}
