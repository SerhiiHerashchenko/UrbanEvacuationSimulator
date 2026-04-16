using UrbanEvacuationSimulator.Core.DTOs;

namespace UrbanEvacuationSimulator.Core.Interfaces;

public interface IMapParser
{
    bool TryParse(string filename, out IReadOnlyList<OsmEdgeDto> edges);
}