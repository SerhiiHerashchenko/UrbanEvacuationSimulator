using UrbanEvacuationSimulator.Core.DTOs;

namespace UrbanEvacuationSimulator.Core.Interfaces;

public interface IMapParser
{
    public bool TryParse(string filePath, out OverpassResponseDto responseDto);
}