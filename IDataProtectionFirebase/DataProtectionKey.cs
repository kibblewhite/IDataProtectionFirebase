namespace IDataProtectionFirebase;

[FirestoreData]
public sealed class DataProtectionKey : IDataProtectionKey
{
    [FirestoreProperty]
    [JsonPropertyName("Value")]
    public string Value { get; init; } = default!;

    [FirestoreProperty]
    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; init; } = default!;

    [FirestoreProperty]
    [JsonPropertyName("DateTimeUtc")]
    public DateTime DateTimeUtc { get; init; }
}
