using System;
using System.Text.Json.Serialization;

namespace UrbanEvacuationSimulator.Core.DTOs;

public class OverpassResponseDto
{
    [JsonPropertyName("elements")]
    public List<OsmElementDto> Elements { get; set; }
}
