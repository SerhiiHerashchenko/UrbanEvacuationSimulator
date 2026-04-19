using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UrbanEvacuationSimulator.Core.DTOs;
using UrbanEvacuationSimulator.Core.Interfaces;

namespace UrbanEvacuationSimulator.Core.MapParsers;

public class JsonMapParser : IMapParser
{
    public bool TryParse(string filePath, out IReadOnlyList<OsmEdgeDto> edges)
    {
        edges = new List<OsmEdgeDto>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Map file not found: {filePath}");
            return false;
        }

        try
        {
            var jsonString = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            using var stream = File.OpenRead(filePath);
            using var document = JsonDocument.Parse(stream);
            var root = document.RootElement;

            if (!root.TryGetProperty("roads", out var roadsArray))
            {
                Console.WriteLine("JSON Parse Error: Root object does not contain a 'roads' array.");
                return false;
            }

            var parsedEdges = roadsArray.Deserialize<List<OsmEdgeDto>>(options);

            if (parsedEdges != null)
            {
                edges = parsedEdges.AsReadOnly();
                return true;
            }

            return false;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[JSON Parsing Error]: {ex.Message}");
            Console.WriteLine($"Failed at Path: {ex.Path} | Line: {ex.LineNumber}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[System Error]: {ex.Message}");
            return false;
        }
    }
}