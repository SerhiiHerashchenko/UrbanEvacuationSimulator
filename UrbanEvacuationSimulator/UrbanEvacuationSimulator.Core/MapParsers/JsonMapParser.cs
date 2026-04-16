using UrbanEvacuationSimulator.Core.Interfaces;
using System.Text.Json;
using UrbanEvacuationSimulator.Core.DTOs;

namespace UrbanEvacuationSimulator.Core.MapParsers;

public class JsonMapParser : IMapParser
{   
    /// <summary>
    /// Try to parse JSON file with graph edges.
    /// </summary>
    /// <param name="filename">path to JSON file.</param>
    /// <param name="edges">Parsing result. Empty list if failed.</param>
    /// <returns>True, if success, otherwise False.</returns>
    public bool TryParse(string filename, out IReadOnlyList<OsmEdgeDto> edges)
    {
        edges = Array.Empty<OsmEdgeDto>();

        try
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            
            using FileStream stream = File.OpenRead(filename);
            
            var parsedEdges = JsonSerializer.Deserialize<List<OsmEdgeDto>>(stream, options);

            if (parsedEdges != null)
            {
                edges = parsedEdges.AsReadOnly();
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}