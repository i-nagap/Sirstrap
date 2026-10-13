namespace Sirstrap.SDK.Weao
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReadCommentHandling = JsonCommentHandling.Skip)]
    [JsonSerializable(typeof(ExploitStatus))]
    [JsonSerializable(typeof(List<ExploitStatus>))]
    [JsonSerializable(typeof(RobloxVersions))]
    [JsonSerializable(typeof(SuncData))]
    [JsonSerializable(typeof(WeaoErrorResponse))]
    internal sealed partial class WeaoJsonContext : JsonSerializerContext
    {
    }
}