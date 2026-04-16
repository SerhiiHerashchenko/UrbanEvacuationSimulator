namespace UrbanEvacuationSimulator.Core.GraphStructures.Structures;

public class Node
{
    public readonly int Id;
    public double Lat { get; set; }
    public double Lon { get; set; }

    public Node(int id, double lat, double lon)
    {
        Id = id;
        Lat = lat;
        Lon = lon;
    }

    public override int GetHashCode() => HashCode.Combine(Lat, Lon);
    public override bool Equals(object? obj) => obj is Node n && n.Lat == Lat && n.Lon == Lon;
}