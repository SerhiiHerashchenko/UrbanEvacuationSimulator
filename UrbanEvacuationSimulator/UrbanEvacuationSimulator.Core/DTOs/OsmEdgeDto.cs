namespace UrbanEvacuationSimulator.Core.DTOs;

public record OsmEdgeDto()
{
    public int Id { get; init; }
    public OsmNodeDto Start { get; init; }
    public OsmNodeDto End { get; init; }
    public string Highway { get; init; }
    public string Surface { get; init; }
    public int? Lanes { get; init; }
    public int? MaxSpeed { get; init; }
}