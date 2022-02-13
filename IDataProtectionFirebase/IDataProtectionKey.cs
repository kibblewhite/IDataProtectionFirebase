namespace IDataProtectionFirebase;

public interface IDataProtectionKey
{
    [JsonPropertyName("Value")]
    string Value { get; init; }

    [JsonPropertyName("ServiceName")]
    string ServiceName { get; init; }

    [JsonPropertyName("DateTimeUtc")]
    DateTime DateTimeUtc { get; init; }
}
