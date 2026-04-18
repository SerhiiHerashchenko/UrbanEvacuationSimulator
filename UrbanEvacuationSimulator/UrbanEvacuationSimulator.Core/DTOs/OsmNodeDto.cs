using System.Text.Json.Serialization;

namespace UrbanEvacuationSimulator.Core.DTOs;

public class OsmNodeDto
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}