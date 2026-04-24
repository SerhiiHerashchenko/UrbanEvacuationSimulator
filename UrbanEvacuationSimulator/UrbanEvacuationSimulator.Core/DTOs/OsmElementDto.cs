using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UrbanEvacuationSimulator.Core.DTOs;

public class OsmElementDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lon")]
    public double? Lon { get; set; }

    [JsonPropertyName("nodes")]
    public List<long> Nodes { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; }
}