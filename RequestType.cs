using System.Text.Json.Serialization;

namespace cli;

internal class RequestType
{
    [JsonIgnore]
    public string url { get; set; }
    public string authCode { get; set; }
    public string? displayName { get; set; }
    public string host { get; set; }
    public int? port { get; set; }
    public int intermediaryPort { get; set; }
}
