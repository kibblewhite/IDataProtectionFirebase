namespace IDataProtectionFirebase;

public sealed class Project
{
    [JsonPropertyName("project_id")]
    public required string Id { get; init; }
}
