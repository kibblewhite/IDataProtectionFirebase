namespace IDataProtectionFirebase;

public sealed class Project
{
    [JsonPropertyName("project_id")]
    public string Id { get; init; } = default!;
}
