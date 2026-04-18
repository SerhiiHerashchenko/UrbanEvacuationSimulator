// \UrbanEvacuationSimulator.Core\DTOs\OsmEdgeDto.cs
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace UrbanEvacuationSimulator.Core.DTOs;

public class OsmEdgeDto
{
    [JsonPropertyName("osm_id")]
    public int Id { get; set; }

    [JsonPropertyName("start_point")]
    public OsmNodeDto Start { get; set; }

    [JsonPropertyName("end_point")]
    public OsmNodeDto End { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; }

    [JsonIgnore]
    public double? Lanes 
    {
        get 
        {
            if (Tags != null && Tags.TryGetValue("lanes", out var lanesStr))
            {
                if (double.TryParse(lanesStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lanes))
                {
                    return lanes;
                }
            }
            return null;
        }
    }
}