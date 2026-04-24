using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using UrbanEvacuationSimulator.Core.DTOs;
using UrbanEvacuationSimulator.Core.Interfaces;

namespace UrbanEvacuationSimulator.Core.MapParsers;

public class JsonMapParser: IMapParser
{
    public bool TryParse(string filePath, out OverpassResponseDto responseDto)
    {
        responseDto = null;
        if (!File.Exists(filePath)) return false;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            using var stream = File.OpenRead(filePath);
            responseDto = JsonSerializer.Deserialize<OverpassResponseDto>(stream, options);
            return responseDto?.Elements != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse error: {ex.Message}");
            return false;
        }
    }
}